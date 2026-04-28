using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Referencias UI")]
    public Image iconImage;
    public TextMeshProUGUI keyLabel;
    public TextMeshProUGUI quantityText;
    public GameObject cooldownOverlay;
    public TextMeshProUGUI cooldownText;

    [Header("Datos")]
    public SkillSlotData data;
    public int slotIndex;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // ✅ Objeto visual temporal que se arrastra — el slot NO se mueve
    private GameObject dragIcon;
    private RectTransform dragIconRect;

    public static SkillSlot draggingFrom;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        slotIndex = transform.GetSiblingIndex();

        Transform iconChild = transform.Find("Icon");
        if (iconChild != null)
            iconImage = iconChild.GetComponent<Image>();

        if (iconImage == null)
            Debug.LogWarning("[SkillSlot] No se encontró hijo 'Icon' en " + gameObject.name);
    }

    void Start()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            Color c = iconImage.color;
            c.a = 0f;
            iconImage.color = c;
        }
        RefreshUI();
    }

    private Canvas ObtenerCanvas()
    {
        Canvas c = GetComponentInParent<Canvas>();
        if (c == null) c = FindAnyObjectByType<Canvas>();
        return c;
    }

    public void SetData(SkillSlotData newData)
    {
        data = newData;
        RefreshUI();
    }

    public void RefreshUI()
    {
        string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        if (keyLabel != null) keyLabel.text = keys[slotIndex];

        if (data == null || data.isEmpty)
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                Color c = iconImage.color;
                c.a = 0f;
                iconImage.color = c;
            }
            if (quantityText != null) quantityText.gameObject.SetActive(false);
            if (cooldownOverlay != null) cooldownOverlay.SetActive(false);
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }

        if (quantityText != null)
        {
            if (data.isPotion && data.quantity > 0)
            {
                quantityText.text = "x" + data.quantity;
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        if (cooldownOverlay != null)
            cooldownOverlay.SetActive(data.isOnCooldown);
    }

    // ---------- DRAG ----------
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (data == null || data.isEmpty) { eventData.pointerDrag = null; return; }

        canvas = ObtenerCanvas();
        if (canvas == null) { eventData.pointerDrag = null; return; }

        draggingFrom = this;

        // ✅ FIX: creamos un ícono flotante temporal en lugar de mover el slot
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform);
        dragIcon.transform.SetAsLastSibling();

        Image dragImage = dragIcon.AddComponent<Image>();
        dragImage.sprite = data.icon;
        dragImage.raycastTarget = false; // no interfiere con el raycast del drop

        dragIconRect = dragIcon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = rectTransform.sizeDelta;
        dragIconRect.anchoredPosition = rectTransform.position;
        dragIconRect.localScale = Vector3.one;

        // Posicionamos el dragIcon donde está el cursor
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );
        dragIconRect.localPosition = localPos;

        // El slot se queda fijo — solo ocultamos levemente el ícono original
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 0.3f;
            iconImage.color = c;
        }

        // blocksRaycasts false para que el drop receiver detecte el slot destino
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Movemos solo el ícono flotante
        if (dragIcon != null && canvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPos
            );
            dragIconRect.localPosition = localPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ✅ Destruimos el ícono flotante
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
            dragIconRect = null;
        }

        // Restauramos alpha del ícono original
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = (data == null || data.isEmpty) ? 0f : 1f;
            iconImage.color = c;
        }

        // Si soltó fuera de la UI, devolver al inventario
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (data != null && !data.isEmpty && data.sourceItem != null && InventoryManager.Instance != null)
            {
                bool devuelto = InventoryManager.Instance.AddItem(data.sourceItem);
                if (devuelto)
                {
                    data.SetEmpty();
                    RefreshUI();
                    draggingFrom = null;
                    return;
                }
            }
        }

        draggingFrom = null;
    }

    // ---------- CLICK DERECHO ----------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (data == null || !data.isPotion || data.quantity <= 0) return;

        PlayerStats player = PlayerStats.Instance;
        if (player == null) return;

        if (player.currentHealth < player.maxHealth)
        {
            player.currentHealth = Mathf.Min(player.currentHealth + data.healAmount, player.maxHealth);
            player.UpdateUI();
            data.quantity--;
            if (data.quantity <= 0) data.SetEmpty();
            RefreshUI();
        }
    }
}