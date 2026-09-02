using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class AppStateMachineTests
    {
        private abstract class SpyState : IAppState
        {
            protected readonly List<string> Log;

            protected SpyState(List<string> log)
            {
                Log = log;
            }

            public void Enter()
            {
                Log.Add($"{GetType().Name}.Enter");
            }

            public void Leave()
            {
                Log.Add($"{GetType().Name}.Leave");
            }
        }

        private sealed class FirstSpyState : SpyState
        {
            public FirstSpyState(List<string> log) : base(log)
            {
            }
        }

        private sealed class SecondSpyState : SpyState
        {
            public SecondSpyState(List<string> log) : base(log)
            {
            }
        }

        [Test]
        public void Changing_state_leaves_the_current_state_before_entering_the_next()
        {
            List<string> log = new();
            AppStateMachine stateMachine = new();
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.Add(new SecondSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            stateMachine.ChangeState<SecondSpyState>();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave", "SecondSpyState.Enter" }));
        }

        [Test]
        public void Disposing_the_machine_leaves_the_current_state()
        {
            List<string> log = new();
            AppStateMachine stateMachine = new();
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            stateMachine.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));
        }

        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private BoardSession _boardSession;
        private FakeCoroutineService _fakeCoroutineService;
        private AppStateMachine _stateMachine;
        private GameplayState _gameplayState;
        private GameCompleteState _gameCompleteState;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            BoardPresenter boardPresenter = new(
                _boardModel, _fakeBoardView, factoryService: null,
                _fakeInputService, new FakeCameraService(), new FakeLogService());
            _boardSession = new BoardSession(boardPresenter);
            _fakeCoroutineService = new FakeCoroutineService();
            _stateMachine = new AppStateMachine();
            _gameplayState = new GameplayState(_boardSession, _stateMachine);
            _gameCompleteState = new GameCompleteState(_boardSession, _stateMachine, _fakeCoroutineService);
            _stateMachine.Add(_gameplayState);
            _stateMachine.Add(_gameCompleteState);
        }

        [Test]
        public void The_gameplay_state_unsubscribes_from_the_session_ending_on_exit()
        {
            _gameplayState.Enter();
            _gameplayState.Leave();

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(_fakeBoardView.WasCleared, Is.False);
            Assert.That(_boardModel.Outcome, Is.Not.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void The_gameplay_state_enters_the_game_complete_state_when_the_ending_is_announced()
        {
            _stateMachine.ChangeState<GameplayState>();

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.PumpToCompletion();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void Re_entering_gameplay_after_each_game_does_not_accumulate_subscriptions()
        {
            _stateMachine.ChangeState<GameplayState>();

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.PumpToCompletion();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.PumpToCompletion();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.PumpToCompletion();

            // An extra, leftover subscription from a prior game would clear the board more than once per ending.
            Assert.That(_fakeBoardView.ClearCount, Is.EqualTo(3));
        }

        [Test]
        public void Many_games_in_a_row_leave_exactly_one_active_subscription()
        {
            _stateMachine.ChangeState<GameplayState>();

            for (int i = 0; i < 5; i++)
            {
                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.PumpToCompletion();
            }

            _boardSession.Dispose();

            Assert.That(_fakeInputService.HasSubscribers, Is.False);
        }

        [Test]
        public void Tearing_down_the_machine_leaves_gameplay_so_a_subsequent_ending_reaches_nothing()
        {
            _stateMachine.ChangeState<GameplayState>();

            _stateMachine.Dispose();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(_fakeBoardView.WasCleared, Is.False);
            Assert.That(_boardModel.Outcome, Is.Not.EqualTo(Outcome.InProgress));
        }
    }
}
