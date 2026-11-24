namespace Ast;

/// <summary>
/// Виды бинарных арифметических операций.
/// </summary>
public enum BinaryOperator
{
    /// <summary>Сложение.</summary>
    Add,

    /// <summary>Вычитание.</summary>
    Subtract,

    /// <summary>Умножение.</summary>
    Multiply,

    /// <summary>Целочисленное деление.</summary>
    Divide,

    /// <summary>Остаток от деления.</summary>
    Remainder,

    /// <summary>Возведение в степень.</summary>
    Power,
}