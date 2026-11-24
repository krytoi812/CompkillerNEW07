using System.IO;

using Ast;
using Execution;
using Lexemes;
using Parsing;
using Xunit;

namespace Parsing.UnitTests;

/// <summary>
/// Интеграционные тесты парсера и интерпретатора языка CompkillerNEW07.
/// Подготовка вынесена в общие вспомогательные методы.
/// </summary>
public sealed class ParserTests
{
    [Theory]
    [MemberData(nameof(ProgramSamples))]
    public void ParseProgram_ValidSource_WritesExpectedOutput(string source, string expectedOutput)
    {
        string output = Execute(source, string.Empty);

        Assert.Equal(expectedOutput, output);
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
    [MemberData(nameof(InvalidSyntaxSamples))]
    public void ParseProgram_InvalidSyntax_ThrowsParserException(string source)
    {
        Parser parser = CreateParser(source);

        Assert.Throws<ParserException>(() => parser.ParseProgram());
    }

    /// <summary>
    /// Негативные сценарии синтаксиса.
    /// </summary>
    /// <returns>Наборы некорректных программ.</returns>
    public static TheoryData<string> InvalidSyntaxSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("{ print 1; }");
        data.Add("main print 1; }");
        data.Add("main { print 1 }");
        data.Add("main { print ; }");
        data.Add("main { print (1; }");
        data.Add("main { print 1;");
        data.Add("main { print 1; } }");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidExecutionSamples))]
    public void Evaluate_InvalidExecution_ThrowsExecutionException(string source)
    {
        Assert.Throws<ExecutionException>(() => Execute(source, string.Empty));
    }

    /// <summary>
    /// Негативные сценарии времени выполнения.
    /// </summary>
    /// <returns>Наборы программ с ошибками выполнения.</returns>
    public static TheoryData<string> InvalidExecutionSamples()
    {
        TheoryData<string> data = new TheoryData<string>();
        data.Add("main { print 1 / 0; }");
        data.Add("main { print 5 % 0; }");
        data.Add("main { print 9223372036854775807 + 1; }");
        return data;
    }

    [Fact]
    public void Evaluate_NegativeExponent_ThrowsNegativeExponentError()
    {
        ExecutionException exception = Assert.Throws<ExecutionException>(
            () => Execute("main { print 2 ^ -1; }", string.Empty));

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
        string output = Execute(source, inputText);

        Assert.Equal(expectedOutput, output);
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
    public void Evaluate_ReadIntWithInvalidInput_ThrowsExecutionException(string inputText)
    {
        Assert.Throws<ExecutionException>(() => Execute("main { print readInt(); }", inputText));
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
        string output = Execute(source, string.Empty);

        Assert.Equal(expectedOutput, output);
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
    public void Evaluate_OverflowingExpression_ThrowsExecutionException(string source)
    {
        Assert.Throws<ExecutionException>(() => Execute(source, string.Empty));
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
    public void Evaluate_NegatingMinimumReadIntValue_ThrowsExecutionException()
    {
        Assert.Throws<ExecutionException>(
            () => Execute("main { print -readInt(); }", "-9223372036854775808\n"));
    }

    /// <summary>
    /// Общая подготовка: создаёт парсер для исходного текста.
    /// </summary>
    /// <param name="source">Исходный текст программы.</param>
    /// <returns>Готовый парсер.</returns>
    private static Parser CreateParser(string source)
    {
        return new Parser(new Lexer(source).Tokenize());
    }

    /// <summary>
    /// Общая подготовка и выполнение: разбирает AST и вычисляет его, возвращая вывод.
    /// </summary>
    /// <param name="source">Исходный текст программы.</param>
    /// <param name="inputText">Содержимое stdin.</param>
    /// <returns>Текст stdout.</returns>
    private static string Execute(string source, string inputText)
    {
        Parser parser = CreateParser(source);
        ProgramNode program = parser.ParseProgram();
        StringWriter output = new StringWriter();
        output.NewLine = "\n";
        AstEvaluator evaluator = new AstEvaluator(new StreamEnvironment(new StringReader(inputText), output));
        evaluator.Evaluate(program);
        return output.ToString();
    }
}