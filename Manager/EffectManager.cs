using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SK.FX;

namespace SK
{
    /* �ۼ���: ȫ���
     * ���: ����Ʈ ȿ���� �ҷ��� ȭ�鿡 �������� �ϴ� ������ Ŭ����
     * �ۼ���: 22�� 6�� 23��
     */

    // ����Ʈ Ǯ�� �ҽ� ����ü
    [System.Serializable]
    public struct EffectPoolList
    {
        public int seedSize;
        public GameObject effectPrefab;
        public string description;
    }

    public class EffectManager : MonoBehaviour
    {
        // �̱��� ����
        public static EffectManager Instance { get; private set; }

        [SerializeField] private Data.EffectData effectData;

        // ����Ʈ ������ ��ųʸ�(Ű: ����Ʈ ID, ��: ����Ʈ�� ������ ť)
        private Dictionary<int, Queue<Effect>> _effectDic;
        // ��� ���� ����Ʈ�� ������ ��ųʸ�(Ű: ����Ʈ �ν��Ͻ� ID, ��: ����Ʈ)
        private Dictionary<int, Effect> _usingEffectDic;

        private Transform _playerTransform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            // ��ųʸ� �ʱ�ȭ
            _effectDic = new Dictionary<int, Queue<Effect>>();
            _usingEffectDic = new Dictionary<int, Effect>();

            Transform thisTransform = transform;
            Effect tempEffect = null;
            for (int i = 0; i < effectData.effectList.Count; i++)
            {
                // ù ������Ʈ ���� �� Effect Ŭ���� ��������
                tempEffect = Instantiate(effectData.effectList[i].effectPrefab, thisTransform).GetComponent<Effect>();
                tempEffect.Initialize();
                // ����� ���� ��� ȣ��Ǵ� �̺�Ʈ�� �Լ� �Ҵ�
                tempEffect.OnEndPlay += PushToPool;

                // ����Ʈ ID
                int effectId = tempEffect.effectId;

                // �ش� Ű�� ������ ���� ���ٸ� ��ųʸ� �߰� �� ť ����
                if (!_effectDic.ContainsKey(effectId))
                    _effectDic.Add(effectId, new Queue<Effect>());
                _effectDic[effectId].Enqueue(tempEffect);

                // ����Ʈ Ǯ�� �õ� ���� ������ �߰�
                int seedSize = effectData.effectList[i].seedSize;
                // �õ尡 2 �̻��̸� �õ�-1 ��ŭ �߰� ���� �� ť�� �߰�
                if (seedSize > 1) 
                {
                    for (int j = 0; j < seedSize - 1; j++)
                    {
                        tempEffect = Instantiate(tempEffect, thisTransform);
                        tempEffect.Initialize();
                        // ����� ���� ��� ȣ��Ǵ� �̺�Ʈ�� �Լ� �Ҵ�
                        tempEffect.OnEndPlay += PushToPool;
                        _effectDic[effectId].Enqueue(tempEffect);
                    }
                }
            }
        }

        public void PlayEffect(int effectId, Transform ownerTransform = null)
        {
            // ��� ������ ����Ʈ�� �ִ� ���
            if (_effectDic[effectId].Count > 0)
            {
                // ť���� ����Ʈ�� ����
                Effect effect = _effectDic[effectId].Dequeue();

                // �÷��̾� Ʈ������ ������
                if (_playerTransform == null)
                    //_playerTransform = GameManager.Instance.Player.mTransform;

                // ����Ʈ�� ���
                effect.Play(ownerTransform != null ? ownerTransform : _playerTransform);
                _usingEffectDic.Add(effect.GetInstanceID(), effect);
            }
        }

        public void PlayEffect(int effectId, Vector3 position, Quaternion rotation)
        {
            // ��� ������ ����Ʈ�� �ִ� ���
            if (_effectDic[effectId].Count > 0)
            {
                // ť���� ����Ʈ�� ����
                Effect effect = _effectDic[effectId].Dequeue();
                // ����Ʈ�� ���
                effect.Play(ref position, ref rotation);
                _usingEffectDic.Add(effect.GetInstanceID(), effect);
            }
        }

        private void PushToPool(int instanceId)
        {
            // ��� ���� ����Ʈ ��ųʸ����� ����Ʈ�� ������
            Effect effect = _usingEffectDic[instanceId];
            // �ش� ����Ʈ ť�� �߰�
            _effectDic[effect.effectId].Enqueue(effect);
            // ��� ���� ����Ʈ ��ųʸ����� ����
            _usingEffectDic.Remove(instanceId);
        }
    }
}
