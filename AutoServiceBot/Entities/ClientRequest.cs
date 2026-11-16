using AutoServiceBot.Entities.Enums;

namespace AutoServiceBot.Entities;

/// <summary>
/// Заявка клиента на обслуживание автомобиля.
/// Используется для временного хранения ответов в процессе заполнения.
/// </summary>
public sealed class ClientRequest
{
    /// <summary>
    /// Идентификатор чата клиента в Telegram.
    /// </summary>
    public long ChatId { get; init; }

    /// <summary>
    /// Текущий этап заполнения заявки.
    /// </summary>
    public ClientRequestStep Step { get; set; }

    /// <summary>
    /// Выбранная услуга автосервиса.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Марка автомобиля.
    /// </summary>
    public string? CarBrand { get; set; }

    /// <summary>
    /// Модель автомобиля.
    /// </summary>
    public string? CarModel { get; set; }

    /// <summary>
    /// Год выпуска автомобиля.
    /// </summary>
    public int? CarYear { get; set; }

    /// <summary>
    /// Описание неисправности или необходимых работ.
    /// </summary>
    public string? ProblemDescription { get; set; }

    /// <summary>
    /// Имя клиента.
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// Контактный номер телефона клиента.
    /// </summary>
    public string? PhoneNumber { get; set; }
}