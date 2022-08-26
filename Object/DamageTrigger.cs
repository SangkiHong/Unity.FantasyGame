using UnityEngine;
using SK.Player;

namespace SK.Object
{
    public class DamageTrigger : MonoBehaviour
    {
        public int damageAmount = 1;

        private Collider thisCollider;

        private void Awake()
        {
            if (!thisCollider) thisCollider = this.GetComponent<Collider>();
        }

        private void OnEnable()
            => thisCollider.enabled = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Strings.Tag_Player))
            {
                thisCollider.enabled = false;
                //PlayerController.Instance.OnDamageTrigger(gameObject, damageAmount);
            }
        }
    }
}