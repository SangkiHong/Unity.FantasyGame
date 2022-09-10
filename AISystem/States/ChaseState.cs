using UnityEngine;

namespace SK.AISystem
{
    /*
        작성자: 홍상기
        내용: 탐색된 타겟을 지정된 거리(전투 가능 거리)만큼 가까워질 때까지 추격
        작성일: 22년 5월 17일
    */

    public class ChaseState : StateBase
    {
        // 생성자
        public ChaseState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // 몬스터 정보를 저장할 변수
        private readonly Monster _monster;

        // 상태 머신을 저장할 변수
        private readonly StateMachine _machine;

        // 타겟의 위치를 저장할 변수
        private Vector3 _targetPos;

        // 타겟과의 거리를 계산 위한 현재 위치 변수
        private Vector3 _transformPos;

        // 타겟과의 거리를 저장할 변수
        private float _targetDistance;

        // 전투 가능 거리를 저장할 변수
        private float _combatDistance;

        // 상태 진입시 호출되는 함수
        public override void StateInit()
        {
            // NavMeshAgent의 업데이트 재가동
            _monster.navAgent.Warp(_monster.thisTransform.position);
            _monster.navAgent.isStopped = false;

            // 애니메이터 파라미터를 전투 중으로 변경
            _monster.anim.SetBool(Strings.AnimPara_OnCombat, true);

            // 전투 가능 거리를 한 번만 계산
            if (_combatDistance == 0)
                _combatDistance = Mathf.Pow(_monster.combat.combatDistance, 2);
        }

        public override void FixedTick()
        {
            // 타겟이 할당되어 있는 경우
            if (_monster.searchRadar.Target)
            {
                // 타겟의 위치 값, 트랜스폼의 위치 값을 변수에 저장
                _targetPos = _monster.searchRadar.Target.transform.position;
                _transformPos = _monster.thisTransform.position;

                // 거리 계산하기 위한 좌표 계산
                var x = Mathf.Pow(_targetPos.x - _transformPos.x, 2);
                var y = Mathf.Pow(_targetPos.y - _transformPos.y, 2);
                var z = Mathf.Pow(_targetPos.z - _transformPos.z, 2);

                // 타겟과의 거리 업데이트하여 변수에 저장
                _targetDistance = x + y + z;

                // 타겟과의 거리가 전투가 가능한 거리가 되면 전투 상태로 전환
                if (_targetDistance < _combatDistance)
                {
                    _machine.ChangeState(_machine.combatState);
                    return;
                }

                // NavMeshAgent의 목표 위치를 설정하는 함수 호출
                _monster.navAgent.SetDestination(_targetPos);
            }
            else
                _machine.ChangeState(_machine.patrolState);
        }
    }
}