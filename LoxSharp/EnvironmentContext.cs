﻿using LoxSharp.Exceptions;
using LoxSharp.Models;

namespace LoxSharp;

public class EnvironmentContext
{
    public readonly EnvironmentContext? enclosing;
    private readonly Dictionary<string, object> values = new();

    public EnvironmentContext()
    {
        enclosing = null;
    }

    public EnvironmentContext(EnvironmentContext _enclosing)
    {
        enclosing = _enclosing;
    }

    public void define(string name, object value)
    {
        values[name] = value;
    }

    public object get(Token name)
    {
        if (values.ContainsKey(name.lexeme))
        {
            return values[name.lexeme];
        }

        if (enclosing != null) return enclosing.get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void assign(Token name, object value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
}
