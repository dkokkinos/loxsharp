using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        public readonly Environment globals;
        private Environment environment;
        private readonly Dictionary<Expr, int> locals = new ();

        public Interpreter()
        {
            globals = new Environment();
            environment = globals;

            globals.Define("clock", new ClockCallable());
        }

        class ClockCallable : LoxCallable
        {
            public int arity() => 0;

            public object call(Interpreter interpreter, List<object> arguments)
                => DateTime.Now;

            public override string ToString() => "<native fn>";
        }

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

        public object visitCallExpr(Expr.Call expr)
        {
            object callee = evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (var arg in expr.arguments)
                arguments.Add(evaluate(arg));

            if (callee is not LoxCallable function)
                throw new RuntimeError(expr.paren, "Can onle call functions and classes.");
            if (arguments.Count != function.arity())
                throw new RuntimeError(expr.paren, $"Expected {function.arity()} " +
                    $"arguments but got {arguments.Count}.");
            return function.call(this, arguments);
        }

        public object visitGetExpr(Expr.Get expr)
        {
            object _object = evaluate(expr._object);
            if (_object is LoxInstance loxInstance)
                return loxInstance.Get(expr.name);

            throw new RuntimeError(expr.name, "Only instances have properties.");
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

        public object visitSetExpr(Expr.Set expr)
        {
            var obj = evaluate(expr._object);

            if (obj is not LoxInstance loxInstance)
                throw new RuntimeError(expr.name, "Only instances have fields.");

            object value = evaluate(expr.value);
            loxInstance.Set(expr.name, value);
            return value;
        }

        public object visitSuperExpr(Expr.Super expr)
        {
            int distance = locals[expr];
            LoxClass superclass = environment.GetAt(distance, "super") as LoxClass;
            LoxInstance obj = environment.GetAt(distance - 1, "this") as LoxInstance;

            LoxFunction method = superclass.findMethod(expr.method.lexeme);
            if (method == null)
                throw new RuntimeError(expr.method, $"Undefined property '{expr.method.lexeme}'.");
            return method.bind(obj);
        }

        public object visitThisExpr(Expr.This expr)
        {
            return lookUpVariable(expr.keyword, expr);
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
            return lookUpVariable(expr.name, expr);
            //return environment.Get(expr.name);
        }

        private object lookUpVariable(Token name, Expr expr)
        {
            if (this.locals.TryGetValue(expr, out int distance))
            {
                return environment.GetAt(distance, name.lexeme);
            }
            else
                return globals.Get(name);
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

        public void resolve(Expr expr, int depth)
        {
            locals.Add(expr, depth);
        }

        public void executeBlock(List<Stmt> statements, Environment environment)
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

        public object visitClassStmt(Stmt.Class stmt)
        {
            LoxClass superclass = null;
            if(stmt.superclass != null)
            {
                superclass = evaluate(stmt.superclass) as LoxClass;
                if (superclass == null)
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
            }

            environment.Define(stmt.name.lexeme, null);

            if(stmt.superclass != null)
            {
                environment = new Environment(environment);
                environment.Define("super", superclass);
            }

            Dictionary<string, LoxFunction> methods = new();
            foreach(var method in stmt.methods)
            {
                LoxFunction function = new(method, environment, method.name.lexeme == "init");
                methods.Add(method.name.lexeme, function);
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, superclass, methods);
            if(superclass != null)
            {
                environment = environment.Enclosing;
            }
            environment.Assign(stmt.name, klass);
            return null;
        }

        public object visitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }


        public object visitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
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


        public object visitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null)
                value = evaluate(stmt.value);
            throw new Return(value);
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

            if (locals.TryGetValue(expr, out int distance))
            {
                environment.AssignAt(distance, expr.name, value);
            }
            else
                globals.Assign(expr.name, value);
            return value;
        }

    }
}
