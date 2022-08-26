using UnityEngine;
using Sirenix.OdinInspector;
using MoreMountains.Feedbacks;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] private string enemyName;
        public string EnemyName => enemyName;

        #region ENEMY STATS
        [TitleGroup("STATS", null, TitleAlignments.Centered)]
        public int HP = 3;
        public float LookSpeed = 1f;
        public float BlinkTime = 0.5f;
        public float NavMeshLinkSpeed = 0.5f;

        [TitleGroup("ATTACK", null, TitleAlignments.Centered)]
        public float AttackCooldown = 2.5f;
        public float AttackStepsize = 0.5f;

        [TitleGroup("DODGE", null, TitleAlignments.Centered)]
        public bool CanDodge;
        [ShowIf("CanDodge")]
        public float DodgeChance = 0.3f;
        [ShowIf("CanDodge")]
        public float DodgeAngle = 30;
        [ShowIf("CanDodge")]
        public float DodgeDistance = 5f;
        [ShowIf("CanDodge")]
        public bool CanCounterAttack;
        [ShowIf("@this.CanDodge && this.CanCounterAttack")]
        public float CounterAttackChance = 0.3f;
        #endregion

        #region SOUND
        [TitleGroup("SOUND", null, TitleAlignments.Centered)]
        public string Sound_Hit;
        public string Sound_Death;
        #endregion
    }
}