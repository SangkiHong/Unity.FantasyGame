using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.AISystem
{
    /*
        작성자: 홍상기
        내용: AI는 제자리에서 시야 범위 내에 타겟이 있는지 체크함
        작성일: 22년 5월 17일
    */

    public class IdleState : StateBase
    {
        // by.상기_생성자_220517
        public IdleState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // by.상기_몬스터 정보를 저장할 변수_220517
        private readonly Monster _monster;

        // by.상기_상태 머신을 저장할 변수_220517
        private readonly StateMachine _machine;

        private float _elapsed;

        // by.상기_상태 진입시 호출되는 함수_220517
        public override void StateInit()
        {
            _elapsed = 0;
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

            // by.상기_유휴 상태 유지 시간이 0 이상일 경우에만 유지 시간 이후에 다시 순찰 상태로 전환 됨_220517
            if (_monster.searchRadar.idleDuration > 0 && _elapsed < _monster.searchRadar.idleDuration)
                _elapsed += _monster._fixedDeltaTime;
            else
                _machine.ChangeState(_machine.patrolState);
        }
    }
}
