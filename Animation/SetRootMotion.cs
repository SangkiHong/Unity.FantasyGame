using UnityEngine;

namespace Sangki.Scripts
{
    public class SetRootMotion : StateMachineBehaviour
    {
        [SerializeField]
        private bool onEnterTrue;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.applyRootMotion = onEnterTrue;
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.applyRootMotion = !onEnterTrue;
        }
    }
}
