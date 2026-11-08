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
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Использование: интерпретатор <путь к файлу программы>");
            return 1;
        }

        try
        {
            Lexer lexer = Lexer.FromFile(args[0]);
            IReadOnlyList<Token> tokens = lexer.Tokenize();
            Parser parser = new Parser(tokens, Console.In, Console.Out);
            parser.ParseProgram();
            return 0;
        }
        catch (LexerException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        catch (ParserException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        catch (FileNotFoundException exception)
        {
            Console.Error.WriteLine($"Файл не найден: {exception.FileName}");
            return 1;
        }
        catch (DirectoryNotFoundException exception)
        {
            Console.Error.WriteLine($"Каталог не найден: {exception.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.Error.WriteLine($"Нет доступа к файлу: {exception.Message}");
            return 1;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Ошибка ввода-вывода: {exception.Message}");
            return 1;
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine($"Некорректный путь к файлу: {exception.Message}");
            return 1;
        }
        catch (NotSupportedException exception)
        {
            Console.Error.WriteLine($"Неподдерживаемый путь к файлу: {exception.Message}");
            return 1;
        }
        catch (SecurityException exception)
        {
            Console.Error.WriteLine($"Операция запрещена политикой безопасности: {exception.Message}");
            return 1;
        }
    }
}