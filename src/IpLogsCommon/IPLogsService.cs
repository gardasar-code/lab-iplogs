using System.Runtime.CompilerServices;
using GardasarCode.Repository.Interfaces;
using IpLogsCommon.Interfaces;
using IpLogsCommon.Models;
using IpLogsCommon.Repository.Entities;
using IpLogsCommon.Repository.Specifications;
using Microsoft.Extensions.Logging;

namespace IpLogsCommon;

public class IPLogsService(IRepository repo, ICache cache, ILoggerFactory loggerFactory)
    : IIPLogsService
{
    private readonly ILogger<IPLogsService>? _logger = loggerFactory.CreateLogger<IPLogsService>();

    /// <inheritdoc />
    public async Task AddConnectionAsync(long userId, string ipAddress, DateTime eventTime,
        CancellationToken cancellationToken = default)
    {
        var newUser = new User
        {
            Id = userId,
            IPAddress = ipAddress,
            LastConnectionTime = eventTime.ToUniversalTime()
        };

        var existingUser = await repo.FirstOrDefaultAsync(new UserSpecification.GetUserById(newUser.Id, false),
            cancellationToken);

        if (existingUser != null)
            repo.SetValues(existingUser, newUser);
        else
            await repo.AddAsync(newUser, cancellationToken).ConfigureAwait(false);

        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await cache.RemoveAsync($"{userId}_{nameof(GetLastConnectionAsync)}").ConfigureAwait(false);
        await cache.RemoveAsync($"{userId}_{nameof(GetUserIPsStream)}").ConfigureAwait(false);

        _logger?.LogInformation("User connection added/updated successfully for userId: {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task<UserLastConnection> GetLastConnectionAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{userId}_{nameof(GetLastConnectionAsync)}";

        var (cached, userLastConnection) = await cache.TryGetValueAsync<UserLastConnection>(cacheKey).ConfigureAwait(false);
        if (cached) return userLastConnection ?? new UserLastConnection();

        var user = await repo.FirstOrDefaultAsync(new UserSpecification.GetUserById(userId, true),
            cancellationToken).ConfigureAwait(false);

        userLastConnection = new UserLastConnection
            { LastConnectionTime = user?.LastConnectionTime, IPAddress = user?.IPAddress };
        await cache.SetAsync(cacheKey, userLastConnection).ConfigureAwait(false);

        if (user == null)
            _logger?.LogWarning("User not found for userId: {UserId}", userId);
        else
            _logger?.LogWarning("User found for userId: {UserId}", userId);

        return userLastConnection;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> GetUserIPsStream(long userId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{userId}_{nameof(GetUserIPsStream)}";

        var (cached, cachedIps) = await cache.TryGetValueAsync<List<string>>(cacheKey).ConfigureAwait(false);

        if (!cached)
        {
            cachedIps = [];

            await foreach (var ip in repo.AsAsyncEnumerableStream(
                                   new UserSpecification.GetUserIpsById(userId, true), cancellationToken)
                               .ConfigureAwait(false))
            {
                cachedIps.Add(ip);
                yield return ip;
            }

            await cache.SetAsync(cacheKey, cachedIps).ConfigureAwait(false);
        }
        else
        {
            if (cachedIps == null) yield break;

            foreach (var ip in cachedIps) yield return ip;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<long> FindUsersByIpPartStream(string ipPart,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ipPart}_{nameof(FindUsersByIpPartStream)}";

        var (cached, cachedIps) = await cache.TryGetValueAsync<List<long>>(cacheKey).ConfigureAwait(false);

        if (!cached)
        {
            cachedIps = [];

            await foreach (var id in repo.AsAsyncEnumerableStream(
                               new UserSpecification.FindUsersByIpPart(ipPart, true),
                               cancellationToken).ConfigureAwait(false))
            {
                cachedIps.Add(id);
                yield return id;
            }

            await cache.SetAsync(cacheKey, cachedIps).ConfigureAwait(false);
        }
        else
        {
            if (cachedIps == null) yield break;

            foreach (var id in cachedIps) yield return id;
        }
    }
}