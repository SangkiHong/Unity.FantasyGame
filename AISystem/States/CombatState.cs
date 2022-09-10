using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.AISystem
{
    /*
        작성자: 홍상기
        내용: 특정 거리 안에 타겟이 있으면 특정 간격으로 공격함
        작성일: 22년 5월 17일
    */

    public class CombatState : StateBase
    {
        // by.상기_생성자_220517
        public CombatState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // by.상기_몬스터 정보를 저장할 변수_220517
        private readonly Monster _monster;

        // by.상기_상태 머신을 저장할 변수_220517
        private readonly StateMachine _machine;

        // 현재 공격을 저장할 변수_220519
        private Combat.AttackData _currentAttack;

        // 공격 배열의 인덱스_220519
        private int _attackIndex; 
        private float _targetDistance;
        private float _elapsed;

        // by.상기_상태 진입시 호출되는 함수_220517
        public override void StateInit()
        {
            // NavMeshAgent의 업데이트 중지
            _monster.navAgent.Warp(_monster.thisTransform.position);
            _monster.navAgent.isStopped = true;
            // 초기화
            _elapsed = 0;
        }

        public override void FixedTick()
        {
            // by.상기_타겟이 할당되어 있는 경우_220517
            if (_monster.searchRadar.Target)
            {
                // 타겟과의 거리 업데이트하여 변수에 저장
                _targetDistance = Vector3.Distance(_monster.searchRadar.Target.transform.position, _monster.thisTransform.position);

                // 타겟과의 거리가 전투가 가능한 거리보다 +1 더 멀어지면 추격 상태로 전환
                if (_targetDistance > _monster.combat.attackDistance)
                    _machine.ChangeState(_machine.chaseState);
            }
        }

        public override void Tick()
        {
            // by.상기_타겟을 바라보는 함수
            RotateToTarget();

            // by.상기_공격 쿨타임 간격에 따라서 공격 함수 호출_220517
            if (_elapsed < _monster.combat.attackCooltime)
                _elapsed += _monster._deltaTime;
            else
            {
                if (_monster.normalAttacks.Length > 0)
                {
                    // 현재 공격을 변수에 할당
                    _currentAttack = _monster.normalAttacks[_attackIndex];

                    // 공격 인덱스가 0 ~ 공격 배열 길이 -1 사이의 값을 순환함
                    _attackIndex = (_attackIndex + 1) % _monster.normalAttacks.Length;

                    // 공격의 애니메이션 이름을 받아 애니메이션 실행하며 트랜지션을 0.2로 고정함
                    _monster.anim.CrossFade(_currentAttack.animName, 0.2f);

                    _monster.combat.BeginAttack(_currentAttack);
                }
                _elapsed = 0;
            }
        }

        // by.상기_상태를 빠져나갈 때 호출되는 함수_220517
        public override void StateExit()
        {

        }

        private void RotateToTarget()
        {
            Vector3 dir = (_monster.searchRadar.Target.transform.position - _monster.thisTransform.position).normalized;
            dir.y = 0;
            _monster.thisTransform.rotation = 
                Quaternion.Lerp(_monster.thisTransform.rotation, Quaternion.LookRotation(dir), _monster._deltaTime * _monster.combat.lookTargetSpeed);
        }
    }
}
