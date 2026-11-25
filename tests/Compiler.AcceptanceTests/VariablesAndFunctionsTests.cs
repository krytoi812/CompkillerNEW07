using System.Globalization;
using System.IO;

using Xunit;

namespace Compiler.AcceptanceTests;

/// <summary>
/// Приёмочные тесты переменных, функций и областей видимости.
/// </summary>
public sealed class VariablesAndFunctionsTests
{
    [Theory]
    [MemberData(nameof(ValidPrograms))]
    public void Run_ValidProgram_WritesExpectedOutput(string source, string input, string expectedOutput)
    {
        (int ExitCode, string Output, string Error) result = Run(source, input);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expectedOutput, result.Output);
        Assert.Empty(result.Error);
    }

    /// <summary>Корректные программы и ожидаемый результат.</summary>
    /// <returns>Исходный код, stdin и stdout.</returns>
    public static TheoryData<string, string, string> ValidPrograms()
    {
        TheoryData<string, string, string> data = new TheoryData<string, string, string>();
        data.Add("main { int x = 3; print x + 5; }", string.Empty, "8\n");
        data.Add("main { int x = 3; x = 7; print x; }", string.Empty, "7\n");
        data.Add("main { int x = 3; int y = x + 2; print y; }", string.Empty, "5\n");
        data.Add("main { int a = 0; int b = 0; print a = b = 7; print a + b; }", string.Empty, "7\n14\n");
        data.Add("main { int x = 0; print 1 + (x = 3); print x; }", string.Empty, "4\n3\n");
        data.Add("main { int x = 0; (x) = 4; print x; }", string.Empty, "4\n");
        data.Add("main { int x = 0; print x = 2 + 3 * 4; }", string.Empty, "14\n");
        data.Add("main { int a = 0; int b = 0; a = b = readInt(); print a; print b; }", "9\n", "9\n9\n");
        data.Add("MAIN { INT Value = 4; value = VALUE + 1; PRINT vAlUe; }", string.Empty, "5\n");
        data.Add("main { int x = 1; { int x = 2; print x; } print x; }", string.Empty, "2\n1\n");
        data.Add("main { int x = 1; { x = 3; } print x; }", string.Empty, "3\n");
        data.Add("main { int x = 4; { int x = x + 1; print x; } print x; }", string.Empty, "5\n4\n");
        data.Add("main { { int x = 1; print x; } { int x = 2; print x; } }", string.Empty, "1\n2\n");
        data.Add("int answer() { return 42; } main { print answer(); }", string.Empty, "42\n");
        data.Add("int add(int a, int b) { return a + b; } main { print add(2, 3); }", string.Empty, "5\n");
        data.Add("int add(int a, int b) { return a + b; } main { print add(add(1, 2), 3); }", string.Empty, "6\n");
        data.Add("int first() { return second(); } int second() { return 8; } main { print first(); }", string.Empty, "8\n");
        data.Add("int f() { return 3; } main { int f = 5; print f; print f(); }", string.Empty, "5\n3\n");
        data.Add("int f(int x) { x = x + 1; return x; } main { int x = 4; print f(x); print x; }", string.Empty, "5\n4\n");
        data.Add("int f(int x) { { int x = 7; print x; } return x; } main { print f(2); }", string.Empty, "7\n2\n");
        data.Add("int f() { int x = 0; x = x + 1; return x; } main { print f(); print f(); }", string.Empty, "1\n1\n");
        data.Add("int f() { { return 5; } print 99; return 7; } main { print f(); }", string.Empty, "5\n");
        data.Add("void show(int x) { print x; } main { show(6); }", string.Empty, "6\n");
        data.Add("void mark() { print 1; return; } int answer() { mark(); return 2; } main { print answer(); }", string.Empty, "1\n2\n");
        data.Add("void show() { print 6; return; print 99; } main { show(); print 1; }", string.Empty, "6\n1\n");
        data.Add("void stop() { { return; } print 99; } main { stop(); print 1; }", string.Empty, "1\n");
        data.Add("int f() { print 1; return 2; } main { f(); print 3; }", string.Empty, "1\n3\n");
        data.Add("INT Twice(INT x) { RETURN x * 2; } MAIN { PRINT twice(4); }", string.Empty, "8\n");
        data.Add("int pair(int a, int b) { return a * 10 + b; } main { int x = 0; print pair(x = x + 1, x = x + 1); print x; }", string.Empty, "12\n2\n");
        data.Add("int difference(int a, int b) { return a - b; } main { print difference(readInt(), readInt()); }", "10\n3\n", "7\n");
        data.Add("main { print abs(-7); print abs(0); print abs(7); }", string.Empty, "7\n0\n7\n");
        data.Add("main { print min(7); print min(3, -2, 8, 1); print max(-5, 2, 9, 4); }", string.Empty, "7\n-2\n9\n");
        data.Add("main { print min(8, max(1, 4), abs(-2)); print MAX(-3); }", string.Empty, "2\n-3\n");
        data.Add("main { int min = 5; print min(min, 3); }", string.Empty, "3\n");
        data.Add("main { int x = 0; print max(x = x + 1, x = x + 1, x = x + 1); print x; }", string.Empty, "3\n3\n");
        data.Add("int take() { return readInt(); } main { print take(); }", "17\n", "17\n");
        data.Add(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Programs", "rectangle.ck")), "1\n2\n7\n9\n", "42\n");
        data.Add(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Programs", "order.ck")), "2\n150\n3\n20\n", "410\n");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidPrograms))]
    public void Run_InvalidProgram_ReturnsDiagnostic(string source, string input, string diagnostic)
    {
        (int ExitCode, string Output, string Error) result = Run(source, input);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains(diagnostic, result.Error, StringComparison.Ordinal);
    }

    /// <summary>Ошибки объявлений, вызовов и областей видимости.</summary>
    /// <returns>Исходный код, stdin и ожидаемая часть диагностики.</returns>
    public static TheoryData<string, string, string> InvalidPrograms()
    {
        TheoryData<string, string, string> data = new TheoryData<string, string, string>();
        data.Add("main { int x; }", string.Empty, "Ожидалась лексема");
        data.Add("main { print x; int x = 1; }", string.Empty, "Неизвестная переменная");
        data.Add("main { x = 1; }", string.Empty, "Неизвестная переменная");
        data.Add("main { int x = x + 1; }", string.Empty, "Неизвестная переменная");
        data.Add("main { int x = 1; int X = 2; }", string.Empty, "уже объявлена");
        data.Add("main { { int x = 1; } print x; }", string.Empty, "Неизвестная переменная");
        data.Add("main { 1 = 2; }", string.Empty, "Левая часть присваивания");
        data.Add("main { int x = 1; x + 1 = 2; }", string.Empty, "Левая часть присваивания");
        data.Add("int f() { return 1; } main { f() = 2; }", string.Empty, "Левая часть присваивания");
        data.Add("int f() { return x; } main { int x = 3; print f(); }", string.Empty, "Неизвестная переменная");
        data.Add("int inner() { return x; } int outer(int x) { return inner(); } main { print outer(8); }", string.Empty, "Неизвестная переменная");
        data.Add("int f(int x) { int x = 2; return x; } main { print f(1); }", string.Empty, "уже объявлена");
        data.Add("int f(int x, int X) { return x; } main {}", string.Empty, "Параметр");
        data.Add("int f() { return 1; } int F() { return 2; } main {}", string.Empty, "уже объявлена");
        data.Add("int min() { return 1; } main {}", string.Empty, "зарезервировано");
        data.Add("main { print missing(); }", string.Empty, "Неизвестная функция");
        data.Add("main { print missing(readInt()); }", string.Empty, "Неизвестная функция");
        data.Add("int f(int x) { return x; } main { print f(); }", string.Empty, "число аргументов");
        data.Add("int f() { return 1; } main { print f(1); }", string.Empty, "число аргументов");
        data.Add("int f() { return 1; } main { print f(readInt()); }", string.Empty, "число аргументов");
        data.Add("main { print abs(); }", string.Empty, "число аргументов");
        data.Add("main { print abs(1, 2); }", string.Empty, "число аргументов");
        data.Add("main { print min(); }", string.Empty, "число аргументов");
        data.Add("main { print max(); }", string.Empty, "число аргументов");
        data.Add("main { print abs(readInt()); }", "-9223372036854775808\n", "Переполнение");
        data.Add("main { return; }", string.Empty, "вне функции");
        data.Add("int f() {} main { print f(); }", string.Empty, "вернуть значение");
        data.Add("int f() { return; } main { print f(); }", string.Empty, "вернуть значение");
        data.Add("void f() { return 1; } main { f(); }", string.Empty, "не возвращает значение");
        data.Add("void f() {} main { print f(); }", string.Empty, "без возвращаемого значения");
        data.Add("void f() {} main { int x = f(); }", string.Empty, "без возвращаемого значения");
        data.Add("void f() {} main { print f() + 1; }", string.Empty, "без возвращаемого значения");
        data.Add("void f() {} int g(int x) { return x; } main { print g(f()); }", string.Empty, "без возвращаемого значения");
        data.Add("int loop() { return loop(); } main { print loop(); }", string.Empty, "глубина вызовов");
        return data;
    }

    [Fact]
    public void Run_RuntimeError_StopsBeforeFollowingStatements()
    {
        (int ExitCode, string Output, string Error) result = Run(
            "int fail() { print 7; return 1 / 0; } main { print fail(); print 99; }",
            string.Empty);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("7\n", result.Output);
        Assert.Contains("Деление на ноль", result.Error, StringComparison.Ordinal);
    }

    private static (int ExitCode, string Output, string Error) Run(string source, string inputText)
    {
        using SourceFile file = new SourceFile(source);
        using StringReader input = new StringReader(inputText);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);
        output.NewLine = "\n";

        int exitCode = Program.Run(new[] { file.FilePath }, input, output, error);
        return (exitCode, output.ToString(), error.ToString());
    }
}