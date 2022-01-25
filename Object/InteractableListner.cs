using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;
using MoreMountains.Feedbacks;

namespace Sangki.Object
{
    public class InteractableListner : MonoBehaviour, IInteractable
    {
        public bool isOneWay = true;

        public event UnityAction OnIneractionCompleted;

        #region INTERACTION
        [Header("MOVE OBJECT")]
        [FoldoutGroup("INTERACTION")]
        [SerializeField]
        private Transform interactee;
        [FoldoutGroup("INTERACTION")]
        [SerializeField]
        private bool withMove;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withMove")]
        [SerializeField]
        private Vector3 toMovePos;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withMove")]
        [SerializeField]
        private float toMoveDuration;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withMove")]
        [SerializeField]
        private float toMoveDelay;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withMove")]
        [SerializeField]
        private Ease toMoveEase;

        [Header("ROTATE OBJECT")]
        [FoldoutGroup("INTERACTION")]
        [SerializeField]
        private bool withRotate;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withRotate")]
        [SerializeField]
        private Vector3 toRotate;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withRotate")]
        [SerializeField]
        private float toRotateDuration;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withRotate")]
        [SerializeField]
        private float toRotateDelay;
        [FoldoutGroup("INTERACTION")]
        [ShowIf("withRotate")]
        [SerializeField]
        private Ease toRotateEase;
        [Space]
        [FoldoutGroup("INTERACTION")]
        [SerializeField]
        private MMFeedbacks feedbacks_Interacting;
        #endregion

        [HideInInspector]
        public bool isInteracting, isInteracted;

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
                    if (withMove) interactee.DOMove(toMovePos, toMoveDuration).SetRelative(true).SetEase(toMoveEase).SetDelay(toMoveDelay)
                            .OnComplete(() =>
                            {
                                if (isInteracting) OnIneractionCompleted?.Invoke();
                                isInteracting = false;
                            });
                    if (withRotate) interactee.DORotate(toRotate, toRotateDuration).SetRelative(true).SetEase(toRotateEase).SetDelay(toRotateDelay)
                            .OnComplete(() =>
                            {
                                if (isInteracting) OnIneractionCompleted?.Invoke();
                                isInteracting = false;
                            });
                }
                // 재정렬
                else
                {
                    // 타겟 움직임 & 회전
                    if (withMove) interactee.DOMove(-toMovePos, toMoveDuration).SetRelative(true).SetEase(toMoveEase).SetDelay(toMoveDelay)
                            .OnComplete(() =>
                            {
                                if (isInteracting) OnIneractionCompleted?.Invoke();
                                isInteracting = false;
                            });
                    if (withRotate) interactee.DORotate(-toRotate, toRotateDuration).SetRelative(true).SetEase(toRotateEase).SetDelay(toRotateDelay)
                            .OnComplete(() =>
                            {
                                if (isInteracting) OnIneractionCompleted?.Invoke();
                                isInteracting = false;
                            });
                }

                // PLAY FEEDBACKS
                feedbacks_Interacting?.PlayFeedbacks();

                isInteracted = !isInteracted;
            }
        }
    }
}
