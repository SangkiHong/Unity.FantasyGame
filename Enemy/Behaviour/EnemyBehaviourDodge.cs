using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace SK.FSM.Behaviour
{
    public class EnemyBehaviourDodge
    {
        private readonly Enemy _enemy;
        private readonly Transform _transform;
        private readonly Data.EnemyData _enemyData;
        private readonly Animator _anim;
        private readonly NavMeshAgent _navAgent;

        private NavMeshHit _navHit;
        private bool _isDodge;

        public EnemyBehaviourDodge(Enemy enemy, Transform transform)
        {
            _enemy = enemy;
            _transform = transform;
            _enemyData = enemy.enemyData;
            _anim = enemy.anim;
            _navAgent = enemy.navAgent;
        }

        public void DodgeAttack(float targetDist)
        {
            if (!_isDodge && _enemy.enemyState == EnemyState.Attack && targetDist <= _navAgent.stoppingDistance + 0.1f)
            {
                if (Random.value < _enemyData.DodgeChance)
                {
                    // Cancel Archer aiming
                    /*if (enemyClass == EnemyClass.Archer)
                    {
                        //CancelAttack();
                    }*/

                    // NavMesh의 길이 있는지 파악 후 위치로 닷지
                    if (NavMesh.SamplePosition(GetDodgePoint(_enemyData.DodgeAngle), out _navHit, _enemyData.DodgeChance, NavMesh.AllAreas))
                    {
                        _isDodge = true;
                        _navAgent.isStopped = true;
                        _anim.SetBool(Strings.AnimPara_OnAttack, false);
                        _transform.DOMove(_navHit.position, 1f)
                            .SetEase(Ease.OutCubic)
                            .OnComplete(() =>
                            {
                                _isDodge = false;
                                _navAgent.Warp(_transform.position);
                                _navAgent.isStopped = false;

                                // 닷지 후 반격
                                /*if (_enemyData.canCounterAttack && Random.value < _enemyData.counterattackChance)
                                    CounterAttack(); // 공격 상태로 변경되도록..*/
                            });
                        _anim.SetTrigger(Strings.AnimPara_Dodge);
                    }
                }
            }
        }

        /*public void CounterAttack()
        {
            _navAgent.isStopped = false;
            _navAgent.Warp(_transform.position);
            _navAgent.SetDestination(targetObject.transform.position);

            _anim.SetBool(Strings.AnimPara_OnAttack, true);
            _anim.SetTrigger(Strings.AnimPara_CounterAttack);
        }*/


        // 회피할 위치를 구하는 함수(후방 각도)
        private Vector3 GetDodgePoint(float angle)
        {
            float randomVal = UnityEngine.Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return _transform.position + (_transform.rotation * Quaternion.Euler(0, angle, 0)) * (Vector3.forward * -_enemy.enemyData.DodgeDistance);
        }
    }
}