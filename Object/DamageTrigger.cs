using UnityEngine;
using Sangki.Player;

namespace Sangki.Object
{
    public class DamageTrigger : MonoBehaviour
    {
        public int damageAmount = 1;

        GameObject thisGameobejct;
        Collider thisCollider;
        readonly string m_tag_Player = "Player";

        private void Awake()
        {
            if (!thisGameobejct) thisGameobejct = gameObject;
            if (!thisCollider) thisCollider = this.GetComponent<Collider>();
        }

        private void OnEnable()
        {
            thisCollider.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(m_tag_Player))
            {
                thisCollider.enabled = false;
                PlayerController.Instance.OnDamageTrigger(thisGameobejct, damageAmount);
            }
        }
    }
}