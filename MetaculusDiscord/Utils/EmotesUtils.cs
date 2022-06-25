using Discord;

namespace MetaculusDiscord.Utils;

public static class EmotesUtils
{
    public static readonly Dictionary<int, Emoji> EmojiDict =
        new()
        {
            {1, new Emoji("1️⃣")},
            {2, new Emoji("2️⃣")},
            {3, new Emoji("3️⃣")},
            {4, new Emoji("4️⃣")},
            {5, new Emoji("5️⃣")}
        };

    public static Dictionary<int, Emoji> GetEmojiNumbersDict()
    {
        return EmojiDict;
    }

    public static async void Decorate(IUserMessage message, int count)
    {
        if (count > EmojiDict.Count)
            throw new ArgumentException("You can't decorate message with more emotes than available");
        await message.AddReactionsAsync(EmojiDict.Values.Take(count));
    }

    public static string SignEmote(double num)
    {
        if (num > 0)
            return "⬆️";
        return "⬇️";
    }
}