using SK.Combat;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    // 1. Fields
    public static string prefabFilePathName = "Resources폴더 안의 파일경로와 파일이름을 채워주세요... example: Unit/Monster/Slime.prefab";
    [SerializeField] protected UnitData info;

    protected int currentHp;

    // 2. Properties
    public UnitData Info 
    { 
        get { return info; }
        set { info = value; }
    }
    public int CurrentHp => currentHp;

    // by.상기_공격 호출 시 배틀매니저에게 공격 시도 함수 호출_220527
   /* protected void TryAttack(SortedSet<int> targets, Attack attack)
        => SceneManager.INSTANCE.BattleManager.TryAttack(this, ref targets, attack);*/

    // by.상기_배틀매니저를 통해 공격이 가해진 경우 타겟에게 호출되는 함수_220527
    public void GetDamaged(Unit attacker, AttackData attackInfo)
    {
        // 현재 HP가 0보다 크지 않은 경우 함수 리턴
        if (currentHp <= 0) return;

        int attackPower = attacker.info.AttackDamage + attackInfo.attackPower;
        currentHp -= attackPower;

        Debug.Log($"{attacker.name} 로부터 {this.name}이(가) 공격을 받았습니다. 데미지: {attackPower} 현재 HP: {currentHp}");

        // 타격 효과 재생
        //SK.Effect.EffectManager.INSTANCE.PlayEffect(1000, transform);

        // 오브젝트풀 테스트
        if (currentHp > 0)
            OnDamage(attacker, attackPower);
        else
            OnDead();
    }

    // by.상기_씬 매니저에서 호출할 고정 업데이트 가상함수
    public virtual void FixedTick() { }
    // by.상기_씬 매니저에서 호출할 업데이트 가상함수
    public virtual void Tick() { }
    // by.상기_유닛이 공격을 받은 경우 호출되는 가상함수
    public virtual void OnDamage(Unit attacker, int attackPower) { }
    // by.상기_유닛이 사망한 경우 호출되는 가상함수
    public virtual void OnDead() { }
}
