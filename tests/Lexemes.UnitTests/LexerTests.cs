using Lexemes;
using Xunit;

namespace Lexemes.UnitTests;

/// <summary>
/// Модульные тесты лексического анализатора языка CompkillerNEW07.
/// </summary>
public sealed class LexerTests
{
    [Fact]
    public void Tokenize_EmptySource_ReturnsOnlyEndOfFile()
    {
        Lexer lexer = new Lexer(string.Empty);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(1, tokens[0].Column);
    }

    [Theory]
    [MemberData(nameof(WhitespaceSamples))]
    public void Tokenize_WhitespaceOnly_ReturnsOnlyEndOfFile(string source)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
    }

    /// <summary>
    /// Наборы пробельных символов: пробел, табуляция, переводы строк.
    /// </summary>
    /// <returns>Наборы входных текстов.</returns>
    public static TheoryData<string> WhitespaceSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add(" ");
        data.Add("\t");
        data.Add("\r\n");
        data.Add(" \t\r\n ");
        return data;
    }

    [Theory]
    [MemberData(nameof(IdentifierSamples))]
    public void Tokenize_Identifier_ReturnsIdentifierTokenWithOriginalSpelling(string source, string expectedValue)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
        Assert.Equal(TokenType.EndOfFile, tokens[1].Type);
    }

    /// <summary>
    /// Идентификаторы из примеров спецификации, включая префиксы ключевых слов.
    /// </summary>
    /// <returns>Пары «входной текст — ожидаемое написание».</returns>
    public static TheoryData<string, string> IdentifierSamples()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        data.Add("x", "x");
        data.Add("summa", "summa");
        data.Add("_counter", "_counter");
        data.Add("_counterspEll", "_counterspEll");
        data.Add("BaSHmAK", "BaSHmAK");
        data.Add("value2", "value2");
        data.Add("iffy", "iffy");
        data.Add("mainx", "mainx");
        return data;
    }

    [Theory]
    [MemberData(nameof(KeywordSamples))]
    public void Tokenize_KeywordInAnyCase_ReturnsKeywordToken(string source, TokenType expectedType)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
        Assert.Equal(TokenType.EndOfFile, tokens[1].Type);
    }

    /// <summary>
    /// Ключевые слова в разном регистре: язык нечувствителен к регистру.
    /// </summary>
    /// <returns>Пары «входной текст — ожидаемый вид лексемы».</returns>
    public static TheoryData<string, TokenType> KeywordSamples()
    {
        TheoryData<string, TokenType> data = new TheoryData<string, TokenType>();
        data.Add("main", TokenType.KeywordMain);
        data.Add("MAIN", TokenType.KeywordMain);
        data.Add("if", TokenType.KeywordIf);
        data.Add("If", TokenType.KeywordIf);
        data.Add("ELSE", TokenType.KeywordElse);
        data.Add("while", TokenType.KeywordWhile);
        data.Add("For", TokenType.KeywordFor);
        data.Add("return", TokenType.KeywordReturn);
        data.Add("var", TokenType.KeywordVar);
        data.Add("STRUCT", TokenType.KeywordStruct);
        data.Add("class", TokenType.KeywordClass);
        data.Add("and", TokenType.KeywordAnd);
        data.Add("OR", TokenType.KeywordOr);
        data.Add("not", TokenType.KeywordNot);
        return data;
    }

    [Theory]
    [MemberData(nameof(BooleanLiteralSamples))]
    public void Tokenize_BooleanLiteral_ReturnsBooleanLiteralToken(string source)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.BooleanLiteral, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
    }

    /// <summary>
    /// Логические литералы в разном регистре.
    /// </summary>
    /// <returns>Наборы входных текстов.</returns>
    public static TheoryData<string> BooleanLiteralSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("true");
        data.Add("false");
        data.Add("TRUE");
        data.Add("False");
        return data;
    }

    [Theory]
    [MemberData(nameof(IntegerLiteralSamples))]
    public void Tokenize_Integer_ReturnsIntegerLiteralToken(string source)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.IntegerLiteral, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
    }

    /// <summary>
    /// Целочисленные литералы, включая ведущие нули.
    /// </summary>
    /// <returns>Наборы входных текстов.</returns>
    public static TheoryData<string> IntegerLiteralSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("0");
        data.Add("314");
        data.Add("007");
        return data;
    }

    [Theory]
    [MemberData(nameof(RealLiteralSamples))]
    public void Tokenize_RealNumber_ReturnsRealLiteralToken(string source)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.RealLiteral, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
    }

    /// <summary>
    /// Вещественные литералы из примеров спецификации.
    /// </summary>
    /// <returns>Наборы входных текстов.</returns>
    public static TheoryData<string> RealLiteralSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("3.14");
        data.Add("0.5");
        data.Add("123.456");
        return data;
    }

    [Fact]
    public void Tokenize_IntegerFollowedByDot_ReturnsIntegerAndDotSeparately()
    {
        Lexer lexer = new Lexer("1.");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.IntegerLiteral, tokens[0].Type);
        Assert.Equal("1", tokens[0].Value);
        Assert.Equal(TokenType.Dot, tokens[1].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_StringLiteral_ReturnsDecodedValue()
    {
        Lexer lexer = new Lexer("\"Hello, world!\"");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal("Hello, world!", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_StringWithEscapeSequences_DecodesThem()
    {
        Lexer lexer = new Lexer("\"a\\nb\\t\\\"c\\\\d\"");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal("a\nb\t\"c\\d", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_StringWithRawNewline_KeepsNewlineAndTracksLines()
    {
        Lexer lexer = new Lexer("\"line1\nline2\"");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal("line1\nline2", tokens[0].Value);
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(2, tokens[1].Line);
    }

    [Fact]
    public void Tokenize_RussianStringLiteral_ReturnsDecodedValue()
    {
        Lexer lexer = new Lexer("\"Привет, мир!\"");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
        Assert.Equal("Привет, мир!", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_LineComment_SkipsCommentText()
    {
        Lexer lexer = new Lexer("a // комментарий до конца строки\nb");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("a", tokens[0].Value);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("b", tokens[1].Value);
        Assert.Equal(2, tokens[1].Line);
    }

    [Fact]
    public void Tokenize_LineCommentAtEndOfFile_SkipsComment()
    {
        Lexer lexer = new Lexer("a // комментария без перевода строки");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_BlockComment_SkipsCommentAndTracksLines()
    {
        Lexer lexer = new Lexer("a /* первая\nвторая */ b");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("b", tokens[1].Value);
        Assert.Equal(2, tokens[1].Line);
    }

    [Theory]
    [MemberData(nameof(SingleCharOperatorSamples))]
    public void Tokenize_SingleCharOperator_ReturnsOperatorToken(string source, TokenType expectedType)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
    }

    /// <summary>
    /// Односимвольные операторы, включая одиночные =, !, &lt;, &gt;.
    /// </summary>
    /// <returns>Пары «входной текст — ожидаемый вид лексемы».</returns>
    public static TheoryData<string, TokenType> SingleCharOperatorSamples()
    {
        TheoryData<string, TokenType> data = new TheoryData<string, TokenType>();
        data.Add("+", TokenType.Plus);
        data.Add("-", TokenType.Minus);
        data.Add("*", TokenType.Star);
        data.Add("/", TokenType.Slash);
        data.Add("%", TokenType.Percent);
        data.Add("=", TokenType.Assign);
        data.Add("!", TokenType.ExclamationMark);
        data.Add("<", TokenType.Less);
        data.Add(">", TokenType.Greater);
        return data;
    }

    [Theory]
    [MemberData(nameof(TwoCharOperatorSamples))]
    public void Tokenize_TwoCharOperator_ReturnsSingleToken(string source, TokenType expectedType)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
    }

    /// <summary>
    /// Двухсимвольные операторы сравнения и логические операторы.
    /// </summary>
    /// <returns>Пары «входной текст — ожидаемый вид лексемы».</returns>
    public static TheoryData<string, TokenType> TwoCharOperatorSamples()
    {
        TheoryData<string, TokenType> data = new TheoryData<string, TokenType>();
        data.Add("==", TokenType.Equal);
        data.Add("!=", TokenType.NotEqual);
        data.Add("<=", TokenType.LessOrEqual);
        data.Add(">=", TokenType.GreaterOrEqual);
        data.Add("&&", TokenType.AmpersandAmpersand);
        data.Add("||", TokenType.VerticalBarVerticalBar);
        return data;
    }

    [Theory]
    [MemberData(nameof(DelimiterSamples))]
    public void Tokenize_Delimiter_ReturnsDelimiterToken(string source, TokenType expectedType)
    {
        Lexer lexer = new Lexer(source);

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(source, tokens[0].Value);
    }

    /// <summary>
    /// Все разделители языка из спецификации.
    /// </summary>
    /// <returns>Пары «входной текст — ожидаемый вид лексемы».</returns>
    public static TheoryData<string, TokenType> DelimiterSamples()
    {
        TheoryData<string, TokenType> data = new TheoryData<string, TokenType>();
        data.Add("(", TokenType.LeftParenthesis);
        data.Add(")", TokenType.RightParenthesis);
        data.Add("{", TokenType.LeftBrace);
        data.Add("}", TokenType.RightBrace);
        data.Add("[", TokenType.LeftBracket);
        data.Add("]", TokenType.RightBracket);
        data.Add(",", TokenType.Comma);
        data.Add(";", TokenType.Semicolon);
        data.Add(":", TokenType.Colon);
        data.Add(".", TokenType.Dot);
        return data;
    }

    [Fact]
    public void Tokenize_ArithmeticExpression_ReturnsCorrectSequence()
    {
        Lexer lexer = new Lexer("x + y / (10 - z * 2)");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(12, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(TokenType.Plus, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal(TokenType.Slash, tokens[3].Type);
        Assert.Equal(TokenType.LeftParenthesis, tokens[4].Type);
        Assert.Equal(TokenType.IntegerLiteral, tokens[5].Type);
        Assert.Equal(TokenType.Minus, tokens[6].Type);
        Assert.Equal(TokenType.Identifier, tokens[7].Type);
        Assert.Equal(TokenType.Star, tokens[8].Type);
        Assert.Equal(TokenType.IntegerLiteral, tokens[9].Type);
        Assert.Equal(TokenType.RightParenthesis, tokens[10].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[11].Type);
    }

    [Fact]
    public void Tokenize_LogicalExpression_ReturnsCorrectSequence()
    {
        Lexer lexer = new Lexer("speed <= limit && limit >= 0 || !flag");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(11, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("speed", tokens[0].Value);
        Assert.Equal(TokenType.LessOrEqual, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal(TokenType.AmpersandAmpersand, tokens[3].Type);
        Assert.Equal(TokenType.Identifier, tokens[4].Type);
        Assert.Equal(TokenType.GreaterOrEqual, tokens[5].Type);
        Assert.Equal(TokenType.IntegerLiteral, tokens[6].Type);
        Assert.Equal(TokenType.VerticalBarVerticalBar, tokens[7].Type);
        Assert.Equal(TokenType.ExclamationMark, tokens[8].Type);
        Assert.Equal(TokenType.Identifier, tokens[9].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[10].Type);
    }

    [Fact]
    public void Tokenize_ArrayAccessAndField_ReturnsCorrectSequence()
    {
        Lexer lexer = new Lexer("v[0] = speed.x;");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(10, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(TokenType.LeftBracket, tokens[1].Type);
        Assert.Equal(TokenType.IntegerLiteral, tokens[2].Type);
        Assert.Equal(TokenType.RightBracket, tokens[3].Type);
        Assert.Equal(TokenType.Assign, tokens[4].Type);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);
        Assert.Equal(TokenType.Dot, tokens[6].Type);
        Assert.Equal(TokenType.Identifier, tokens[7].Type);
        Assert.Equal(TokenType.Semicolon, tokens[8].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[9].Type);
    }

    [Fact]
    public void Tokenize_StructDeclaration_ReturnsCorrectSequence()
    {
        Lexer lexer = new Lexer("struct Pair { int a, b; }");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(10, tokens.Count);
        Assert.Equal(TokenType.KeywordStruct, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal("Pair", tokens[1].Value);
        Assert.Equal(TokenType.LeftBrace, tokens[2].Type);
        Assert.Equal(TokenType.KeywordInt, tokens[3].Type);
        Assert.Equal(TokenType.Identifier, tokens[4].Type);
        Assert.Equal(TokenType.Comma, tokens[5].Type);
        Assert.Equal(TokenType.Identifier, tokens[6].Type);
        Assert.Equal(TokenType.Semicolon, tokens[7].Type);
        Assert.Equal(TokenType.RightBrace, tokens[8].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[9].Type);
    }

    [Fact]
    public void Tokenize_SmallProgram_ReturnsExpectedTokenSequence()
    {
        Lexer lexer = new Lexer("main var x = 10 + 2.5; // конец");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(9, tokens.Count);
        Assert.Equal(TokenType.KeywordMain, tokens[0].Type);
        Assert.Equal(TokenType.KeywordVar, tokens[1].Type);
        Assert.Equal(TokenType.Identifier, tokens[2].Type);
        Assert.Equal("x", tokens[2].Value);
        Assert.Equal(TokenType.Assign, tokens[3].Type);
        Assert.Equal(TokenType.IntegerLiteral, tokens[4].Type);
        Assert.Equal("10", tokens[4].Value);
        Assert.Equal(TokenType.Plus, tokens[5].Type);
        Assert.Equal(TokenType.RealLiteral, tokens[6].Type);
        Assert.Equal("2.5", tokens[6].Value);
        Assert.Equal(TokenType.Semicolon, tokens[7].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[8].Type);
    }

    [Fact]
    public void Tokenize_MultilineSource_TracksLinesAndColumns()
    {
        Lexer lexer = new Lexer("x =\n  42;");

        IReadOnlyList<Token> tokens = lexer.Tokenize();

        Assert.Equal(5, tokens.Count);
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(1, tokens[0].Column);
        Assert.Equal(1, tokens[1].Line);
        Assert.Equal(3, tokens[1].Column);
        Assert.Equal(2, tokens[2].Line);
        Assert.Equal(3, tokens[2].Column);
        Assert.Equal(2, tokens[3].Line);
        Assert.Equal(5, tokens[3].Column);
    }

    [Theory]
    [MemberData(nameof(InvalidSourceSamples))]
    public void Tokenize_InvalidSource_ThrowsLexerException(string source)
    {
        Lexer lexer = new Lexer(source);

        Assert.Throws<LexerException>(() => lexer.Tokenize());
    }

    /// <summary>
    /// Негативные сценарии: неизвестные символы, одиночные &amp; и |,
    /// незакрытая строка, неизвестное экранирование, незакрытый комментарий.
    /// </summary>
    /// <returns>Наборы некорректных входных текстов.</returns>
    public static TheoryData<string> InvalidSourceSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("@");
        data.Add("#");
        data.Add("&");
        data.Add("|");
        data.Add("\"abc");
        data.Add("\"abc\\");
        data.Add("\"a\\qb\"");
        data.Add("a /* незакрытый комментарий");
        return data;
    }
}