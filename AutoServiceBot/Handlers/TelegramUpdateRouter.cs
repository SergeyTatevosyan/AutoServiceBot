using Telegram.Bot.Types;

namespace AutoServiceBot.Handlers;

/// <summary>
/// Маршрутизирует обновления Telegram к соответствующим обработчикам.
/// </summary>
public sealed class TelegramUpdateRouter : ITelegramUpdateHandler
{
    private readonly MessageHandler _messageHandler;
    private readonly CallbackQueryHandler _callbackQueryHandler;

    public TelegramUpdateRouter(
        MessageHandler messageHandler,
        CallbackQueryHandler callbackQueryHandler)
    {
        _messageHandler = messageHandler;
        _callbackQueryHandler = callbackQueryHandler;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not null)
        {
            await _messageHandler.HandleAsync(
                update.Message,
                cancellationToken);

            return;
        }

        if (update.CallbackQuery is not null)
        {
            await _callbackQueryHandler.HandleAsync(
                update.CallbackQuery,
                cancellationToken);
        }
    }
}