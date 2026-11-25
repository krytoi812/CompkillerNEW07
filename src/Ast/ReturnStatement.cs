namespace Ast;

/// <summary>Возврат из функции с необязательным значением.</summary>
public sealed class ReturnStatement : Statement
{
    public ReturnStatement(Expression? expression)
    {
        Expression = expression;
    }

    public Expression? Expression { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitReturn(this);
    }
}