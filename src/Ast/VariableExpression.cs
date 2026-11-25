namespace Ast;

/// <summary>Чтение значения переменной.</summary>
public sealed class VariableExpression : Expression
{
    public VariableExpression(string name)
    {
        Name = name;
    }

    public string Name { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitVariable(this);
    }
}