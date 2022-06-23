using MetaculusDiscord.Model;
using Microsoft.EntityFrameworkCore;

namespace MetaculusDiscord.Data;

public class Data
{
    private readonly Dictionary<ulong, ResponseLinks> _responses;

    // using separate dbcontext for each query
    private readonly IDbContextFactory<MetaculusContext> _contextFactory;

    public async Task<bool> TryAddUserQuestionAlertAsync(UserQuestionAlert alert)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (db.UserQuestionAlerts.Any(a => a.UserId == alert.UserId && a.QuestionId == alert.QuestionId))
        {
            return false;
        }
        else
        {
            db.UserQuestionAlerts.Add(alert);
            await db.SaveChangesAsync();
            return true;
        }
    }

    public async Task<bool> TryAddChannelQuestionAlertAsync(ChannelQuestionAlert alert)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (db.ChannelQuestionAlerts.Any(a => a.ChannelId == alert.ChannelId && a.QuestionId == alert.QuestionId))
            return false;
        db.ChannelQuestionAlerts.Add(alert);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryRemoveUserQuestionAlertAsync(UserQuestionAlert alert)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var dbAlert =
            db.UserQuestionAlerts.FirstOrDefault(a => a.UserId == alert.UserId && a.QuestionId == alert.QuestionId);
        if (dbAlert is null) return false;

        db.UserQuestionAlerts.Remove(dbAlert);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryRemoveChannelQuestionAlertAsync(ChannelQuestionAlert alert)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var dbAlert =
            db.ChannelQuestionAlerts.FirstOrDefault(a =>
                a.ChannelId == alert.ChannelId && a.QuestionId == alert.QuestionId);
        if (dbAlert is null) return false;

        db.ChannelQuestionAlerts.Remove(dbAlert);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserQuestionAlert>> GetAllUserQuestionAlertsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return context.UserQuestionAlerts.ToList();
    }

    public async Task<IEnumerable<ChannelQuestionAlert>> GetAllChannelQuestionAlerts()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return context.ChannelQuestionAlerts.ToList();
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


    public Data()
    {
        _responses = new Dictionary<ulong, ResponseLinks>();
        _contextFactory = new MetaculusContext.MetaculusContextFactory();
    }
}