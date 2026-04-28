using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configuración")]
    public GameObject inventoryPanel;
    public List<InventorySlot> allSlots = new List<InventorySlot>();

    [Header("Sistema de Oro")]
    public int oroActual;
    public TextMeshProUGUI oroTexto;
    public ItemData itemOroReferencia;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateOroUI();

        if (inventoryPanel != null)
        {
            InventorySlot[] slotsFound = inventoryPanel.GetComponentsInChildren<InventorySlot>(true);
            allSlots.Clear();
            allSlots.AddRange(slotsFound);

            foreach (InventorySlot slot in allSlots)
                slot.UpdateUI();
        }
        else
        {
            Debug.LogError("❌ No has asignado el InventoryPanel");
        }
    }

    // =========================
    // 💰 ORO
    // =========================
    public void AddOro(int cantidad)
    {
        oroActual += cantidad;
        UpdateOroUI();
        Debug.Log("Oro añadido: " + cantidad + " | Total: " + oroActual);
    }

    public void UpdateOroUI()
    {
        if (oroTexto != null)
            oroTexto.text = oroActual.ToString("N0");
    }

    // =========================
    // 📦 SISTEMA VIEJO (NO TOCAR)
    // =========================
    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        if (item.esAcumulable)
        {
            foreach (InventorySlot slot in allSlots)
            {
                if (slot.item != null && slot.item.nombreItem == item.nombreItem)
                {
                    slot.item.cantidad += item.cantidad;
                    slot.UpdateUI();
                    return true;
                }
            }
        }

        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == null)
            {
                ItemData itemParaAñadir = item;

                if (!item.name.Contains("(Clone)"))
                    itemParaAñadir = item.CreateInstance();

                slot.AddItem(itemParaAñadir);
                slot.UpdateUI();
                return true;
            }
        }

        Debug.LogWarning("¡Inventario lleno!");
        return false;
    }

    // =========================
    // 🔥 NUEVO SISTEMA (ITEM INSTANCE)
    // =========================
    public bool AddItemInstance(ItemInstance instance)
    {
        if (instance == null) return false;

        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == null)
            {
                // 🔥 FIX: una sola llamada que setea item + itemInstance correctamente
                slot.AddItemInstance(instance);

                Debug.Log("Item RANDOM añadido: " + instance.baseData.nombreItem +
                          " | Bonus: " + instance.bonuses.Count);
                return true;
            }
        }

        Debug.LogWarning("¡Inventario lleno!");
        return false;
    }

    // =========================
    public int GetEmptySlotsCount()
    {
        int count = 0;
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == null) count++;
        }
        return count;
    }

    public void RefreshAllSlots()
    {
        foreach (InventorySlot slot in allSlots)
            slot.UpdateUI();
    }
}