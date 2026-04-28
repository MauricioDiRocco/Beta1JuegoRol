using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SkillSlot))]
public class SkillSlotDropReceiver : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private SkillSlot skillSlot;
    private Image bgImage;
    private Color normalColor;
    private Color highlightColor = new Color(0.5f, 0.4f, 1f, 0.4f);

    void Awake()
    {
        skillSlot = GetComponent<SkillSlot>();
        bgImage = GetComponent<Image>();
        if (bgImage != null) normalColor = bgImage.color;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = normalColor;

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        // CASO 1: viene del inventario
        ItemDragger dragger = dropped.GetComponent<ItemDragger>();
        if (dragger != null && dragger.sourceSlot != null && dragger.sourceSlot.item != null)
        {
            HotbarManager.Instance.AssignItemToSlot(dragger.sourceSlot.item, skillSlot.slotIndex);
            dragger.sourceSlot.ClearSlot();
            return;
        }

        // CASO 2: swap entre slots del hotbar
        SkillSlot origen = SkillSlot.draggingFrom;
        if (origen == null || origen == skillSlot) return;

        SkillSlotData bufferTemp = ScriptableObject.CreateInstance<SkillSlotData>();
        bufferTemp.CopyFrom(skillSlot.data);

        skillSlot.data.CopyFrom(origen.data);
        origen.data.CopyFrom(bufferTemp);

        Destroy(bufferTemp);

        skillSlot.RefreshUI();
        origen.RefreshUI();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        if (bgImage != null) bgImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = normalColor;
    }
}