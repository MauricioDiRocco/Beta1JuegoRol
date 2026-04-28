using UnityEngine;

public enum ItemRarity
{
    Comun,
    Magico,
    Raro,
    Unico,
    Excepcional
}

public enum ItemType
{
    Consumible,
    Arma,
    Armadura,
    Escudo,
    Casco,
    Botas
}

[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Sistema Inventario/Item")]
public class ItemData : ScriptableObject
{
    [Header("Información Básica")]
    public string nombreItem;
    [TextArea] public string descripcion;
    public Sprite icono;
    public ItemType tipo;
    public ItemRarity rareza;
    public int nivelRequerido = 1;

    // =============================
    // 🔹 SISTEMA ACTUAL (NO TOCAR)
    // =============================
    [Header("Estadísticas (Sistema Actual)")]
    public int curaVida;
    public int dañoExtra;
    public int defensaExtra;

    // =============================
    // 🔥 BASE VARIABLE
    // =============================
    [Header("Base Variable (Nuevo Sistema)")]
    public int minBaseDamage;
    public int maxBaseDamage;
    public int minBaseDefense;
    public int maxBaseDefense;

    // =============================
    // 🔥 BONUS GENERALES
    // =============================
    [Header("Bonus Generales")]
    public int minBonusValue = 5;
    public int maxBonusValue = 25;
    [Tooltip("Cantidad máxima de bonus posibles")]
    public int maxBonuses = 4;

    // =============================
    // 🛡️ BONUS EXCLUSIVOS ARMADURA
    // (Armadura, Casco, Botas, Escudo)
    // =============================
    [Header("Bonus Armadura — HP Máxima")]
    public int minBonusHP = 100;
    public int maxBonusHP = 1000;

    [Header("Bonus Armadura — Evasión (%)")]
    public float minBonusEvasion = 0f;
    public float maxBonusEvasion = 7f;

    [Header("Bonus Armadura — Regen HP (HP/seg)")]
    public float minBonusRegenHP = 1f;
    public float maxBonusRegenHP = 20f;

    [Header("Bonus Armadura — Regen Mana (Mana/seg)")]
    public float minBonusRegenMana = 1f;
    public float maxBonusRegenMana = 20f;

    [Header("Bonus Armadura — Velocidad de Movimiento (%)")]
    public float minBonusMoveSpeed = 5f;
    public float maxBonusMoveSpeed = 20f;

    // =============================
    // ⚔️ BONUS EXCLUSIVOS ARMA
    // =============================
    [Header("Bonus Arma — Probabilidad de Crítico (%)")]
    public float minBonusCritChance = 2f;
    public float maxBonusCritChance = 10f;

    [Header("Bonus Arma — Penetración de Armadura (%)")]
    public float minBonusArmorPen = 2f;
    public float maxBonusArmorPen = 10f;

    [Header("Bonus Arma — Lifesteal (%)")]
    public float minBonusLifesteal = 2f;
    public float maxBonusLifesteal = 10f;

    [Header("Bonus Arma — Mana por Hit (%)")]
    public float minBonusManaOnHit = 2f;
    public float maxBonusManaOnHit = 10f;

    [Header("Bonus Arma — Velocidad de Ataque (%)")]
    public float minBonusAttackSpeed = 2f;
    public float maxBonusAttackSpeed = 15f;

    // BonusDamage usa valores fijos ponderados: 10 / 25 / 50
    // No necesita campos de rango, están hardcodeados en ItemInstance

    // =============================
    // 🔹 TIPO DE ITEM
    // =============================
    [Header("Tipo de Ítem")]
    public bool esConsumible;
    public bool esAcumulable;
    public int cantidad = 1;

    [Header("Dimensiones (Futuro)")]
    public int celdasOcupadas = 1;

    public ItemData CreateInstance()
    {
        ItemData instance = Instantiate(this);

        instance.nombreItem = this.nombreItem;
        instance.descripcion = this.descripcion;
        instance.icono = this.icono;
        instance.tipo = this.tipo;
        instance.rareza = this.rareza;

        instance.dañoExtra = this.dañoExtra;
        instance.defensaExtra = this.defensaExtra;
        instance.curaVida = this.curaVida;

        instance.minBaseDamage = this.minBaseDamage;
        instance.maxBaseDamage = this.maxBaseDamage;
        instance.minBaseDefense = this.minBaseDefense;
        instance.maxBaseDefense = this.maxBaseDefense;

        instance.minBonusValue = this.minBonusValue;
        instance.maxBonusValue = this.maxBonusValue;
        instance.maxBonuses = this.maxBonuses;

        // Armadura
        instance.minBonusHP = this.minBonusHP;
        instance.maxBonusHP = this.maxBonusHP;
        instance.minBonusEvasion = this.minBonusEvasion;
        instance.maxBonusEvasion = this.maxBonusEvasion;
        instance.minBonusRegenHP = this.minBonusRegenHP;
        instance.maxBonusRegenHP = this.maxBonusRegenHP;
        instance.minBonusRegenMana = this.minBonusRegenMana;
        instance.maxBonusRegenMana = this.maxBonusRegenMana;
        instance.minBonusMoveSpeed = this.minBonusMoveSpeed;
        instance.maxBonusMoveSpeed = this.maxBonusMoveSpeed;

        // Arma
        instance.minBonusCritChance = this.minBonusCritChance;
        instance.maxBonusCritChance = this.maxBonusCritChance;
        instance.minBonusArmorPen = this.minBonusArmorPen;
        instance.maxBonusArmorPen = this.maxBonusArmorPen;
        instance.minBonusLifesteal = this.minBonusLifesteal;
        instance.maxBonusLifesteal = this.maxBonusLifesteal;
        instance.minBonusManaOnHit = this.minBonusManaOnHit;
        instance.maxBonusManaOnHit = this.maxBonusManaOnHit;
        instance.minBonusAttackSpeed = this.minBonusAttackSpeed;
        instance.maxBonusAttackSpeed = this.maxBonusAttackSpeed;

        instance.esConsumible = this.esConsumible;
        instance.esAcumulable = this.esAcumulable;
        instance.cantidad = this.cantidad;
        instance.celdasOcupadas = this.celdasOcupadas;

        return instance;
    }

    public bool EsEquipo()
    {
        return tipo == ItemType.Arma ||
               tipo == ItemType.Armadura ||
               tipo == ItemType.Escudo ||
               tipo == ItemType.Casco ||
               tipo == ItemType.Botas;
    }

    public bool EsArmadura()
    {
        return tipo == ItemType.Armadura ||
               tipo == ItemType.Escudo ||
               tipo == ItemType.Casco ||
               tipo == ItemType.Botas;
    }

    public bool UsaSistemaRandom()
    {
        return EsEquipo() && (minBaseDamage > 0 || minBaseDefense > 0);
    }
}