using System;
using Lox;
namespace Lox
{
    public abstract class Expr
    {
        public interface Visitor<R>
        {
            R visitBinaryExpr(Binary expr);
            R visitGroupingExpr(Grouping expr);
            R visitLiteralExpr(Literal expr);
            R visitUnaryExpr(Unary expr);
        }
        public class Binary : Expr
        {
            public Binary(Expr left, Token _operator, Expr right)
            {
                this.left = left;
                this._operator = _operator;
                this.right = right;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitBinaryExpr(this);
            }

            public readonly Expr left;
            public readonly Token _operator;
            public readonly Expr right;
        }
        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                this.expression = expression;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitGroupingExpr(this);
            }

            public readonly Expr expression;
        }
        public class Literal : Expr
        {
            public Literal(Object value)
            {
                this.value = value;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitLiteralExpr(this);
            }

            public readonly Object value;
        }
        public class Unary : Expr
        {
            public Unary(Token _operator, Expr right)
            {
                this._operator = _operator;
                this.right = right;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitUnaryExpr(this);
            }

            public readonly Token _operator;
            public readonly Expr right;
        }

        public abstract R accept<R>(Visitor<R> visitor);
    }
}
