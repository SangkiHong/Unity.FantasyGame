using SK.Combat;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    // 1. Fields
    public static string prefabFilePathName = "Resources���� ���� ���ϰ�ο� �����̸��� ä���ּ���... example: Unit/Monster/Slime.prefab";
    [SerializeField] protected UnitData info;

    protected int currentHp;

    // 2. Properties
    public UnitData Info 
    { 
        get { return info; }
        set { info = value; }
    }
    public int CurrentHp => currentHp;

    // by.���_���� ȣ�� �� ��Ʋ�Ŵ������� ���� �õ� �Լ� ȣ��_220527
   /* protected void TryAttack(SortedSet<int> targets, Attack attack)
        => SceneManager.INSTANCE.BattleManager.TryAttack(this, ref targets, attack);*/

    // by.���_��Ʋ�Ŵ����� ���� ������ ������ ��� Ÿ�ٿ��� ȣ��Ǵ� �Լ�_220527
    public void GetDamaged(Unit attacker, AttackData attackInfo)
    {
        // ���� HP�� 0���� ũ�� ���� ��� �Լ� ����
        if (currentHp <= 0) return;

        int attackPower = attacker.info.AttackDamage + attackInfo.attackPower;
        currentHp -= attackPower;

        Debug.Log($"{attacker.name} �κ��� {this.name}��(��) ������ �޾ҽ��ϴ�. ������: {attackPower} ���� HP: {currentHp}");

        // Ÿ�� ȿ�� ���
        //SK.Effect.EffectManager.INSTANCE.PlayEffect(1000, transform);

        // ������ƮǮ �׽�Ʈ
        if (currentHp > 0)
            OnDamage(attacker, attackPower);
        else
            OnDead();
    }

    // by.���_�� �Ŵ������� ȣ���� ���� ������Ʈ �����Լ�
    public virtual void FixedTick() { }
    // by.���_�� �Ŵ������� ȣ���� ������Ʈ �����Լ�
    public virtual void Tick() { }
    // by.���_������ ������ ���� ��� ȣ��Ǵ� �����Լ�
    public virtual void OnDamage(Unit attacker, int attackPower) { }
    // by.���_������ ����� ��� ȣ��Ǵ� �����Լ�
    public virtual void OnDead() { }
}
