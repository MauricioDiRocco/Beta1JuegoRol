using UnityEngine;
using System.Collections;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance;

    [Header("Slots del Hotbar (0=key1 ... 9=key0)")]
    public SkillSlot[] slots = new SkillSlot[10];

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        foreach (SkillSlot slot in slots)
        {
            if (slot != null && slot.data != null)
            {
                slot.data.SetEmpty();
                slot.RefreshUI();
            }
        }
    }

    void Update()
    {
        CheckKeyInput();
    }

    void CheckKeyInput()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                ActivateSlot(i);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) ActivateSlot(9);
    }

    public void ActivateSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        SkillSlot slot = slots[index];
        if (slot == null || slot.data == null || slot.data.isEmpty) return;

        if (slot.data.isPotion)
            UsePotion(slot);
        else
            UseSkill(slot);
    }

    void UsePotion(SkillSlot slot)
    {
        PlayerStats player = PlayerStats.Instance;
        if (player == null || slot.data.quantity <= 0) return;

        player.currentHealth = Mathf.Min(player.currentHealth + slot.data.healAmount, player.maxHealth);
        player.UpdateUI();
        slot.data.quantity--;
        if (slot.data.quantity <= 0) slot.data.SetEmpty();
        slot.RefreshUI();
    }

    void UseSkill(SkillSlot slot)
    {
        if (slot.data.isOnCooldown) return;
        Debug.Log("Usando habilidad: " + slot.data.skillName);
    }

    public IEnumerator StartCooldown(SkillSlot slot, float duration)
    {
        slot.data.isOnCooldown = true;
        slot.data.cooldownRemaining = duration;
        slot.RefreshUI();

        while (slot.data.cooldownRemaining > 0f)
        {
            slot.data.cooldownRemaining -= Time.deltaTime;
            yield return null;
        }

        slot.data.isOnCooldown = false;
        slot.data.cooldownRemaining = 0f;
        slot.RefreshUI();
    }

    public bool AssignItemToSlot(ItemData item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        return AssignItemToSkillSlot(item, slots[slotIndex]);
    }

    public bool AssignItemToSkillSlot(ItemData item, SkillSlot slot)
    {
        if (slot == null || slot.data == null) return false;

        // ✅ FIX APILADO: si el slot ya tiene el mismo tipo de item, sumamos cantidad
        if (!slot.data.isEmpty && slot.data.skillName == item.nombreItem)
        {
            slot.data.quantity += item.cantidad;

            // Sincronizamos el sourceItem con la cantidad actualizada
            if (slot.data.sourceItem != null)
                slot.data.sourceItem.cantidad = slot.data.quantity;

            slot.RefreshUI();
            return true;
        }

        // Slot vacío o item distinto — asignamos normalmente
        slot.data.skillName = item.nombreItem;
        slot.data.icon = item.icono;
        slot.data.isPotion = item.curaVida > 0;
        slot.data.healAmount = item.curaVida;
        slot.data.quantity = item.cantidad;
        slot.data.isEmpty = false;
        slot.data.sourceItem = item;

        slot.RefreshUI();
        return true;
    }
}