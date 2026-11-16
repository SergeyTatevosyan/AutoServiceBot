namespace AutoServiceBot.Entities.Enums;

/// <summary>
/// Этапы заполнения заявки клиента.
/// </summary>
public enum ClientRequestStep
{
    /// <summary>
    /// Выбор услуги автосервиса.
    /// </summary>
    SelectService = 0,

    /// <summary>
    /// Ввод марки автомобиля.
    /// </summary>
    EnterCarBrand = 1,

    /// <summary>
    /// Ввод модели автомобиля.
    /// </summary>
    EnterCarModel = 2,

    /// <summary>
    /// Ввод года выпуска автомобиля.
    /// </summary>
    EnterCarYear = 3,

    /// <summary>
    /// Ввод описания неисправности.
    /// </summary>
    EnterProblemDescription = 4,

    /// <summary>
    /// Ввод имени клиента.
    /// </summary>
    EnterClientName = 5,

    /// <summary>
    /// Получение номера телефона клиента.
    /// </summary>
    EnterPhoneNumber = 6
}