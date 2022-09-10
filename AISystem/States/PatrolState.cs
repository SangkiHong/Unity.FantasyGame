using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.AISystem
{
    /*
        작성자: 홍상기
        내용: 특정 순찰 범위 안에서 랜덤한 위치로 이동하며 타겟을 순찰
        작성일: 22년 5월 17일
    */
    
    public class PatrolState : StateBase
    {
        // by.상기_생성자_220517
        public PatrolState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // by.상기_몬스터 정보를 저장할 변수_220517
        private readonly Monster _monster;

        // by.상기_상태 머신을 저장할 변수_220517
        private readonly StateMachine _machine;

        // by.상기_순찰할 목표 지점을 저장할 변수_220517
        private Vector3 _patrolPos;

        // by.상기_상태 진입시 호출되는 함수_220517
        public override void StateInit()
        {
            // SearchRadar 컴포넌트를 통해 순찰 범위 내 랜덤한 위치를 가져옴
            _patrolPos = _monster.searchRadar.GetPatrolPoint();

            // NavMeshAgent의 목표 위치를 설정하는 함수 호출
            _monster.navAgent.SetDestination(_patrolPos);

            // 애니메이터 파라미터를 비전투 중으로 변경
            _monster.anim.SetBool(Strings.AnimPara_OnCombat, false);
        }

        public override void FixedTick()
        {
            // by.상기_범위 내 타겟 탐색_220517
            if (_monster.searchRadar.FindTarget())
            {
                // 타겟을 발견한 경우 추격 상태로 전환
                _machine.ChangeState(_machine.chaseState);
                return;
            }

            // by.상기_순찰 목표지점에 도착한 경우 유휴 상태로 전환_220518
            if (_monster.navAgent.remainingDistance < 0.1f)
                _machine.ChangeState(_machine.idleState);
        }

        // by.상기_상태를 빠져나갈 때 호출되는 함수_220517
        public override void StateExit()
        {

        }
    }
}