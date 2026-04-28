using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance;

    [Header("Panel del Tooltip")]
    public GameObject tooltipPanel;
    public RectTransform tooltipRect;

    [Header("Textos")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI descText;

    [Header("Stats")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI healText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI extraStatText;

    [Header("Barra de rareza")]
    public Image rarityBar;

    public GameObject rowAttack;
    public GameObject rowDefense;
    public GameObject rowHeal;
    public GameObject rowQuantity;

    private Canvas canvas;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipRect != null && tooltipRect.gameObject.activeSelf)
            MoveToMouse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SHOW — ItemData
    // ─────────────────────────────────────────────────────────────────────────
    public void Show(ItemData item)
    {
        if (item == null) return;
        tooltipPanel.SetActive(true);
        tooltipRect.SetAsLastSibling();

        nameText.text = item.nombreItem;
        nameText.color = GetRarityColor(item.rareza);
        typeText.text = GetTypeLabel(item.tipo);
        rarityText.text = item.rareza.ToString();
        rarityText.color = GetRarityColor(item.rareza);
        if (rarityBar != null) rarityBar.color = GetRarityColor(item.rareza);
        if (descText != null) descText.text = item.descripcion;

        if (rowAttack != null) rowAttack.SetActive(item.dañoExtra > 0);
        if (attackText != null && item.dañoExtra > 0) attackText.text = "+" + item.dañoExtra;

        if (rowDefense != null) rowDefense.SetActive(item.defensaExtra > 0);
        if (defenseText != null && item.defensaExtra > 0) defenseText.text = "+" + item.defensaExtra;

        if (rowHeal != null) rowHeal.SetActive(item.curaVida > 0);
        if (healText != null && item.curaVida > 0) healText.text = "+" + item.curaVida + " HP";

        if (rowQuantity != null) rowQuantity.SetActive(item.esAcumulable);
        if (quantityText != null && item.esAcumulable) quantityText.text = "x" + item.cantidad;

        if (extraStatText != null) { extraStatText.text = ""; extraStatText.gameObject.SetActive(false); }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        MoveToMouse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SHOW — ItemInstance
    // ─────────────────────────────────────────────────────────────────────────
    public void Show(ItemInstance instance)
    {
        if (instance == null || instance.baseData == null) return;

        ItemData item = instance.baseData;
        ItemRarity rareza = instance.rareza;

        tooltipPanel.SetActive(true);
        tooltipRect.SetAsLastSibling();

        nameText.text = item.nombreItem;
        nameText.color = GetRarityColor(rareza);
        typeText.text = GetTypeLabel(item.tipo);
        rarityText.text = rareza.ToString();
        rarityText.color = GetRarityColor(rareza);
        if (rarityBar != null) rarityBar.color = GetRarityColor(rareza);
        if (descText != null) descText.text = item.descripcion;

        // Daño base con rango
        if (rowAttack != null) rowAttack.SetActive(instance.finalDamage > 0);
        if (attackText != null && instance.finalDamage > 0)
            attackText.text = instance.rangeMaxDamage > 0
                ? "+" + instance.finalDamage + " <color=#888888>[" + instance.rangeMinDamage + "-" + instance.rangeMaxDamage + "]</color>"
                : "+" + instance.finalDamage;

        // Defensa base con rango
        if (rowDefense != null) rowDefense.SetActive(instance.finalDefense > 0);
        if (defenseText != null && instance.finalDefense > 0)
            defenseText.text = instance.rangeMaxDefense > 0
                ? "+" + instance.finalDefense + " <color=#888888>[" + instance.rangeMinDefense + "-" + instance.rangeMaxDefense + "]</color>"
                : "+" + instance.finalDefense;

        if (rowHeal != null) rowHeal.SetActive(false);
        if (rowQuantity != null) rowQuantity.SetActive(false);

        // Bonus extras
        if (extraStatText != null)
        {
            if (instance.bonuses != null && instance.bonuses.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ItemStatBonus bonus in instance.bonuses)
                    sb.AppendLine(FormatBonus(bonus, item));

                extraStatText.text = sb.ToString().TrimEnd();
                extraStatText.gameObject.SetActive(true);
            }
            else
            {
                extraStatText.text = "";
                extraStatText.gameObject.SetActive(false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        MoveToMouse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Formatea cada bonus con su unidad y rango correcto
    // ─────────────────────────────────────────────────────────────────────────
    string FormatBonus(ItemStatBonus bonus, ItemData item)
    {
        string col = GetStatHexColor(bonus.statType);
        string label = GetStatLabel(bonus.statType);
        string rango = "";
        string valor = "";

        switch (bonus.statType)
        {
            // ── Atributos base ───────────────────────────────────────────────
            case StatType.Strength:
            case StatType.Dexterity:
            case StatType.Intelligence:
            case StatType.Vitality:
                valor = "+" + bonus.value;
                rango = $" <color=#888888>[{item.minBonusValue}-{item.maxBonusValue}]</color>";
                break;

            // ── Bonus armadura ───────────────────────────────────────────────
            case StatType.BonusHP:
                valor = "+" + bonus.value + " HP";
                rango = $" <color=#888888>[{item.minBonusHP}-{item.maxBonusHP}]</color>";
                break;

            case StatType.Evasion:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Evasión";
                rango = $" <color=#888888>[{item.minBonusEvasion:F1}-{item.maxBonusEvasion:F1}%]</color>";
                break;

            case StatType.RegenHP:
                valor = "+" + bonus.valueFloat.ToString("F1") + " HP/seg";
                rango = $" <color=#888888>[{item.minBonusRegenHP:F1}-{item.maxBonusRegenHP:F1}]</color>";
                break;

            case StatType.RegenMana:
                valor = "+" + bonus.valueFloat.ToString("F1") + " Mana/seg";
                rango = $" <color=#888888>[{item.minBonusRegenMana:F1}-{item.maxBonusRegenMana:F1}]</color>";
                break;

            case StatType.MoveSpeed:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Velocidad";
                rango = $" <color=#888888>[{item.minBonusMoveSpeed:F1}-{item.maxBonusMoveSpeed:F1}%]</color>";
                break;

            // ── Bonus arma ───────────────────────────────────────────────────
            case StatType.CritChance:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Crítico";
                rango = $" <color=#888888>[{item.minBonusCritChance:F1}-{item.maxBonusCritChance:F1}%]</color>";
                break;

            case StatType.ArmorPen:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Penetración";
                rango = $" <color=#888888>[{item.minBonusArmorPen:F1}-{item.maxBonusArmorPen:F1}%]</color>";
                break;

            case StatType.Lifesteal:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Lifesteal";
                rango = $" <color=#888888>[{item.minBonusLifesteal:F1}-{item.maxBonusLifesteal:F1}%]</color>";
                break;

            case StatType.ManaOnHit:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Mana/Golpe";
                rango = $" <color=#888888>[{item.minBonusManaOnHit:F1}-{item.maxBonusManaOnHit:F1}%]</color>";
                break;

            case StatType.AttackSpeed:
                valor = "+" + bonus.valueFloat.ToString("F1") + "% Vel. Ataque";
                rango = $" <color=#888888>[{item.minBonusAttackSpeed:F1}-{item.maxBonusAttackSpeed:F1}%]</color>";
                break;

            case StatType.BonusDamage:
                valor = "+" + bonus.value + " Daño";
                rango = " <color=#888888>[10 / 25 / 50]</color>";
                break;

            default:
                valor = "+" + bonus.value;
                break;
        }

        return $"<color={col}>{valor}</color>{rango}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SHOW — SkillSlotData
    // ─────────────────────────────────────────────────────────────────────────
    public void ShowSkill(SkillSlotData skill)
    {
        tooltipPanel.SetActive(true);
        nameText.text = skill.skillName;
        nameText.color = new Color(0.69f, 0.66f, 0.93f);
        typeText.text = skill.isPotion ? "Consumible" : "Habilidad";
        rarityText.text = skill.isPotion ? "Común" : "Habilidad";
        if (rarityBar != null) rarityBar.color = skill.isPotion ? Color.white : new Color(0.5f, 0.46f, 0.87f);
        if (rowAttack != null) rowAttack.SetActive(false);
        if (rowDefense != null) rowDefense.SetActive(false);
        if (rowHeal != null) rowHeal.SetActive(skill.isPotion && skill.healAmount > 0);
        if (healText != null && skill.isPotion) healText.text = "+" + skill.healAmount + " HP";
        if (rowQuantity != null) rowQuantity.SetActive(skill.isPotion);
        if (quantityText != null && skill.isPotion) quantityText.text = "x" + skill.quantity;
        if (descText != null) descText.text = skill.isPotion ? "Usala con click derecho o presionando la tecla." : "";
        if (extraStatText != null) { extraStatText.text = ""; extraStatText.gameObject.SetActive(false); }
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        MoveToMouse();
    }

    public void Hide() { if (tooltipPanel != null) tooltipPanel.SetActive(false); }

    void MoveToMouse()
    {
        if (tooltipRect == null) return;
        tooltipRect.position = Input.mousePosition + new Vector3(20f, 20f, 0f);
    }

    Color GetRarityColor(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Comun: return Color.white;
            case ItemRarity.Magico: return new Color(0.33f, 0.57f, 1f);
            case ItemRarity.Raro: return new Color(1f, 0.78f, 0.2f);
            case ItemRarity.Unico: return new Color(1f, 0.5f, 0.1f);
            case ItemRarity.Excepcional: return new Color(0.8f, 0.2f, 1f);
            default: return Color.white;
        }
    }

    string GetTypeLabel(ItemType tipo)
    {
        switch (tipo)
        {
            case ItemType.Arma: return "Arma";
            case ItemType.Armadura: return "Armadura — pecho";
            case ItemType.Casco: return "Armadura — cabeza";
            case ItemType.Botas: return "Armadura — pies";
            case ItemType.Escudo: return "Escudo";
            case ItemType.Consumible: return "Consumible";
            default: return "";
        }
    }

    string GetStatLabel(StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength: return "Fuerza";
            case StatType.Dexterity: return "Destreza";
            case StatType.Intelligence: return "Inteligencia";
            case StatType.Vitality: return "Vitalidad";
            case StatType.BonusHP: return "HP Máxima";
            case StatType.Evasion: return "Evasión";
            case StatType.RegenHP: return "Regen HP";
            case StatType.RegenMana: return "Regen Mana";
            case StatType.MoveSpeed: return "Velocidad";
            case StatType.CritChance: return "Crítico";
            case StatType.ArmorPen: return "Penetración";
            case StatType.Lifesteal: return "Lifesteal";
            case StatType.ManaOnHit: return "Mana/Golpe";
            case StatType.AttackSpeed: return "Vel. Ataque";
            case StatType.BonusDamage: return "Daño";
            default: return stat.ToString();
        }
    }

    string GetStatHexColor(StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength: return "#FF6060";
            case StatType.Dexterity: return "#60FF90";
            case StatType.Intelligence: return "#60AAFF";
            case StatType.Vitality: return "#FFDD55";
            case StatType.BonusHP: return "#FF4444";
            case StatType.Evasion: return "#AAFFAA";
            case StatType.RegenHP: return "#FF8888";
            case StatType.RegenMana: return "#88AAFF";
            case StatType.MoveSpeed: return "#FFFFAA";
            case StatType.CritChance: return "#FFD700";
            case StatType.ArmorPen: return "#FF9900";
            case StatType.Lifesteal: return "#FF5555";
            case StatType.ManaOnHit: return "#AA88FF";
            case StatType.AttackSpeed: return "#88FFDD";
            case StatType.BonusDamage: return "#FF4400";
            default: return "#FFFFFF";
        }
    }
}