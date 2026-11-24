namespace Ast;

/// <summary>
/// Узел бинарной арифметической операции.
/// </summary>
public sealed class BinaryExpression : Expression
{
    /// <summary>
    /// Создаёт узел бинарной операции.
    /// </summary>
    /// <param name="operatorKind">Вид бинарной операции.</param>
    /// <param name="left">Левый операнд.</param>
    /// <param name="right">Правый операнд.</param>
    public BinaryExpression(BinaryOperator operatorKind, Expression left, Expression right)
    {
        Operator = operatorKind;
        Left = left;
        Right = right;
    }

    /// <summary>
    /// Вид бинарной операции.
    /// </summary>
    public BinaryOperator Operator { get; }

    /// <summary>
    /// Левый операнд.
    /// </summary>
    public Expression Left { get; }

    /// <summary>
    /// Правый операнд.
    /// </summary>
    public Expression Right { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitBinary(this);
    }
}