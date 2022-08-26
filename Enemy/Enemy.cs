using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using EPOOutline;
using SK.Utilities;
using SK.FSM.Behaviour;

namespace SK.FSM
{
    public enum EnemyState { Idle, Patrol, Seek, Chase, Attack, Dead }

    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        internal EnemyState enemyState;

        [SerializeField] internal Data.EnemyData enemyData;

        #region COMPONENTS
        [Header("COMPONENTS")]
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] internal Animator anim;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] internal NavMeshAgent navAgent;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] private SearchRadar searchRadar;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] private Outlinable outline;
        #endregion

        #region PROPERTY
        public Outlinable Outline => outline;
        #endregion
        
        #region ETC
        internal Transform thisTransform;
        internal Collider thisCollider;
        internal Rigidbody thisRigidbody;

        private EnemyBehaviourDodge dodge;
        private EnemyStatsUI enemyHealthBar;

        internal GameObject targetObject;
#endregion


        public void Awake()
        {
            thisTransform = this.transform;

            // 회피 가능한 경우 행동 클래스 생성
            if (enemyData.CanDodge) 
                dodge = new EnemyBehaviourDodge(this, thisTransform);
        }

        public void FixedTick() { }
        public void Tick() { }

        public abstract void Attack();

        public void Damage(int damageAmount)
        {
            
        }

        public abstract void Dead();

        // 전투 중인 경우 목표물 향해 회전
        private void FollowTarget()
        {
            Vector3 dir = targetObject.transform.position - thisTransform.position;
            thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * enemyData.LookSpeed);
        }

        private bool CheckPlayerAlive()
        {
            if (Player.PlayerController.Instance.IsDead)
            {
                anim.SetBool(Strings.AnimPara_OnCombat, false);
                navAgent.ResetPath();
                enemyState = EnemyState.Seek;
                return false;
            }
            else
            {
                return true;
            }
        }

        private void StickEnemyUI(bool toStick, bool isHealthBar = true)
        {
            if (toStick)
            {
                if (!enemyHealthBar)
                    enemyHealthBar = UIPoolManager.instance.GetObject(Strings.S_EnemyStatsUI, thisTransform.position).GetComponent<EnemyStatsUI>();

                if (isHealthBar) enemyHealthBar.Assign(thisTransform, enemyData.HP);
                else enemyHealthBar.Assign(thisTransform, true);
            }
            else
            {
                if (enemyHealthBar) enemyHealthBar.Unassign();
            }
        }
    }
}