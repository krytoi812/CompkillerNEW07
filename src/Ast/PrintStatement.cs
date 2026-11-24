namespace Ast;

/// <summary>
/// Инструкция вывода print: печатает значение выражения.
/// </summary>
public sealed class PrintStatement : Statement
{
    /// <summary>
    /// Создаёт инструкцию вывода.
    /// </summary>
    /// <param name="expression">Выражение, значение которого печатается.</param>
    public PrintStatement(Expression expression)
    {
        Expression = expression;
    }

    /// <summary>
    /// Выражение, значение которого выводится.
    /// </summary>
    public Expression Expression { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitPrintStatement(this);
    }
}