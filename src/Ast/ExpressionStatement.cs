namespace Ast;

/// <summary>
/// Инструкция-выражение: вычисляет значение, но не выводит его.
/// </summary>
public sealed class ExpressionStatement : Statement
{
    /// <summary>
    /// Создаёт инструкцию-выражение.
    /// </summary>
    /// <param name="expression">Выражение инструкции.</param>
    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
    }

    /// <summary>
    /// Выражение, которое вычисляется и отбрасывается.
    /// </summary>
    public Expression Expression { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitExpressionStatement(this);
    }
}