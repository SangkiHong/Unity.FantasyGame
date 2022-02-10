using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sangki.Enemy;

namespace Sangki.Object
{
    public class InteractableListner : MonoBehaviour, IInteractable
    {
        public bool isOneWay = true;

        public event UnityAction OnIneractionCompleted;

        #region INTERACTEE
        [Serializable]
        private struct Interactee
        {
            [Header("MOVE OBJECT")]
            [FoldoutGroup("INTERACTION")]
            public Transform interactee;

            [FoldoutGroup("INTERACTION")]
            public bool withMove;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withMove")]
            public Vector3 toMovePos;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withMove")]
            public float toMoveDuration;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withMove")]
            public float toMoveDelay;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withMove")]
            public Ease toMoveEase;

            [Header("ROTATE OBJECT")]
            [FoldoutGroup("INTERACTION")]
            public bool withRotate;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withRotate")]
            public Vector3 toRotate;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withRotate")]
            public float toRotateDuration;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withRotate")]
            public float toRotateDelay;

            [FoldoutGroup("INTERACTION")]
            [ShowIf("withRotate")]
            public Ease toRotateEase;

            [Space]

            [FoldoutGroup("INTERACTION")]
            public MMFeedbacks feedbacks_Interacting;
        }
        #endregion

        #region MONSTER INTERACTION
        [SerializeField]
        private bool isMonsterOpen;
        [ShowIf("isMonsterOpen")]
        [SerializeField]
        private EnemyContoller[] enemies;
        #endregion

        [SerializeField]
        private Interactee[] interactees;  

        [HideInInspector]
        public bool isInteracting, isInteracted;

        private int remainEnemies;

        private void OnEnable()
        {
            if (isMonsterOpen)
            {
                for (int i = 0; i < enemies.Length; i++) 
                {
                    enemies[i].OnDied += Interact;
                    ++remainEnemies;
                }
            }
        }

        private void OnDestroy()
        {
            if (isMonsterOpen)
            {
                for (int i = 0; i < enemies.Length; i++) if (enemies[i]) enemies[i].OnDied -= Interact;
            }
        }

        public void Interact()
        {
            if (isMonsterOpen)
            {
                --remainEnemies;
                if (remainEnemies > 0) return;
            }

            // 작동 중에 사용 금지
            if (!isInteracting)
            {
                if (isOneWay && isInteracted) return; // 한번만 작동 가능

                isInteracting = true;

                // 작동
                if (!isInteracted)
                {
                    // 타겟 움직임 & 회전
                    for (int i = 0; i < interactees.Length; i++)
                    {
                        if (interactees[i].withMove) 
                            interactees[i].interactee.DOMove(interactees[i].toMovePos, interactees[i].toMoveDuration)
                                .SetRelative(true)
                                .SetEase(interactees[i].toMoveEase)
                                .SetDelay(interactees[i].toMoveDelay)
                                .OnComplete(() =>
                                {
                                    if (isInteracting) OnIneractionCompleted?.Invoke();
                                    isInteracting = false;
                                });
                        if (interactees[i].withRotate) 
                                interactees[i].interactee.DORotate(interactees[i].toRotate, interactees[i].toRotateDuration)
                                .SetRelative(true)
                                .SetEase(interactees[i].toRotateEase)
                                .SetDelay(interactees[i].toRotateDelay)
                                .OnComplete(() =>
                                {
                                    if (isInteracting) OnIneractionCompleted?.Invoke();
                                    isInteracting = false;
                                });
                        // PLAY FEEDBACKS
                        interactees[i].feedbacks_Interacting?.PlayFeedbacks();
                    }
                }
                // 재정렬
                else
                {
                    // 타겟 움직임 & 회전
                    for (int i = 0; i < interactees.Length; i++)
                    {
                        if (interactees[i].withMove) interactees[i].interactee.DOMove(-interactees[i].toMovePos, interactees[i].toMoveDuration)
                                .SetRelative(true)
                                .SetEase(interactees[i].toMoveEase)
                                .SetDelay(interactees[i].toMoveDelay)
                                .OnComplete(() =>
                                {
                                    if (isInteracting) OnIneractionCompleted?.Invoke();
                                    isInteracting = false;
                                });
                        if (interactees[i].withRotate) interactees[i].interactee.DORotate(-interactees[i].toRotate, interactees[i].toRotateDuration)
                                .SetRelative(true)
                                .SetEase(interactees[i].toRotateEase)
                                .SetDelay(interactees[i].toRotateDelay)
                                .OnComplete(() =>
                                {
                                    if (isInteracting) OnIneractionCompleted?.Invoke();
                                    isInteracting = false;
                                });
                        // PLAY FEEDBACKS
                        interactees[i].feedbacks_Interacting?.PlayFeedbacks();
                    }
                }


                isInteracted = !isInteracted;
            }
        }
    }
}
