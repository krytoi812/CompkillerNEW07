namespace Ast;

/// <summary>
/// Интерфейс посетителя узлов абстрактного синтаксического дерева.
/// </summary>
public interface IAstVisitor
{
    /// <summary>Посещает узел программы.</summary>
    /// <param name="node">Узел программы.</param>
    void VisitProgram(ProgramNode node);

    /// <summary>Посещает инструкцию-выражение.</summary>
    /// <param name="node">Узел инструкции-выражения.</param>
    void VisitExpressionStatement(ExpressionStatement node);

    /// <summary>Посещает инструкцию вывода.</summary>
    /// <param name="node">Узел инструкции вывода.</param>
    void VisitPrintStatement(PrintStatement node);

    /// <summary>Посещает целочисленный литерал.</summary>
    /// <param name="node">Узел литерала.</param>
    void VisitIntegerLiteral(IntegerLiteralExpression node);

    /// <summary>Посещает вызов readInt().</summary>
    /// <param name="node">Узел вызова readInt.</param>
    void VisitReadInt(ReadIntExpression node);

    /// <summary>Посещает унарную операцию.</summary>
    /// <param name="node">Узел унарной операции.</param>
    void VisitUnary(UnaryExpression node);

    /// <summary>Посещает бинарную операцию.</summary>
    /// <param name="node">Узел бинарной операции.</param>
    void VisitBinary(BinaryExpression node);

    /// <summary>Посещает объявление переменной.</summary>
    void VisitVariableDeclaration(VariableDeclaration node);

    /// <summary>Посещает чтение переменной.</summary>
    void VisitVariable(VariableExpression node);

    /// <summary>Посещает присваивание.</summary>
    void VisitAssignment(AssignmentExpression node);

    /// <summary>Посещает блок инструкций.</summary>
    void VisitBlock(BlockStatement node);

    /// <summary>Посещает определение функции.</summary>
    void VisitFunctionDeclaration(FunctionDeclaration node);

    /// <summary>Посещает вызов функции.</summary>
    void VisitCall(CallExpression node);

    /// <summary>Посещает возврат из функции.</summary>
    void VisitReturn(ReturnStatement node);
}