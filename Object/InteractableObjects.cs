using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using EPOOutline;
using MoreMountains.Feedbacks;

namespace Sangki.Object
{
    public class InteractableObjects : MonoBehaviour, IInteractable
    {
        private enum InteractType { Lever, Button, Hit }


        #region COMPONENTS
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Outlinable outline;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private MMFeedbacks feedbacks_Interacting;
        #endregion

        #region SWITCH of Button
        [FoldoutGroup("SWITCH")]
        [EnumToggleButtons]
        [SerializeField]
        private InteractType interactType;
        [FoldoutGroup("SWITCH")]
        [ShowIf("interactType", InteractType.Button)]
        [SerializeField]
        private bool justTouchSwitch;
        [FoldoutGroup("SWITCH")]
        [SerializeField]
        private bool isOneWay;
        [FoldoutGroup("SWITCH")]
        [SerializeField]
        private Transform switchTrans;
        [FoldoutGroup("SWITCH")]
        [Header("MOVE SWITCH")]
        [SerializeField]
        private bool switchMove;
        [FoldoutGroup("SWITCH")]
        [ShowIf("switchMove")]
        [SerializeField]
        private Vector3 switchToMove;
        [FoldoutGroup("SWITCH")]
        [ShowIf("switchMove")]
        [SerializeField]
        private float switchMoveDuration;
        [FoldoutGroup("SWITCH")]
        [ShowIf("switchMove")]
        [SerializeField]
        private Ease switchMoveEase;

        [Header("ROTATE SWITCH")]
        [FoldoutGroup("SWITCH")]
        [SerializeField]
        private bool switchRotate;
        [FoldoutGroup("SWITCH")]
        [ShowIf("switchRotate")]
        [SerializeField]
        private Vector3 switchToRotate;
        [FoldoutGroup("SWITCH")]
        [ShowIf("switchRotate")]
        [SerializeField]
        private float switchRotateDuration;
        [FoldoutGroup("SWITCH")]
        [ShowIf("switchRotate")]
        [SerializeField]
        private Ease switchRotateEase;
        #endregion

        #region TARGET
        [FoldoutGroup("TARGET")]
        [SerializeField]
        private GameObject interactee;
        [Header("MOVE OBJECT")]
        [FoldoutGroup("TARGET")]
        [SerializeField]
        private bool withMove;
        [FoldoutGroup("TARGET")]
        [ShowIf("withMove")]
        [SerializeField]
        private Vector3 toMovePos;
        [FoldoutGroup("TARGET")]
        [ShowIf("withMove")]
        [SerializeField]
        private float toMoveDuration;
        [FoldoutGroup("TARGET")]
        [ShowIf("withMove")]
        [SerializeField]
        private float toMoveDelay;
        [FoldoutGroup("TARGET")]
        [ShowIf("withMove")]
        [SerializeField]
        private Ease toMoveEase;

        [Header("ROTATE OBJECT")]
        [FoldoutGroup("TARGET")]
        [SerializeField]
        private bool withRotate;
        [FoldoutGroup("TARGET")]
        [ShowIf("withRotate")]
        [SerializeField]
        private Vector3 toRotate;
        [FoldoutGroup("TARGET")]
        [ShowIf("withRotate")]
        [SerializeField]
        private float toRotateDuration;
        [FoldoutGroup("TARGET")]
        [ShowIf("withRotate")]
        [SerializeField]
        private float toRotateDelay;
        [FoldoutGroup("TARGET")]
        [ShowIf("withRotate")]
        [SerializeField]
        private Ease toRotateEase;
        #endregion

        readonly string m_Tag_Player = "Player";
        bool isInteracting, isInteracted, isSwitchAvailable, isClose;

        private void SwitchAvailable(bool isAvailable)
        {
            isSwitchAvailable = isAvailable;

            if (isAvailable)
            {
                if (outline) outline.enabled = true;
                Player.PlayerController.Instance.OnInteractable += Interact;
                Manager.GameManager.Instance.UIManager.OnInteractButton(true);
            }
            else
            {
                if (outline) outline.enabled = false;
                Player.PlayerController.Instance.OnInteractable -= Interact;
                Manager.GameManager.Instance.UIManager.OnInteractButton(false);
            }
        }

        public void Interact() 
        {
            // 작동 중에 사용 금지
            if (!isInteracting)
            {
                if (isOneWay && isInteracted) return; // 한번만 작동 가능

                isInteracting = true;

                // 작동
                if (!isInteracted)
                {
                    // 타겟 움직임 & 회전
                    if (withMove) interactee.transform.DOMove(toMovePos, toMoveDuration).SetRelative(true).SetEase(toMoveEase).SetDelay(toMoveDelay)
                            .OnComplete(() => 
                            {
                                isInteracting = false; 
                                if (!isOneWay && isClose && !justTouchSwitch) SwitchAvailable(true);
                            });
                    if (withRotate) interactee.transform.DORotate(toRotate, toRotateDuration).SetRelative(true).SetEase(toRotateEase).SetDelay(toRotateDelay)
                            .OnComplete(() => 
                            {
                                isInteracting = false; 
                                if (!isOneWay && isClose && !justTouchSwitch) SwitchAvailable(true);
                            });

                    // 스위치 움직임 & 회전
                    if (switchMove) switchTrans.DOMove(switchToMove, switchMoveDuration).SetRelative(true).SetEase(switchMoveEase);
                    if (switchRotate) switchTrans.DORotate(switchToRotate, switchRotateDuration).SetRelative(true).SetEase(switchRotateEase);
                }
                // 재정렬
                else
                {
                    // 타겟 움직임 & 회전
                    if (withMove) interactee.transform.DOMove(-toMovePos, toMoveDuration).SetRelative(true).SetEase(toMoveEase).SetDelay(toMoveDelay)
                            .OnComplete(() => 
                            {
                                isInteracting = false; 
                                if (!isOneWay && isClose && !justTouchSwitch) SwitchAvailable(true); 
                            });
                    if (withRotate) interactee.transform.DORotate(-toRotate, toRotateDuration).SetRelative(true).SetEase(toRotateEase).SetDelay(toRotateDelay)
                            .OnComplete(() => 
                            {
                                isInteracting = false; 
                                
                                if (!isOneWay && isClose && !justTouchSwitch) SwitchAvailable(true); 
                            });

                    // 스위치 움직임 & 회전
                    if (switchMove) switchTrans.DOMove(-switchToMove, switchMoveDuration).SetRelative(true).SetEase(switchMoveEase);
                    if (switchRotate) switchTrans.DORotate(-switchToRotate, switchRotateDuration).SetRelative(true).SetEase(switchRotateEase);
                }

                // PLAY FEEDBACKS
                feedbacks_Interacting?.PlayFeedbacks();

                SwitchAvailable(false);
                isInteracted = !isInteracted;
            }
        }

        #region TRIGGER
        private void OnTriggerEnter(Collider other)
        {
            if (isOneWay && isInteracted || Player.PlayerController.Instance.isDead) return;

            if (other.CompareTag(m_Tag_Player)) 
            {
                isClose = true;
                if (!justTouchSwitch)
                {
                    SwitchAvailable(true);
                }
                else
                {
                    Interact();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(m_Tag_Player))
            {
                isClose = false;
                if (!justTouchSwitch)
                {
                    if (!isSwitchAvailable) SwitchAvailable(false);
                }
                else
                {
                    if (!isOneWay && isInteracted && !Player.PlayerController.Instance.isDead) Interact();
                }
            }
        }
        #endregion
    }
}
