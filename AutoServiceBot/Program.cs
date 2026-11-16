using AutoServiceBot.Handlers;
using AutoServiceBot.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Telegram Bot Token
var botToken = builder.Configuration["TelegramBotToken"];

if (string.IsNullOrWhiteSpace(botToken))
    throw new InvalidOperationException("Токен Telegram-бота не задан в конфигурации!");

// Telegram Bot Client
builder.Services.AddSingleton<ITelegramBotClient>(
    _ => new TelegramBotClient(botToken));

// Временное хранилище заявок
builder.Services.AddSingleton<IClientRequestStorage, ClientRequestStorage>();

// Обработчики Telegram
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddSingleton<CallbackQueryHandler>();
builder.Services.AddSingleton<ITelegramUpdateHandler, TelegramUpdateRouter>();

// Hosted Service для Long Polling
builder.Services.AddHostedService<TelegramBotService>();

// Регистрация контроллеров и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Настройка HTTP-конвейера
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();