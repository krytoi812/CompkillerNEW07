namespace Ast;

/// <summary>
/// Узел целочисленного литерала.
/// </summary>
public sealed class IntegerLiteralExpression : Expression
{
    /// <summary>
    /// Создаёт узел литерала.
    /// </summary>
    /// <param name="value">Значение литерала.</param>
    public IntegerLiteralExpression(long value)
    {
        Value = value;
    }

    /// <summary>
    /// Значение целочисленного литерала.
    /// </summary>
    public long Value { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitIntegerLiteral(this);
    }
}