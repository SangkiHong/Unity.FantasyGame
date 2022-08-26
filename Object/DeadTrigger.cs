using UnityEngine;
using SK.FSM;

namespace SK
{
    public class DeadTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Strings.Tag_Player))
            {
                Player.PlayerController.Instance.Damage(int.MaxValue);
            }
            if (other.CompareTag(Strings.Tag_Enemy))
            {
                other.GetComponent<EnemyContoller>().Damage(int.MaxValue);
            }
        }
    }
}
