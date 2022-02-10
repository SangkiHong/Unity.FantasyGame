
using UnityEngine;
using System.Collections.Generic;

namespace Sangki
{
    public class AttackColliderSwitch : MonoBehaviour
    {
        [SerializeField]
        private List<BoxCollider> weaponColliders;
        [SerializeField] 
        private float timing_duration;

        [HideInInspector]
        public bool isCancel;

        private ParticleSystem attackTrail;
        private readonly string string_EndAttack = "EndAttack";

        private void Start()
        {
            if (weaponColliders.Count != 0)
            {
                for (int i = 0; i < weaponColliders.Count; i++) 
                {
                    weaponColliders[i].enabled = false; 
                    if (!attackTrail) attackTrail = weaponColliders[i].GetComponentInChildren<ParticleSystem>(); 
                    attackTrail?.gameObject.SetActive(false);
                }
            }
            
        }

        public void DoAttack()
        {
            if (weaponColliders.Count != 0)
            {
                if (!isCancel)
                {
                    attackTrail?.gameObject.SetActive(true);
                    if (!isCancel) for (int i = 0; i < weaponColliders.Count; i++) weaponColliders[i].enabled = true;

                    if (timing_duration > 0) Invoke(string_EndAttack, timing_duration);
                }
                isCancel = false;
            }
        }

        public void EndAttack()
        {
            if (weaponColliders.Count != 0)
            {
                isCancel = false;
                attackTrail?.gameObject.SetActive(false);
                for (int i = 0; i < weaponColliders.Count; i++) weaponColliders[i].enabled = false;
            }
        }
    }
}
