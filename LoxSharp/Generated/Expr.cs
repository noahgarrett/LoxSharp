namespace LoxSharp;

using LoxSharp.Models;

public abstract class Expr
{
  public interface Visitor<T> {
     T visitAssignExpr(Assign expr);
     T visitBinaryExpr(Binary expr);
     T visitGroupingExpr(Grouping expr);
     T visitLiteralExpr(Literal expr);
     T visitLogicalExpr(Logical expr);
     T visitUnaryExpr(Unary expr);
     T visitVariableExpr(Variable expr);
  }
  public class Assign : Expr {
      public Assign(Token name, Expr value) {
         this.name = name;
         this.value = value;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitAssignExpr(this);
      }

     public readonly Token name;
     public readonly Expr value;
  }
  public class Binary : Expr {
      public Binary(Expr left, Token oper, Expr right) {
         this.left = left;
         this.oper = oper;
         this.right = right;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitBinaryExpr(this);
      }

     public readonly Expr left;
     public readonly Token oper;
     public readonly Expr right;
  }
  public class Grouping : Expr {
      public Grouping(Expr expression) {
         this.expression = expression;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitGroupingExpr(this);
      }

     public readonly Expr expression;
  }
  public class Literal : Expr {
      public Literal(object value) {
         this.value = value;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitLiteralExpr(this);
      }

     public readonly object value;
  }
  public class Logical : Expr {
      public Logical(Expr left, Token oper, Expr right) {
         this.left = left;
         this.oper = oper;
         this.right = right;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitLogicalExpr(this);
      }

     public readonly Expr left;
     public readonly Token oper;
     public readonly Expr right;
  }
  public class Unary : Expr {
      public Unary(Token oper, Expr right) {
         this.oper = oper;
         this.right = right;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitUnaryExpr(this);
      }

     public readonly Token oper;
     public readonly Expr right;
  }
  public class Variable : Expr {
      public Variable(Token name) {
         this.name = name;
  }

      public override T accept<T>(Visitor<T> visitor) {
          return visitor.visitVariableExpr(this);
      }

     public readonly Token name;
  }

  public abstract T accept<T>(Visitor<T> visitor);
}
