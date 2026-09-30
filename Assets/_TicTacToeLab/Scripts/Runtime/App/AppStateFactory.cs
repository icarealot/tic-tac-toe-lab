using System;

namespace TicTacToeLab.Runtime
{
    internal sealed class AppStateFactory
    {
        private readonly AppStateMachine _stateMachine;
        private readonly BoardPresenter _boardPresenter;
        private readonly IAppUI _appUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;
        private readonly IRandomService _randomService;

        public AppStateFactory(
            AppStateMachine stateMachine,
            BoardPresenter boardPresenter,
            IAppUI appUI,
            IDelayScheduler delayScheduler,
            IInputService inputService,
            IRandomService randomService)
        {
            _stateMachine = stateMachine;
            _boardPresenter = boardPresenter;
            _appUI = appUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
            _randomService = randomService;
        }

        public IAppState CreateHomeState()
        {
            return new HomeAppState(_stateMachine, this, _appUI, _inputService);
        }

        public IAppState CreateBotSelectionState()
        {
            return new BotSelectionAppState(_stateMachine, this, _appUI, _inputService);
        }

        public IAppState CreateGameplayState(GameSetup setup)
        {
            IBot bot = setup.Mode == GameMode.Pve
                ? CreateBot(setup.BotDifficulty.Value)
                : null;

            return new GameplayAppState(
                _stateMachine,
                this,
                _boardPresenter,
                _appUI,
                _delayScheduler,
                _inputService,
                setup,
                bot,
                _randomService);
        }

        public IAppState CreateOutcomeState(Outcome outcome, GameSetup setup)
        {
            return new OutcomeAppState(_stateMachine, this, _appUI, _delayScheduler, _inputService, outcome, setup);
        }

        private IBot CreateBot(BotDifficulty botDifficulty)
        {
            return botDifficulty switch
            {
                BotDifficulty.Amateur => new AmateurBot(_randomService),
                BotDifficulty.Professional => new ProfessionalBot(_randomService),
                _ => throw new ArgumentOutOfRangeException(nameof(botDifficulty), botDifficulty, "The gameplay state requires a supported bot difficulty."),
            };
        }
    }
}
