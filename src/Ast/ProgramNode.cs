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
        : this(Array.Empty<FunctionDeclaration>(), statements)
    {
    }

    /// <summary>Создаёт программу с функциями и блоком main.</summary>
    /// <param name="functions">Определения функций.</param>
    /// <param name="statements">Инструкции main.</param>
    public ProgramNode(IReadOnlyList<FunctionDeclaration> functions, IReadOnlyList<Statement> statements)
    {
        Functions = functions;
        Statements = statements;
    }

    /// <summary>Определения функций программы.</summary>
    public IReadOnlyList<FunctionDeclaration> Functions { get; }

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