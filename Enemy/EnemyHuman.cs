using UnityEngine;
using DG.Tweening;

namespace SK.FSM
{
    public enum EnemyClass { Normal, Archer, Wizard }

    public class EnemyHuman : Enemy
    {
        [SerializeField] private EnemyClass enemyClass;

        internal Sequence shotArrowSequence;

        public override void Attack()
        {
            
        }

        public override void Dead()
        {

        }

        public void StopDuringCastingSkill(bool isStop = true)
        {
            navAgent.isStopped = isStop;
            anim.SetBool(Strings.AnimPara_OnCombat, isStop);
            anim.SetBool(Strings.AnimPara_OnMove, !isStop);
        }
    }
}
