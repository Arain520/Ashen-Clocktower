public static class NarrativePlaybackFlags
{
    private static bool shouldPlayNewGameIntro;

    public static void RequestNewGameIntro()
    {
        shouldPlayNewGameIntro = true;
    }

    public static bool ConsumeNewGameIntroRequest()
    {
        if (!shouldPlayNewGameIntro)
            return false;

        shouldPlayNewGameIntro = false;
        return true;
    }
}
