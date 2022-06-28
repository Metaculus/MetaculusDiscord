using MetaculusDiscord.Model;
using Newtonsoft.Json;

namespace MetaculusDiscord.Utils;

/// <summary>
/// Static class holding methods that are used when we want to query the API without using the internal state.
/// </summary>
public static class ApiUtils
{
    /// <summary>
    /// Queries the API and calls the constructor of the AlertQuestion.
    /// </summary>
    /// <param name="id">id of the question</param>
    /// <returns>Returns inside a task, the question with the given id. If there is an error returns null.</returns>
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

    /// <param name="categoryId">string id of the category</param>
    /// <returns>Task containing whether category with this id exists.</returns>
    public static async Task<bool> IsCategoryValid(string categoryId)
    {
        using var client = new HttpClient();
        try
        {
            var response = await client.GetStringAsync("https://www.metaculus.com/api2/categories/" + categoryId);
        }
        catch (HttpRequestException)
        {
            return false;
        }

        return true;
    }
}