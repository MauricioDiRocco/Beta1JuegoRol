using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    [Header("Referencias")]
    public InventorySlot sourceSlot;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();

        // 🔥 FIX: buscar el slot recorriendo TODOS los padres, no solo el inmediato
        if (sourceSlot == null)
            sourceSlot = BuscarSlotEnPadres();
    }

    // Recorre la jerarquía hacia arriba hasta encontrar un InventorySlot
    InventorySlot BuscarSlotEnPadres()
    {
        Transform t = transform.parent;
        while (t != null)
        {
            InventorySlot slot = t.GetComponent<InventorySlot>();
            if (slot != null) return slot;
            t = t.parent;
        }
        return null;
    }

    private Canvas ObtenerCanvas()
    {
        Canvas c = GetComponentInParent<Canvas>();
        if (c == null) c = FindAnyObjectByType<Canvas>();
        return c;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sourceSlot != null)
            sourceSlot.OnPointerClick(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 🔥 FIX: si no se encontró en Awake, intentar de nuevo con la búsqueda robusta
        if (sourceSlot == null)
            sourceSlot = BuscarSlotEnPadres();

        if (sourceSlot == null || sourceSlot.item == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        canvas = ObtenerCanvas();

        if (canvas == null)
        {
            Debug.LogError("[ItemDragger] No se encontró ningún Canvas en la escena.");
            eventData.pointerDrag = null;
            return;
        }

        originalParent = transform.parent;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Detectar si soltamos en el HOTBAR
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            SkillSlot skillSlot = eventData.pointerCurrentRaycast.gameObject
                .GetComponentInParent<SkillSlot>();

            if (skillSlot != null && sourceSlot != null && sourceSlot.item != null)
            {
                HotbarManager.Instance.AssignItemToSlot(sourceSlot.item, skillSlot.slotIndex);
                FinalizarArrastre();
                return;
            }
        }

        if (!EventSystem.current.IsPointerOverGameObject())
            TirarObjetoAlSuelo();

        FinalizarArrastre();
    }

    private void FinalizarArrastre()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        RegresarAlPadreOriginal();
    }

    void TirarObjetoAlSuelo()
    {
        if (sourceSlot == null || sourceSlot.item == null || sourceSlot.lootPrefabReferencia == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 0.5f;
        GameObject drop = Instantiate(sourceSlot.lootPrefabReferencia, spawnPos, Quaternion.identity);

        LootItem li = drop.GetComponent<LootItem>();
        if (li != null)
        {
            // 🔥 FIX: preservar instancia random al tirar al suelo
            if (sourceSlot.itemInstance != null)
            {
                li.itemData = sourceSlot.itemInstance.baseData;
                li.itemInstance = sourceSlot.itemInstance;
            }
            else
            {
                li.item = sourceSlot.item;
            }
            li.ConfigurarVisuales();
        }

        sourceSlot.ClearSlot();
    }

    private void RegresarAlPadreOriginal()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}