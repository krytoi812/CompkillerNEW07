namespace Ast;

/// <summary>Присваивание переменной, возвращающее записанное значение.</summary>
public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public Expression Value { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitAssignment(this);
    }
}