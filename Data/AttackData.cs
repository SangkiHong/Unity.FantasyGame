using UnityEngine;

namespace SK.Combat
{
    [CreateAssetMenu(fileName = "Attack_", menuName = "Combat/Attack")]
    public class AttackData : ScriptableObject
    {
        public string animName;
        public int attackPower = 1;
        public int attackAngle = 60;
        public bool isStrongAttack;
        public bool onRootMotion;
    }
}