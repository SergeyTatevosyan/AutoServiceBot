using AutoServiceBot.Configuration;
using AutoServiceBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using AutoServiceBot.Entities.Enums;

namespace AutoServiceBot.Handlers;

/// <summary>
/// Обрабатывает нажатия inline-кнопок Telegram.
/// </summary>
public sealed class CallbackQueryHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IClientRequestStorage _requestStorage;

    public CallbackQueryHandler(
        ITelegramBotClient botClient,
        IClientRequestStorage requestStorage)
    {
        _botClient = botClient;
        _requestStorage = requestStorage;
    }

    /// <summary>
    /// Обработка нажатия inline-кнопки.
    /// </summary>
    /// <param name="callbackQuery">Полученный callback-запрос.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task HandleAsync(
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var callbackData = callbackQuery.Data;

        if (string.IsNullOrWhiteSpace(callbackData))
            return;

        if (!callbackData.StartsWith("service:"))
            return;

        var serviceId = callbackData["service:".Length..];

        if (!AutoServiceCatalog.Services.TryGetValue(serviceId, out var serviceName))
        {
            await _botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "Выбранная услуга не найдена.",
                cancellationToken: cancellationToken);

            return;
        }

        var chatId = callbackQuery.Message?.Chat.Id;

        if (chatId is null)
            return;

        // Сохраняем выбранную услугу во временной заявке.
        var request = _requestStorage.GetOrCreate(chatId.Value);
        request.ServiceName = serviceName;
        request.Step = ClientRequestStep.EnterCarBrand;

        // Убираем индикатор загрузки после нажатия кнопки.
        await _botClient.AnswerCallbackQuery(
            callbackQuery.Id,
            cancellationToken: cancellationToken);

        await _botClient.SendMessage(
            chatId.Value,
            $"Отлично! Вы выбрали: {serviceName}.\n\n" +
            "Теперь укажите марку вашего автомобиля.\n\n" +
            "Например: Volkswagen, Audi, BMW, Toyota.",
            cancellationToken: cancellationToken);
    }
}