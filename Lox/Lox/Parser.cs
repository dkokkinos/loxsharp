﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lox.TokenType;

namespace Lox
{
    public class Parser
    {
        private class ParseError : Exception { 
            
        }

        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        private Expr expression()
        {
            return assignment();
        }

        private Stmt declaration()
        {
            try
            {
                if (match(CLASS)) return classDeclaration();
                if (match(FUN)) return function("function");
                if (match(VAR)) return varDeclaration();
                return statement();
            }
            catch (ParseError error)
            {
                synchronize();
                return null;
            }
        }

        private Stmt classDeclaration()
        {
            Token name = consume(IDENTIFIER, "Expect class name.");
            consume(LEFT_BRACE, "Expect '{' before class body.");

            List<Stmt.Function> methods = new List<Stmt.Function>();
            while(!check(RIGHT_BRACE) && !isAtEnd())
            {
                methods.Add(function("method"));
            }

            consume(RIGHT_BRACE, "Expect '}' after class body.");
            return new Stmt.Class(name, methods);
        }

        private Stmt statement()
        {
            if (match(FOR)) return forStatement();
            if (match(IF)) return ifStatement();
            if (match(PRINT)) return printStatement();
            if (match(RETURN)) return returnStatement();
            if (match(WHILE)) return whileStatement();
            if (match(LEFT_BRACE)) return new Stmt.Block(block());

            return expressionStatement();
        }

        private Stmt forStatement()
        {
            consume(LEFT_PAREN, "Expect '(' after 'for'.");
            Stmt initializer;
            if (match(SEMICOLON))
                initializer = null;
            else if (match(VAR))
                initializer = varDeclaration();
            else
                initializer = expressionStatement();

            Expr condition = null;
            if (!check(SEMICOLON))
                condition = expression();
            consume(SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!check(RIGHT_PAREN))
                increment = expression();

            consume(RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = statement();

            //desugaring
            if(increment != null)
            {
                body = new Stmt.Block(new List<Stmt>() {
                    body,
                    new Stmt.Expression(increment)
                });
            }

            if (condition == null)
                condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if(initializer != null)
            {
                body = new Stmt.Block(new List<Stmt>()
                {
                    initializer, body
                });
            }

            return body;
        }

        private Stmt ifStatement()
        {
            consume(LEFT_PAREN, "Expect '(' after if.");
            var condition = expression();
            consume(RIGHT_PAREN, "Expect ')' after if condition.");
            Stmt thenBranch = statement();
            Stmt elseBranch = null;
            if (match(ELSE))
                elseBranch = statement();

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt returnStatement()
        {
            Token keyword = previous();
            Expr value = null;
            if(!check(SEMICOLON))
            {
                value = expression();
            }
            consume(SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt varDeclaration()
        {
            var name = consume(IDENTIFIER, "Expect variable name.");
            Expr initializer = null;
            if (match(EQUAL))
            {
                initializer = expression();
            }

            consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt whileStatement()
        {
            consume(LEFT_PAREN, "Expected '(' after while.");
            var condition = expression();
            consume(RIGHT_PAREN, "Expected ')' after while condition.");
            var body = statement();
            return new Stmt.While(condition, body);
        }

        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt.Function function(string kind)
        {
            Token name = consume(IDENTIFIER, "Expect " + kind + " name.");
            consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");

            List<Token> _params = new List<Token>();
            if (!check(RIGHT_PAREN))
            {
                do
                {
                    if (_params.Count >= 255)
                        error(peek(), "Can't have more than 255 parameters.");

                    _params.Add(consume(IDENTIFIER, "Expect parameter name."));
                } while (match(COMMA));
            }

            consume(RIGHT_PAREN, "Expect ')' after parameters.");
            consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
            var body = block();
            return new Stmt.Function(name, _params, body);
        }

        private List<Stmt> block()
        {
            List<Stmt> statements = new List<Stmt>();
            while(!check(RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(RIGHT_BRACE, "Expect '}' after block.");

            return statements;
        }

        private Expr assignment()
        {
            Expr expr = or();

            if (match(EQUAL))
            {
                Token equals = previous();
                Expr value = assignment();

                if(expr is Expr.Variable v)
                {
                    return new Expr.Assign(v.name, value);
                }else if(expr is Expr.Get get)
                {
                    return new Expr.Set(get._object, get.name, value);
                }

                error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr or()
        {
            Expr expr = and();

            while(match(OR))
            {
                Token _operator = previous();
                var right = and();
                expr = new Expr.Logical(expr, _operator, right);
            }
            return expr;
        }

        private Expr and()
        {
            Expr expr = equality();

            while (match(AND))
            {
                Token _operator = previous();
                var right = equality();
                expr = new Expr.Logical(expr, _operator, right);
            }

            return expr;
        }

        private Expr equality()
        {
            Expr expr = comparison();

            while (match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token @operator = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr comparison()
        {
            Expr expr = term();

            while(match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token @operator = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr term()
        {
            Expr expr = factor();

            while(match(MINUS, PLUS))
            {
                Token @operator = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr factor()
        {
            Expr expr = unary();

            while(match(SLASH, STAR))
            {
                Token @operator = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr unary()
        {
            if(match(BANG, MINUS))
            {
                Token @operator = previous();
                Expr right = unary();
                return new Expr.Unary(@operator, right);
            }
            else
            {
                return call();
            }
        }

        private Expr call()
        {
            var expr = primary();
            while (true)
            {
                if (match(LEFT_PAREN))
                    expr = finishCall(expr);
                else if (match(DOT))
                {
                    Token name = consume(IDENTIFIER, "Expect property name after '.'.");
                    expr = new Expr.Get(expr, name);
                }
                else
                    break;
            }

            return expr;
        }

        private Expr finishCall(Expr expr)
        {
            List<Expr> arguments = new List<Expr>();
            if (!check(RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count() >= 255)
                        error(peek(), "Can't have more than 255 arguments.");
                    arguments.Add(expression());
                } while (match(COMMA));
            }

            Token paren = consume(RIGHT_PAREN, "Expect ')' after arguments.");
            return new Expr.Call(expr, paren, arguments);
        }

        private Expr primary()
        {
            if (match(TRUE))
                return new Expr.Literal(true);
            if (match(FALSE))
                return new Expr.Literal(false);
            if (match(NIL))
                return new Expr.Literal(null);

            if (match(NUMBER, STRING))
                return new Expr.Literal(previous().literal);

            if(match(THIS)) 
                return new Expr.This(previous());

            if (match(IDENTIFIER))
                return new Expr.Variable(previous());
            
            if (match(LEFT_PAREN))
            {
                Expr expr = expression();
                consume(RIGHT_PAREN, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }

            throw error(peek(), "Expect expression.");
        }

        private bool match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        private Token consume(TokenType type, string message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }


        private bool check(TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        private bool isAtEnd()
        {
            return peek().type == EOF;
        }

        private Token peek()
        {
            return tokens[current];
        }

        private Token previous()
        {
            return tokens[current - 1];
        }

        private ParseError error(Token token, string message)
        {
            Lox.error(token, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().type == SEMICOLON) return;

                switch (peek().type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                advance();
            }
        }
    }
}
