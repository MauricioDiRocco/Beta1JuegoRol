using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventorySlot inventorySlot;
    private EquipmentSlot equipmentSlot;
    private SkillSlot skillSlot;

    private float delay = 0.4f;
    private float timer = 0f;
    private bool isHovering = false;

    void Awake()
    {
        inventorySlot = GetComponent<InventorySlot>();
        equipmentSlot = GetComponent<EquipmentSlot>();
        skillSlot = GetComponent<SkillSlot>();
    }

    void Update()
    {
        if (!isHovering) return;
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            ShowTooltip();
            isHovering = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        isHovering = true;
        timer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        timer = 0f;
        TooltipSystem.Instance?.Hide();
    }

    void ShowTooltip()
    {
        // SkillSlot tiene prioridad propia
        if (skillSlot != null && skillSlot.data != null && !skillSlot.data.isEmpty)
        {
            TooltipSystem.Instance?.ShowSkill(skillSlot.data);
            return;
        }

        // 🔥 FIX: Prioridad ItemInstance en InventorySlot
        if (inventorySlot != null)
        {
            if (inventorySlot.itemInstance != null)
            {
                TooltipSystem.Instance?.Show(inventorySlot.itemInstance);
                return;
            }
            if (inventorySlot.item != null)
            {
                TooltipSystem.Instance?.Show(inventorySlot.item);
                return;
            }
        }

        // 🔥 FIX: Prioridad ItemInstance en EquipmentSlot (campo correcto: currentItemInstance)
        if (equipmentSlot != null)
        {
            if (equipmentSlot.currentItemInstance != null)
            {
                TooltipSystem.Instance?.Show(equipmentSlot.currentItemInstance);
                return;
            }
            if (equipmentSlot.currentItem != null)
            {
                TooltipSystem.Instance?.Show(equipmentSlot.currentItem);
                return;
            }
        }
    }
}