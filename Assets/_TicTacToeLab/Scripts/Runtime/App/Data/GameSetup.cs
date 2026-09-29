using System;
using BotDifficultyValue = TicTacToeLab.Runtime.BotDifficulty;

namespace TicTacToeLab.Runtime
{
    public sealed class GameSetup
    {
        public GameMode Mode { get; }
        public BotDifficultyValue? BotDifficulty { get; }

        public GameSetup(GameMode mode, BotDifficultyValue? botDifficulty)
        {
            Validate(mode, botDifficulty);
            Mode = mode;
            BotDifficulty = botDifficulty;
        }

        private static void Validate(GameMode mode, BotDifficultyValue? botDifficulty)
        {
            if (mode == GameMode.Pvp)
            {
                if (botDifficulty != null)
                {
                    throw new ArgumentException(
                        "A PvP setup cannot include bot difficulty.",
                        nameof(botDifficulty));
                }

                return;
            }

            if (mode == GameMode.Pve)
            {
                if (!botDifficulty.HasValue
                    || (botDifficulty.Value != BotDifficultyValue.Amateur
                        && botDifficulty.Value != BotDifficultyValue.Professional))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(botDifficulty),
                        botDifficulty,
                        "A PvE setup requires Amateur or Professional bot difficulty.");
                }

                return;
            }

            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "A game setup requires PvP or PvE mode.");
        }
    }
}
