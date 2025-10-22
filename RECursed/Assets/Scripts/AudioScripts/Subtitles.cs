public static class Subtitles
{
    public static void Show(string text) =>
        SubtitleUI.Instance?.Show(text);

    public static void ShowFor(string text, float seconds) =>
        SubtitleUI.Instance?.ShowForSeconds(text, seconds);

    public static void Hide() =>
        SubtitleUI.Instance?.Hide();
}
