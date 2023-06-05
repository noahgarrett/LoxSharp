using LoxSharp.Models;
using LoxSharp.Utils;

namespace LoxSharp;

public class Resolver : Expr.Visitor<Unit>, Stmt.Visitor<Unit>
{
    private readonly Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();

    public Resolver(Interpreter _interpreter)
    {
        interpreter = _interpreter;
    }

    #region Expression Nodes
    public Unit visitAssignExpr(Expr.Assign expr)
    {
        resolve(expr.value);
        resolveLocal(expr, expr.name);
        return Unit.Default;
    }

    public Unit visitBinaryExpr(Expr.Binary expr)
    {
        throw new NotImplementedException();
    }

    public Unit visitCallExpr(Expr.Call expr)
    {
        throw new NotImplementedException();
    }

    public Unit visitGroupingExpr(Expr.Grouping expr)
    {
        throw new NotImplementedException();
    }

    public Unit visitLiteralExpr(Expr.Literal expr)
    {
        throw new NotImplementedException();
    }

    public Unit visitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }

    public Unit visitUnaryExpr(Expr.Unary expr)
    {
        throw new NotImplementedException();
    }

    public Unit visitVariableExpr(Expr.Variable expr)
    {
        if (scopes.Count > 0 && scopes.Peek().GetValueOrDefault(expr.name.lexeme) == false)
        {
            Program.Error(expr.name, "Can't read local variable in its own initializer.");
        }

        resolveLocal(expr, expr.name);
        return Unit.Default;
    }
    #endregion

    #region Statement Nodes
    public Unit visitBlockStmt(Stmt.Block stmt)
    {
        beginScope();
        resolve(stmt.statements);
        endScope();

        return Unit.Default;
    }

    public Unit visitExpressionStmt(Stmt.Expression stmt)
    {
        resolve(stmt.expression); 
        return Unit.Default;
    }

    public Unit visitFunctionStmt(Stmt.Function stmt)
    {
        declare(stmt.name);
        define(stmt.name);

        resolveFunction(stmt);

        return Unit.Default;
    }

    public Unit visitIfStmt(Stmt.If stmt)
    {
        resolve(stmt.condition);
        resolve(stmt.thenBranch);
        if (stmt.elseBranch != null) resolve(stmt.elseBranch);

        return Unit.Default;
    }

    public Unit visitPrintStmt(Stmt.Print stmt)
    {
        resolve(stmt.expression);
        return Unit.Default;
    }

    public Unit visitReturnStmt(Stmt.Return stmt)
    {
        if (stmt.value != null) resolve(stmt.value);

        return Unit.Default;
    }

    public Unit visitVarStmt(Stmt.Var stmt)
    {
        declare(stmt.name);

        if (stmt.intitializer != null)
            resolve(stmt.intitializer);

        define(stmt.name);

        return Unit.Default;
    }

    public Unit visitWhileStmt(Stmt.While stmt)
    {
        resolve(stmt.condition);
        resolve(stmt.body);

        return Unit.Default;
    }
    #endregion

    #region Helpers
    public void resolve(List<Stmt> statements)
    {
        foreach (Stmt stmt in statements)
        {
            resolve(stmt);
        }
    }

    private void resolve(Stmt stmt)
    {
        stmt.accept(this);
    }

    private void resolve(Expr expr)
    {
        expr.accept(this);
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
        if (scopes.Count == 0) return;

        Dictionary<string, bool> scope = scopes.Peek();
        scope.Add(name.lexeme, false);
    }

    private void define(Token name)
    {
        if (scopes.Count == 0) return;

        scopes.Peek().Add(name.lexeme, true);
    }

    private void resolveLocal(Expr expr, Token name)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.lexeme))
            {
                interpreter.resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void resolveFunction(Stmt.Function function)
    {
        beginScope();

        foreach (Token param in function.parameters)
        {
            declare(param);
            define(param);
        }

        resolve(function.body);
        endScope();
    }
    #endregion
}
