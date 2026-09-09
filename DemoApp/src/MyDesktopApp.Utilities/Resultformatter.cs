namespace MyDesktopApp.Utilities;

/// <summary>
/// Small formatting helper, kept separate from the Core calculation logic
/// so display/formatting concerns don't mix with business logic.
/// </summary>
public static class ResultFormatter
{
    public static string FormatResult(double value)
    {
        return $"Result: {value}";
    }

    public static string FormatError(string message)
    {
        return $"Result: {message}";
    }
}