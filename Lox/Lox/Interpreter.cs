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
                    return (decimal)left - (decimal)right;
                case TokenType.PLUS:
                    if(left is string l && right is string r)
                    {
                        return l + r;
                    }else if(left is decimal ld && right is decimal rd)
                    {
                        return ld + rd;
                    }
                    
                    var l_str = left.ToString();
                    var r_str = right.ToString();
                    return l_str + r_str;
                    
                case TokenType.STAR:
                    checkNumberOperands(expr._operator, left, right);
                    return (decimal)left * (decimal)right;
                case TokenType.SLASH:
                    checkNumberOperands(expr._operator, left, right);
                    return (decimal)left / (decimal)right;
                case TokenType.GREATER:
                    checkNumberOperands(expr._operator, left, right);
                    return (decimal)left > (decimal)right;
                case TokenType.GREATER_EQUAL:
                    checkNumberOperands(expr._operator, left, right);
                    return (decimal)left >= (decimal)right;
                case TokenType.LESS:
                    checkNumberOperands(expr._operator, left, right);
                    return (decimal)left < (decimal)right;
                case TokenType.LESS_EQUAL:
                    checkNumberOperands(expr._operator, left, right);
                    return (decimal)left <= (decimal)right;
                case TokenType.BANG_EQUAL:
                    return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return left.Equals( right );
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

            if(obj is decimal d)
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

        public object visitLogicalExpr(Expr.Logical expr)
        {
            var left = evaluate(expr);
            if(expr._operator.type == TokenType.OR)
            {
                if (isTruthy(left)) return left;
            }
            else
            {
                if (!isTruthy(left)) return left;
            }

            return evaluate(expr.right);
        }

        public object visitUnaryExpr(Expr.Unary expr)
        {
            var value = evaluate(expr.right);
            switch (expr._operator.type)
            {
                case TokenType.MINUS:
                    checkNumberOperand(expr._operator, value);
                    return -(decimal)value;
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
            if (right is decimal) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private void checkNumberOperands(Token @operator, object left, object right)
        {
            if (left is decimal && right is decimal)
                return;
            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        private bool isTruthy(object value)
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

        public object visitIfStmt(Stmt.If stmt)
        {
            if (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
                execute(stmt.elseBranch);

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

        public object visitWhileStmt(Stmt.While stmt)
        {
            while (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.body);
            }

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
