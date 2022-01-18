using System.Collections;
using UnityEngine;

namespace Sangki.Enemy
{
    public abstract class Enemy : MonoBehaviour
    {
        public int speed;
        public int health;
        public int gold;

        public abstract void Attack();

        public virtual void Die()
        {
            Destroy(this.gameObject);
        }
    }

    public class Orc : Enemy
    {
        public override void Attack()
        {
            throw new System.NotImplementedException();
        }

        public override void Die()
        {
            // custom particles
            base.Die();
        }
    }
}