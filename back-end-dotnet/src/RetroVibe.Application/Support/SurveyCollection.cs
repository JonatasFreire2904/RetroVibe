namespace RetroVibe.Application.Support;

public static class SurveyCollection
{
    public static bool IsEnabled => !string.Equals(
        Environment.GetEnvironmentVariable("SURVEY_COLLECTION_ENABLED"), "false", StringComparison.OrdinalIgnoreCase);
}
