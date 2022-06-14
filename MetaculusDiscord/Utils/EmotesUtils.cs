using Discord;

namespace MetaculusDiscord.Utils;

public static class EmotesUtils
{
    private static Dictionary<int,Emoji> EmojiDict =
          new Dictionary<int, Emoji>(){
                {1,new Emoji("1️⃣")},
                {2,new Emoji("2️⃣")},
                {3,new Emoji("3️⃣")},
                {4,new Emoji("4️⃣")},
                {5,new Emoji("5️⃣")}};
    public static Dictionary<int,Emoji> GetEmojiNumbersDict() => EmojiDict;
    
}