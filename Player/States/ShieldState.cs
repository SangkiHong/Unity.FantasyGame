using UnityEngine;
using SK.Player;

namespace SK.States
{
    public class ShieldState : State
    {
        private readonly Player.PlayerController _player;
        private readonly StateMachine _stateMachine;

        private Animator _anim;

        private float _shieldLayerWeight, _parryingTimer, _parryingTime;
        private bool _isOnShield;

        public ShieldState(Player.PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _anim = _player.anim;
            _parryingTime = _player.playerData.parryingTime;
        }

        public void _()
        {
            if (_player.canDamage)
            {
                float fixedDeltaTime = _player.fixedDeltaTime;
                if (_isOnShield)
                {
                    // Shield Motion On
                    if (_shieldLayerWeight < 1)
                    {
                        _shieldLayerWeight += fixedDeltaTime * 5f;
                        _anim.SetLayerWeight(1, _shieldLayerWeight);
                    }

                    // Parrying Timer
                    if (_parryingTimer < _parryingTime) _parryingTimer += fixedDeltaTime;
                }
                else
                {
                    // Shield Motion Off
                    if (_shieldLayerWeight > 0)
                    {
                        _shieldLayerWeight -= fixedDeltaTime * 5f;
                        _anim.SetLayerWeight(1, _shieldLayerWeight);
                    }
                }
            }
        }

        private bool IsSuccessShield(bool isEnemy, Vector3 targetDir)
        {
            var enemyAngle = Vector3.Angle(targetDir, _player.thisTransform.forward);
            if (enemyAngle < 90)
            {
                if (isEnemy)
                {
                    // 패링 작동
                    if (_parryingTimer < _parryingTime && _shieldLayerWeight < 0.5f)
                    {
                        _shieldLayerWeight = 0;
                        _anim.SetLayerWeight(1, 0);
                        _anim.SetTrigger(Strings.AnimPara_Parrying);
                        _player.feedback_Parrying?.PlayFeedbacks();
                        // FX
                        //PoolManager.instance.GetObject(m_ObjectPool_SwordImpactGold, shieldParent.position);

                        _isOnShield = false;
                    }
                    // 일반 쉴드
                    else _player.feedback_ShieldDefense?.PlayFeedbacks();
                }

                return true;
            }
            return false;
        }
    }
}