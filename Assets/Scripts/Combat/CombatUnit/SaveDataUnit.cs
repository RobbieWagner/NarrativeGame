using UnityEngine;

public class SaveDataUnit : Unit
{
    public SerializableUnit serializableUnit;

    protected override void Awake()
    {
        base.Awake();
    } 

    protected override void InitializeUnit()
    {
        if(serializableUnit != null)
        {
            maxStatValues[UnitStat.Brawn] = serializableUnit.Brawn;
            maxStatValues[UnitStat.Agility] = serializableUnit.Agility;
            maxStatValues[UnitStat.Defense] = serializableUnit.Defense;
            maxStatValues[UnitStat.Psych] = serializableUnit.Psych;
            maxStatValues[UnitStat.Focus] = serializableUnit.Focus;
            maxStatValues[UnitStat.Heart] = serializableUnit.Heart;

            InitializeStats();

            HP = serializableUnit.HP;
            Mana = serializableUnit.Mana;
        }
        else
        {
            Debug.LogWarning($"Could not find save data for unit {UnitName}");
            base.InitializeUnit();
        }
    }
}