using Discord;

namespace MetaculusDiscord.Utils;

/// <summary>
/// Static class holding emoji for numbers and 
/// </summary>
public static class EmotesUtils
{
    public static Dictionary<int, Emoji> NumberEmoji { get; } =
        new()
        {
            {1, new Emoji("1️⃣")},
            {2, new Emoji("2️⃣")},
            {3, new Emoji("3️⃣")},
            {4, new Emoji("4️⃣")},
            {5, new Emoji("5️⃣")}
        };

    /// <summary>
    /// Puts number emoji on message.
    /// </summary>
    /// <param name="message">The message to be decorated.</param>
    /// <param name="count">With how many emoji</param>
    public static async void NumberDecorate(IUserMessage message, int count)
    {
        if (count > NumberEmoji.Count)
            throw new ArgumentException("You can't decorate message with more emotes than available");
        await message.AddReactionsAsync(NumberEmoji.Values.Take(count));
    }

    /// <param name="num">change</param>
    /// <returns>Arrow emoji depending on the sign of *num*</returns>
    public static string SignEmote(double num)
    {
        return num > 0 ? "⬆️" : "⬇️";
    }
}