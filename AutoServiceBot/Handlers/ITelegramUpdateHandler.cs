using Telegram.Bot.Types;

namespace AutoServiceBot.Handlers;

/// <summary>
/// Интерфейс обработчика обновлений Telegram.
/// </summary>
public interface ITelegramUpdateHandler
{
    /// <summary>
    /// Обработка обновления Telegram.
    /// </summary>
    /// <param name="update">Полученное обновление.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task HandleAsync(Update update, CancellationToken cancellationToken);
}