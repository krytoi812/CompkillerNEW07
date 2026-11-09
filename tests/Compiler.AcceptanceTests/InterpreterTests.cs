using System.Globalization;
using System.IO;

using Xunit;

namespace Compiler.AcceptanceTests;

/// <summary>
/// Приёмочные тесты всего интерпретатора через тот же запуск, который использует CLI.
/// </summary>
public sealed class InterpreterTests
{
    [Theory]
    [MemberData(nameof(ValidPrograms))]
    public void Run_ValidProgram_ReturnsSuccessAndExpectedOutput(string source, string inputText, string expectedOutput)
    {
        using SourceFile file = new SourceFile(source);
        using StringReader input = new StringReader(inputText);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);
        output.NewLine = "\n";

        int exitCode = Program.Run(new[] { file.FilePath }, input, output, error);

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedOutput, output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    /// <summary>
    /// Корректные программы, входные данные и ожидаемый вывод.
    /// </summary>
    /// <returns>Сценарии для всех правил и альтернатив грамматики.</returns>
    public static TheoryData<string, string, string> ValidPrograms()
    {
        TheoryData<string, string, string> data = new TheoryData<string, string, string>();
        data.Add("main {}", string.Empty, string.Empty);
        data.Add("main { 2 + 3; }", string.Empty, string.Empty);
        data.Add("main { print 42; }", string.Empty, "42\n");
        data.Add("main { print 2 + 3; }", string.Empty, "5\n");
        data.Add("main { print 10 - 3 - 2; }", string.Empty, "5\n");
        data.Add("main { print 2 + 3 * 4; }", string.Empty, "14\n");
        data.Add("main { print (2 + 3) * 4; }", string.Empty, "20\n");
        data.Add("main { print 20 / 5 / 2; }", string.Empty, "2\n");
        data.Add("main { print 20 / 5 * 2; }", string.Empty, "8\n");
        data.Add("main { print -7 / 2; print 7 / -2; }", string.Empty, "-3\n-3\n");
        data.Add("main { print 17 % 5; }", string.Empty, "2\n");
        data.Add("main { print -7 % 3; print 7 % -3; }", string.Empty, "-1\n1\n");
        data.Add("main { print 2 ^ 3 ^ 2; }", string.Empty, "512\n");
        data.Add("main { print -2 ^ 2; }", string.Empty, "-4\n");
        data.Add("main { print (-2) ^ 2; }", string.Empty, "4\n");
        data.Add("main { print -(5 + 3); }", string.Empty, "-8\n");
        data.Add("main { print 0 ^ 0; }", string.Empty, "1\n");
        data.Add("main { print 0 ^ 9223372036854775807; }", string.Empty, "0\n");
        data.Add("main { print 1 ^ 9223372036854775807; }", string.Empty, "1\n");
        data.Add("main { print (-1) ^ 9223372036854775807; }", string.Empty, "-1\n");
        data.Add("main { print (-1) ^ 9223372036854775806; }", string.Empty, "1\n");
        data.Add("main { print 9223372036854775807; }", string.Empty, "9223372036854775807\n");
        data.Add("main { print -9223372036854775807 - 1; }", string.Empty, "-9223372036854775808\n");
        data.Add("main { print (-2) ^ 63; }", string.Empty, "-9223372036854775808\n");
        data.Add("main { print 007; }", string.Empty, "7\n");
        data.Add("MAIN { PRINT READINT(); }", "12\n", "12\n");
        data.Add("main { print 1; 2 + 3; print 4; }", string.Empty, "1\n4\n");
        data.Add("main { print readInt(); }", "42\n", "42\n");
        data.Add("main { print readInt(); }", "  +17  \n", "17\n");
        data.Add("main { print readInt(); }", "-9223372036854775808\n", "-9223372036854775808\n");
        data.Add("main { print readInt(); }", "9223372036854775807\n", "9223372036854775807\n");
        data.Add("main { print readInt() * readInt(); }", "6\n7\n", "42\n");
        data.Add("main { readInt(); print readInt(); }", "3\n4\n", "4\n");
        data.Add("main { print readInt() - readInt() - readInt(); }", "10\n4\n2\n", "4\n");
        data.Add("/* начало */ main { print 1 /* внутри */ + 2; } // конец", string.Empty, "3\n");
        data.Add("main {\r\n print 3;\r\n}", string.Empty, "3\n");
        data.Add("main\u00A0{\vprint\f3; }", string.Empty, "3\n");
        data.Add("main { print 10 + 7 * (2 * 3 + 4) + 5 + -5; }", string.Empty, "80\n");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidPrograms))]
    public void Run_InvalidProgram_ReturnsFailureAndWritesDiagnostic(
        string source,
        string inputText,
        string expectedOutput,
        string diagnostic)
    {
        using SourceFile file = new SourceFile(source);
        using StringReader input = new StringReader(inputText);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);
        output.NewLine = "\n";

        int exitCode = Program.Run(new[] { file.FilePath }, input, output, error);

        Assert.Equal(1, exitCode);
        Assert.Equal(expectedOutput, output.ToString());
        Assert.Contains(diagnostic, error.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Ошибки лексики, синтаксиса, арифметики и ввода; выполнение после ошибки прекращается.
    /// </summary>
    /// <returns>Исходный текст, stdin, stdout до ошибки и часть диагностики.</returns>
    public static TheoryData<string, string, string, string> InvalidPrograms()
    {
        TheoryData<string, string, string, string> data = new TheoryData<string, string, string, string>();
        data.Add(string.Empty, string.Empty, string.Empty, "Ожидалась лексема");
        data.Add("{ print 1; }", string.Empty, string.Empty, "Ожидалась лексема");
        data.Add("main print 1; }", string.Empty, string.Empty, "Ожидалась лексема");
        data.Add("main { print 1 }", string.Empty, string.Empty, "Ожидалась лексема");
        data.Add("main { print (1 + 2; }", string.Empty, string.Empty, "Ожидалась лексема");
        data.Add("main { print 1;", string.Empty, "1\n", "Ожидалась закрывающая фигурная скобка");
        data.Add("main {} main {}", string.Empty, string.Empty, "Ожидалась лексема");
        data.Add("main { print ; }", string.Empty, string.Empty, "Неожиданная лексема");
        data.Add("main { var x = 1; }", string.Empty, string.Empty, "Неожиданная лексема");
        data.Add("main { print x; }", string.Empty, string.Empty, "Неожиданная лексема");
        data.Add("main { print 1.5; }", string.Empty, string.Empty, "Неожиданная лексема");
        data.Add("main { print \"текст\"; }", string.Empty, string.Empty, "Неожиданная лексема");
        data.Add("main { print true; }", string.Empty, string.Empty, "Неожиданная лексема");
        data.Add("main { print @; }", string.Empty, string.Empty, "Неизвестный символ");
        data.Add("main { /* нет конца", string.Empty, string.Empty, "Незакрытый многострочный комментарий");
        data.Add("main { print 1 / 0; }", string.Empty, string.Empty, "Деление на ноль");
        data.Add("main { print 1 % 0; }", string.Empty, string.Empty, "Деление на ноль");
        data.Add("main { print 2 ^ -1; }", string.Empty, string.Empty, "Отрицательный показатель степени");
        data.Add("main { print 9223372036854775808; }", string.Empty, string.Empty, "слишком велик");
        data.Add("main { print -9223372036854775808; }", string.Empty, string.Empty, "слишком велик");
        data.Add("main { print 9223372036854775807 + 1; }", string.Empty, string.Empty, "Переполнение при сложении");
        data.Add("main { print (-9223372036854775807 - 1) - 1; }", string.Empty, string.Empty, "Переполнение при вычитании");
        data.Add("main { print 9223372036854775807 * 2; }", string.Empty, string.Empty, "Переполнение при умножении");
        data.Add("main { print (-9223372036854775807 - 1) / -1; }", string.Empty, string.Empty, "Переполнение при делении");
        data.Add("main { print (-9223372036854775807 - 1) % -1; }", string.Empty, string.Empty, "Переполнение при взятии остатка");
        data.Add("main { print -readInt(); }", "-9223372036854775808\n", string.Empty, "Переполнение при изменении знака");
        data.Add("main { print 2 ^ 64; }", string.Empty, string.Empty, "Переполнение при возведении в степень");
        data.Add("main { print 2 ^ 63; }", string.Empty, string.Empty, "Переполнение при умножении");
        data.Add("main { print readInt(); }", string.Empty, string.Empty, "конец входного потока");
        data.Add("main { print readInt(); }", "\n", string.Empty, "не является целым числом");
        data.Add("main { print readInt(); }", "abc\n", string.Empty, "не является целым числом");
        data.Add("main { print readInt(); }", "1.5\n", string.Empty, "не является целым числом");
        data.Add("main { print readInt(); }", "9223372036854775808\n", string.Empty, "не является целым числом");
        data.Add("main { print readInt(); }", "1 2\n", string.Empty, "не является целым числом");
        data.Add("main { print 7; 1 / 0; print 9; }", string.Empty, "7\n", "Деление на ноль");
        data.Add("main { print 7; print ; print 9; }", string.Empty, "7\n", "Неожиданная лексема");
        data.Add("main { print readInt(); print readInt(); print 9; }", "7\nabc\n", "7\n", "не является целым числом");
        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public void Run_InvalidArguments_ReturnsUsageError(string[] args)
    {
        using StringReader input = new StringReader(string.Empty);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);

        int exitCode = Program.Run(args, input, output, error);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Использование:", error.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Отсутствующий и лишний аргументы командной строки.
    /// </summary>
    /// <returns>Некорректные наборы аргументов.</returns>
    public static TheoryData<string[]> InvalidArguments()
    {
        TheoryData<string[]> data = new TheoryData<string[]>();
        data.Add(Array.Empty<string>());
        data.Add(new[] { "program.ck", "extra" });
        return data;
    }

    [Theory]
    [MemberData(nameof(MissingPaths))]
    public void Run_MissingSource_ReturnsFileError(string relativePath, string diagnostic)
    {
        string path = Path.Combine(Path.GetTempPath(), $"compkiller-{Guid.NewGuid():N}", relativePath);
        using StringReader input = new StringReader(string.Empty);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);

        int exitCode = Program.Run(new[] { path }, input, output, error);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains(diagnostic, error.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Несуществующие пути с отсутствующим каталогом.
    /// </summary>
    /// <returns>Путь и ожидаемая диагностика.</returns>
    public static TheoryData<string, string> MissingPaths()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        data.Add("program.ck", "Каталог не найден:");
        data.Add(Path.Combine("nested", "program.ck"), "Каталог не найден:");
        return data;
    }

    [Fact]
    public void Run_DeletedSource_ReturnsFileNotFoundError()
    {
        using SourceFile file = new SourceFile("main {}");
        File.Delete(file.FilePath);
        using StringReader input = new StringReader(string.Empty);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);

        int exitCode = Program.Run(new[] { file.FilePath }, input, output, error);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Файл не найден:", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Run_DirectoryInsteadOfFile_ReturnsError()
    {
        using StringReader input = new StringReader(string.Empty);
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
        using StringWriter error = new StringWriter(CultureInfo.InvariantCulture);

        int exitCode = Program.Run(new[] { Path.GetTempPath() }, input, output, error);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.NotEmpty(error.ToString());
    }
}