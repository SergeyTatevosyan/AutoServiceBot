using AutoServiceBot.Configuration;
using AutoServiceBot.Entities.Enums;
using AutoServiceBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AutoServiceBot.Handlers;

/// <summary>
/// Обрабатывает текстовые сообщения и команды Telegram.
/// </summary>
public sealed class MessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IClientRequestStorage _requestStorage;
    private readonly long _groupChatId;

    public MessageHandler(
        ITelegramBotClient botClient,
        IClientRequestStorage requestStorage,
        IConfiguration configuration)
    {
        _botClient = botClient;
        _requestStorage = requestStorage;

        if (!long.TryParse(
            configuration["TelegramGroupChatId"],
            out _groupChatId))
        {
            throw new InvalidOperationException(
                "Не удалось получить корректный ID группы Telegram.");
        }
    }

    /// <summary>
    /// Обработка входящего сообщения.
    /// </summary>
    /// <param name="message">Полученное сообщение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task HandleAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        // Проверяем, отправил ли клиент контакт.
        if (message.Contact is not null)
        {
            await HandlePhoneNumberAsync(
                chatId,
                message.Contact,
                cancellationToken);

            return;
        }

        if (message.Text is null)
            return;

        var messageText = message.Text.Trim();

        if (messageText == "/start" ||
            messageText.StartsWith("/start ") ||
            messageText == "🚀 Начать")
        {
            await StartRequestAsync(chatId, cancellationToken);
            return;
        }

        var request = _requestStorage.GetOrCreate(chatId);

        switch (request.Step)
        {
            case ClientRequestStep.EnterCarBrand:
                await HandleCarBrandAsync(
                    request,
                    messageText,
                    cancellationToken);
                break;

            case ClientRequestStep.EnterCarModel:
                await HandleCarModelAsync(
                    request,
                    messageText,
                    cancellationToken);
                break;

            case ClientRequestStep.EnterCarYear:
                await HandleCarYearAsync(
                    request,
                    messageText,
                    cancellationToken);
                break;
            case ClientRequestStep.EnterProblemDescription:
                await HandleProblemDescriptionAsync(
                    request,
                    messageText,
                    cancellationToken);
                break;
            case ClientRequestStep.EnterClientName:
                await HandleClientNameAsync(
                    request,
                    messageText,
                    cancellationToken);
                break;
            case ClientRequestStep.EnterPhoneNumber:
                await HandlePhoneNumberTextAsync(
                    request,
                    messageText,
                    cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Начало заполнения заявки.
    /// </summary>
    private async Task StartRequestAsync(
        long chatId,
        CancellationToken cancellationToken)
    {
        _requestStorage.Remove(chatId);

        var request = _requestStorage.GetOrCreate(chatId);
        request.Step = ClientRequestStep.SelectService;

        await _botClient.SendMessage(
            chatId,
            "🚗 Добро пожаловать в AutoServicePro!\n\n" +
            "Рады приветствовать вас!\n\n" +
            "С помощью нашего бота вы можете быстро оставить заявку " +
            "на обслуживание или ремонт автомобиля.\n\n" +
            "Выберите интересующую вас услугу:",
            replyMarkup: BuildMainMenu(),
            cancellationToken: cancellationToken);

        await _botClient.SendMessage(
            chatId,
            "👇 Выберите услугу из списка:",
            replyMarkup: BuildServicesMenu(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка ввода марки автомобиля.
    /// </summary>
    private async Task HandleCarBrandAsync(
        Entities.ClientRequest request,
        string messageText,
        CancellationToken cancellationToken)
    {
        if (messageText.Length < 2 || messageText.Length > 50)
        {
            await _botClient.SendMessage(
                request.ChatId,
                "Пожалуйста, укажите корректное название марки автомобиля.",
                cancellationToken: cancellationToken);

            return;
        }

        request.CarBrand = messageText;
        request.Step = ClientRequestStep.EnterCarModel;

        await _botClient.SendMessage(
            request.ChatId,
            "Отлично! Теперь укажите модель вашего автомобиля.\n\n" +
            "Например: Golf 6, A4, Camry, Solaris.",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка ввода модели автомобиля.
    /// </summary>
    private async Task HandleCarModelAsync(
        Entities.ClientRequest request,
        string messageText,
        CancellationToken cancellationToken)
    {
        if (messageText.Length < 1 || messageText.Length > 100)
        {
            await _botClient.SendMessage(
                request.ChatId,
                "Пожалуйста, укажите корректную модель автомобиля.",
                cancellationToken: cancellationToken);

            return;
        }

        request.CarModel = messageText;
        request.Step = ClientRequestStep.EnterCarYear;

        await _botClient.SendMessage(
            request.ChatId,
            $"Автомобиль: {request.CarBrand} {request.CarModel}.\n\n" +
            "Теперь укажите год выпуска автомобиля.\n\n" +
            "Например: 2012.",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка ввода года выпуска автомобиля.
    /// </summary>
    private async Task HandleCarYearAsync(
        Entities.ClientRequest request,
        string messageText,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(messageText, out var carYear) ||
            carYear < 1950 ||
            carYear > DateTime.UtcNow.Year)
        {
            await _botClient.SendMessage(
                request.ChatId,
                $"Пожалуйста, укажите корректный год выпуска автомобиля (от 1950 до {DateTime.UtcNow.Year}).",
                cancellationToken: cancellationToken);

            return;
        }

        request.CarYear = carYear;
        request.Step = ClientRequestStep.EnterProblemDescription;

        await _botClient.SendMessage(
            request.ChatId,
            $"Отлично! Автомобиль: {request.CarBrand} {request.CarModel}, {request.CarYear} г.\n\n" +
            "Теперь опишите, какая проблема возникла с автомобилем или какие работы необходимо выполнить.",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка описания неисправности или необходимых работ.
    /// </summary>
    private async Task HandleProblemDescriptionAsync(
        Entities.ClientRequest request,
        string messageText,
        CancellationToken cancellationToken)
    {
        if (messageText.Length < 5 || messageText.Length > 1000)
        {
            await _botClient.SendMessage(
                request.ChatId,
                "Пожалуйста, опишите проблему подробнее (от 5 до 1000 символов).",
                cancellationToken: cancellationToken);

            return;
        }

        request.ProblemDescription = messageText;
        request.Step = ClientRequestStep.EnterClientName;

        await _botClient.SendMessage(
            request.ChatId,
            "Спасибо! Описание проблемы сохранено. 👍\n\n" +
            "Теперь укажите ваше имя.",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка имени клиента.
    /// </summary>
    private async Task HandleClientNameAsync(
        Entities.ClientRequest request,
        string messageText,
        CancellationToken cancellationToken)
    {
        if (messageText.Length < 2 || messageText.Length > 100)
        {
            await _botClient.SendMessage(
                request.ChatId,
                "Пожалуйста, укажите корректное имя (от 2 до 100 символов).",
                cancellationToken: cancellationToken);

            return;
        }

        request.ClientName = messageText;
        request.Step = ClientRequestStep.EnterPhoneNumber;

        await _botClient.SendMessage(
            request.ChatId,
            $"Приятно познакомиться, {request.ClientName}! 👋\n\n" +
            "Теперь поделитесь своим номером телефона, чтобы автосервис мог связаться с вами.",
            replyMarkup: BuildPhoneNumberMenu(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Обработка номера телефона, введённого вручную.
    /// </summary>
    private async Task HandlePhoneNumberTextAsync(
        Entities.ClientRequest request,
        string messageText,
        CancellationToken cancellationToken)
    {
        // Оставляем только цифры для проверки номера.
        var digits = new string(
            messageText.Where(char.IsDigit).ToArray());

        if (digits.Length < 10 || digits.Length > 15)
        {
            await _botClient.SendMessage(
                request.ChatId,
                "Пожалуйста, укажите корректный номер телефона.\n\n" +
                "Например: +7 900 123-45-67",
                replyMarkup: BuildPhoneNumberMenu(),
                cancellationToken: cancellationToken);

            return;
        }

        request.PhoneNumber = messageText;
        await SendCompletedRequestAsync(
            request,
            cancellationToken);
    }

    /// <summary>
    /// Отправка готовой заявки в группу автосервиса.
    /// </summary>
    private async Task SendCompletedRequestAsync(
        Entities.ClientRequest request,
        CancellationToken cancellationToken)
    {
        var requestMessage = BuildRequestMessage(request);

        await _botClient.SendMessage(
            _groupChatId,
            requestMessage,
            cancellationToken: cancellationToken);

        _requestStorage.Remove(request.ChatId);

        await _botClient.SendMessage(
            request.ChatId,
            "✅ Ваша заявка успешно отправлена!\n\n" +
            "Спасибо, что выбрали AutoServicePro.\n\n" +
            "Наш специалист свяжется с вами в ближайшее время.\n\n" +
            "Если хотите оставить ещё одну заявку, нажмите кнопку «🚀 Начать».",
            replyMarkup: BuildMainMenu(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Создание клавиатуры для отправки номера телефона.
    /// </summary>
    private static ReplyKeyboardMarkup BuildPhoneNumberMenu()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
            new KeyboardButton("📱 Поделиться номером телефона")
            {
                RequestContact = true
            }
            })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };
    }

    /// <summary>
    /// Обработка номера телефона клиента и отправка готовой заявки.
    /// </summary>
    private async Task HandlePhoneNumberAsync(
        long chatId,
        Telegram.Bot.Types.Contact contact,
        CancellationToken cancellationToken)
    {
        var request = _requestStorage.GetOrCreate(chatId);

        if (request.Step != ClientRequestStep.EnterPhoneNumber)
            return;

        // Проверяем, что клиент отправил именно свой контакт.
        if (contact.UserId is null ||
            contact.UserId != chatId)
        {
            await _botClient.SendMessage(
                chatId,
                "Пожалуйста, отправьте свой номер телефона с помощью кнопки ниже.",
                replyMarkup: BuildPhoneNumberMenu(),
                cancellationToken: cancellationToken);

            return;
        }

        if (string.IsNullOrWhiteSpace(contact.PhoneNumber))
        {
            await _botClient.SendMessage(
                chatId,
                "Не удалось получить номер телефона. Попробуйте ещё раз.",
                replyMarkup: BuildPhoneNumberMenu(),
                cancellationToken: cancellationToken);

            return;
        }

        request.PhoneNumber = contact.PhoneNumber;

        await SendCompletedRequestAsync(
            request,
            cancellationToken);
    }

    /// <summary>
    /// Формирование текстового сообщения с заявкой клиента.
    /// </summary>
    private static string BuildRequestMessage(
        Entities.ClientRequest request)
    {
        return
            "🚗 НОВАЯ ЗАЯВКА AutoServicePro\n\n" +

            "📋 УСЛУГА\n" +
            $"{request.ServiceName}\n\n" +

            "🚘 АВТОМОБИЛЬ\n" +
            $"Марка: {request.CarBrand}\n" +
            $"Модель: {request.CarModel}\n" +
            $"Год выпуска: {request.CarYear}\n\n" +

            "🔧 ОПИСАНИЕ ПРОБЛЕМЫ\n" +
            $"{request.ProblemDescription}\n\n" +

            "👤 КОНТАКТНЫЕ ДАННЫЕ\n" +
            $"Имя: {request.ClientName}\n" +
            $"Телефон: {request.PhoneNumber}\n\n" +

            "━━━━━━━━━━━━━━━━━━\n" +
            "Заявка получена через Telegram-бота." +

            "\n";
    }

    /// <summary>
    /// Создание клавиатуры выбора услуг автосервиса.
    /// </summary>
    private static InlineKeyboardMarkup BuildServicesMenu()
    {
        var buttons = AutoServiceCatalog.Services
            .Select(service => InlineKeyboardButton.WithCallbackData(
                service.Value,
                $"service:{service.Key}"))
            .ToList();

        var rows = new List<InlineKeyboardButton[]>();

        // Разбиваем кнопки на строки по две.
        for (var i = 0; i < buttons.Count; i += 2)
        {
            rows.Add(buttons.Skip(i).Take(2).ToArray());
        }

        return new InlineKeyboardMarkup(rows);
    }

    /// <summary>
    /// Создание постоянного главного меню бота.
    /// </summary>
    private static ReplyKeyboardMarkup BuildMainMenu()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
            new KeyboardButton("🚀 Начать")
            })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };
    }
}