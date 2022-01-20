using UnityEngine;

namespace Sangki.Scripts
{
    public class BoolChangeCount : StateMachineBehaviour
    {
        [SerializeField]
        private bool onEnterTrue;
        [SerializeField]
        private string targetBool;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (onEnterTrue) animator.SetBool(targetBool, true);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(targetBool, false);
        }
    }
}
