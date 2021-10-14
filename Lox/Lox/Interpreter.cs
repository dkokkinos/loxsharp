using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private Environment environment = new Environment();

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach(var statement in statements)
                {
                    execute(statement);
                }
            }catch(RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public object visitBinaryExpr(Expr.Binary expr)
        {
            var left = evaluate(expr.left);
            var right = evaluate(expr.right);

            switch (expr._operator.type)
            {
                case TokenType.MINUS:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if(left is string l && right is string r)
                    {
                        return l + r;
                    }else if(left is double ld && right is double rd)
                    {
                        return ld + rd;
                    }
                    
                    var l_str = left.ToString();
                    var r_str = right.ToString();
                    return l_str + r_str;
                    
                case TokenType.STAR:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left * (double)right;
                case TokenType.SLASH:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left / (double)right;
                case TokenType.GREATER:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    checkNumberOperands(expr._operator, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return isEqual(left, right);
            }

            // unreachable
            return null;
        }


        private bool isEqual(object left, object right)
        {
            if (left == null && right == null)
                return true;
            if (left == null)
                return false;
            return left == right;
        }

        private string stringify(object obj)
        {
            if (obj == null) return "nil";

            if(obj is double d)
            {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                    text = text.Substring(0, text.Length - 2);
                return text;
            }

            return obj.ToString();
        }

        public object visitGroupingExpr(Expr.Grouping expr)
        {
            return evaluate(expr.expression);
        }

        public object visitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object visitUnaryExpr(Expr.Unary expr)
        {
            var value = evaluate(expr.right);
            switch (expr._operator.type)
            {
                case TokenType.MINUS:
                    checkNumberOperand(expr._operator, value);
                    return -(double)value;
                case TokenType.BANG:
                    return isTruthy(value);
            }

            // unreachable
            return null;
        }

        public object visitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.name);
        }

        private void checkNumberOperand(Token @operator, object right)
        {
            if (right is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private void checkNumberOperands(Token @operator, object left, object right)
        {
            if (left is double && right is double)
                return;
            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        private object isTruthy(object value)
        {
            if (value == null)
                return false;
            if(value is bool b)
                return b;
            return true;
        }

        private object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        private void execute(Stmt statement)
        {
            statement.accept(this);
        }

        private void executeBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                foreach (var statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object visitBlockStmt(Stmt.Block stmt)
        {
            executeBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object visitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public object visitPrintStmt(Stmt.Print stmt)
        {
            var value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        public object visitVarStmt(Stmt.Var stmt)
        {
            object value = null;

            if(stmt.initializer != null)
                value = evaluate(stmt.initializer);
            
            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object visitAssignExpr(Expr.Assign expr)
        {
            var value = evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

    }
}
