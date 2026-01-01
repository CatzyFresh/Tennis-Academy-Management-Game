using System.Collections.Generic;

namespace TennisAcademyManager.Core
{
    public class GameStateMachine
    {
        private readonly Dictionary<System.Type, IGameState> states = new();
        private IGameState currentState;

        public void RegisterState<T>(T state) where T : IGameState
        {
            states[typeof(T)] = state;
        }

        public void ChangeState<T>() where T : IGameState
        {
            currentState?.Exit();
            currentState = states[typeof(T)];
            currentState.Enter();
        }
    }
}
