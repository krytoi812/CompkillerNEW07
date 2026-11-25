using System.Globalization;

using Ast;

namespace Execution;

/// <summary>
/// Вычислитель AST стековым методом: реализует IAstVisitor,
/// хранит стек значений и делегирует ввод-вывод окружению IEnvironment.
/// </summary>
public sealed class AstEvaluator : IAstVisitor
{
    private const int MaximumCallDepth = 128;

    private readonly Stack<long?> _values = new Stack<long?>();
    private readonly IEnvironment _environment;
    private readonly Dictionary<string, FunctionDeclaration> _functions = new Dictionary<string, FunctionDeclaration>(StringComparer.OrdinalIgnoreCase);
    private VariableScope _scope = new VariableScope(null);
    private int _callDepth;
    private bool _returning;
    private long? _returnValue;
    private FunctionReturnType _returnType;

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
        try
        {
            program.Accept(this);
        }
        finally
        {
            _values.Clear();
        }
    }

    /// <inheritdoc/>
    public void VisitProgram(ProgramNode node)
    {
        _functions.Clear();
        _scope = new VariableScope(null);
        _callDepth = 0;
        _returning = false;
        _returnValue = null;
        _returnType = FunctionReturnType.Void;
        foreach (FunctionDeclaration function in node.Functions)
        {
            function.Accept(this);
        }

        ExecuteStatements(node.Statements);
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
        long value = EvaluateValue(node.Expression);
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
        long value = EvaluateValue(node.Operand);
        _values.Push(CheckedNegate(value));
    }

    /// <inheritdoc/>
    public void VisitBinary(BinaryExpression node)
    {
        long left = EvaluateValue(node.Left);
        long right = EvaluateValue(node.Right);
        _values.Push(Compute(node.Operator, left, right));
    }

    /// <inheritdoc/>
    public void VisitVariableDeclaration(VariableDeclaration node)
    {
        _scope.Declare(node.Name, EvaluateValue(node.Initializer));
    }

    /// <inheritdoc/>
    public void VisitVariable(VariableExpression node)
    {
        _values.Push(_scope.Read(node.Name));
    }

    /// <inheritdoc/>
    public void VisitAssignment(AssignmentExpression node)
    {
        long value = EvaluateValue(node.Value);
        _scope.Assign(node.Name, value);
        _values.Push(value);
    }

    /// <inheritdoc/>
    public void VisitBlock(BlockStatement node)
    {
        VariableScope previousScope = _scope;
        _scope = new VariableScope(previousScope);
        try
        {
            ExecuteStatements(node.Statements);
        }
        finally
        {
            _scope = previousScope;
        }
    }

    /// <inheritdoc/>
    public void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        if (IsBuiltin(node.Name.ToLowerInvariant()))
        {
            throw new ExecutionException($"Имя функции '{node.Name}' зарезервировано.");
        }

        if (!_functions.TryAdd(node.Name, node))
        {
            throw new ExecutionException($"Функция '{node.Name}' уже объявлена.");
        }

        HashSet<string> parameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string parameter in node.Parameters)
        {
            if (!parameters.Add(parameter))
            {
                throw new ExecutionException($"Параметр '{parameter}' указан несколько раз в функции '{node.Name}'.");
            }
        }
    }

    /// <inheritdoc/>
    public void VisitCall(CallExpression node)
    {
        string name = node.Name.ToLowerInvariant();
        _functions.TryGetValue(node.Name, out FunctionDeclaration? function);
        bool validArgumentCount;
        if (function is not null)
        {
            validArgumentCount = node.Arguments.Count == function.Parameters.Count;
        }
        else if (IsBuiltin(name))
        {
            validArgumentCount = name switch
            {
                "readint" => node.Arguments.Count == 0,
                "abs" => node.Arguments.Count == 1,
                _ => node.Arguments.Count > 0,
            };
        }
        else
        {
            throw new ExecutionException($"Неизвестная функция '{node.Name}'.");
        }

        if (!validArgumentCount)
        {
            throw new ExecutionException($"Неверное число аргументов функции '{node.Name}'.");
        }

        long[] arguments = new long[node.Arguments.Count];
        for (int index = 0; index < arguments.Length; index += 1)
        {
            arguments[index] = EvaluateValue(node.Arguments[index]);
        }

        _values.Push(function is null ? CallBuiltin(name, arguments) : CallFunction(function, arguments));
    }

    /// <inheritdoc/>
    public void VisitReturn(ReturnStatement node)
    {
        if (_callDepth == 0)
        {
            throw new ExecutionException("return нельзя использовать вне функции.");
        }

        if (_returnType == FunctionReturnType.Void)
        {
            if (node.Expression is not null)
            {
                throw new ExecutionException("Функция void не возвращает значение.");
            }

            _returnValue = null;
        }
        else
        {
            if (node.Expression is null)
            {
                throw new ExecutionException("Функция int должна вернуть значение.");
            }

            _returnValue = EvaluateValue(node.Expression);
        }

        _returning = true;
    }

    private static bool IsBuiltin(string name)
    {
        return name is "readint" or "abs" or "min" or "max";
    }

    private long CallBuiltin(string name, long[] arguments)
    {
        return name switch
        {
            "readint" => ReadIntegerInput(),
            "abs" => arguments[0] < 0 ? CheckedNegate(arguments[0]) : arguments[0],
            "min" => arguments.Min(),
            "max" => arguments.Max(),
            _ => throw new ExecutionException($"Неизвестная функция '{name}'."),
        };
    }

    private long? CallFunction(FunctionDeclaration function, long[] arguments)
    {
        if (_callDepth >= MaximumCallDepth)
        {
            throw new ExecutionException("Превышена допустимая глубина вызовов функций.");
        }

        VariableScope previousScope = _scope;
        FunctionReturnType previousReturnType = _returnType;
        bool previousReturning = _returning;
        long? previousReturnValue = _returnValue;
        _scope = new VariableScope(null);
        _returnType = function.ReturnType;
        _returning = false;
        _returnValue = null;
        _callDepth += 1;
        try
        {
            for (int index = 0; index < function.Parameters.Count; index += 1)
            {
                _scope.Declare(function.Parameters[index], arguments[index]);
            }

            ExecuteStatements(function.Body.Statements);
            if (function.ReturnType == FunctionReturnType.Int && !_returning)
            {
                throw new ExecutionException($"Функция '{function.Name}' должна вернуть значение.");
            }

            return _returnValue;
        }
        finally
        {
            _scope = previousScope;
            _returnType = previousReturnType;
            _returning = previousReturning;
            _returnValue = previousReturnValue;
            _callDepth -= 1;
        }
    }

    private void ExecuteStatements(IReadOnlyList<Statement> statements)
    {
        foreach (Statement statement in statements)
        {
            statement.Accept(this);
            if (_returning)
            {
                break;
            }
        }
    }

    private long EvaluateValue(Expression expression)
    {
        expression.Accept(this);
        long? value = _values.Pop();
        if (!value.HasValue)
        {
            throw new ExecutionException("Вызов функции без возвращаемого значения нельзя использовать в выражении.");
        }

        return value.Value;
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
            BinaryOperator.Power => CheckedPower(left, right),
            _ => throw new ExecutionException("Неизвестный бинарный оператор."),
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