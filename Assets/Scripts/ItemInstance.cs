using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Strength,
    Dexterity,
    Intelligence,
    Vitality,

    BonusHP,
    Evasion,
    RegenHP,
    RegenMana,
    MoveSpeed,

    CritChance,
    ArmorPen,
    Lifesteal,
    ManaOnHit,
    AttackSpeed,
    BonusDamage
}

[System.Serializable]
public class ItemStatBonus
{
    public StatType statType;
    public int value;
    public float valueFloat;
}

public class ItemInstance
{
    public ItemData baseData;
    public ItemRarity rareza;

    public int finalDamage;
    public int finalDefense;

    public int rangeMinDamage;
    public int rangeMaxDamage;
    public int rangeMinDefense;
    public int rangeMaxDefense;

    public List<ItemStatBonus> bonuses = new List<ItemStatBonus>();

    public ItemInstance(ItemData data)
    {
        baseData = data;
        GenerateStats();
    }

    void GenerateStats()
    {
        rareza = SortearRareza(baseData.rareza);
        GenerateBaseStats();
        GenerateBonuses();
    }

    ItemRarity SortearRareza(ItemRarity rarezaMinima)
    {
        float[] chances = new float[] { 55f, 25f, 12f, 6f, 2f };
        for (int i = 0; i < (int)rarezaMinima; i++) chances[i] = 0f;

        float total = 0f;
        foreach (float c in chances) total += c;

        float roll = Random.Range(0f, total);
        float acum = 0f;
        for (int i = 0; i < chances.Length; i++)
        {
            acum += chances[i];
            if (roll <= acum) return (ItemRarity)i;
        }
        return rarezaMinima;
    }

    float GetRarityMultiplier(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Comun: return 1.0f;
            case ItemRarity.Magico: return 1.4f;
            case ItemRarity.Raro: return 1.8f;
            case ItemRarity.Unico: return 2.4f;
            case ItemRarity.Excepcional: return 3.2f;
            default: return 1.0f;
        }
    }

    void GenerateBaseStats()
    {
        float mult = GetRarityMultiplier(rareza);

        if (baseData.minBaseDamage > 0)
        {
            rangeMinDamage = Mathf.RoundToInt(baseData.minBaseDamage * mult);
            rangeMaxDamage = Mathf.RoundToInt(baseData.maxBaseDamage * mult);
            finalDamage = Random.Range(rangeMinDamage, rangeMaxDamage + 1);
        }

        if (baseData.minBaseDefense > 0)
        {
            rangeMinDefense = Mathf.RoundToInt(baseData.minBaseDefense * mult);
            rangeMaxDefense = Mathf.RoundToInt(baseData.maxBaseDefense * mult);
            finalDefense = Random.Range(rangeMinDefense, rangeMaxDefense + 1);
        }
    }

    void GenerateBonuses()
    {
        int bonusCount = GetBonusCountByRarity(rareza);
        if (bonusCount <= 0) return;

        List<StatType> pool = GetBonusPool();
        int maxPosibles = Mathf.Min(bonusCount, pool.Count);

        for (int i = 0; i < maxPosibles; i++)
        {
            int index = Random.Range(0, pool.Count);
            StatType stat = pool[index];
            pool.RemoveAt(index);

            ItemStatBonus bonus = new ItemStatBonus();
            bonus.statType = stat;
            AssignBonusValue(bonus);
            bonuses.Add(bonus);
        }
    }

    List<StatType> GetBonusPool()
    {
        bool esArma = baseData.tipo == ItemType.Arma;

        if (esArma)
        {
            return new List<StatType>
            {
                StatType.Strength,
                StatType.Dexterity,
                StatType.CritChance,
                StatType.ArmorPen,
                StatType.Lifesteal,
                StatType.ManaOnHit,
                StatType.AttackSpeed,
                StatType.BonusDamage
            };
        }
        else
        {
            return new List<StatType>
            {
                StatType.Vitality,
                StatType.Intelligence,
                StatType.BonusHP,
                StatType.Evasion,
                StatType.RegenHP,
                StatType.RegenMana,
                StatType.MoveSpeed
            };
        }
    }

    void AssignBonusValue(ItemStatBonus bonus)
    {
        ItemData d = baseData;

        switch (bonus.statType)
        {
            case StatType.Strength:
            case StatType.Dexterity:
            case StatType.Intelligence:
            case StatType.Vitality:
                bonus.value = GetWeightedInt(d.minBonusValue, d.maxBonusValue);
                break;

            case StatType.BonusHP:
                bonus.value = GetWeightedInt(d.minBonusHP, d.maxBonusHP);
                break;

            case StatType.Evasion:
                bonus.valueFloat = GetWeightedFloat(d.minBonusEvasion, d.maxBonusEvasion);
                break;

            case StatType.RegenHP:
                bonus.valueFloat = GetWeightedFloat(d.minBonusRegenHP, d.maxBonusRegenHP);
                break;

            case StatType.RegenMana:
                bonus.valueFloat = GetWeightedFloat(d.minBonusRegenMana, d.maxBonusRegenMana);
                break;

            case StatType.MoveSpeed:
                bonus.valueFloat = GetWeightedFloat(d.minBonusMoveSpeed, d.maxBonusMoveSpeed);
                break;

            case StatType.CritChance:
                bonus.valueFloat = GetWeightedFloat(d.minBonusCritChance, d.maxBonusCritChance);
                break;

            case StatType.ArmorPen:
                bonus.valueFloat = GetWeightedFloat(d.minBonusArmorPen, d.maxBonusArmorPen);
                break;

            case StatType.Lifesteal:
                bonus.valueFloat = GetWeightedFloat(d.minBonusLifesteal, d.maxBonusLifesteal);
                break;

            case StatType.ManaOnHit:
                bonus.valueFloat = GetWeightedFloat(d.minBonusManaOnHit, d.maxBonusManaOnHit);
                break;

            case StatType.AttackSpeed:
                bonus.valueFloat = GetWeightedFloat(d.minBonusAttackSpeed, d.maxBonusAttackSpeed);
                break;

            case StatType.BonusDamage:
                int roll = Random.Range(0, 100);
                if (roll < 60) bonus.value = 10;
                else if (roll < 90) bonus.value = 25;
                else bonus.value = 50;
                break;
        }
    }

    // 🔥 FIX REAL ACÁ
    int GetBonusCountByRarity(ItemRarity r)
    {
        int roll = Random.Range(0, 100);

        switch (r)
        {
            case ItemRarity.Comun:
                return (roll < 10) ? 1 : 0; // 🔻 antes 20%

            case ItemRarity.Magico:
                if (roll < 50) return 1;   // 50%
                if (roll < 70) return 2;   // 20%
                return 0;                  // 30% SIN BONUS

            case ItemRarity.Raro:
                if (roll < 40) return 2;
                if (roll < 65) return 3;
                return 1;

            case ItemRarity.Unico:
                if (roll < 40) return 3;
                if (roll < 70) return 4;
                return 2;

            case ItemRarity.Excepcional:
                if (roll < 35) return 3;
                if (roll < 60) return 4;
                if (roll < 80) return 5;
                return 2;
        }

        return 0;
    }

    int GetWeightedInt(int min, int max)
    {
        float t = Mathf.Pow(Random.value, 2.5f);
        return Mathf.RoundToInt(Mathf.Lerp(min, max, t));
    }

    float GetWeightedFloat(float min, float max)
    {
        float t = Mathf.Pow(Random.value, 2.5f);
        float raw = Mathf.Lerp(min, max, t);
        return Mathf.Round(raw * 10f) / 10f;
    }
}