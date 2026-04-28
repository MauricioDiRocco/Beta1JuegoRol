using UnityEngine;

public class WorldItemTooltip : MonoBehaviour
{
    private LootItem lootItem;
    private bool mouseOver = false;
    private float hoverTimer = 0f;
    private float delay = 0.3f;
    private bool tooltipShowing = false;

    void Awake()
    {
        lootItem = GetComponent<LootItem>();

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sc = gameObject.AddComponent<SphereCollider>();
            sc.radius = 0.4f;
            sc.isTrigger = true;
        }
    }

    void Update()
    {
        if (!mouseOver)
        {
            if (tooltipShowing)
            {
                TooltipSystem.Instance?.Hide();
                tooltipShowing = false;
            }
            hoverTimer = 0f;
            return;
        }

        hoverTimer += Time.deltaTime;

        if (hoverTimer >= delay && !tooltipShowing)
        {
            if (lootItem != null)
            {
                // 🔥 FIX CLAVE: PRIORIDAD INSTANCE
                if (lootItem.itemInstance != null)
                {
                    TooltipSystem.Instance?.Show(lootItem.itemInstance);
                }
                else if (lootItem.item != null)
                {
                    TooltipSystem.Instance?.Show(lootItem.item);
                }

                tooltipShowing = true;
            }
        }
    }

    void OnMouseEnter()
    {
        mouseOver = true;
    }

    void OnMouseExit()
    {
        mouseOver = false;
        tooltipShowing = false;
        TooltipSystem.Instance?.Hide();
    }

    void OnDestroy()
    {
        if (tooltipShowing)
            TooltipSystem.Instance?.Hide();
    }
}