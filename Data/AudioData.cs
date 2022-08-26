using System.Collections.Generic;
using UnityEngine;
using SK.Utilities;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "AudioData_", menuName = "Game Data/AudioData")]
    public class AudioData : ScriptableObject
    {
        // ����� Ǯ ������ ��ųʸ�
        public SerializableDictionary<string, AudioPoolList> audioDictionary;

        public void CopyData(SerializableDictionary<string, AudioPoolList> sourceData)
        {
            audioDictionary = new SerializableDictionary<string, AudioPoolList>();

            foreach (KeyValuePair<string, AudioPoolList> data in sourceData)
                audioDictionary.Add(data.Key, data.Value);
        }
    }
}
