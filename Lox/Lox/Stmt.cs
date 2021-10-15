using System;
using Lox;
using System.Collections.Generic;
namespace Lox {
public abstract class Stmt {
  public interface Visitor<R> {
    R visitBlockStmt(Block stmt);
    R visitExpressionStmt(Expression stmt);
    R visitIfStmt(If stmt);
    R visitPrintStmt(Print stmt);
    R visitVarStmt(Var stmt);
    R visitWhileStmt(While stmt);
  }
  public class Block : Stmt {
    public Block(List<Stmt> statements) {
      this.statements = statements;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitBlockStmt(this);
    }

   public readonly List<Stmt> statements;
  }
  public class Expression : Stmt {
    public Expression(Expr expression) {
      this.expression = expression;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitExpressionStmt(this);
    }

   public readonly Expr expression;
  }
  public class If : Stmt {
    public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
      this.condition = condition;
      this.thenBranch = thenBranch;
      this.elseBranch = elseBranch;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitIfStmt(this);
    }

   public readonly Expr condition;
   public readonly Stmt thenBranch;
   public readonly Stmt elseBranch;
  }
  public class Print : Stmt {
    public Print(Expr expression) {
      this.expression = expression;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitPrintStmt(this);
    }

   public readonly Expr expression;
  }
  public class Var : Stmt {
    public Var(Token name, Expr initializer) {
      this.name = name;
      this.initializer = initializer;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitVarStmt(this);
    }

   public readonly Token name;
   public readonly Expr initializer;
  }
  public class While : Stmt {
    public While(Expr condition, Stmt body) {
      this.condition = condition;
      this.body = body;
    }

    public override R accept<R>(Visitor<R> visitor) {
      return visitor.visitWhileStmt(this);
    }

   public readonly Expr condition;
   public readonly Stmt body;
  }

  public abstract R accept<R>(Visitor<R> visitor);
}
}
