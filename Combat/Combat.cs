using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SK.AISystem;

namespace SK.Combat
{
    /*
        작성자: 홍상기
        내용: 전투에 관련된 기능의 컴포넌트
        작성일: 22년 5월 17일
    */

    public class Combat : MonoBehaviour
    {
        // 공격 판정이 실행되는 순간에 호출할 이벤트
        public UnityAction<SortedSet<int>, AttackData> OnAttack;

        [Header("Debug")]        
        public bool debugCombatRange; // 전투 범위 디버그
        public bool debugAttackRange; // 공격 범위 디버그

        [Header("Attack")]
        public float combatDistance = 3.5f; // 전투 범위 거리
        [SerializeField] internal float attackDistance = 15; // 공격 가능 거리
        [SerializeField] private float attackAngle = 120; // 공격 가능 각도
        [SerializeField] internal float attackCooltime = 1; // 공격 간격 시간
        [SerializeField] internal float lookTargetSpeed = 1; // 타겟을 향하도록 회전하는 스피드
        [SerializeField] private Vector3 offset; // 공격 판정 시작 위치 오프셋
        [SerializeField] private LayerMask targetLayerMask; // 공격 판정의 레이어마스크

        [Header("Reference")]
        [SerializeField] private Animator _anim;
        //[SerializeField] private Combo _combo;
        // 범위 내 오브젝트를 탐색하는 유틸리티 클래스
        private SearchRadar _searchRadar;
        private SearchUtility _searchUtility;

        // 공격할 대상을 탐색하여 대상의 해시값을 저장할 버퍼
        private SortedSet<int> _targetBuff;

        // 현재 공격을 저장할 변수
        private AttackData _currentAttack;

        private void Awake()
        {
            // 초기화
            _targetBuff = new SortedSet<int>();
            if (!_anim) _anim = GetComponent<Animator>();
            //if (!_combo) _combo = GetComponent<Combo>();
            _searchRadar = GetComponent<SearchRadar>();
            if (_searchRadar == null) _searchUtility = new SearchUtility(transform);
        }

        // 공격 버튼에 의한 공격 실행하며 애니메이션 작동
        /*public void BeginAttack(int keyIndex)
        {
            // 공격 중이면 함수를 리턴
            if (_anim.GetBool(Strings.AnimPara_IsInteracting)) return;

            // 콤보 정보를 통해 전달 받은 공격 정보를 현재 공격 변수에 저장
            _currentAttack = _combo.GetCombo(keyIndex);

            // 공격이 유효하면 공격의 애니메이션 이름을 받아 애니메이션 실행(트랜지션을 0.2로 고정)
            if (_currentAttack)
                _anim.CrossFade(_currentAttack.animName, 0.2f);

            // 음향 효과 재생
            //Audio.AudioManager.INSTANCE.PlayAudio(5, _transform);
        }*/

        // 공격 정보를 받아 공격 애니메이션 실행
        public void BeginAttack(AttackData attack)
        {
            // 전달 받은 공격 정보를 변수에 저장
            _currentAttack = attack;

            // 공격의 애니메이션 이름을 받아 애니메이션 실행하며 트랜지션을 0.2로 고정
            _anim.CrossFade(_currentAttack.animName, 0.2f);

            // 음향 효과 재생
            //Audio.AudioManager.INSTANCE.PlayAudio(4, _transform);
        }

        // 범위 내 대상에게 공격(애니메이션의 이벤트로 함수 호출)
        // 공격 범위 안 타겟 탐색 및 타격(공격 각도)
        public void Attack() => SearchAndAttack(_currentAttack.attackAngle);
        
        // 공격 범위 내 타겟 탐색 및 공격(공격 범위 각도)
        private void SearchAndAttack(float degree)
        {
            _targetBuff.Clear(); // 퍼버 초기화

            // 범위 내 타겟을 탐색하여 퍼버에 추가
            if (!_searchRadar) // 탐색 레이더가 없는 경우(플레이어)
                _searchUtility.SearchTargets(offset, degree, attackDistance, ref _targetBuff, targetLayerMask);
            else // 탐색 레이더가 있는 경우(몬스터)
                _searchRadar.searchUtility.SearchTargets(offset, degree, attackDistance, ref _targetBuff, targetLayerMask);

            // 퍼버에 추가된 오브젝트가 있는 경우 공격 이벤트 호출
            if (_targetBuff != null && _targetBuff.Count > 0)
                OnAttack?.Invoke(_targetBuff, _currentAttack);
        }

        // 전투 가능 범위와 공격 범위에 대한 디버깅
        #region Debug
        private void DrawAttackRange(Vector3 positionOffset, float fieldOfViewAngle, float viewDistance, Color color)
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            color.a = 0.1f;
            UnityEditor.Handles.color = color;

            var halfFOV = fieldOfViewAngle * 0.5f;
            var beginDirection = Quaternion.AngleAxis(-halfFOV, transform.up) * transform.forward;
            UnityEditor.Handles.DrawSolidArc(transform.TransformPoint(positionOffset), transform.up, beginDirection, fieldOfViewAngle, viewDistance);

            UnityEditor.Handles.color = oldColor;
#endif
        }

        // Draw the line of sight
        private void OnDrawGizmosSelected()
        {
            if (debugCombatRange) DrawAttackRange(offset, 360, combatDistance, Color.magenta);
            if (debugAttackRange) DrawAttackRange(offset, attackAngle, attackDistance, Color.red);
        }
        #endregion
    }
}
