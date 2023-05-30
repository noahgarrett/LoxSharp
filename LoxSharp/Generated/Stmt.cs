﻿namespace LoxSharp;

using LoxSharp.Models;

public abstract class Stmt
{
  public interface Visitor<T> {
     T visitBlockStmt(Block stmt);
     T visitExpressionStmt(Expression stmt);
     T visitIfStmt(If stmt);
     T visitPrintStmt(Print stmt);
     T visitVarStmt(Var stmt);
  }
  public class Block : Stmt {
      public Block(List<Stmt> statements) {
         this.statements = statements;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitBlockStmt(this);
      }

     public readonly List<Stmt> statements;
  }
  public class Expression : Stmt {
      public Expression(Expr expression) {
         this.expression = expression;
  }

      public override T accept<T>(Visitor<T> visitor) {
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

      public override T accept<T>(Visitor<T> visitor) {
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

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitPrintStmt(this);
      }

     public readonly Expr expression;
  }
  public class Var : Stmt {
      public Var(Token name, Expr intitializer) {
         this.name = name;
         this.intitializer = intitializer;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitVarStmt(this);
      }

     public readonly Token name;
     public readonly Expr intitializer;
  }

  public abstract T accept<T>(Visitor<T> visitor);
}