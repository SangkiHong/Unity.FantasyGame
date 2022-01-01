using UnityEngine;

public class OnStartEvent : StateMachineBehaviour
{
    private enum EventType { ChangeState, PlayerAttack }

    [SerializeField]
    private EventType eventType;

    [SerializeField]
    private string targetBool;

    [SerializeField]
    private bool switchOnStart;

    int hash_TartgetBool,
        hash_IsAttack;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hash_TartgetBool == 0)
        {
            if (targetBool != string.Empty) hash_TartgetBool = Animator.StringToHash(targetBool);
        }
        if (hash_IsAttack == 0) hash_IsAttack = Animator.StringToHash("isAttack");

        switch (eventType)
        {
            case EventType.ChangeState:
                animator.SetBool(hash_TartgetBool, switchOnStart);
                break;
            case EventType.PlayerAttack:
                animator.SetBool(hash_IsAttack, true);
                break;
        }
    }
}
