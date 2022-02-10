﻿using UnityEngine;

namespace Sangki.States
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }

        public void Initialize(State startingState)
        {
            CurrentState = startingState;
            startingState.Enter();
        }

        public void ChangeState(State newState)
        {
            if (CurrentState == newState) return;

            CurrentState.Exit();

            CurrentState = newState;
            newState.Enter();
            Player.PlayerController.Instance.CurrentState = newState.ToString();
        }
    }
}