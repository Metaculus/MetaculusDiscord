using MetaculusDiscord.Model;
using Microsoft.EntityFrameworkCore;

namespace MetaculusDiscord.Data;

public class Data
{
    // using separate dbcontext for each query
    private readonly IDbContextFactory<MetaculusContext> _contextFactory;
    private readonly Dictionary<ulong, ResponseLinks> _responses;


    public Data()
    {
        _responses = new Dictionary<ulong, ResponseLinks>();
        _contextFactory = new MetaculusContext.MetaculusContextFactory();
    }

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

    public async Task<IEnumerable<TAlert>> GetAllAlertsAsync<TAlert>() where TAlert : Alert 
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        if (typeof(TAlert) == typeof(UserQuestionAlert))
            return (IEnumerable<TAlert>) context.UserQuestionAlerts.ToList();
        if (typeof(TAlert) == typeof(ChannelQuestionAlert))
            return (IEnumerable<TAlert>) context.ChannelQuestionAlerts.ToList();
        if (typeof(TAlert) == typeof(ChannelCategoryAlert))
            return (IEnumerable<TAlert>) context.CategoryChannelAlerts.ToList();
        throw new ArgumentException("TAlert must be either UserQuestionAlert or ChannelQuestionAlert");
    }

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


    public bool TryAddResponse(ResponseLinks response)
    {
        return _responses.TryAdd(response.Id, response);
    }

    public bool TryRemoveResponse(ResponseLinks response)
    {
        return _responses.Remove(response.Id, out _);
    }

    public bool TryGetResponse(ulong id, out ResponseLinks response)
    {
        return _responses.TryGetValue(id, out response!);
    }
}