using AutoServiceBot.Entities;

namespace AutoServiceBot.Services;

/// <summary>
/// Интерфейс временного хранилища заявок клиентов.
/// </summary>
public interface IClientRequestStorage
{
    /// <summary>
    /// Получение заявки клиента или создание новой.
    /// </summary>
    /// <param name="chatId">Идентификатор чата клиента.</param>
    ClientRequest GetOrCreate(long chatId);

    /// <summary>
    /// Удаление временной заявки клиента.
    /// </summary>
    /// <param name="chatId">Идентификатор чата клиента.</param>
    void Remove(long chatId);
}