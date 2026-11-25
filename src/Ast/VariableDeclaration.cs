namespace Ast;

/// <summary>Объявление целочисленной переменной с начальным значением.</summary>
public sealed class VariableDeclaration : Declaration
{
    public VariableDeclaration(string name, Expression initializer)
    {
        Name = name;
        Initializer = initializer;
    }

    public string Name { get; }

    public Expression Initializer { get; }

    /// <inheritdoc/>
    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitVariableDeclaration(this);
    }
}