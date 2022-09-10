using UnityEngine;

namespace SK
{
    public class Strings
    {
        #region Animation
        public static readonly int AnimPara_OnMove = Animator.StringToHash("OnMove");
        public static readonly int AnimPara_MoveBlend = Animator.StringToHash("MoveBlend");
        public static readonly int AnimPara_Sidewalk = Animator.StringToHash("Sidewalk");
        public static readonly int AnimPara_Dead = Animator.StringToHash("Dead");
        public static readonly int AnimPara_Jump = Animator.StringToHash("Jump");
        public static readonly int AnimPara_Land = Animator.StringToHash("Land");
        public static readonly int AnimPara_Falling = Animator.StringToHash("Falling");
        public static readonly int AnimPara_MeleeSpeed = Animator.StringToHash("MeleeSpeed");
        public static readonly int AnimPara_ChargingEnd = Animator.StringToHash("ChargingEnd");
        public static readonly int AnimPara_OnCombat = Animator.StringToHash("OnCombat");
        public static readonly int AnimPara_OnAttack = Animator.StringToHash("OnAttack");
        public static readonly int AnimPara_Aimming = Animator.StringToHash("Aimming");
        public static readonly int AnimPara_ShotArrow = Animator.StringToHash("ShotArrow");
        public static readonly int AnimPara_CastSkill = Animator.StringToHash("CastSkill");
        public static readonly int AnimPara_CounterAttack = Animator.StringToHash("CounterAttack");
        public static readonly int AnimPara_IsInteracting = Animator.StringToHash("IsInteracting");
        #endregion

        #region Tag
        public static readonly string Tag_Player = "Player";
        public static readonly string Tag_Enemy = "Enemy";
        public static readonly string Tag_Ground = "Ground";
        public static readonly string Tag_DamageMelee = "DamageMelee";
        public static readonly string Tag_DamageObject = "DamageObject";
        #endregion 

        public static readonly string S_EnemyStatsUI = "EnemyStatsUI";
        public static readonly string S_Arrow = "Arrow";
        public static readonly string S_Fireball = "Fireball";

        #region STRINGS
        public static readonly string ObjectPool_SwordImpactGold = "SwordImpactGold";
        #endregion

        #region SOUND
        public static readonly string Sound_FootSound = "FX_FootSound";
        #endregion
    }
}