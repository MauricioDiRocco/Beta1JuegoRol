using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración del Slot")]
    public ItemType tipoDeEquipo;
    public Image iconImage;

    [Header("Datos Actuales")]
    public ItemData currentItem; // compatibilidad (NO borrar)
    public ItemInstance currentItemInstance; // 🔥 NUEVO (el importante)

    private void Start()
    {
        UpdateUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        ItemDragger dragger = dropped.GetComponent<ItemDragger>();

        InventorySlot sourceSlot = (dragger != null)
            ? dragger.sourceSlot
            : dropped.GetComponentInParent<InventorySlot>();

        if (sourceSlot != null && sourceSlot.item != null)
        {
            if (sourceSlot.item.tipo == tipoDeEquipo)
            {
                EquipItem(sourceSlot);
            }
            else
            {
                Debug.Log("Este ítem no va en este slot. Requerido: " + tipoDeEquipo);
            }
        }
    }

    void EquipItem(InventorySlot source)
    {
        ItemData newItem = source.item;
        ItemInstance newInstance = source.itemInstance;

        // 🔁 INTERCAMBIO
        if (currentItem != null)
        {
            ItemData oldItem = currentItem;
            ItemInstance oldInstance = currentItemInstance;

            currentItem = newItem;
            currentItemInstance = newInstance;

            // devolvemos el anterior al inventario
            if (oldInstance != null)
                source.SetItem(oldInstance);
            else
                source.AddItem(oldItem);
        }
        else
        {
            // SLOT VACÍO
            currentItem = newItem;
            currentItemInstance = newInstance;

            source.ClearSlot();
        }

        UpdateUI();

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.CalculateStats();
        }
    }

    public void UpdateUI()
    {
        if (iconImage == null) return;

        if (currentItem != null)
        {
            iconImage.sprite = currentItem.icono;
            iconImage.enabled = true;
            iconImage.preserveAspect = true;

            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // CLICK DERECHO → DESEQUIPAR
        if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
        {
            bool added = false;

            if (currentItemInstance != null)
                added = InventoryManager.Instance.AddItemInstance(currentItemInstance);
            else
                added = InventoryManager.Instance.AddItem(currentItem);

            if (added)
            {
                currentItem = null;
                currentItemInstance = null;

                UpdateUI();

                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.CalculateStats();
                }
            }
            else
            {
                Debug.LogWarning("¡Inventario lleno! No puedes desequipar.");
            }
        }
    }

    // =============================
    // 🔥 TOOLTIP FIX (SIN ROMPER NADA)
    // =============================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipSystem.Instance == null) return;

        if (currentItemInstance != null)
            TooltipSystem.Instance.Show(currentItemInstance);
        else if (currentItem != null)
            TooltipSystem.Instance.Show(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance?.Hide();
    }
}