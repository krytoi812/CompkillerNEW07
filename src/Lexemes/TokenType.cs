namespace Lexemes;

/// <summary>
/// Виды лексем языка CompkillerNEW07.
/// </summary>
public enum TokenType
{
    /// <summary>Конец входного текста.</summary>
    EndOfFile,

    /// <summary>Идентификатор: имя переменной, функции, структуры и т.п.</summary>
    Identifier,

    /// <summary>Целочисленный литерал, например 314.</summary>
    IntegerLiteral,

    /// <summary>Вещественный литерал, например 3.14.</summary>
    RealLiteral,

    /// <summary>Строковый литерал в двойных кавычках.</summary>
    StringLiteral,

    /// <summary>Логический литерал true или false.</summary>
    BooleanLiteral,

    /// <summary>Ключевое слово main.</summary>
    KeywordMain,

    /// <summary>Ключевое слово if.</summary>
    KeywordIf,

    /// <summary>Ключевое слово else.</summary>
    KeywordElse,

    /// <summary>Ключевое слово while.</summary>
    KeywordWhile,

    /// <summary>Ключевое слово for.</summary>
    KeywordFor,

    /// <summary>Ключевое слово return.</summary>
    KeywordReturn,

    /// <summary>Ключевое слово var.</summary>
    KeywordVar,

    /// <summary>Ключевое слово struct.</summary>
    KeywordStruct,

    /// <summary>Ключевое слово class.</summary>
    KeywordClass,

    /// <summary>Ключевое слово int (тип целых чисел).</summary>
    KeywordInt,

    /// <summary>Ключевое слово float (тип вещественных чисел).</summary>
    KeywordFloat,

    /// <summary>Ключевое слово string (строковый тип).</summary>
    KeywordString,

    /// <summary>Ключевое слово bool (логический тип).</summary>
    KeywordBool,

    /// <summary>Логический оператор and (зарезервированное слово).</summary>
    KeywordAnd,

    /// <summary>Логический оператор or (зарезервированное слово).</summary>
    KeywordOr,

    /// <summary>Логический оператор not (зарезервированное слово).</summary>
    KeywordNot,

    /// <summary>Оператор сложения +.</summary>
    Plus,

    /// <summary>Оператор вычитания -.</summary>
    Minus,

    /// <summary>Оператор умножения *.</summary>
    Star,

    /// <summary>Оператор деления /.</summary>
    Slash,

    /// <summary>Оператор взятия остатка %.</summary>
    Percent,

    /// <summary>Оператор присваивания =.</summary>
    Assign,

    /// <summary>Оператор сравнения ==.</summary>
    Equal,

    /// <summary>Оператор сравнения !=.</summary>
    NotEqual,

    /// <summary>Оператор сравнения &lt;.</summary>
    Less,

    /// <summary>Оператор сравнения &lt;=.</summary>
    LessOrEqual,

    /// <summary>Оператор сравнения &gt;.</summary>
    Greater,

    /// <summary>Оператор сравнения &gt;=.</summary>
    GreaterOrEqual,

    /// <summary>Логический оператор &amp;&amp;.</summary>
    AmpersandAmpersand,

    /// <summary>Логический оператор ||.</summary>
    VerticalBarVerticalBar,

    /// <summary>Логический оператор !.</summary>
    ExclamationMark,

    /// <summary>Откруглая скобка (.</summary>
    LeftParenthesis,

    /// <summary>Круглая скобка ).</summary>
    RightParenthesis,

    /// <summary>Фигурная скобка {.</summary>
    LeftBrace,

    /// <summary>Фигурная скобка }.</summary>
    RightBrace,

    /// <summary>Квадратная скобка [.</summary>
    LeftBracket,

    /// <summary>Квадратная скобка ].</summary>
    RightBracket,

    /// <summary>Запятая.</summary>
    Comma,

    /// <summary>Точка с запятой.</summary>
    Semicolon,

    /// <summary>Двоеточие.</summary>
    Colon,

    /// <summary>Точка.</summary>
    Dot,
}