using System.Text;

namespace Lexemes;

/// <summary>
/// Лексический анализатор языка CompkillerNEW07.
/// Читает исходный текст посимвольно слева направо: позиция чтения только
/// увеличивается, распознавание многосимвольных лексем выполняется через
/// предпросмотр символов вперёд.
/// </summary>
public sealed class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    /// <summary>
    /// Зарезервированные слова языка в нижнем регистре: язык нечувствителен
    /// к регистру, поэтому сравнение выполняется после приведения к нижнему регистру.
    /// </summary>
        /// <summary>
    /// Зарезервированные слова языка в нижнем регистре: язык нечувствителен
    /// к регистру, поэтому сравнение выполняется после приведения к нижнему регистру.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
    {
        ["main"] = TokenType.KeywordMain,
        ["if"] = TokenType.KeywordIf,
        ["else"] = TokenType.KeywordElse,
        ["while"] = TokenType.KeywordWhile,
        ["for"] = TokenType.KeywordFor,
        ["return"] = TokenType.KeywordReturn,
        ["var"] = TokenType.KeywordVar,
        ["struct"] = TokenType.KeywordStruct,
        ["class"] = TokenType.KeywordClass,
        ["int"] = TokenType.KeywordInt,
        ["float"] = TokenType.KeywordFloat,
        ["string"] = TokenType.KeywordString,
        ["bool"] = TokenType.KeywordBool,
        ["and"] = TokenType.KeywordAnd,
        ["or"] = TokenType.KeywordOr,
        ["not"] = TokenType.KeywordNot,
        ["true"] = TokenType.BooleanLiteral,
        ["false"] = TokenType.BooleanLiteral,
    };

    /// <summary>
    /// Создаёт анализатор для исходного текста программы.
    /// </summary>
    /// <param name="source">Полный текст исходного файла.</param>
    public Lexer(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Разбирает весь исходный текст и возвращает список лексем,
    /// завершающийся лексемой конца файла.
    /// </summary>
    /// <returns>Список распознанных лексем.</returns>
    /// <exception cref="LexerException">Если текст содержит недопустимую лексему.</exception>
    public IReadOnlyList<Token> Tokenize()
    {
        List<Token> tokens = new List<Token>();
        while (true)
        {
            Token token = NextToken();
            tokens.Add(token);
            if (token.Type == TokenType.EndOfFile)
            {
                return tokens;
            }
        }
    }

    /// <summary>
    /// Распознаёт одну следующую лексему, пропуская пробелы и комментарии.
    /// </summary>
    /// <returns>Следующая лексема или лексема конца файла.</returns>
    private Token NextToken()
    {
        SkipWhitespaceAndComments();
        if (_position >= _source.Length)
        {
            return new Token(TokenType.EndOfFile, string.Empty, _line, _column);
        }

        char current = Peek(0);
        if (char.IsDigit(current))
        {
            return ReadNumber();
        }

        if (current == '"')
        {
            return ReadString();
        }

        if (char.IsLetter(current) || current == '_')
        {
            return ReadIdentifierOrKeyword();
        }

        return ReadOperatorOrDelimiter();
    }

    /// <summary>
    /// Возвращает символ на расстоянии offset от позиции чтения,
    /// не сдвигая позицию; за концом текста возвращает символ с кодом ноль.
    /// </summary>
    /// <param name="offset">Смещение предпросмотра вперёд.</param>
    /// <returns>Символ источника или нулевой символ за концом текста.</returns>
    private char Peek(int offset)
    {
        int index = _position + offset;
        if (index >= _source.Length)
        {
            return '\0';
        }

        return _source[index];
    }

    /// <summary>
    /// Сдвигает позицию чтения на один символ вперёд и обновляет номер строки и колонки.
    /// </summary>
    /// <returns>Прочитанный символ.</returns>
    private char Advance()
    {
        char current = _source[_position];
        _position += 1;
        if (current == '\n')
        {
            _line += 1;
            _column = 1;
        }
        else
        {
            _column += 1;
        }

        return current;
    }

    /// <summary>
    /// Пропускает пробельные символы и комментарии обоих видов.
    /// </summary>
    private void SkipWhitespaceAndComments()
    {
        while (_position < _source.Length)
        {
            char current = Peek(0);
            if (char.IsWhiteSpace(current))
            {
                Advance();
                continue;
            }

            if (current == '/' && Peek(1) == '/')
            {
                SkipLineComment();
                continue;
            }

            if (current == '/' && Peek(1) == '*')
            {
                SkipBlockComment();
                continue;
            }

            return;
        }
    }

    /// <summary>
    /// Пропускает однострочный комментарий до конца строки или до конца файла.
    /// </summary>
    private void SkipLineComment()
    {
        while (_position < _source.Length && Peek(0) != '\n')
        {
            Advance();
        }
    }

    /// <summary>
    /// Пропускает многострочный комментарий до закрывающей последовательности.
    /// </summary>
    /// <exception cref="LexerException">Если комментарий не закрыт до конца файла.</exception>
    private void SkipBlockComment()
    {
        int startLine = _line;
        int startColumn = _column;
        Advance(); // Символ '/'.
        Advance(); // Символ '*'.
        while (true)
        {
            if (_position >= _source.Length)
            {
                throw new LexerException(
                    $"Незакрытый многострочный комментарий, начатый на строке {startLine}, колонке {startColumn}.");
            }

            if (Peek(0) == '*' && Peek(1) == '/')
            {
                Advance();
                Advance();
                return;
            }

            Advance();
        }
    }

    /// <summary>
    /// Читает целочисленный или вещественный литерал.
    /// Точка становится частью числа только если за ней следует цифра,
    /// иначе точка останется отдельной лексемой-разделителем.
    /// </summary>
    /// <returns>Лексема числового литерала.</returns>
    private Token ReadNumber()
    {
        int startLine = _line;
        int startColumn = _column;
        StringBuilder builder = new StringBuilder();
        while (char.IsDigit(Peek(0)))
        {
            builder.Append(Advance());
        }

        TokenType type = TokenType.IntegerLiteral;
        if (Peek(0) == '.' && char.IsDigit(Peek(1)))
        {
            builder.Append(Advance()); // Точка.
            while (char.IsDigit(Peek(0)))
            {
                builder.Append(Advance());
            }

            type = TokenType.RealLiteral;
        }

        return new Token(type, builder.ToString(), startLine, startColumn);
    }

    /// <summary>
    /// Читает строковый литерал в двойных кавычках, раскрывая управляющие
    /// последовательности \\, \", \n и \t; неизвестная последовательность — ошибка.
    /// </summary>
    /// <returns>Лексема строкового литерала с расшифрованным содержимым.</returns>
    /// <exception cref="LexerException">Если строка не закрыта или экранирование неизвестно.</exception>
    private Token ReadString()
    {
        int startLine = _line;
        int startColumn = _column;
        Advance(); // Открывающая кавычка.
        StringBuilder builder = new StringBuilder();
        while (true)
        {
            if (_position >= _source.Length)
            {
                throw new LexerException(
                    $"Незакрытая строка, начатая на строке {startLine}, колонке {startColumn}.");
            }

            char current = Advance();
            if (current == '"')
            {
                break;
            }

            if (current == '\\')
            {
                if (_position >= _source.Length)
                {
                    throw new LexerException(
                        $"Незакрытая строка, начатая на строке {startLine}, колонке {startColumn}.");
                }

                char escaped = Advance();
                char decoded = escaped switch
                {
                    'n' => '\n',
                    't' => '\t',
                    '"' => '"',
                    '\\' => '\\',
                    _ => throw new LexerException(
                        $"Неизвестная управляющая последовательность с символом '{escaped}' на строке {_line}, колонке {_column}."),
                };
                builder.Append(decoded);
                continue;
            }

            builder.Append(current);
        }

        return new Token(TokenType.StringLiteral, builder.ToString(), startLine, startColumn);
    }

    /// <summary>
    /// Читает слово из букв, цифр и подчёркиваний и определяет, является ли оно
    /// зарезервированным словом; регистр при сравнении не учитывается.
    /// </summary>
    /// <returns>Лексема ключевого слова или идентификатора.</returns>
    private Token ReadIdentifierOrKeyword()
    {
        int startLine = _line;
        int startColumn = _column;
        StringBuilder builder = new StringBuilder();
        while (true)
        {
            char current = Peek(0);
            if (!char.IsLetterOrDigit(current) && current != '_')
            {
                break;
            }

            builder.Append(Advance());
        }

        string value = builder.ToString();
        string lowercased = value.ToLowerInvariant();
        if (Keywords.TryGetValue(lowercased, out TokenType keywordType))
        {
            return new Token(keywordType, value, startLine, startColumn);
        }

        return new Token(TokenType.Identifier, value, startLine, startColumn);
    }

    /// <summary>
    /// Читает оператор или разделитель; для символов =, !, &lt;, &gt;, &amp;, |
    /// выполняет предпросмотр следующего символа, чтобы отличить == от = и т.п.
    /// </summary>
    /// <returns>Лексема оператора или разделителя.</returns>
    /// <exception cref="LexerException">Если символ не входит в лексику языка.</exception>
    private Token ReadOperatorOrDelimiter()
    {
        int startLine = _line;
        int startColumn = _column;
        char current = Advance();
        switch (current)
        {
            case '+':
                return new Token(TokenType.Plus, "+", startLine, startColumn);
            case '-':
                return new Token(TokenType.Minus, "-", startLine, startColumn);
            case '*':
                return new Token(TokenType.Star, "*", startLine, startColumn);
            case '/':
                return new Token(TokenType.Slash, "/", startLine, startColumn);
            case '%':
                return new Token(TokenType.Percent, "%", startLine, startColumn);
            case '(':
                return new Token(TokenType.LeftParenthesis, "(", startLine, startColumn);
            case ')':
                return new Token(TokenType.RightParenthesis, ")", startLine, startColumn);
            case '{':
                return new Token(TokenType.LeftBrace, "{", startLine, startColumn);
            case '}':
                return new Token(TokenType.RightBrace, "}", startLine, startColumn);
            case '[':
                return new Token(TokenType.LeftBracket, "[", startLine, startColumn);
            case ']':
                return new Token(TokenType.RightBracket, "]", startLine, startColumn);
            case ',':
                return new Token(TokenType.Comma, ",", startLine, startColumn);
            case ';':
                return new Token(TokenType.Semicolon, ";", startLine, startColumn);
            case ':':
                return new Token(TokenType.Colon, ":", startLine, startColumn);
            case '.':
                return new Token(TokenType.Dot, ".", startLine, startColumn);
            case '=':
                if (Peek(0) == '=')
                {
                    Advance();
                    return new Token(TokenType.Equal, "==", startLine, startColumn);
                }

                return new Token(TokenType.Assign, "=", startLine, startColumn);
            case '!':
                if (Peek(0) == '=')
                {
                    Advance();
                    return new Token(TokenType.NotEqual, "!=", startLine, startColumn);
                }

                return new Token(TokenType.ExclamationMark, "!", startLine, startColumn);
            case '<':
                if (Peek(0) == '=')
                {
                    Advance();
                    return new Token(TokenType.LessOrEqual, "<=", startLine, startColumn);
                }

                return new Token(TokenType.Less, "<", startLine, startColumn);
            case '>':
                if (Peek(0) == '=')
                {
                    Advance();
                    return new Token(TokenType.GreaterOrEqual, ">=", startLine, startColumn);
                }

                return new Token(TokenType.Greater, ">", startLine, startColumn);
            case '&':
                if (Peek(0) == '&')
                {
                    Advance();
                    return new Token(TokenType.AmpersandAmpersand, "&&", startLine, startColumn);
                }

                throw new LexerException(
                    $"Одиночный символ '&' на строке {startLine}, колонке {startColumn}: ожидался оператор '&&'.");
            case '|':
                if (Peek(0) == '|')
                {
                    Advance();
                    return new Token(TokenType.VerticalBarVerticalBar, "||", startLine, startColumn);
                }

                throw new LexerException(
                    $"Одиночный символ '|' на строке {startLine}, колонке {startColumn}: ожидался оператор '||'.");
            default:
                throw new LexerException(
                    $"Неизвестный символ '{current}' на строке {startLine}, колонке {startColumn}.");
        }
    }
}