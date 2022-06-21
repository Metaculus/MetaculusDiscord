using MetaculusDiscord.Model;
using Microsoft.EntityFrameworkCore;

namespace MetaculusDiscord.Data;

public class Data
{
    private Dictionary<ulong, ResponseLinks> Responses { get; }

    // using separate dbcontext for each query
    private readonly IDbContextFactory<MetaculusContext> _contextFactory;
    public bool TryAddUserQuestionAlert(UserQuestionAlert alert)
    {
        using var db = _contextFactory.CreateDbContext();
        if (db.UserQuestionAlerts.Any(a => a.UserId == alert.UserId && a.QuestionId == alert.QuestionId))
        {
            return false;
        }
        else
        {
            db.UserQuestionAlerts.Add(alert);
            db.SaveChanges();
            return true;
        }
    }

    public bool TryAddChannelQuestionAlert(ChannelQuestionAlert alert)
    {
        using var db = _contextFactory.CreateDbContext();
        if (db.ChannelQuestionAlerts.Any(a => a.ChannelId == alert.ChannelId && a.QuestionId == alert.QuestionId))
            return false;
        db.ChannelQuestionAlerts.Add(alert);
        db.SaveChanges();
        return true;
    }

    public bool TryRemoveUserQuestionAlert(UserQuestionAlert alert)
    {
        using var db = _contextFactory.CreateDbContext();
        UserQuestionAlert? dbAlert = db.UserQuestionAlerts.FirstOrDefault(a => a.UserId == alert.UserId && a.QuestionId == alert.QuestionId);
        if (dbAlert is null) return false;
        
        db.UserQuestionAlerts.Remove(dbAlert);
        db.SaveChanges();
        return true;

    }

    public bool TryRemoveChannelQuestionAlert(ChannelQuestionAlert alert)
    {
        using var db = _contextFactory.CreateDbContext();
        ChannelQuestionAlert? dbAlert = db.ChannelQuestionAlerts.FirstOrDefault(a => a.ChannelId == alert.ChannelId && a.QuestionId == alert.QuestionId);
        if (dbAlert is null) return false;
        
        db.ChannelQuestionAlerts.Remove(dbAlert);
        db.SaveChanges();
        return true;

    }

    public IEnumerable<UserQuestionAlert> AllUserQuestionAlerts()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.UserQuestionAlerts.ToList();
    }

    public IEnumerable<ChannelQuestionAlert> AllChannelQuestionAlerts()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.ChannelQuestionAlerts.ToList();
    }


    public bool TryAddResponse(ResponseLinks response)
    {
        return Responses.TryAdd(response.Id, response);
    }

    public bool TryRemoveResponse(ResponseLinks response)
    {
        return Responses.Remove(response.Id, out _);
    }

    public bool TryGetResponse(ulong id, out ResponseLinks response)
    {
        return Responses.TryGetValue(id, out response!);
    }


    public Data()
    {
        Responses = new Dictionary<ulong, ResponseLinks>();
        _contextFactory = new MetaculusContext.MetaculusContextFactory();
    }
}