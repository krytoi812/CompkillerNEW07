using System.Globalization;

using Ast;

namespace Execution;

/// <summary>
/// Вычислитель AST стековым методом: реализует IAstVisitor,
/// хранит стек значений и делегирует ввод-вывод окружению IEnvironment.
/// </summary>
public sealed class AstEvaluator : IAstVisitor
{
    private readonly Stack<long> _values = new Stack<long>();
    private readonly IEnvironment _environment;

    /// <summary>
    /// Создаёт вычислитель с указанным окружением ввода-вывода.
    /// </summary>
    /// <param name="environment">Окружение ввода-вывода.</param>
    public AstEvaluator(IEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Выполняет программу, обходя её AST.
    /// </summary>
    /// <param name="program">Корневой узел программы.</param>
    public void Evaluate(ProgramNode program)
    {
        program.Accept(this);
    }

    /// <inheritdoc/>
    public void VisitProgram(ProgramNode node)
    {
        foreach (Statement statement in node.Statements)
        {
            statement.Accept(this);
        }
    }

    /// <inheritdoc/>
    public void VisitExpressionStatement(ExpressionStatement node)
    {
        node.Expression.Accept(this);
        _values.Pop();
    }

    /// <inheritdoc/>
    public void VisitPrintStatement(PrintStatement node)
    {
        node.Expression.Accept(this);
        long value = _values.Pop();
        _environment.Output.WriteLine(value);
    }

    /// <inheritdoc/>
    public void VisitIntegerLiteral(IntegerLiteralExpression node)
    {
        _values.Push(node.Value);
    }

    /// <inheritdoc/>
    public void VisitReadInt(ReadIntExpression node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _values.Push(ReadIntegerInput());
    }

    /// <inheritdoc/>
    public void VisitUnary(UnaryExpression node)
    {
        node.Operand.Accept(this);
        long value = _values.Pop();
        _values.Push(CheckedNegate(value));
    }

    /// <inheritdoc/>
    public void VisitBinary(BinaryExpression node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        long right = _values.Pop();
        long left = _values.Pop();
        _values.Push(Compute(node.Operator, left, right));
    }

    /// <summary>
    /// Читает одну строку из ввода и преобразует её в знаковое 64-битное целое.
    /// </summary>
    /// <returns>Прочитанное целое число.</returns>
    /// <exception cref="ExecutionException">Если конец ввода или строка некорректна.</exception>
    private long ReadIntegerInput()
    {
        string? inputText = _environment.Input.ReadLine();
        if (inputText is null)
        {
            throw new ExecutionException("Функция readInt(): достигнут конец входного потока.");
        }

        const NumberStyles styles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign;
        if (!long.TryParse(inputText, styles, CultureInfo.InvariantCulture, out long value))
        {
            throw new ExecutionException(
                $"Функция readInt(): '{inputText}' не является целым числом в диапазоне 64-битного типа.");
        }

        return value;
    }

    /// <summary>
    /// Применяет бинарную операцию к двум значениям.
    /// </summary>
    private static long Compute(BinaryOperator operation, long left, long right)
    {
        return operation switch
        {
            BinaryOperator.Add => CheckedAdd(left, right),
            BinaryOperator.Subtract => CheckedSubtract(left, right),
            BinaryOperator.Multiply => CheckedMultiply(left, right),
            BinaryOperator.Divide => CheckedDivide(left, right),
            BinaryOperator.Remainder => CheckedRemainder(left, right),
            _ => CheckedPower(left, right),
        };
    }

    /// <summary>Сложение с проверкой переполнения.</summary>
    private static long CheckedAdd(long left, long right)
    {
        try
        {
            return checked(left + right);
        }
        catch (OverflowException)
        {
            throw new ExecutionException("Переполнение при сложении.");
        }
    }

    /// <summary>Вычитание с проверкой переполнения.</summary>
    private static long CheckedSubtract(long left, long right)
    {
        try
        {
            return checked(left - right);
        }
        catch (OverflowException)
        {
            throw new ExecutionException("Переполнение при вычитании.");
        }
    }

    /// <summary>Умножение с проверкой переполнения.</summary>
    private static long CheckedMultiply(long left, long right)
    {
        try
        {
            return checked(left * right);
        }
        catch (OverflowException)
        {
            throw new ExecutionException("Переполнение при умножении.");
        }
    }

    /// <summary>Целочисленное деление с проверкой деления на ноль и переполнения.</summary>
    private static long CheckedDivide(long left, long right)
    {
        if (right == 0)
        {
            throw new ExecutionException("Деление на ноль.");
        }

        try
        {
            return checked(left / right);
        }
        catch (OverflowException)
        {
            throw new ExecutionException("Переполнение при делении.");
        }
    }

    /// <summary>Остаток от деления с проверкой деления на ноль.</summary>
    private static long CheckedRemainder(long left, long right)
    {
        if (right == 0)
        {
            throw new ExecutionException("Деление на ноль.");
        }

        try
        {
            return checked(left % right);
        }
        catch (OverflowException)
        {
            throw new ExecutionException("Переполнение при взятии остатка.");
        }
    }

    /// <summary>Изменение знака с проверкой переполнения.</summary>
    private static long CheckedNegate(long value)
    {
        try
        {
            return checked(-value);
        }
        catch (OverflowException)
        {
            throw new ExecutionException("Переполнение при изменении знака.");
        }
    }

    /// <summary>Возведение в неотрицательную целую степень с проверкой переполнения.</summary>
    private static long CheckedPower(long basis, long exponent)
    {
        if (exponent < 0)
        {
            throw new ExecutionException("Отрицательный показатель степени недопустим для целых чисел.");
        }

        if (basis == 0)
        {
            return exponent == 0 ? 1 : 0;
        }

        if (basis == 1)
        {
            return 1;
        }

        if (basis == -1)
        {
            return exponent % 2 == 0 ? 1 : -1;
        }

        if (exponent > 63)
        {
            throw new ExecutionException("Переполнение при возведении в степень.");
        }

        long result = 1;
        for (long step = 0; step < exponent; step += 1)
        {
            result = CheckedMultiply(result, basis);
        }

        return result;
    }
}