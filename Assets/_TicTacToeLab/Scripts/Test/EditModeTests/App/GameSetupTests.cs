using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameSetupTests
    {
        [Test]
        public void PvP_is_a_setup_without_a_bot_difficulty()
        {
            GameSetup sut = new(GameMode.Pvp, null);

            Assert.That(sut.Mode, Is.EqualTo(GameMode.Pvp));
            Assert.That(sut.BotDifficulty, Is.Null);
        }

        [TestCase(BotDifficulty.Amateur)]
        [TestCase(BotDifficulty.Professional)]
        public void PvE_is_a_setup_carrying_the_chosen_bot_difficulty(BotDifficulty botDifficulty)
        {
            GameSetup sut = new(GameMode.Pve, botDifficulty);

            Assert.That(sut.Mode, Is.EqualTo(GameMode.Pve));
            Assert.That(sut.BotDifficulty, Is.EqualTo(botDifficulty));
        }

        [Test]
        public void PvE_rejects_a_bot_difficulty_outside_Amateur_and_Professional()
        {
            Assert.That(
                () => new GameSetup(GameMode.Pve, (BotDifficulty)(-1)),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "PvE must reject a bot difficulty outside Amateur and Professional.");
        }

        [Test]
        public void PvP_rejects_a_bot_difficulty()
        {
            Assert.That(
                () => new GameSetup(GameMode.Pvp, BotDifficulty.Amateur),
                Throws.TypeOf<ArgumentException>(),
                "PvP must reject a bot difficulty.");
        }

        [Test]
        public void PvE_requires_a_bot_difficulty()
        {
            Assert.That(
                () => new GameSetup(GameMode.Pve, null),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "PvE must require a bot difficulty.");
        }

        [Test]
        public void An_undefined_game_mode_is_rejected()
        {
            Assert.That(
                () => new GameSetup((GameMode)(-1), null),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "A setup must use a defined game mode.");
        }
    }
}
