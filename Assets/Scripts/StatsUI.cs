using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class StatsUI : MonoBehaviour
{
    public GameObject panel;

    [Header("Textos Principales")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI magicDamageText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI pointsText;

    [Header("Atributos Base")]
    public TextMeshProUGUI vitText;
    public TextMeshProUGUI strText;
    public TextMeshProUGUI intText;
    public TextMeshProUGUI dexText;

    [Header("Bonus Extra")]
    public TextMeshProUGUI extraBonusText;

    [Header("Botones +")]
    public Button btnVit;
    public Button btnStr;
    public Button btnInt;
    public Button btnDex;

    [Header("Experiencia")]
    public Image expBar;
    public TextMeshProUGUI expText;

    public PlayerStats stats;

    void Start()
    {
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();

        if (btnVit) btnVit.onClick.AddListener(() => stats.AddVit());
        if (btnStr) btnStr.onClick.AddListener(() => stats.AddStr());
        if (btnInt) btnInt.onClick.AddListener(() => stats.AddInt());
        if (btnDex) btnDex.onClick.AddListener(() => stats.AddDex());

        if (panel != null) panel.SetActive(false);

        Refresh();
    }

    void Update()
    {
        HandleInput();

        if (panel != null && panel.activeSelf)
            Refresh();
    }

    void HandleInput()
    {
        if (panel == null) return;

        if (Input.GetKeyDown(KeyCode.C))
            TogglePanel();

        if (Input.GetKeyDown(KeyCode.Escape) && panel.activeSelf)
            ClosePanel();
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf)
            Refresh();
    }

    // =========================
    // BONUS PRINCIPALES
    // =========================
    void GetEquipmentBonuses(out int bonusStr, out int bonusDex, out int bonusIntel, out int bonusVit)
    {
        bonusStr = 0;
        bonusDex = 0;
        bonusIntel = 0;
        bonusVit = 0;

        EquipmentSlot[] equipSlots = FindObjectsByType<EquipmentSlot>(FindObjectsSortMode.None);

        foreach (EquipmentSlot slot in equipSlots)
        {
            if (slot.currentItemInstance == null) continue;

            foreach (ItemStatBonus bonus in slot.currentItemInstance.bonuses)
            {
                switch (bonus.statType)
                {
                    case StatType.Strength: bonusStr += bonus.value; break;
                    case StatType.Dexterity: bonusDex += bonus.value; break;
                    case StatType.Intelligence: bonusIntel += bonus.value; break;
                    case StatType.Vitality: bonusVit += bonus.value; break;
                }
            }
        }
    }

    // =========================
    // BONUS EXTRA (FIX REAL)
    // =========================
    void GetExtraBonuses(out string result)
    {
        result = "";

        Dictionary<StatType, float> bonusSum = new Dictionary<StatType, float>();

        EquipmentSlot[] equipSlots = FindObjectsByType<EquipmentSlot>(FindObjectsSortMode.None);

        foreach (EquipmentSlot slot in equipSlots)
        {
            if (slot.currentItemInstance == null) continue;

            foreach (ItemStatBonus bonus in slot.currentItemInstance.bonuses)
            {
                // ignorar stats base
                if (bonus.statType == StatType.Strength ||
                    bonus.statType == StatType.Dexterity ||
                    bonus.statType == StatType.Intelligence ||
                    bonus.statType == StatType.Vitality)
                    continue;

                // 🔥 CLAVE: usar float si existe
                float value = bonus.valueFloat != 0 ? bonus.valueFloat : bonus.value;

                if (bonusSum.ContainsKey(bonus.statType))
                    bonusSum[bonus.statType] += value;
                else
                    bonusSum.Add(bonus.statType, value);
            }
        }

        foreach (var kvp in bonusSum)
        {
            result += FormatBonusLine(kvp.Key, kvp.Value) + "\n";
        }
    }

    // =========================
    // FORMATO BONUS (FIX % REAL)
    // =========================
    string FormatBonusLine(StatType type, float value)
    {
        string nombre = type.ToString();

        // 🔥 si viene de valueFloat → es porcentaje
        if (value > 0f && value < 1f)
        {
            float percent = value * 100f;
            return "<color=#60FF90>" + nombre + " +" + percent.ToString("0.#") + "%</color>";
        }
        else
        {
            return "<color=#60FF90>" + nombre + " +" + value.ToString("0.#") + "</color>";
        }
    }

    // =========================
    string FormatStat(string nombre, int baseValue, int bonus)
    {
        if (bonus > 0)
            return nombre + ": " + baseValue + " <color=#60FF90>(+" + bonus + ")</color>";

        return nombre + ": " + baseValue;
    }

    void Refresh()
    {
        if (stats == null) return;

        GetEquipmentBonuses(out int bStr, out int bDex, out int bIntel, out int bVit);

        if (levelText != null) levelText.text = "Level: " + stats.nivel;
        if (pointsText != null) pointsText.text = "PTS: " + stats.statPoints;
        if (hpText != null) hpText.text = "HP: " + stats.currentHealth + "/" + stats.maxHealth;
        if (manaText != null) manaText.text = "SP: " + stats.currentMana + "/" + stats.maxMana;
        if (damageText != null) damageText.text = "ATK: " + stats.minDamage + "-" + stats.maxDamage;
        if (magicDamageText != null) magicDamageText.text = "MATK: " + stats.magicDamage;
        if (defenseText != null) defenseText.text = "DEF: " + stats.defense;

        if (strText != null) strText.text = FormatStat("STR", stats.str, bStr);
        if (vitText != null) vitText.text = FormatStat("VIT", stats.vit, bVit);
        if (intText != null) intText.text = FormatStat("INT", stats.intel, bIntel);
        if (dexText != null) dexText.text = FormatStat("DEX", stats.dex, bDex);

        bool mostrarBotones = stats.statPoints > 0;
        if (btnVit) btnVit.gameObject.SetActive(mostrarBotones);
        if (btnStr) btnStr.gameObject.SetActive(mostrarBotones);
        if (btnInt) btnInt.gameObject.SetActive(mostrarBotones);
        if (btnDex) btnDex.gameObject.SetActive(mostrarBotones);

        if (expBar != null) expBar.fillAmount = (float)stats.currentExp / stats.expToNextLevel;
        if (expText != null) expText.text = stats.currentExp + " / " + stats.expToNextLevel;

        // 🔥 BONUS EXTRA
        GetExtraBonuses(out string extra);

        if (extraBonusText != null)
            extraBonusText.text = extra;
    }
}