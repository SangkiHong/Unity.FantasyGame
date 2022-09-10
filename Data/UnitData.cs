using UnityEngine;

[CreateAssetMenu(fileName = "Unit_", menuName = "Game Data/Unit Data")]
public class UnitData : ScriptableObject
{
    public int UnitID;
    public byte Level;
    public int MaxHp;
    public float MovementSpeed;
    public int AttackDamage;
    public byte Defense;
}