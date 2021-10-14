using System;
using Lox;
using System.Collections.Generic;
namespace Lox {
public abstract class Expr {
  public interface Visitor<R> {
    R visitAssignExpr(Assign expr);
    R visitBinaryExpr(Binary expr);
    R visitGroupingExpr(Grouping expr);
    R visitLiteralExpr(Literal expr);
    R visitUnaryExpr(Unary expr);
    R visitVariableExpr(Variable expr);
  }
  public class Assign : Expr {
    public Assign(Token name, Expr value) {
      this.name = name;
      this.value = value;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitAssignExpr(this);
    }

   public readonly Token name;
   public readonly Expr value;
  }
  public class Binary : Expr {
    public Binary(Expr left, Token _operator, Expr right) {
      this.left = left;
      this._operator = _operator;
      this.right = right;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitBinaryExpr(this);
    }

   public readonly Expr left;
   public readonly Token _operator;
   public readonly Expr right;
  }
  public class Grouping : Expr {
    public Grouping(Expr expression) {
      this.expression = expression;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitGroupingExpr(this);
    }

   public readonly Expr expression;
  }
  public class Literal : Expr {
    public Literal(Object value) {
      this.value = value;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitLiteralExpr(this);
    }

   public readonly Object value;
  }
  public class Unary : Expr {
    public Unary(Token _operator, Expr right) {
      this._operator = _operator;
      this.right = right;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitUnaryExpr(this);
    }

   public readonly Token _operator;
   public readonly Expr right;
  }
  public class Variable : Expr {
    public Variable(Token name) {
      this.name = name;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitVariableExpr(this);
    }

   public readonly Token name;
  }

  public abstract R accept<R>(Visitor<R> visitor);
}
}
