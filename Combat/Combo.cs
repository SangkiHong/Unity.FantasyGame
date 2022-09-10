using UnityEngine;

namespace SK.Combat
{
    /*
        작성자: 홍상기
        내용: 플레이어의 콤보 공격에 대한 정보를 받아 공격 정보를 반환해주는 컴포넌트
        작성일: 22년 5월 29일
    */
    public class Combo : MonoBehaviour
    {
        // 트리 구조의 콤보 노드
        private class ComboTreeNode
        {
            public AttackData attack;
            public ComboTreeNode aCombo;
            public ComboTreeNode bCombo;
        }

        // 콤보 리스트 구조체
        [System.Serializable]
        public struct ComboList
        {
            public string combo;
            public AttackData attack;
        }

        // 콤보 가능 시간
        [SerializeField] private float comboIntervalTime;

        // 콤보 리스트
        [SerializeField] private ComboList[] comboList;

        // 콤보 트리 루트
        private ComboTreeNode _rootNode;
        // 현재 콤보노드 위치
        private ComboTreeNode _currentCombo;

        private readonly string _methodName = "CancelCombo";
        private readonly string aKey = "A";
        private readonly string bKey = "B";
        private readonly char _comma = ',';

        private void Awake()
        {
            // 루트 노드 초기화
            _rootNode = new ComboTreeNode();
            _currentCombo = _rootNode;

            // 콤보 리스트 세팅
            for (int i = 0; i < comboList.Length; i++)
                SetCombo(comboList[i]);
        }

        private void SetCombo(ComboList comboList)
        {
            ComboTreeNode targetComboNode = _rootNode;

            string[] combo = comboList.combo.Split(_comma);

            // 타겟 콤보 노드를 탐색
            for (int i = 0; i < combo.Length; i++)
            {
                if (combo[i] == aKey)
                {
                    if (targetComboNode.aCombo == null)
                        targetComboNode.aCombo = new ComboTreeNode();

                    targetComboNode = targetComboNode.aCombo;
                }
                else if (combo[i] == bKey)
                {
                    if (targetComboNode.bCombo == null)
                        targetComboNode.bCombo = new ComboTreeNode();

                    targetComboNode = targetComboNode.bCombo;
                }
            }

            targetComboNode.attack = comboList.attack;
        }

        public AttackData GetCombo(int keyIndex)
        {
            // aKey를 통한 콤보 접근한 경우
            if (keyIndex == 0)
            {
                // aCombo 노드가 있는 경우
                if (_currentCombo.aCombo != null)
                    _currentCombo = _currentCombo.aCombo;
                else
                    _currentCombo = _rootNode.aCombo;
            }
            // bKey를 통한 콤보 접근한 경우
            else
            {
                // bCombo 노드가 있는 경우
                if (_currentCombo.bCombo != null)
                    _currentCombo = _currentCombo.bCombo;
                else
                    _currentCombo = _rootNode.bCombo;
            }

            if (_currentCombo == null)
                return null;

            CancelInvoke();
            Invoke(_methodName, comboIntervalTime);
            return _currentCombo.attack;
        }

        private void CancelCombo() => _currentCombo = _rootNode;
    }
}
