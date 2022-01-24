using UnityEngine;
using Sangki.Enemy;

namespace Sangki
{
    public class DeadTrigger : MonoBehaviour
    {
        readonly string m_Tag_Player = "Player";
        readonly string m_Tag_Enemy = "Enemy";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(m_Tag_Player))
            {
                Player.PlayerController.Instance.isDead = true;
                Player.PlayerController.Instance.Damage(int.MaxValue);
            }
            if (other.CompareTag(m_Tag_Enemy))
            {
                other.GetComponent<EnemyContoller>().Damage(int.MaxValue);
            }
        }
    }
}
