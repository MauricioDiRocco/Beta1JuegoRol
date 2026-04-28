using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int maxHealth = 100;
    public int currentHealth;
    public int maxMana = 50;
    public int currentMana;

    public GameObject damagePopupPrefab;

    [Header("UI Bars")]
    public Image hpBar;
    public Image manaBar;
    public Image expBar;

    [Header("UI Text")]
    public TextMeshProUGUI expText;

    [Header("Panels")]
    public GameObject statsPanel;
    public GameObject inventoryPanel;

    [Header("Targeting")]
    public Transform currentTarget;
    public Image enemyHpBar;
    public GameObject enemyUI;

    private bool isDead = false;
    public bool IsDead => isDead;

    private PlayerController pc;
    private Animator animator;

    [Header("Level System")]
    public int nivel = 1;
    public long currentExp = 0;
    public long expToNextLevel;
    public int statPoints = 0;

    [Header("Audio Settings")]
    public AudioClip levelUpSound;
    public AudioClip swordSwingSound;
    private AudioSource audioSource;

    [Header("Attributes")]
    public int vit = 4;
    public int intel = 3;
    public int str = 6;
    public int dex = 3;

    [Header("Final Stats — base")]
    public int minDamage;
    public int maxDamage;
    public int defense;
    public int magicDamage;

    [Header("Final Stats — bonus de equipo")]
    public float critChance;      // %
    public float armorPen;        // %
    public float lifesteal;       // %
    public float manaOnHit;       // %
    public float attackSpeed;     // %
    public float evasion;         // %
    public float regenHP;         // HP/seg
    public float regenMana;       // Mana/seg
    public float moveSpeedBonus;  // %

    // Regen acumulada (fracción por frame)
    private float regenHPAccum;
    private float regenManaAccum;

    void Awake() { Instance = this; }

    void Start()
    {
        pc = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        CalculateExpRequired();
        CalculateStats();

        currentHealth = maxHealth;
        currentMana = maxMana;

        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        UpdateUI();
        if (enemyUI != null) enemyUI.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;
        HandleInputPanels();
        UpdateEnemyUI();
        ApplyRegen();
    }

    // Regen de HP y Mana por segundo
    void ApplyRegen()
    {
        if (regenHP > 0 && currentHealth < maxHealth)
        {
            regenHPAccum += regenHP * Time.deltaTime;
            if (regenHPAccum >= 1f)
            {
                int heal = Mathf.FloorToInt(regenHPAccum);
                currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
                regenHPAccum -= heal;
                UpdateUI();
            }
        }

        if (regenMana > 0 && currentMana < maxMana)
        {
            regenManaAccum += regenMana * Time.deltaTime;
            if (regenManaAccum >= 1f)
            {
                int regen = Mathf.FloorToInt(regenManaAccum);
                currentMana = Mathf.Min(currentMana + regen, maxMana);
                regenManaAccum -= regen;
                UpdateUI();
            }
        }
    }

    void HandleInputPanels()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel != null)
            {
                bool s = !inventoryPanel.activeSelf;
                inventoryPanel.SetActive(s);
                if (s) UpdateUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (statsPanel != null)
            {
                bool s = !statsPanel.activeSelf;
                statsPanel.SetActive(s);
                if (s) UpdateUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf) inventoryPanel.SetActive(false);
            if (statsPanel != null && statsPanel.activeSelf) statsPanel.SetActive(false);
        }
    }

    void CalculateExpRequired()
    {
        expToNextLevel = (long)(Mathf.Pow(nivel, 3) * 100);
    }

    public void PlaySwordSound()
    {
        if (audioSource != null && swordSwingSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(swordSwingSound);
        }
    }

    public void CalculateStats()
    {
        // ── Resetear todos los bonus ─────────────────────────────────────────
        int bonusDamage = 0;
        int bonusDefense = 0;
        int bonusStr = 0;
        int bonusDex = 0;
        int bonusIntel = 0;
        int bonusVit = 0;
        int bonusHP = 0;

        critChance = 0f;
        armorPen = 0f;
        lifesteal = 0f;
        manaOnHit = 0f;
        attackSpeed = 0f;
        evasion = 0f;
        regenHP = 0f;
        regenMana = 0f;
        moveSpeedBonus = 0f;

        EquipmentSlot[] equipSlots = FindObjectsByType<EquipmentSlot>(FindObjectsSortMode.None);
        foreach (EquipmentSlot slot in equipSlots)
        {
            if (slot.currentItemInstance != null)
            {
                bonusDamage += slot.currentItemInstance.finalDamage;
                bonusDefense += slot.currentItemInstance.finalDefense;

                foreach (ItemStatBonus bonus in slot.currentItemInstance.bonuses)
                {
                    switch (bonus.statType)
                    {
                        // Atributos base
                        case StatType.Strength: bonusStr += bonus.value; break;
                        case StatType.Dexterity: bonusDex += bonus.value; break;
                        case StatType.Intelligence: bonusIntel += bonus.value; break;
                        case StatType.Vitality: bonusVit += bonus.value; break;

                        // Armadura
                        case StatType.BonusHP: bonusHP += bonus.value; break;
                        case StatType.Evasion: evasion += bonus.valueFloat; break;
                        case StatType.RegenHP: regenHP += bonus.valueFloat; break;
                        case StatType.RegenMana: regenMana += bonus.valueFloat; break;
                        case StatType.MoveSpeed: moveSpeedBonus += bonus.valueFloat; break;

                        // Arma
                        case StatType.CritChance: critChance += bonus.valueFloat; break;
                        case StatType.ArmorPen: armorPen += bonus.valueFloat; break;
                        case StatType.Lifesteal: lifesteal += bonus.valueFloat; break;
                        case StatType.ManaOnHit: manaOnHit += bonus.valueFloat; break;
                        case StatType.AttackSpeed: attackSpeed += bonus.valueFloat; break;
                        case StatType.BonusDamage: bonusDamage += bonus.value; break;
                    }
                }
            }
            else if (slot.currentItem != null)
            {
                bonusDamage += slot.currentItem.dañoExtra;
                bonusDefense += slot.currentItem.defensaExtra;
            }
        }

        int totalStr = str + bonusStr;
        int totalDex = dex + bonusDex;
        int totalIntel = intel + bonusIntel;
        int totalVit = vit + bonusVit;

        maxHealth = (totalVit * 50) + bonusHP;
        maxMana = totalIntel * 40;

        minDamage = (totalStr * 3) + bonusDamage;
        maxDamage = (totalStr * 5) + bonusDamage;
        magicDamage = totalIntel * 4;
        defense = (totalVit * 2 + totalDex) + bonusDefense;

        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentMana > maxMana) currentMana = maxMana;

        UpdateUI();
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel) { currentExp -= expToNextLevel; LevelUp(); }
        UpdateUI();
    }

    void LevelUp()
    {
        nivel++;
        statPoints += 3;
        if (audioSource != null && levelUpSound != null) audioSource.PlayOneShot(levelUpSound);
        CalculateExpRequired();
        CalculateStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }

    public void AddVit() { if (statPoints > 0) { vit++; statPoints--; CalculateStats(); } }
    public void AddStr() { if (statPoints > 0) { str++; statPoints--; CalculateStats(); } }
    public void AddInt() { if (statPoints > 0) { intel++; statPoints--; CalculateStats(); } }
    public void AddDex() { if (statPoints > 0) { dex++; statPoints--; CalculateStats(); } }

    public int GetPhysicalDamage()
    {
        int raw = Random.Range(minDamage, maxDamage + 1);

        // Aplicar crítico
        if (critChance > 0 && Random.Range(0f, 100f) < critChance)
            raw = Mathf.RoundToInt(raw * 2f);

        return raw;
    }

    // Llamar desde el sistema de combate al golpear
    public void OnHitEnemy(int damageDone)
    {
        // Lifesteal
        if (lifesteal > 0)
        {
            int heal = Mathf.RoundToInt(damageDone * (lifesteal / 100f));
            currentHealth = Mathf.Min(currentHealth + heal, maxHealth);
        }

        // Mana por golpe
        if (manaOnHit > 0)
        {
            int mana = Mathf.RoundToInt(damageDone * (manaOnHit / 100f));
            currentMana = Mathf.Min(currentMana + mana, maxMana);
        }

        UpdateUI();
    }

    public void SetTarget(Transform target)
    {
        if (isDead) return;
        currentTarget = target;
        if (enemyUI != null) enemyUI.SetActive(target != null);
    }

    public void ClearTarget()
    {
        currentTarget = null;
        if (enemyUI != null) enemyUI.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Aplicar evasión
        if (evasion > 0 && Random.Range(0f, 100f) < evasion)
        {
            ShowDamage(0); // miss
            return;
        }

        // Aplicar penetración de armadura del enemigo (no aplica aquí, aplica en Enemy)
        currentHealth -= damage;
        ShowDamage(damage);
        UpdateUI();
        if (currentHealth <= 0) Die();
    }

    void ShowDamage(int damage)
    {
        if (damagePopupPrefab == null) return;
        Vector3 pos = transform.position + Vector3.up * 2f;
        GameObject popup = Instantiate(damagePopupPrefab, pos, Quaternion.identity);
        DamagePopup dp = popup.GetComponent<DamagePopup>();
        if (damage == 0) { dp.Setup(0); dp.SetColor(Color.gray); }
        else { dp.Setup(damage); dp.SetColor(Color.red); }
    }

    public void UpdateUI()
    {
        if (hpBar != null) hpBar.fillAmount = (float)currentHealth / maxHealth;
        if (manaBar != null) manaBar.fillAmount = (float)currentMana / maxMana;
        if (expBar != null) expBar.fillAmount = (float)currentExp / expToNextLevel;
        if (expText != null) expText.text = currentExp.ToString("N0") + " / " + expToNextLevel.ToString("N0");

        Debug.Log("UI Refrescada. HP: " + currentHealth + "/" + maxHealth);
    }

    void UpdateEnemyUI()
    {
        if (currentTarget == null) { ClearTarget(); return; }
        Enemy enemy = currentTarget.GetComponent<Enemy>();
        if (enemy == null || !enemy.IsAlive()) { ClearTarget(); return; }
        if (enemyHpBar != null) enemyHpBar.fillAmount = enemy.GetHealthPercent();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        ClearTarget();
        UpdateUI();
        if (pc != null) pc.OnPlayerDeath();
    }

    public void RespawnReset()
    {
        isDead = false;
        CalculateStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }
}