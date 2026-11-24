namespace Ast;

/// <summary>
/// Узел программы: точка входа main и список инструкций.
/// </summary>
public sealed class ProgramNode : AstNode
{
    /// <summary>
    /// Создаёт узел программы.
    /// </summary>
    /// <param name="statements">Инструкции блока main.</param>
    public ProgramNode(IReadOnlyList<Statement> statements)
    {
        Statements = statements;
    }

    /// <summary>
    /// Инструкции программы в порядке следования.
    /// </summary>
    public IReadOnlyList<Statement> Statements { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitProgram(this);
    }
}