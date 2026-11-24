namespace Ast;

/// <summary>
/// Узел унарной операции.
/// </summary>
public sealed class UnaryExpression : Expression
{
    /// <summary>
    /// Создаёт узел унарной операции.
    /// </summary>
    /// <param name="operatorKind">Вид унарной операции.</param>
    /// <param name="operand">Операнд.</param>
    public UnaryExpression(UnaryOperator operatorKind, Expression operand)
    {
        Operator = operatorKind;
        Operand = operand;
    }

    /// <summary>
    /// Вид унарной операции.
    /// </summary>
    public UnaryOperator Operator { get; }

    /// <summary>
    /// Операнд унарной операции.
    /// </summary>
    public Expression Operand { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitUnary(this);
    }
}