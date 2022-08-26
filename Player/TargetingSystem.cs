using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SK.FSM;

namespace SK.Utility
{
    public class TargetingSystem : MonoBehaviour
    {
        [SerializeField]
        private bool isOnTargeting;
        [SerializeField]
        private float searchDistance = 10f;
        [SerializeField]
        private GameObject button_ReleaseTarget;

        private Transform thisTransform;
        private Animator anim;
        [SerializeField]
        private EnemyContoller targetObject;
        [SerializeField]
        private List<EnemyContoller> targetList = new List<EnemyContoller>();

        public bool IsOnTargeting => isOnTargeting;
        public EnemyContoller TargetObject => targetObject;

        private readonly int m_Anim_Para_MoveBlend = Animator.StringToHash("MoveBlend");
        private readonly int m_Anim_Para_Sidewalk = Animator.StringToHash("Sidewalk");

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            anim = GetComponent<Animator>();
        }

        public void SetTargeting(bool isOn)
        {
            if (isOn) // On Targeting
            {
                if (FindTarget()) 
                {
                    TargetingSwitch(true);
                    return;
                }
            }

            // Off Targeting
            TargetingSwitch(false);
            
        }

        private void TargetingSwitch(bool isOn)
        {
            isOnTargeting = isOn;
            button_ReleaseTarget.SetActive(isOn);
            
            if (targetObject) targetObject.Outline.enabled = isOn; // Outline

            if (isOn && targetObject)
            {
                Vector3 look = Quaternion.LookRotation(targetObject.transform.position - transform.position).eulerAngles;
                look.x = 0; look.z = 0;
                thisTransform.eulerAngles = look;
            }
            else
            {
                targetObject = null;
                anim.SetFloat(m_Anim_Para_MoveBlend, 0);
                anim.SetFloat(m_Anim_Para_Sidewalk, 0);
            }
        }

        public void TargetCheck()
        {
            if (targetList == null) return;

            if (targetList.Count > 0)
            {
                for (int i = 0; i < targetList.Count; i++)
                {
                    if (targetList[i].isDead)
                    {
                        if (targetList[i] == targetObject)
                        {
                            if (targetObject) targetObject.Outline.enabled = false; // Outline
                            targetObject = null;
                        }
                        targetList.RemoveAt(i);
                        --i;
                    }
                }
            }

            // Check Target Remain
            if (targetList.Count > 0) SetTargeting(true);
            else TargetingSwitch(false);
        }
        
        // Find Target
        private bool FindTarget()
        {
            // Get All Enemy List
            if (targetList.Count == 0)
            {
                targetList = GameObject.FindObjectsOfType<EnemyContoller>().ToList();
                if (targetList.Count > 0)
                {
                    // Find Nearest Target by LINQ
                    targetObject = targetList.OrderBy(obj =>
                    {
                        return Vector3.Distance(thisTransform.position, obj.transform.position);
                    }).FirstOrDefault();

                    return true;
                }
                return false;
            }
            else
            {
                if (targetObject) targetObject.Outline.enabled = false; // Outline

                for (int i = 0; i < targetList.Count; i++)
                {
                    if (targetList[i] != targetObject)
                    {
                        if (Vector3.Distance(thisTransform.position, targetList[i].transform.position) <= searchDistance)
                        {
                            targetObject = targetList[i];
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

    }
}
