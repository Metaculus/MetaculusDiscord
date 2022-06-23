using MetaculusDiscord.Model;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace MCDTests;

public class ParsingTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private HttpClient client = new HttpClient();

    public ParsingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task AlertQuestionParseTest()
    {
        
        // numeric unresolved
        var response00 = client.GetStringAsync("https://www.metaculus.com/api2/questions/9450");
        // numeric resolved
        var response01 = client.GetStringAsync("https://www.metaculus.com/api2/questions/402");
        // date ambiguous resolved
        var response10 = client.GetStringAsync("https://www.metaculus.com/api2/questions/9459");
        // date unresolved, >1 prediction edge case
        var response11 = client.GetStringAsync("https://www.metaculus.com/api2/questions/5237");
        // date resolved
        var response12 = client.GetStringAsync("https://www.metaculus.com/api2/questions/9719");
        // binrary unresolved
        var response20 = client.GetStringAsync("https://www.metaculus.com/api2/questions/5265");
        // binary resolved
        var response21 = client.GetStringAsync("https://www.metaculus.com/api2/questions/602");
        
        // convert to dynamic json (can't use normal parsing because API is inconsistent)
        var response00Json = JsonConvert.DeserializeObject<dynamic>(await response00);
        var response01Json = JsonConvert.DeserializeObject<dynamic>(await  response01);
        var response10Json = JsonConvert.DeserializeObject<dynamic>(await response10);
        var response11Json = JsonConvert.DeserializeObject<dynamic>(await response11);
        var response12Json = JsonConvert.DeserializeObject<dynamic>(await response12);
        var response20Json = JsonConvert.DeserializeObject<dynamic>(await response20);
        var response21Json = JsonConvert.DeserializeObject<dynamic>(await response21);
        
        // parse as AlertQuestion
        // this should not throw an exception
        var response00AlertQuestion = new AlertQuestion(response00Json);
        var response01AlertQuestion = new AlertQuestion(response01Json);
        var response10AlertQuestion = new AlertQuestion(response10Json);
        var response11AlertQuestion = new AlertQuestion(response11Json);
        var response12AlertQuestion = new AlertQuestion(response12Json);
        var response20AlertQuestion = new AlertQuestion(response20Json);
        var response21AlertQuestion = new AlertQuestion(response21Json);

        _testOutputHelper.WriteLine(response11AlertQuestion.Value.ToString());
        // Assert.Equal(1, response00AlertQuestion.Value);
    }
}