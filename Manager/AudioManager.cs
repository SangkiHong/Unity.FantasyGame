using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SK.Utilities;

namespace SK
{
    /* �ۼ���: ȫ���
     * ����: �����(�����, ȿ����)�� ���õ� ����� ���� �Ŵ��� Ŭ����
     * �ۼ���: 22�� 6�� 21��
     */

    // ���� Ǯ�� �ҽ� ����ü
    [System.Serializable]
    public struct AudioPoolList
    {
        public int seedSize;
        public AudioClip audioClip;
        public AudioMixerGroup audioMixer;
        [Range(0.0f, 1.0f)] public float volume; // ��� ����(0~1)
        public float maxPlayDistance; // ȿ���� ��� ���� �ִ� �Ÿ�(0�� ��� 2D�� ���)
    }

    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        public delegate void UpdateHandler();
        public event UpdateHandler OnUpdate;

        // ������� �ͼ�
        [SerializeField] private AudioMixerGroup BGMAudioMixer;
        // ����� �ҽ� ������
        [SerializeField] private GameObject audioSourcePrefab;
        // ����� Ǯ ������
        public Data.AudioData audioData;
        // ����� �ҽ� �迭�� ������ ��ųʸ�
        private Dictionary<string, AudioSource[]> _audioSourceDictionary;

        private AudioSource _currentBGMAudioSource, _prevBGMAudioSource;
        private Transform _playerTransform;
        private float _crossTime, _crossElapsed;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            // �ʱ�ȭ
            Initialize();
        }

        private void Update()
            => OnUpdate?.Invoke();

        private void Initialize()
        {
            // ��ųʸ� �ʱ�ȭ
            _audioSourceDictionary = new Dictionary<string, AudioSource[]>();

            GameObject sourceParentObject;
            GameObject sourceChildObject;
            AudioSource tmpSource;

            foreach (KeyValuePair<string, AudioPoolList> pool in audioData.audioDictionary)
            {
                // Ǯ �̸��� ���� �θ� ������Ʈ ����
                sourceParentObject = new GameObject(pool.Key);
                sourceParentObject.transform.SetParent(transform);
                int seedSize = pool.Value.seedSize;

                // ����� �ҽ� ��ųʸ��� �迭 �ʱ�ȭ
                AudioSource[] audioSources = new AudioSource[seedSize];
                _audioSourceDictionary.Add(pool.Key, audioSources);
                // Ʈ������ ĳ�� 
                Transform tmpTr = sourceParentObject.transform;

                for (int i = 0; i < seedSize; i++)
                {
                    // ����� �ҽ� ������Ʈ�� ���� ������Ʈ ���� 
                    sourceChildObject = Instantiate(audioSourcePrefab, tmpTr);
                    tmpSource = sourceChildObject.GetComponent<AudioSource>();
                    tmpSource.clip = pool.Value.audioClip;
                    tmpSource.outputAudioMixerGroup = pool.Value.audioMixer;
                    tmpSource.volume = pool.Value.volume;
                    tmpSource.playOnAwake = false;
                    // BGM�� ��� Loop ����
                    if (tmpSource.outputAudioMixerGroup == BGMAudioMixer)
                        tmpSource.loop = true;
                    // 3D ���� ����
                    if (pool.Value.maxPlayDistance > 0)
                    {
                        tmpSource.spatialBlend = 1;
                        tmpSource.maxDistance = pool.Value.maxPlayDistance;
                    }

                    // ������Ʈ�� ��ųʸ��� �߰�
                    _audioSourceDictionary[pool.Key][i] = tmpSource;
                    tmpSource.gameObject.SetActive(false);
                }
            }
            Debug.Log("����� Ǯ �غ� �Ϸ�");
        }

        // ��Ʈ�� Ű�� ������ ��ųʸ��� �����Ͽ� �ش� ����� �ҽ��� ���
        public void PlayAudio(string audioKey, Transform playTransform)
        {
            //if (_playerTransform == null && GameManager.Instance.Player)
                //_playerTransform = GameManager.Instance.Player.mTransform;

            for (int i = 0; i < _audioSourceDictionary[audioKey].Length; i++)
            {
                // ������� ���� ������ҽ��� Ž��
                if (!_audioSourceDictionary[audioKey][i].gameObject.activeSelf)
                {
                    // 3D ����� ���
                    if (_audioSourceDictionary[audioKey][i].spatialBlend == 1)
                    {
                        float maxDistance = _audioSourceDictionary[audioKey][i].maxDistance;
                        // ����ȭ�� ���� �����Ͽ� �÷��̾���� �Ÿ��� ��
                        maxDistance *= maxDistance;
                        float playerDistance = MyMath.Instance.GetDistance(playTransform.position, _playerTransform.position);

                        // ȿ������ �ִ� �Ÿ��� �÷��̾���� �Ÿ����� �� ��� ������� ����
                        if (playerDistance > maxDistance) return;
                    }

                    _audioSourceDictionary[audioKey][i].gameObject.SetActive(true);
                    _audioSourceDictionary[audioKey][i].transform.position = playTransform.position;
                    _audioSourceDictionary[audioKey][i].Play();
                    return;
                }
            }
        }

        // ��Ʈ�� Ű�� ������ ��ųʸ��� �����Ͽ� �ش� ����� �ҽ��� ���
        public void PlayAudio(string audioKey)
        {
            //if (_playerTransform == null && GameManager.Instance.Player)
            //    _playerTransform = GameManager.Instance.Player.mTransform;

            for (int i = 0; i < _audioSourceDictionary[audioKey].Length; i++)
            {
                if (!_audioSourceDictionary[audioKey][i].isPlaying)
                {
                    if (_playerTransform)
                        _audioSourceDictionary[audioKey][i].transform.position = _playerTransform.position;

                    _audioSourceDictionary[audioKey][i].gameObject.SetActive(true);
                    _audioSourceDictionary[audioKey][i].Play();
                    return;
                }
            }
        }

        // ���̵� �� �ƿ����� ������� �÷���(ũ�ν� �÷���)
        public void PlayBackGroundMusic(string audioKey, float crossPlayTime)
        {
            // Ű�� �ش��ϴ� ������ҽ��� ���ų� ���� ��� ���� ������ǰ� ������ Ű�� ���޵Ǿ��ٸ� ����
            if (!_audioSourceDictionary.ContainsKey(audioKey) || 
                _currentBGMAudioSource == _audioSourceDictionary[audioKey][0])
                return;

            // ���� ������� ������� ������ҽ��� ���� ������� ������ ��ü
            if (_currentBGMAudioSource)
                _prevBGMAudioSource = _currentBGMAudioSource;

            // ���ο� ������� �ҽ��� ��ųʸ����� ������
            _currentBGMAudioSource = _audioSourceDictionary[audioKey][0];
            _currentBGMAudioSource.gameObject.SetActive(true);

            // �� �Ŵ����� �ִ� ���
            /*if (SceneManager.Instance)
            {
                // ���ο� ��������� ���� �ʱ�ȭ
                _currentBGMAudioSource.volume = 0;
                // ���� �ʱ�ȭ
                _crossTime = crossPlayTime;
                _crossElapsed = 0;
                // �� �Ŵ����� ������Ʈ �̺�Ʈ�� �Լ� ���
                SceneManager.Instance.OnUpdate += CrossPlay;
            }*/

            // ���ο� ������� ���
            _currentBGMAudioSource.Play();
        }

        // ũ�ν� �÷���
        private void CrossPlay()
        {
            _crossElapsed += Time.deltaTime;

            // ũ�ν� ��
            if (_crossElapsed < _crossTime)
            {
                // ���ο� ������� ������ ������ �÷���
                _currentBGMAudioSource.volume = _crossElapsed / _crossTime;
                // ���� ������� ������ ������ ����
                if (_prevBGMAudioSource)
                    _prevBGMAudioSource.volume = _crossTime - _crossElapsed;
            }
            // ũ�ν� �Ϸ�
            else
            {
                _currentBGMAudioSource.volume = 1;

                if (_prevBGMAudioSource)
                {
                    _prevBGMAudioSource.volume = 0;
                    _prevBGMAudioSource.Stop();
                    _prevBGMAudioSource.gameObject.SetActive(true);
                    _prevBGMAudioSource = null;
                }
                //SceneManager.Instance.OnUpdate -= CrossPlay;
            }
        }
    }
}
