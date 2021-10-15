using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lox.Expr;

namespace Lox
{
    public class AstPrinter : Visitor<string>
    {
        public string print(Expr expr)
        {
            return expr.accept(this);
        }

        public string visitAssignExpr(Assign expr)
        {
            throw new NotImplementedException();
        }

        public string visitBinaryExpr(Binary expr)
        {
            return parenthesize(expr._operator.lexeme,
                        expr.left, expr.right);
        }

        public string visitGroupingExpr(Grouping expr)
        {
            return parenthesize("group", expr.expression);
        }

        public string visitLiteralExpr(Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string visitLogicalExpr(Logical expr)
        {
            throw new NotImplementedException();
        }

        public string visitUnaryExpr(Unary expr)
        {
            return parenthesize(expr._operator.lexeme, expr.right);
        }

        public string visitVariableExpr(Variable expr)
        {
            throw new NotImplementedException();
        }

        private String parenthesize(String name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

    }
}
