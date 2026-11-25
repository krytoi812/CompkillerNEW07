namespace Ast;

/// <summary>Определение пользовательской функции.</summary>
public sealed class FunctionDeclaration : Declaration
{
    public FunctionDeclaration(string name, FunctionReturnType returnType, IReadOnlyList<string> parameters, BlockStatement body)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Body = body;
    }

    public string Name { get; }

    public FunctionReturnType ReturnType { get; }

    public IReadOnlyList<string> Parameters { get; }

    public BlockStatement Body { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitFunctionDeclaration(this);
    }
}