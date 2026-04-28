using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI cantidadText;

    public ItemData item;
    public ItemInstance itemInstance;

    [Header("Configuración de Drop")]
    public GameObject lootPrefabReferencia;

    // =============================
    // 🔹 AGREGAR ITEM NORMAL
    // =============================
    public void AddItem(ItemData newItem)
    {
        item = newItem;
        itemInstance = null; // 🔥 SIEMPRE limpiar instance

        UpdateUI();
    }

    // =============================
    // 🔥 AGREGAR ITEM INSTANCE
    // =============================
    public void AddItemInstance(ItemInstance instance)
    {
        itemInstance = instance;
        item = instance.baseData;

        UpdateUI();
    }

    // 🔥 NECESARIO para EquipmentSlot
    public void SetItem(ItemInstance instance)
    {
        itemInstance = instance;
        item = instance.baseData;

        UpdateUI();
    }

    // =============================
    public void UpdateUI()
    {
        if (item != null)
        {
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.enabled = true;
                iconImage.sprite = item.icono;
                iconImage.preserveAspect = true;

                Color c = iconImage.color;
                c.a = 1f;
                iconImage.color = c;

                iconImage.rectTransform.anchoredPosition = Vector2.zero;
                iconImage.rectTransform.localScale = Vector3.one;
            }

            if (cantidadText != null)
            {
                // 🔥 SOLO STACKEA ITEMS NORMALES
                if (itemInstance == null && item.esAcumulable && item.cantidad > 1)
                {
                    cantidadText.text = item.cantidad.ToString();
                    cantidadText.gameObject.SetActive(true);
                }
                else
                {
                    cantidadText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        item = null;
        itemInstance = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (cantidadText != null)
        {
            cantidadText.text = "";
            cantidadText.gameObject.SetActive(false);
        }
    }

    // =============================
    // 🔹 CLICK DERECHO → USAR ITEM
    // =============================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            UseItem();
    }

    void UseItem()
    {
        if (item == null) return;

        // 🔥 NO usar items random
        if (itemInstance != null) return;

        if (item.curaVida <= 0) return;

        PlayerStats player = PlayerStats.Instance;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();

        if (player == null) return;

        if (player.currentHealth < player.maxHealth)
        {
            player.currentHealth = Mathf.Min(player.currentHealth + item.curaVida, player.maxHealth);
            player.UpdateUI();

            if (item.esConsumible)
            {
                item.cantidad--;

                if (item.cantidad <= 0)
                    ClearSlot();
                else
                    UpdateUI();
            }
        }
    }

    // =============================
    // 🔥 TOOLTIP (FIX REAL)
    // =============================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipSystem.Instance == null) return;

        // 🔥 SIEMPRE prioriza instance
        if (itemInstance != null)
            TooltipSystem.Instance.Show(itemInstance);
        else if (item != null)
            TooltipSystem.Instance.Show(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance?.Hide();
    }

    // =============================
    // 🔹 DRAG & DROP
    // =============================
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        ItemDragger dragger = dropped.GetComponent<ItemDragger>();

        if (dragger != null)
        {
            InventorySlot sourceSlot = dragger.sourceSlot;

            if (sourceSlot == null || sourceSlot == this) return;
            if (sourceSlot.item == null) return;

            // 🔥 SI HAY INSTANCE → SIEMPRE SWAP
            if (sourceSlot.itemInstance != null || this.itemInstance != null)
            {
                SwapSlots(sourceSlot);
                return;
            }

            ItemData itemDestino = this.item;
            ItemData itemOrigen = sourceSlot.item;

            // 🔥 STACK SOLO PARA ITEMS BASE
            if (itemDestino != null
                && itemDestino.esAcumulable
                && itemOrigen.esAcumulable
                && itemDestino.nombreItem == itemOrigen.nombreItem)
            {
                itemDestino.cantidad += itemOrigen.cantidad;
                sourceSlot.ClearSlot();
                UpdateUI();
                return;
            }

            this.AddItem(itemOrigen);

            if (itemDestino != null)
                sourceSlot.AddItem(itemDestino);
            else
                sourceSlot.ClearSlot();
        }
    }

    // =============================
    // 🔥 SWAP REAL (CON INSTANCE)
    // =============================
    void SwapSlots(InventorySlot other)
    {
        ItemData tempItem = other.item;
        ItemInstance tempInstance = other.itemInstance;

        other.item = this.item;
        other.itemInstance = this.itemInstance;

        this.item = tempItem;
        this.itemInstance = tempInstance;

        this.UpdateUI();
        other.UpdateUI();
    }
}