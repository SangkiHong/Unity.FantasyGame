using UnityEngine;
using Sangki.Player;

namespace Sangki.Object
{
    public class DamageTrigger : MonoBehaviour
    {
        [HideInInspector]
        public int damageAmount = 1;

        GameObject thisGameobejct;
        readonly string m_tag_Player = "Player";

        private void Awake()
        {
            thisGameobejct = gameObject;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(m_tag_Player))
            {
                PlayerController.Instance.OnDamageTrigger(thisGameobejct, damageAmount);
            }
        }
    }
}
