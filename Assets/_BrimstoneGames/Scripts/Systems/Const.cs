

namespace _DPS
{
    public static class Const
    {
        // Make sure none of the labels contain another label inside the string 
        // avoid situations like "SCORE_TXT" and "HIGHSCORE_TXT" because the last
        // will be called as "SCORE_TXT" instead
        public const string WinStreakTxt = "WIN_STREAK_TXT";
        public const string BonusLifeTxt = "BONUS_TXT";
        public const string ContextDelayTxt = "CONTEXT_DELAY";
        public const string ContextDurationTxt = "CONTEXT_DURATION";
        public const string InstructionsTxt = "INSTRUCTIONS";
        public const string GameNameTxt = "GAME_NAME_TXT";
        public const string ScoreWin1 = "SCORE_WIN";
        public const string ScoreLoss1 = "SCORE_LOSS";
        public const string ScoreTxt = "SCORE_TXT";
        public const string HighScoreTxt = "SCORE_TOP_TXT";
        public const string HighScoreEndTxt = "SCORE_TOP_TXT_END";
    }
}
