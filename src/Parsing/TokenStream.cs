using Lexemes;

namespace Parsing;

/// <summary>
/// Поток лексем: хранит список лексем и позицию разбора,
/// предоставляет просмотр текущей лексемы и движение вперёд.
/// </summary>
public sealed class TokenStream
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    /// <summary>
    /// Создаёт поток для списка лексем, полученного от лексического анализатора.
    /// </summary>
    /// <param name="tokens">Список лексем программы.</param>
    public TokenStream(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    /// <summary>
    /// Текущая лексема потока, не сдвигая позицию разбора.
    /// </summary>
    public Token Current => _tokens[_position];

    /// <summary>
    /// Сдвигает позицию разбора на одну лексему вперёд, не заходя за конец файла.
    /// </summary>
    /// <returns>Лексема, которая была текущей.</returns>
    public Token Advance()
    {
        Token token = _tokens[_position];
        if (_position < _tokens.Count - 1)
        {
            _position += 1;
        }

        return token;
    }
}