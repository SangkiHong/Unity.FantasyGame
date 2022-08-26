using UnityEngine;

namespace SK.Scripts
{
    public class BoolChangeCount : StateMachineBehaviour
    {
        [SerializeField]
        private bool onEnter;
        [SerializeField]
        private bool onExit;
        [SerializeField]
        private string targetBool;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(targetBool, onEnter);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(targetBool, onExit);
        }
    }
}
