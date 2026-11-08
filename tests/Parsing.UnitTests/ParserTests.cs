using System.IO;

using Lexemes;
using Parsing;
using Xunit;

namespace Parsing.UnitTests;

/// <summary>
/// Интеграционные тесты парсера и интерпретатора языка CompkillerNEW07.
/// </summary>
public sealed class ParserTests
{
    [Theory]
    [MemberData(nameof(ProgramSamples))]
    public void ParseProgram_ValidSource_WritesExpectedOutput(string source, string expectedOutput)
    {
        Lexer lexer = new Lexer(source);
        StringWriter output = new StringWriter();
        output.NewLine = "\n";
        Parser parser = new Parser(lexer.Tokenize(), new StringReader(string.Empty), output);

        parser.ParseProgram();

        Assert.Equal(expectedOutput, output.ToString());
    }

    /// <summary>
    /// Пары «исходный текст программы — ожидаемый вывод».
    /// </summary>
    /// <returns>Наборы сценариев выполнения.</returns>
    public static TheoryData<string, string> ProgramSamples()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        data.Add("main { }", string.Empty);
        data.Add("main { print 2 + 3 * 4; }", "14\n");
        data.Add("main { print (2 + 3) * 4; }", "20\n");
        data.Add("main { print 10 - 3 - 2; }", "5\n");
        data.Add("main { print 7 / 2; }", "3\n");
        data.Add("main { print 17 % 5; }", "2\n");
        data.Add("main { print 2 ^ 3 ^ 2; }", "512\n");
        data.Add("main { print -2 ^ 2; }", "-4\n");
        data.Add("main { print -(5 + 3); }", "-8\n");
        data.Add("main { print -5 + 3; }", "-2\n");
        data.Add("main { 1 + 1; }", string.Empty);
        data.Add("main { print 1; print 2; print 3; }", "1\n2\n3\n");
        data.Add("main { print 17 % 5 + 2 ^ 3; }", "10\n");
        data.Add("/* комментарий */ main { print 42; } // конец", "42\n");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidProgramSamples))]
    public void ParseProgram_InvalidSource_ThrowsParserException(string source)
    {
        Lexer lexer = new Lexer(source);
        StringWriter output = new StringWriter();
        Parser parser = new Parser(lexer.Tokenize(), new StringReader(string.Empty), output);

        Assert.Throws<ParserException>(() => parser.ParseProgram());
    }

    /// <summary>
    /// Негативные сценарии: синтаксические ошибки и ошибки времени выполнения.
    /// </summary>
    /// <returns>Наборы некорректных программ.</returns>
    public static TheoryData<string> InvalidProgramSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("{ print 1; }");
        data.Add("main print 1; }");
        data.Add("main { print 1 }");
        data.Add("main { print ; }");
        data.Add("main { print (1; }");
        data.Add("main { print 1;");
        data.Add("main { print 1; } }");
        data.Add("main { print 1 / 0; }");
        data.Add("main { print 5 % 0; }");
        data.Add("main { print 9223372036854775807 + 1; }");
        return data;
    }

    [Fact]
    public void ParseProgram_NegativeExponent_ThrowsNegativeExponentError()
    {
        Lexer lexer = new Lexer("main { print 2 ^ -1; }");
        StringReader input = new StringReader(string.Empty);
        StringWriter output = new StringWriter();
        Parser parser = new Parser(lexer.Tokenize(), input, output);

        ParserException exception = Assert.Throws<ParserException>(() => parser.ParseProgram());

        Assert.Equal(
            "Отрицательный показатель степени недопустим для целых чисел.",
            exception.Message);
    }

    [Theory]
    [MemberData(nameof(ValidInputSamples))]
    public void ParseProgram_ReadInt_WritesExpectedOutput(
        string source,
        string inputText,
        string expectedOutput)
    {
        Lexer lexer = new Lexer(source);
        StringReader input = new StringReader(inputText);
        StringWriter output = new StringWriter();
        output.NewLine = "\n";
        Parser parser = new Parser(lexer.Tokenize(), input, output);

        parser.ParseProgram();

        Assert.Equal(expectedOutput, output.ToString());
    }

    /// <summary>
    /// Корректные программы с чтением положительного, отрицательного и граничного значения.
    /// </summary>
    /// <returns>Программа, содержимое stdin и ожидаемый stdout.</returns>
    public static TheoryData<string, string, string> ValidInputSamples()
    {
        TheoryData<string, string, string> data = new TheoryData<string, string, string>();
        data.Add("main { print readInt(); }", "42\n", "42\n");
        data.Add("main { print readInt() + 8; }", "-2\n", "6\n");
        data.Add("main { print readInt() ^ 2; }", "7\n", "49\n");
        data.Add("main { print readInt() + readInt(); }", "2\n3\n", "5\n");
        data.Add("main { print readInt(); }", "  +17  \n", "17\n");
        data.Add(
            "main { print readInt(); }",
            "-9223372036854775808\n",
            "-9223372036854775808\n");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidInputSamples))]
    public void ParseProgram_ReadIntWithInvalidInput_ThrowsParserException(string inputText)
    {
        Lexer lexer = new Lexer("main { print readInt(); }");
        StringReader input = new StringReader(inputText);
        StringWriter output = new StringWriter();
        Parser parser = new Parser(lexer.Tokenize(), input, output);

        Assert.Throws<ParserException>(() => parser.ParseProgram());
    }

    /// <summary>
    /// Некорректные данные и конец входного потока.
    /// </summary>
    /// <returns>Строки stdin, не содержащие допустимое целое число.</returns>
    public static TheoryData<string> InvalidInputSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add(string.Empty);
        data.Add("abc\n");
        data.Add("1.5\n");
        data.Add("9223372036854775808\n");
        return data;
    }

    [Theory]
    [MemberData(nameof(ValidBoundarySamples))]
    public void ParseProgram_BoundaryExpression_WritesExpectedOutput(string source, string expectedOutput)
    {
        Lexer lexer = new Lexer(source);
        StringReader input = new StringReader(string.Empty);
        StringWriter output = new StringWriter();
        output.NewLine = "\n";
        Parser parser = new Parser(lexer.Tokenize(), input, output);

        parser.ParseProgram();

        Assert.Equal(expectedOutput, output.ToString());
    }

    /// <summary>
    /// Выражения на границе допустимых значений и степени.
    /// </summary>
    /// <returns>Программа и ожидаемый вывод.</returns>
    public static TheoryData<string, string> ValidBoundarySamples()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        data.Add("main { print (-2) ^ 63; }", "-9223372036854775808\n");
        data.Add("main { print 0 ^ 0; }", "1\n");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidBoundarySamples))]
    public void ParseProgram_OverflowingExpression_ThrowsParserException(string source)
    {
        Lexer lexer = new Lexer(source);
        StringReader input = new StringReader(string.Empty);
        StringWriter output = new StringWriter();
        Parser parser = new Parser(lexer.Tokenize(), input, output);

        Assert.Throws<ParserException>(() => parser.ParseProgram());
    }

    /// <summary>
    /// Выражения, результат которых не помещается в знаковый 64-битный тип.
    /// </summary>
    /// <returns>Некорректная программа.</returns>
    public static TheoryData<string> InvalidBoundarySamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("main { print 2 ^ 64; }");
        data.Add("main { print 9223372036854775807 * 2; }");
        return data;
    }

    [Fact]
    public void ParseProgram_NegatingMinimumReadIntValue_ThrowsParserException()
    {
        Lexer lexer = new Lexer("main { print -readInt(); }");
        StringReader input = new StringReader("-9223372036854775808\n");
        StringWriter output = new StringWriter();
        Parser parser = new Parser(lexer.Tokenize(), input, output);

        Assert.Throws<ParserException>(() => parser.ParseProgram());
    }
}