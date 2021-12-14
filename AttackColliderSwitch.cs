
using UnityEngine;
using System.Collections;
using Sangki.Scripts.Player;

namespace Sangki.Scripts
{
    public class AttackColliderSwitch : MonoBehaviour
    {
        [SerializeField]
        private GameObject armParent;
        [SerializeField] private float timing_Init;
        [SerializeField] private float timing_duration;

        [HideInInspector]
        public bool isCancel;

        private BoxCollider weaponCollider;
        private ParticleSystem attackTrail;
        private WaitForSeconds ws_Init, ws;

        private void Awake()
        {
            ws_Init = new WaitForSeconds(timing_Init);
            ws = new WaitForSeconds(timing_duration);
        }

        private void Start()
        {
            weaponCollider = armParent.GetComponentInChildren<BoxCollider>();
            if (weaponCollider) weaponCollider.enabled = false;
            attackTrail = armParent.GetComponentInChildren<ParticleSystem>();
            attackTrail?.gameObject.SetActive(false);
        }

        public void DoAttack()
        {
            if (weaponCollider)
            {
                isCancel = false;
                StartCoroutine(StateLoop());
            }
        }

        IEnumerator StateLoop()
        {
            yield return ws_Init;
            attackTrail?.gameObject.SetActive(true);
            if (!isCancel) weaponCollider.enabled = true;
            yield return ws;
            attackTrail?.gameObject.SetActive(false);
            weaponCollider.enabled = false;
        }
    }
}
