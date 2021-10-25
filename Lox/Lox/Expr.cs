using System;
using Lox;
using System.Collections.Generic;
namespace Lox {
public abstract class Expr {
  public interface Visitor<R> {
    R visitAssignExpr(Assign expr);
    R visitBinaryExpr(Binary expr);
    R visitCallExpr(Call expr);
    R visitGetExpr(Get expr);
    R visitGroupingExpr(Grouping expr);
    R visitLiteralExpr(Literal expr);
    R visitLogicalExpr(Logical expr);
    R visitSetExpr(Set expr);
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
  public class Call : Expr {
    public Call(Expr callee, Token paren, List<Expr> arguments) {
      this.callee = callee;
      this.paren = paren;
      this.arguments = arguments;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitCallExpr(this);
    }

   public readonly Expr callee;
   public readonly Token paren;
   public readonly List<Expr> arguments;
  }
  public class Get : Expr {
    public Get(Expr _object, Token name) {
      this._object = _object;
      this.name = name;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitGetExpr(this);
    }

   public readonly Expr _object;
   public readonly Token name;
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
  public class Logical : Expr {
    public Logical(Expr left, Token _operator, Expr right) {
      this.left = left;
      this._operator = _operator;
      this.right = right;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitLogicalExpr(this);
    }

   public readonly Expr left;
   public readonly Token _operator;
   public readonly Expr right;
  }
  public class Set : Expr {
    public Set(Expr _object, Token name, Expr value) {
      this._object = _object;
      this.name = name;
      this.value = value;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitSetExpr(this);
    }

   public readonly Expr _object;
   public readonly Token name;
   public readonly Expr value;
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
