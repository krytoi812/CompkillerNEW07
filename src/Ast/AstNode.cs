namespace Ast;

/// <summary>
/// Базовый класс всех узлов абстрактного синтаксического дерева.
/// </summary>
public abstract class AstNode
{
    /// <summary>
    /// Принимает посетителя для обхода узла.
    /// </summary>
    /// <param name="visitor">Посетитель узлов дерева.</param>
    public abstract void Accept(IAstVisitor visitor);
}