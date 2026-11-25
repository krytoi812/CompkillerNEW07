namespace Ast;

/// <summary>Вызов функции с аргументами.</summary>
public sealed class CallExpression : Expression
{
    public CallExpression(string name, IReadOnlyList<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public IReadOnlyList<Expression> Arguments { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitCall(this);
    }
}