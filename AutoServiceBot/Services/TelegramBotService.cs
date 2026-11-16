using AutoServiceBot.Handlers;
using Telegram.Bot;

namespace AutoServiceBot.Services;

/// <summary>
/// Сервис для запуска Telegram-бота через Long Polling.
/// </summary>
public sealed class TelegramBotService : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ITelegramUpdateHandler _updateHandler;
    private readonly ILogger<TelegramBotService> _logger;

    private CancellationTokenSource? _cts;

    public TelegramBotService(
        ITelegramBotClient botClient,
        ITelegramUpdateHandler updateHandler,
        ILogger<TelegramBotService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();

        var botInfo = await _botClient.GetMe(cancellationToken);

        _logger.LogInformation(
            "Telegram-бот @{BotUsername} успешно подключен.",
            botInfo.Username);

        _botClient.StartReceiving(
            async (client, update, token) =>
            {
                try
                {
                    await _updateHandler.HandleAsync(update, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке обновления Telegram.");
                }
            },
            async (client, exception, source, token) =>
            {
                _logger.LogError(
                    exception,
                    "Ошибка Telegram API. Источник: {ErrorSource}",
                    source);
            },
            receiverOptions: null,
            cancellationToken: _cts.Token);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            _logger.LogInformation("Telegram-бот остановлен.");
        }

        return Task.CompletedTask;
    }
}