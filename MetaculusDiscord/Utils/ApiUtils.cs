using MetaculusDiscord.Model;
using Newtonsoft.Json;

namespace MetaculusDiscord.Utils;

public class ApiUtils
{
    public static async Task<AlertQuestion?> GetAlertQuestionFromIdAsync(long id)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.metaculus.com/api2/questions/{id}");
            // deserialize to dynamic
            var dynamicQuestion = JsonConvert.DeserializeObject<dynamic?>(response);
            if (dynamicQuestion == null)
                return null;
            return new AlertQuestion(dynamicQuestion);
        }
        catch (Exception)
        {
            return null;
        }
    }
}