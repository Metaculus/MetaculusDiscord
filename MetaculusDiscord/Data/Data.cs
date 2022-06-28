using MetaculusDiscord.Model;
using Microsoft.EntityFrameworkCore;

namespace MetaculusDiscord.Data;

/// <summary>
/// Responsible for interaction with the database and short term storage.
/// </summary>
public class Data
{
    /// <summary>
    /// Using separate DbContext for each query enables concurrency.
    /// </summary>
    private readonly IDbContextFactory<MetaculusContext> _contextFactory;

    /// <summary>
    /// Short term storage of links that can be sent when a message with search results is selected.
    /// <remarks>todo: If the bot were used by a large amount of users this dictionary would have to get cleaned up,
    /// from time to time. But for now it's not a problem and each restart fixes it.</remarks>
    /// </summary>
    private readonly Dictionary<ulong, ResponseLinks> _responses;

    public Data()
    {
        _responses = new Dictionary<ulong, ResponseLinks>();
        _contextFactory = new MetaculusContext.MetaculusContextFactory();
    }

    /// <summary>
    /// Adds an alert to the corresponding table in the database. If the alert already exists, returns false.
    /// </summary>
    /// <param name="alert">The alert that should be added.</param>
    /// <typeparam name="TAlert">Type of alert that should be added.</typeparam>
    /// <returns>Whether the adding was successful</returns>
    public async Task<bool> TryAddAlertAsync<TAlert>(TAlert alert) where TAlert : Alert
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (alert is UserQuestionAlert userAlert)
        {
            if (db.UserQuestionAlerts.Any(a => a.UserId == userAlert.UserId && a.QuestionId == userAlert.QuestionId))
                return false;
            db.UserQuestionAlerts.Add(userAlert);
        }
        else if (alert is ChannelQuestionAlert channelAlert)
        {
            if (db.ChannelQuestionAlerts.Any(a =>
                    a.ChannelId == channelAlert.ChannelId && a.QuestionId == channelAlert.QuestionId))
                return false;
            db.ChannelQuestionAlerts.Add(channelAlert);
        }
        else if (alert is ChannelCategoryAlert categoryAlert)
        {
            if (db.CategoryChannelAlerts.Any(a =>
                    a.CategoryId == categoryAlert.CategoryId && a.ChannelId == categoryAlert.ChannelId))
                return false;
            db.CategoryChannelAlerts.Add(categoryAlert);
        }
        else
        {
            return false;
        }

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Removes an alert from the corresponding table in the database. If th alert was not present, returns false.
    /// </summary>
    /// <param name="alert">The alert that should be removed.</param>
    /// <typeparam name="TAlert">Type of alert that should be removed.</typeparam>
    /// <returns>Whether the removal was successful</returns>
    public async Task<bool> TryRemoveAlertAsync<TAlert>(TAlert alert) where TAlert : Alert
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (alert is UserQuestionAlert userQuestionAlert)
        {
            var dbAlert =
                db.UserQuestionAlerts.FirstOrDefault(a =>
                    a.UserId == userQuestionAlert.UserId && a.QuestionId == userQuestionAlert.QuestionId);
            if (dbAlert is null) return false;

            db.UserQuestionAlerts.Remove(dbAlert);
        }
        else if (alert is ChannelQuestionAlert channelQuestionAlert)
        {
            var dbAlert =
                db.ChannelQuestionAlerts.FirstOrDefault(a =>
                    a.ChannelId == channelQuestionAlert.ChannelId && a.QuestionId == channelQuestionAlert.QuestionId);
            if (dbAlert is null) return false;

            db.ChannelQuestionAlerts.Remove(dbAlert);
        }
        else if (alert is ChannelCategoryAlert categoryChannelAlert)
        {
            var dbAlert =
                db.CategoryChannelAlerts.FirstOrDefault(a =>
                    a.CategoryId == categoryChannelAlert.CategoryId && a.ChannelId == categoryChannelAlert.ChannelId);
            if (dbAlert is null) return false;

            db.CategoryChannelAlerts.Remove(dbAlert);
        }
        else
        {
            return false;
        }

        await db.SaveChangesAsync();
        return true;
    }

    /// <typeparam name="TAlert">Alert</typeparam>
    /// <returns>All alerts from the corresponding table.</returns>
    public async Task<IEnumerable<TAlert>> GetAllAlertsAsync<TAlert>() where TAlert : Alert
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (typeof(TAlert) == typeof(UserQuestionAlert))
            return (IEnumerable<TAlert>) context.UserQuestionAlerts.ToList();
        if (typeof(TAlert) == typeof(ChannelQuestionAlert))
            return (IEnumerable<TAlert>) context.ChannelQuestionAlerts.ToList();
        if (typeof(TAlert) == typeof(ChannelCategoryAlert))
            return (IEnumerable<TAlert>) context.CategoryChannelAlerts.ToList();
        throw new ArgumentException(
            "TAlert must be either UserQuestionAlert or ChannelQuestionAlert or ChannelCategoryAlert");
    }

    /// <summary>
    /// Removes a list of alerts from the database.
    /// </summary>
    /// <typeparam name="TAlert">Alert</typeparam>
    public async Task RemoveAlerts<TAlert>(IEnumerable<TAlert> alerts) where TAlert : Alert
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (alerts is IEnumerable<UserQuestionAlert> userAlerts)
            context.UserQuestionAlerts.RemoveRange(userAlerts);
        else if (alerts is IEnumerable<ChannelQuestionAlert> channelAlerts)
            context.ChannelQuestionAlerts.RemoveRange(channelAlerts);
        else if (alerts is IEnumerable<ChannelCategoryAlert> categoryAlerts)
            context.CategoryChannelAlerts.RemoveRange(categoryAlerts);
        else
            throw new ArgumentException("TAlert must be either UserQuestionAlert or ChannelQuestionAlert");
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves response links.
    /// </summary>
    /// <param name="response">ResponseLinks to be saved</param>
    /// <returns>Whether adding was successful</returns>
    public bool TryAddResponse(ResponseLinks response)
    {
        return _responses.TryAdd(response.Id, response);
    }

    /// <summary>
    /// Retrieves response links from message id.
    /// </summary>
    /// <returns></returns>
    public bool TryGetResponse(ulong id, out ResponseLinks response)
    {
        return _responses.TryGetValue(id, out response!);
    }
}