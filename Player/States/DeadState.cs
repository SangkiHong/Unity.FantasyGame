using SK.Player;
using UnityEngine;
using EPOOutline;

namespace SK.States
{
    public class DeadState : State
    {
        private readonly Player.PlayerController _player;
        private readonly StateMachine _stateMachine;


        public DeadState(Player.PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            _player.anim.SetTrigger(Strings.AnimPara_Dead);
            Outlinable[] outlinables = _player.GetComponentsInChildren<Outlinable>();
            foreach (var outline in outlinables)
            {
                outline.enabled = false;
            }
            outlinables = null;
        }
    }
}