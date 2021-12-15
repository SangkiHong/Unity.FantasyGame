
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Sangki.Scripts
{
    public class AttackColliderSwitch : MonoBehaviour
    {
        [SerializeField]
        private List<BoxCollider> weaponColliders;
        [SerializeField] private float timing_Init;
        [SerializeField] private float timing_duration;

        [HideInInspector]
        public bool isCancel;

        private ParticleSystem attackTrail;
        private WaitForSeconds ws_Init, ws;

        private void Awake()
        {
            ws_Init = new WaitForSeconds(timing_Init);
            ws = new WaitForSeconds(timing_duration);
        }

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
                isCancel = false;
                StartCoroutine(StateLoop());
            }
        }

        IEnumerator StateLoop()
        {
            Debug.Log("StartLoop");
            yield return ws_Init;
            attackTrail?.gameObject.SetActive(true);
            if (!isCancel) for (int i = 0; i < weaponColliders.Count; i++) weaponColliders[i].enabled = true;
            yield return ws;
            attackTrail?.gameObject.SetActive(false);
            for (int i = 0; i < weaponColliders.Count; i++) weaponColliders[i].enabled = false;
        }
    }
}
