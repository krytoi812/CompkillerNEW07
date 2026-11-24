namespace Ast;

/// <summary>
/// Узел вызова встроенной функции readInt().
/// </summary>
public sealed class ReadIntExpression : Expression
{
    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitReadInt(this);
    }
}