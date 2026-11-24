using Ast;
using Lexemes;

namespace Parsing;

/// <summary>
/// Синтаксический анализатор языка CompkillerNEW07 на основе рекурсивного спуска.
/// Строит абстрактное синтаксическое дерево программы; вычисление выполняется отдельно.
/// </summary>
public sealed class Parser
{
    private readonly TokenStream _tokens;

    /// <summary>
    /// Создаёт анализатор для списка лексем программы.
    /// </summary>
    /// <param name="tokens">Список лексем программы.</param>
    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = new TokenStream(tokens);
    }

    /// <summary>
    /// Текущая лексема потока разбора.
    /// </summary>
    private Token Current => _tokens.Current;

    /// <summary>
    /// Разбирает программу целиком и возвращает корневой узел AST.
    /// </summary>
    /// <returns>Корневой узел программы.</returns>
    /// <exception cref="ParserException">Если программа не соответствует грамматике.</exception>
    public ProgramNode ParseProgram()
    {
        Expect(TokenType.KeywordMain);
        Expect(TokenType.LeftBrace);
        List<Statement> statements = new List<Statement>();
        while (Current.Type != TokenType.RightBrace)
        {
            if (Current.Type == TokenType.EndOfFile)
            {
                throw new ParserException(
                    $"Ожидалась закрывающая фигурная скобка '}}' на строке {Current.Line}, колонке {Current.Column}.");
            }

            statements.Add(ParseStatement());
        }

        Expect(TokenType.RightBrace);
        Expect(TokenType.EndOfFile);
        return new ProgramNode(statements);
    }

    /// <summary>
    /// Разбирает инструкцию: выражение либо вывод результата через print.
    /// </summary>
    /// <returns>Узел инструкции.</returns>
    private Statement ParseStatement()
    {
        if (Current.Type == TokenType.KeywordPrint)
        {
            Advance();
            Expression expression = ParseExpression();
            Expect(TokenType.Semicolon);
            return new PrintStatement(expression);
        }

        Expression value = ParseExpression();
        Expect(TokenType.Semicolon);
        return new ExpressionStatement(value);
    }

    /// <summary>
    /// Разбирает выражение.
    /// </summary>
    /// <returns>Узел выражения.</returns>
    private Expression ParseExpression()
    {
        return ParseAdditive();
    }

    /// <summary>
    /// Разбирает сложение и вычитание с левой ассоциативностью.
    /// </summary>
    /// <returns>Узел выражения.</returns>
    private Expression ParseAdditive()
    {
        Expression left = ParseMultiplicative();
        while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
        {
            BinaryOperator operation = ToBinaryOperator(Advance().Type);
            Expression right = ParseMultiplicative();
            left = new BinaryExpression(operation, left, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает умножение, деление и остаток с левой ассоциативностью.
    /// </summary>
    /// <returns>Узел выражения.</returns>
    private Expression ParseMultiplicative()
    {
        Expression left = ParseUnary();
        while (Current.Type == TokenType.Star || Current.Type == TokenType.Slash || Current.Type == TokenType.Percent)
        {
            BinaryOperator operation = ToBinaryOperator(Advance().Type);
            Expression right = ParseUnary();
            left = new BinaryExpression(operation, left, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает необязательный унарный минус.
    /// </summary>
    /// <returns>Узел выражения.</returns>
    private Expression ParseUnary()
    {
        if (Current.Type == TokenType.Minus)
        {
            Advance();
            return new UnaryExpression(UnaryOperator.Negate, ParsePower());
        }

        return ParsePower();
    }

    /// <summary>
    /// Разбирает возведение в степень с правой ассоциативностью.
    /// </summary>
    /// <returns>Узел выражения.</returns>
    private Expression ParsePower()
    {
        Expression basis = ParsePrimary();
        if (Current.Type == TokenType.Caret)
        {
            Advance();
            Expression exponent = ParseUnary();
            return new BinaryExpression(BinaryOperator.Power, basis, exponent);
        }

        return basis;
    }

    /// <summary>
    /// Разбирает целочисленный литерал, вызов readInt() или выражение в скобках.
    /// </summary>
    /// <returns>Узел выражения.</returns>
    /// <exception cref="ParserException">Если встречена неожиданная лексема.</exception>
    private Expression ParsePrimary()
    {
        if (Current.Type == TokenType.IntegerLiteral)
        {
            string text = Advance().Value;
            return new IntegerLiteralExpression(ParseIntegerValue(text));
        }

        if (Current.Type == TokenType.KeywordReadInt)
        {
            Advance();
            Expect(TokenType.LeftParenthesis);
            Expect(TokenType.RightParenthesis);
            return new ReadIntExpression();
        }

        if (Current.Type == TokenType.LeftParenthesis)
        {
            Advance();
            Expression result = ParseExpression();
            Expect(TokenType.RightParenthesis);
            return result;
        }

        throw new ParserException(
            $"Неожиданная лексема '{Current.Value}' на строке {Current.Line}, колонке {Current.Column}.");
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
        if (!long.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out long value))
        {
            throw new ParserException($"Целочисленный литерал '{text}' слишком велик.");
        }

        return value;
    }

    /// <summary>
    /// Сопоставляет вид лексемы оператора с видом бинарной операции AST.
    /// </summary>
    /// <param name="type">Вид лексемы оператора.</param>
    /// <returns>Вид бинарной операции.</returns>
    private static BinaryOperator ToBinaryOperator(TokenType type)
    {
        return type switch
        {
            TokenType.Plus => BinaryOperator.Add,
            TokenType.Minus => BinaryOperator.Subtract,
            TokenType.Star => BinaryOperator.Multiply,
            TokenType.Slash => BinaryOperator.Divide,
            TokenType.Percent => BinaryOperator.Remainder,
            _ => BinaryOperator.Power,
        };
    }
}