using LoxSharp.Enums;
using LoxSharp.Exceptions;
using LoxSharp.Models;

namespace LoxSharp;

public class Parser
{

    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> _tokens)
    {
        tokens = _tokens;
    }

    public List<Stmt> parse()
    {
        List<Stmt> statements = new();

        while (!isAtEnd())
        {
            statements.Add(declaration());
        }

        return statements;
    }

    #region Statements
    private Stmt declaration()
    {
        try
        {
            if (match(TokenType.Var)) return varDeclaration();

            return statement();
        }
        catch (ParseError error)
        {
            synchronize();
            return null;
        }
    }

    private Stmt varDeclaration()
    {
        Token name = consume(TokenType.Identifier, "Expect variable name.");

        Expr? initializer = null;
        if (match(TokenType.Equal))
        {
            initializer = expression();
        }

        consume(TokenType.SemiColon, "Expect ';' after variable declaration");
        return new Stmt.Var(name, initializer);
    }

    private Stmt statement()
    {
        if (match(TokenType.If)) return ifStatement();
        if (match(TokenType.Print)) return printStatment();
        if (match(TokenType.LeftBrace)) return new Stmt.Block(block());

        return expressionStatement();
    }

    private Stmt ifStatement()
    {
        consume(TokenType.LeftParen, "Expect '(' after 'if'.");

        Expr condition = expression();

        consume(TokenType.RightParen, "Expect ')' after if condition.");

        Stmt thenBranch = statement();
        Stmt? elseBranch = null;
        
        if (match(TokenType.Else))
        {
            elseBranch = statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt printStatment()
    {
        Expr value = expression();
        consume(TokenType.SemiColon, "Expect ';' after value");
        return new Stmt.Print(value);
    }

    private Stmt expressionStatement()
    {
        Expr expr = expression();
        consume(TokenType.SemiColon, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private List<Stmt> block()
    {
        List<Stmt> statements = new();

        while (!check(TokenType.RightBrace) && !isAtEnd())
        {
            statements.Add(declaration());
        }

        consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }
    #endregion

    #region Expressions
    private Expr expression()
    {
        return assignment();
    }

    private Expr assignment()
    {
        Expr expr = or();

        if (match(TokenType.Equal))
        {
            Token equals = previous();
            Expr value = assignment();

            if (expr is Expr.Variable)
            {
                Token name = ((Expr.Variable)expr).name;
                return new Expr.Assign(name, value);
            }

            error(equals, "Invalid assignment target");
        }

        return expr;
    }

    private Expr or()
    {
        Expr expr = and();

        while (match(TokenType.Or))
        {
            Token oper = previous();
            Expr right = and();
            
            expr = new Expr.Logical(expr, oper, right);
        }

        return expr;
    }

    private Expr and()
    {
        Expr expr = equality();

        while (match(TokenType.And))
        {
            Token oper = previous();
            Expr right = equality();

            expr = new Expr.Logical(expr, oper, right);
        }

        return expr;
    }

    private Expr equality()
    {
        Expr expr = comparison();

        while (match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token oper = previous();
            Expr right = comparison();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr comparison()
    {
        Expr expr = term();

        while (match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token oper = previous();
            Expr right = term();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr term()
    {
        Expr expr = factor();

        while (match(TokenType.Minus, TokenType.Plus))
        {
            Token oper = previous();
            Expr right = factor();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr factor()
    {
        Expr expr = unary();

        while (match(TokenType.Slash, TokenType.Star))
        {
            Token oper = previous();
            Expr right = unary();

            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr unary()
    {
        if (match(TokenType.Bang, TokenType.Minus))
        {
            Token oper = previous();
            Expr right = unary();

            return new Expr.Unary(oper, right);
        }

        return primary();
    }

    private Expr primary()
    {
        if (match(TokenType.False)) return new Expr.Literal(false);
        if (match(TokenType.True)) return new Expr.Literal(true);
        if (match(TokenType.Nil)) return new Expr.Literal(null);
        if (match(TokenType.Number, TokenType.String)) return new Expr.Literal(previous().literal);

        if (match(TokenType.Identifier)) return new Expr.Variable(previous());

        if (match(TokenType.LeftParen))
        {
            Expr expr = expression();
            consume(TokenType.RightParen, "Expect ')' after expression");
            return new Expr.Grouping(expr);
        }

        throw error(peek(), "Expect expression.");
    }
    #endregion

    #region Helper Methods
    private bool match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (check(type))
            {
                advance();
                return true;
            }
        }

        return false;
    }

    private bool check(TokenType type)
    {
        if (isAtEnd()) return false;
        return peek().type == type;
    }

    private Token advance()
    {
        if (!isAtEnd()) current++;
        return previous();
    }

    private bool isAtEnd()
    {
        return peek().type == TokenType.EOF;
    }

    private Token peek()
    {
        return tokens[current];
    }

    private Token previous()
    {
        return tokens[current - 1];
    }

    private Token consume(TokenType type, string message)
    {
        if (check(type)) return advance();

        throw error(peek(), message);
    }

    private ParseError error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseError();
    }

    private void synchronize()
    {
        advance();

        while (!isAtEnd())
        {
            if (previous().type == TokenType.SemiColon) return;

            switch (peek().type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            advance();
        }
    }
    #endregion
}
