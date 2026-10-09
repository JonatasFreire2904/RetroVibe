using System.Text.Json;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Api.Controllers;

internal static class SurveyQuestionnaire
{
    public const int Version = 2;

    public static int RequiredRatings(string role) => role == "FACILITATOR" ? 24 : 19;

    public static int[]? Ratings(SurveyResponseRow response)
    {
        if (response.QuestionnaireVersion != Version || response.RatingsJson is null) return null;
        try
        {
            var ratings = JsonSerializer.Deserialize<int[]>(response.RatingsJson);
            return ratings?.Length == RequiredRatings(response.RespondentRole) && ratings.All(x => x is >= 1 and <= 5)
                ? ratings : null;
        }
        catch (JsonException) { return null; }
    }

    public static double SusScore(int[] ratings)
    {
        var adjusted = Enumerable.Range(0, 10)
            .Sum(i => i % 2 == 0 ? ratings[i] - 1 : 5 - ratings[i]);
        return Math.Round(adjusted * 2.5, 1);
    }

    public static double UesAverage(int[] ratings) =>
        Math.Round(ratings.Skip(10).Take(9).Average(), 1);

    public static double? TeamAverage(int[] ratings) => ratings.Length == 24
        ? Math.Round(ratings.Skip(19).Take(5).Average(), 1) : null;

    public static object Summary(IEnumerable<SurveyResponseRow> all, string role)
    {
        var responses = all.Where(r => r.RespondentRole == role)
            .Select(Ratings).Where(r => r is not null).Cast<int[]>().ToList();
        return new
        {
            count = responses.Count,
            susAverage = Average(responses.Select(SusScore)),
            uesAverage = Average(responses.Select(UesAverage)),
            teamAverage = role == "FACILITATOR"
                ? Average(responses.Select(r => TeamAverage(r)!.Value)) : null,
            questions = Enumerable.Range(1, RequiredRatings(role)).Select(number => new
            {
                number, answered = responses.Count,
                average = Average(responses.Select(r => (double)r[number - 1]))
            }).ToList()
        };
    }

    private static double? Average(IEnumerable<double> values)
    {
        var scores = values.ToList();
        return scores.Count == 0 ? null : Math.Round(scores.Average(), 1);
    }
}
