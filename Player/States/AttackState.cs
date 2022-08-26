using SK.Player;
using UnityEngine;

namespace SK.States
{
    public class AttackState : State
    {
        private readonly Player.PlayerController _player;
        private readonly StateMachine _stateMachine;

        private float intervalTimer, _chargeTime, chargeTimer;
        private bool isAttack, isCharged, isChargingStart, isCharging;

        public AttackState(Player.PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _chargeTime = _player.playerData.chargeTime;
        }

        public override void Enter()
        {
            base.Enter();

            if (!isAttack)
            {
                isAttack = true;
                intervalTimer = 0;

                // Normal Attack
                if (!isCharged) NormalAttack();
                // 360 degree Attack
                else
                {
                    isCharged = false;
                    _player.anim.SetTrigger(Strings.AnimPara_ChargingEnd);
                    //player.particle_RoundSlash.Play();
                }
            }
        }

        public override void FixedTick()
        {
            base.FixedTick();

            float fixedDeltaTime = _player.fixedDeltaTime;

            if (isAttack && !_player.anim.GetBool(Strings.AnimPara_OnAttack))
            {
                if (intervalTimer < 0.5f)
                    intervalTimer += _player.fixedDeltaTime;
                else
                {
                    _stateMachine.ChangeState(_stateMachine.STATE_Locomotion);
                }
            }
            
            // Charging Attack Timer
            if (isCharging && !isCharged)
            {
                if (_chargeTime > chargeTimer)
                {
                    chargeTimer += fixedDeltaTime;

                    if (chargeTimer > 0.3f && !isChargingStart)
                    {
                        isChargingStart = true;
                        //particle_Charging.Play();
                    }
                }
                else
                {
                    isCharged = true;
                    isChargingStart = false;
                    // TODO SK - 공격 모션 애니메이션 호출(Crossfade)
                    //anim.SetTrigger(Strings.AnimPara_ChargingAttack);
                    //particle_ChargeComplete.Play();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            isAttack = false;
        }

        private void NormalAttack()
        {
            intervalTimer = 0;
            // TODO SK - 공격 모션 애니메이션 호출(Crossfade)
            //_player.anim.SetTrigger(m_Anim_Para_SwordAttack);
            //if (player.particle_Charging.isPlaying) player.particle_Charging.Stop();
        }
    }
}