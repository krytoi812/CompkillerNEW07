namespace Lexemes;

/// <summary>
/// Лексема исходного текста: вид, написание и позиция начала.
/// Для строкового литерала Value хранит расшифрованное содержимое строки,
/// для остальных лексем — текст в том виде, как он встретился в источнике.
/// </summary>
/// <param name="Type">Вид лексемы.</param>
/// <param name="Value">Текст лексемы.</param>
/// <param name="Line">Номер строки начала лексемы, нумерация с единицы.</param>
/// <param name="Column">Номер колонки начала лексемы, нумерация с единицы.</param>
public sealed record Token(TokenType Type, string Value, int Line, int Column);