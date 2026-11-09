using System.Security;

using Lexemes;
using Parsing;

namespace Compiler;

/// <summary>
/// Точка входа консольного интерпретатора языка CompkillerNEW07.
/// Читает исходную программу из файла, ввод программы — из stdin,
/// а результаты выполнения выводит в stdout.
/// </summary>
public static class Program
{
    /// <summary>
    /// Запускает интерпретатор. Единственный аргумент командной строки — путь к файлу программы.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Ноль при успехе, единица при любой ожидаемой ошибке выполнения.</returns>
    public static int Main(string[] args)
    {
        return Run(args, Console.In, Console.Out, Console.Error);
    }

    /// <summary>
    /// Выполняет программу из файла с указанными потоками ввода, вывода и ошибок.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <param name="input">Ввод программы.</param>
    /// <param name="output">Вывод программы.</param>
    /// <param name="error">Поток диагностических сообщений.</param>
    /// <returns>Ноль при успехе, единица при ошибке.</returns>
    public static int Run(string[] args, TextReader input, TextWriter output, TextWriter error)
    {
        if (args.Length != 1)
        {
            error.WriteLine("Использование: интерпретатор <путь к файлу программы>");
            return 1;
        }

        try
        {
            Lexer lexer = Lexer.FromFile(args[0]);
            IReadOnlyList<Token> tokens = lexer.Tokenize();
            Parser parser = new Parser(tokens, input, output);
            parser.ParseProgram();
            return 0;
        }
        catch (LexerException exception)
        {
            error.WriteLine(exception.Message);
            return 1;
        }
        catch (ParserException exception)
        {
            error.WriteLine(exception.Message);
            return 1;
        }
        catch (FileNotFoundException exception)
        {
            error.WriteLine($"Файл не найден: {exception.FileName}");
            return 1;
        }
        catch (DirectoryNotFoundException exception)
        {
            error.WriteLine($"Каталог не найден: {exception.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException exception)
        {
            error.WriteLine($"Нет доступа к файлу: {exception.Message}");
            return 1;
        }
        catch (IOException exception)
        {
            error.WriteLine($"Ошибка ввода-вывода: {exception.Message}");
            return 1;
        }
        catch (ArgumentException exception)
        {
            error.WriteLine($"Некорректный путь к файлу: {exception.Message}");
            return 1;
        }
        catch (NotSupportedException exception)
        {
            error.WriteLine($"Неподдерживаемый путь к файлу: {exception.Message}");
            return 1;
        }
        catch (SecurityException exception)
        {
            error.WriteLine($"Операция запрещена политикой безопасности: {exception.Message}");
            return 1;
        }
    }
}