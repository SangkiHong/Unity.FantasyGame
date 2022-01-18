using UnityEngine;
using Sangki.Enemy;
using DG.Tweening;

[RequireComponent(typeof(EnemyContoller))]
public class EnemyBoss : MonoBehaviour
{
    public enum BossName { BudKing }

    public BossName boss;

    [SerializeField]
    private float skillDuration = 1.8f;

    private EnemyContoller enemyContoller;
    private Transform thisTransform;

    private Vector3 skillOjbDiretion;

    readonly string m_PoolKey_Leaf = "Leaf";

    private void Awake()
    {
        thisTransform = this.transform;
        enemyContoller = GetComponent<EnemyContoller>();
    }

    public void BossSkill()
    {
        skillOjbDiretion = Vector3.zero;
        skillOjbDiretion.y = thisTransform.eulerAngles.y;

        switch (boss)
        {
            case BossName.BudKing:
                for (int i = 0; i < 10; i++)
                    PoolManager.instance.GetObject(m_PoolKey_Leaf, thisTransform.position + Vector3.up * 1.7f, skillOjbDiretion + Vector3.up * i * 160);
                
                DOVirtual.DelayedCall(skillDuration, () => { enemyContoller.StopDuringCastingSkill(false); });
                break;
            default:
                break;
        }
    }
}
