using System.Globalization;

using Lexemes;

namespace Parsing;

/// <summary>
/// Синтаксический анализатор языка CompkillerNEW07 на основе рекурсивного спуска.
/// Интерпретация выполняется непосредственно во время разбора, без построения AST.
/// </summary>
public sealed class Parser
{
    private readonly TokenStream _tokens;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    /// <summary>
    /// Создаёт анализатор, использующий стандартный поток ввода и указанный поток вывода.
    /// </summary>
    /// <param name="tokens">Список лексем программы.</param>
    /// <param name="output">Поток вывода инструкции print.</param>
    public Parser(IReadOnlyList<Token> tokens, TextWriter output)
        : this(tokens, Console.In, output)
    {
    }

    /// <summary>
    /// Создаёт анализатор с явно заданными потоками ввода и вывода.
    /// </summary>
    /// <param name="tokens">Список лексем программы.</param>
    /// <param name="input">Поток ввода для встроенной функции readInt.</param>
    /// <param name="output">Поток вывода инструкции print.</param>
    public Parser(IReadOnlyList<Token> tokens, TextReader input, TextWriter output)
    {
        _tokens = new TokenStream(tokens);
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Текущая лексема потока разбора.
    /// </summary>
    private Token Current => _tokens.Current;

    /// <summary>
    /// Разбирает программу целиком: точку входа main, блок инструкций и конец файла.
    /// </summary>
    /// <exception cref="ParserException">Если программа не соответствует грамматике.</exception>
    public void ParseProgram()
    {
        Expect(TokenType.KeywordMain);
        Expect(TokenType.LeftBrace);
        while (Current.Type != TokenType.RightBrace)
        {
            if (Current.Type == TokenType.EndOfFile)
            {
                throw new ParserException(
                    $"Ожидалась закрывающая фигурная скобка '}}' на строке {Current.Line}, колонке {Current.Column}.");
            }

            ParseStatement();
        }

        Expect(TokenType.RightBrace);
        Expect(TokenType.EndOfFile);
    }

    /// <summary>
    /// Разбирает инструкцию: выражение либо вывод результата через print.
    /// </summary>
    private void ParseStatement()
    {
        if (Current.Type == TokenType.KeywordPrint)
        {
            Advance();
            long value = ParseExpression();
            Expect(TokenType.Semicolon);
            _output.WriteLine(value);
            return;
        }

        ParseExpression();
        Expect(TokenType.Semicolon);
    }

    /// <summary>
    /// Разбирает выражение.
    /// </summary>
    /// <returns>Вычисленное значение выражения.</returns>
    private long ParseExpression()
    {
        return ParseAdditive();
    }

    /// <summary>
    /// Разбирает сложение и вычитание с левой ассоциативностью.
    /// </summary>
    /// <returns>Вычисленное значение.</returns>
    private long ParseAdditive()
    {
        long result = ParseMultiplicative();
        while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
        {
            TokenType operation = Advance().Type;
            long right = ParseMultiplicative();
            result = operation == TokenType.Plus
                ? CheckedAdd(result, right)
                : CheckedSubtract(result, right);
        }

        return result;
    }

    /// <summary>
    /// Разбирает умножение, деление и остаток с левой ассоциативностью.
    /// </summary>
    /// <returns>Вычисленное значение.</returns>
    private long ParseMultiplicative()
    {
        long result = ParseUnary();
        while (Current.Type == TokenType.Star || Current.Type == TokenType.Slash || Current.Type == TokenType.Percent)
        {
            TokenType operation = Advance().Type;
            long right = ParseUnary();
            result = operation switch
            {
                TokenType.Star => CheckedMultiply(result, right),
                TokenType.Slash => CheckedDivide(result, right),
                _ => CheckedRemainder(result, right),
            };
        }

        return result;
    }

    /// <summary>
    /// Разбирает необязательный унарный минус.
    /// </summary>
    /// <returns>Вычисленное значение.</returns>
    private long ParseUnary()
    {
        if (Current.Type == TokenType.Minus)
        {
            Advance();
            return CheckedNegate(ParsePower());
        }

        return ParsePower();
    }

    /// <summary>
    /// Разбирает возведение в степень с правой ассоциативностью.
    /// Показатель разбирается как унарное выражение, поэтому отрицательный показатель
    /// доходит до проверки в CheckedPower и вызывает ошибку выполнения.
    /// </summary>
    /// <returns>Вычисленное значение.</returns>
    private long ParsePower()
    {
        long basis = ParsePrimary();
        if (Current.Type == TokenType.Caret)
        {
            Advance();
            long exponent = ParseUnary();
            return CheckedPower(basis, exponent);
        }

        return basis;
    }

    /// <summary>
    /// Разбирает целочисленный литерал, вызов readInt() или выражение в скобках.
    /// </summary>
    /// <returns>Вычисленное значение.</returns>
    /// <exception cref="ParserException">Если встречена неожиданная лексема.</exception>
    private long ParsePrimary()
    {
        if (Current.Type == TokenType.IntegerLiteral)
        {
            string text = Advance().Value;
            return ParseIntegerValue(text);
        }

        if (Current.Type == TokenType.KeywordReadInt)
        {
            Advance();
            Expect(TokenType.LeftParenthesis);
            Expect(TokenType.RightParenthesis);
            return ReadIntegerInput();
        }

        if (Current.Type == TokenType.LeftParenthesis)
        {
            Advance();
            long result = ParseExpression();
            Expect(TokenType.RightParenthesis);
            return result;
        }

        throw new ParserException(
            $"Неожиданная лексема '{Current.Value}' на строке {Current.Line}, колонке {Current.Column}.");
    }

    /// <summary>
    /// Читает одну строку из входного потока и преобразует её в знаковое 64-битное целое число.
    /// </summary>
    /// <returns>Прочитанное целое число.</returns>
    /// <exception cref="ParserException">Если достигнут конец ввода или строка некорректна.</exception>
    private long ReadIntegerInput()
    {
        string? inputText = _input.ReadLine();
        if (inputText is null)
        {
            throw new ParserException("Функция readInt(): достигнут конец входного потока.");
        }

        const NumberStyles styles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign;
        if (!long.TryParse(inputText, styles, CultureInfo.InvariantCulture, out long value))
        {
            throw new ParserException(
                $"Функция readInt(): '{inputText}' не является целым числом в диапазоне 64-битного типа.");
        }

        return value;
    }

    /// <summary>
    /// Сдвигает позицию разбора на одну лексему вперёд.
    /// </summary>
    /// <returns>Лексема, которая была текущей.</returns>
    private Token Advance()
    {
        return _tokens.Advance();
    }

    /// <summary>
    /// Проверяет вид текущей лексемы и сдвигает позицию разбора.
    /// </summary>
    /// <param name="type">Ожидаемый вид лексемы.</param>
    /// <returns>Проверенная лексема.</returns>
    /// <exception cref="ParserException">Если текущая лексема не совпадает с ожидаемой.</exception>
    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
        {
            throw new ParserException(
                $"Ожидалась лексема {type}, но встречена '{Current.Value}' на строке {Current.Line}, колонке {Current.Column}.");
        }

        return Advance();
    }

    /// <summary>
    /// Преобразует текст целочисленного литерала в значение с проверкой переполнения.
    /// </summary>
    /// <param name="text">Текст литерала.</param>
    /// <returns>Значение литерала.</returns>
    /// <exception cref="ParserException">Если литерал не помещается в диапазон long.</exception>
    private static long ParseIntegerValue(string text)
    {
        if (!long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out long value))
        {
            throw new ParserException($"Целочисленный литерал '{text}' слишком велик.");
        }

        return value;
    }

    /// <summary>
    /// Сложение с проверкой переполнения.
    /// </summary>
    private static long CheckedAdd(long left, long right)
    {
        try
        {
            return checked(left + right);
        }
        catch (OverflowException)
        {
            throw new ParserException("Переполнение при сложении.");
        }
    }

    /// <summary>
    /// Вычитание с проверкой переполнения.
    /// </summary>
    private static long CheckedSubtract(long left, long right)
    {
        try
        {
            return checked(left - right);
        }
        catch (OverflowException)
        {
            throw new ParserException("Переполнение при вычитании.");
        }
    }

    /// <summary>
    /// Умножение с проверкой переполнения.
    /// </summary>
    private static long CheckedMultiply(long left, long right)
    {
        try
        {
            return checked(left * right);
        }
        catch (OverflowException)
        {
            throw new ParserException("Переполнение при умножении.");
        }
    }

    /// <summary>
    /// Целочисленное деление с проверкой деления на ноль и переполнения.
    /// </summary>
    private static long CheckedDivide(long left, long right)
    {
        if (right == 0)
        {
            throw new ParserException("Деление на ноль.");
        }

        try
        {
            return checked(left / right);
        }
        catch (OverflowException)
        {
            throw new ParserException("Переполнение при делении.");
        }
    }

    /// <summary>
    /// Остаток от деления с проверкой деления на ноль.
    /// </summary>
    private static long CheckedRemainder(long left, long right)
    {
        if (right == 0)
        {
            throw new ParserException("Деление на ноль.");
        }

        try
        {
            return checked(left % right);
        }
        catch (OverflowException)
        {
            throw new ParserException("Переполнение при взятии остатка.");
        }
    }

    /// <summary>
    /// Изменение знака с проверкой переполнения.
    /// </summary>
    private static long CheckedNegate(long value)
    {
        try
        {
            return checked(-value);
        }
        catch (OverflowException)
        {
            throw new ParserException("Переполнение при изменении знака.");
        }
    }

    /// <summary>
    /// Возведение в неотрицательную целую степень с проверкой переполнения.
    /// </summary>
    /// <exception cref="ParserException">Если показатель отрицателен или результат переполняется.</exception>
    private static long CheckedPower(long basis, long exponent)
    {
        if (exponent < 0)
        {
            throw new ParserException("Отрицательный показатель степени недопустим для целых чисел.");
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
            throw new ParserException("Переполнение при возведении в степень.");
        }

        long result = 1;
        for (long step = 0; step < exponent; step += 1)
        {
            result = CheckedMultiply(result, basis);
        }

        return result;
    }
}