namespace Ast;

/// <summary>Блок с отдельной областью видимости.</summary>
public sealed class BlockStatement : Statement
{
    public BlockStatement(IReadOnlyList<Statement> statements)
    {
        Statements = statements;
    }

    public IReadOnlyList<Statement> Statements { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitBlock(this);
    }
}