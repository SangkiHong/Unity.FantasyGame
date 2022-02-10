using UnityEngine;

namespace Sangki
{

    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public int attackPower = 1;
        
        public float attackComboInterval = 1.5f;
        
        public int maxHealth = 5;
        
        public float speed = 5;

        public float speedOnShield = 0.2f;

        public float speedOnTargeting = 0.5f;

        public float jumpForce = 16;

        public float jumpIntervalDelay = 0.6f;

        public float diveRollingForce = 3f;

        public float blinkTime = 1;

        public float shieldRotateSpeed = 3.5f;

        public float chargeTime = 0.7f;

        public float parryingTime = 0.3f;

        public float stepSize = 0.3f;
    }
}
