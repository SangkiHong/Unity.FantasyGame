using UnityEngine;
using Sangki.Player;
using Sangki.Enemy;

namespace Sangki.States
{
    public abstract class State
    {
        protected PlayerController player;
        protected EnemyContoller enemy;
        protected StateMachine stateMachine;

        protected State(PlayerController player, StateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        protected State(EnemyContoller enemy, StateMachine stateMachine)
        {
            this.enemy = enemy;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }

        public virtual void HandleInput() { }

        public virtual void LogicUpdate() { }

        public virtual void PhysicsUpdate() { }

        public virtual void Exit() { }
    }
}
