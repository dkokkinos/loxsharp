using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    
    public class Resolver : Expr.Visitor<object>, Stmt.Visitor<object>
    {

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD
        }


        private enum ClassType
        {
            NONE,
            CLASS
        }

        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes;
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            scopes = new();
        }



        public object visitAssignExpr(Expr.Assign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitBinaryExpr(Expr.Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitBlockStmt(Stmt.Block stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }


        public object visitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            declare(stmt.name);
            define(stmt.name);

            beginScope();
            scopes.Peek().Add("this", true);

            foreach(var method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                resolveFunction(method, declaration);
            }

            endScope();
            currentClass = enclosingClass;
            return null;
        }

        public object visitCallExpr(Expr.Call expr)
        {
            resolve(expr.callee);

            foreach (var arg in expr.arguments)
                resolve(arg);

            return null;
        }

        public object visitGetExpr(Expr.Get expr)
        {
            resolve(expr._object);
            return null;
        }

        public object visitExpressionStmt(Stmt.Expression stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object visitFunctionStmt(Stmt.Function stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object visitGroupingExpr(Expr.Grouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public object visitIfStmt(Stmt.If stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if (stmt.elseBranch != null)
                resolve(stmt.elseBranch);
            return null;
        }

        public object visitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object visitLogicalExpr(Expr.Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitSetExpr(Expr.Set expr)
        {
            resolve(expr.value);
            resolve(expr._object);
            return null;
        }

        public object visitThisExpr(Expr.This expr)
        {
            if(currentClass == ClassType.NONE)
            {
                Lox.error(expr.keyword, "Can't use 'this' outside of a class.");
                return null;
            }
            resolveLocal(expr, expr.keyword);
            return null;
        }

        public object visitPrintStmt(Stmt.Print stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object visitUnaryExpr(Expr.Unary expr)
        {
            resolve(expr.right);
            return null;
        }

        public object visitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Any() && 
                scopes.Peek().ContainsKey(expr.name.lexeme) &&
                !scopes.Peek()[expr.name.lexeme])
                Lox.error(expr.name, "Can't read local variable in its own initializer.");

            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitVarStmt(Stmt.Var stmt)
        {
            declare(stmt.name);
            if (stmt.initializer != null)
                resolve(stmt.initializer);

            define(stmt.name);
            return null;
        }

        public object visitWhileStmt(Stmt.While stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }


        public void resolve(Stmt stmt)
        {
            stmt.accept(this);
        }

        private void resolve(Expr expr)
        {
            expr.accept(this);
        }

        private void resolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            beginScope();
            foreach(var param in function._params)
            {
                declare(param);
                define(param);
            }

            resolve(function.body);
            endScope();

            currentFunction = enclosingFunction;
        }

        public object visitReturnStmt(Stmt.Return stmt)
        {
            if(currentFunction == FunctionType.NONE)
            {
                Lox.error(stmt.keyword, "Can't return from top-level code.");
            }
            if(stmt.value != null)
                resolve(stmt.value);
            return null;
        }

        private void beginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void endScope()
        {
            scopes.Pop();
        }

        private void declare(Token name)
        {
            if (!scopes.Any()) return;

            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Lox.error(name,
                    "Already a variable with this name in this scope.");
            }

            scope.Add(name.lexeme, false);
        }

        private void define(Token name)
        {
            if (!scopes.Any()) return;
            scopes.Peek()[name.lexeme] = true;
        }

        public void resolve(List<Stmt> statements)
        {
            foreach (var stmt in statements)
                resolve(stmt);
        }

        private void resolveLocal(Expr expr, Token name)
        {
            for(int  i = scopes.Count() - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.resolve(expr, scopes.Count() - 1 - i);
                    return;
                }
            }
        }

    }
}
