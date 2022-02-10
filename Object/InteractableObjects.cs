using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using EPOOutline;
using MoreMountains.Feedbacks;
using Sangki.Enemy;

namespace Sangki.Object
{
    public class InteractableObjects : MonoBehaviour, IInteractable
    {
        #region VARIABLE
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

        [SerializeField]
        private InteractableListner interactableListner;

        readonly string m_Tag_Player = "Player";
        bool isSwitchAvailable, isClose, isInteracted;
        #endregion

        private void OnEnable()
        {
            if (interactableListner) 
            {
                interactableListner.OnIneractionCompleted += OnInteractionCompleted;
            }
        }

        private void OnDestroy()
        {
            if (interactableListner)
            {
                interactableListner.OnIneractionCompleted -= OnInteractionCompleted;
            }
        }

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
            // �۵� �߿� ��� ����
            if (!interactableListner.isInteracting && isClose)
            {
                if (interactableListner.isOneWay && isInteracted) return; // �ѹ��� �۵� ����

                isInteracted = !isInteracted;

                // �۵�
                if (!isInteracted)
                {
                    // Ÿ�� ������ & ȸ��
                    interactableListner.Interact();

                    // ����ġ ������ & ȸ��
                    if (switchMove) switchTrans.DOMove(switchToMove, switchMoveDuration).SetRelative(true).SetEase(switchMoveEase);
                    if (switchRotate) switchTrans.DORotate(switchToRotate, switchRotateDuration).SetRelative(true).SetEase(switchRotateEase);
                }
                // ������
                else
                {
                    // Ÿ�� ������ & ȸ��
                    interactableListner.Interact();

                    // ����ġ ������ & ȸ��
                    if (switchMove) switchTrans.DOMove(-switchToMove, switchMoveDuration).SetRelative(true).SetEase(switchMoveEase);
                    if (switchRotate) switchTrans.DORotate(-switchToRotate, switchRotateDuration).SetRelative(true).SetEase(switchRotateEase);
                }

                // PLAY FEEDBACKS
                feedbacks_Interacting?.PlayFeedbacks();

                SwitchAvailable(false);
            }
        }

        #region TRIGGER
        private void OnTriggerEnter(Collider other)
        {
            if (interactableListner.isOneWay && isInteracted || Player.PlayerController.Instance.isDead) return;

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
                    if (!interactableListner.isOneWay && isInteracted && !Player.PlayerController.Instance.isDead) Interact();
                }
            }
        }
        #endregion

        #region EVENT
        private void OnInteractionCompleted()
        {
            if (!interactableListner.isOneWay && isClose && !justTouchSwitch) SwitchAvailable(true);
        }
        #endregion
    }
}
