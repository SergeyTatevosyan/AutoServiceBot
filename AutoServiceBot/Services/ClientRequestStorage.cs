using AutoServiceBot.Entities;
using System.Collections.Concurrent;

namespace AutoServiceBot.Services;

/// <summary>
/// Временное хранилище заявок клиентов в оперативной памяти.
/// </summary>
public sealed class ClientRequestStorage : IClientRequestStorage
{
    private readonly ConcurrentDictionary<long, ClientRequest> _requests = new();

    /// <inheritdoc/>
    public ClientRequest GetOrCreate(long chatId)
    {
        return _requests.GetOrAdd(
            chatId,
            id => new ClientRequest
            {
                ChatId = id
            });
    }

    /// <inheritdoc/>
    public void Remove(long chatId)
    {
        _requests.TryRemove(chatId, out _);
    }
}