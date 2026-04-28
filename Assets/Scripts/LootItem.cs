using UnityEngine;
using TMPro;

public class LootItem : MonoBehaviour
{
    public ItemData itemData;
    public ItemInstance itemInstance;

    public TextMeshPro nombreTexto;

    public ItemData item
    {
        get => itemData;
        set => itemData = value;
    }

    private Transform camaraPrincipal;
    private float hoverTimer = 0f;
    private bool tooltipShowing = false;

    private void Start()
    {
        camaraPrincipal = Camera.main.transform;

        if (itemData != null)
        {
            if (itemData.EsEquipo())
            {
                itemInstance = new ItemInstance(itemData);
                Debug.Log("✅ INSTANCE GENERADA: " + itemData.nombreItem + " | Rareza: " + itemInstance.rareza);
            }
            else
            {
                itemInstance = null;
            }

            ConfigurarVisuales();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            StartCoroutine(DetenerFisicasLentamente(rb));
        }
    }

    private void Update()
    {
        if (nombreTexto != null && camaraPrincipal != null)
            nombreTexto.transform.LookAt(nombreTexto.transform.position + camaraPrincipal.forward);

        bool mouseEncima = EstaElMouseEncima();

        if (mouseEncima)
        {
            hoverTimer += Time.deltaTime;

            if (hoverTimer >= 0.4f && !tooltipShowing)
            {
                if (TooltipSystem.Instance != null)
                {
                    if (itemInstance != null)
                        TooltipSystem.Instance.Show(itemInstance);
                    else
                        TooltipSystem.Instance.Show(itemData);

                    tooltipShowing = true;
                }
            }
        }
        else
        {
            if (hoverTimer > 0f || tooltipShowing)
            {
                hoverTimer = 0f;
                if (tooltipShowing)
                {
                    TooltipSystem.Instance?.Hide();
                    tooltipShowing = false;
                }
            }
        }
    }

    bool EstaElMouseEncima()
    {
        if (Camera.main == null) return false;

        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
                return true;
        }
        return false;
    }

    public void ConfigurarVisuales()
    {
        if (nombreTexto == null || itemData == null) return;

        nombreTexto.text = itemData.nombreItem;

        // 🔥 Usar rareza de la instancia si existe, si no la del asset base
        ItemRarity rarezaReal = (itemInstance != null) ? itemInstance.rareza : itemData.rareza;

        switch (rarezaReal)
        {
            case ItemRarity.Comun: nombreTexto.color = Color.white; break;
            case ItemRarity.Magico: nombreTexto.color = new Color(0.2f, 0.4f, 1f); break;
            case ItemRarity.Raro: nombreTexto.color = Color.yellow; break;
            case ItemRarity.Unico: nombreTexto.color = new Color(1f, 0.5f, 0f); break;
            case ItemRarity.Excepcional: nombreTexto.color = Color.magenta; break;
        }
    }

    private void OnMouseDown()
    {
        if (PlayerStats.Instance == null) return;

        float distancia = Vector3.Distance(transform.position, PlayerStats.Instance.transform.position);

        if (distancia <= 3f)
            Recoger();
        else
            Debug.Log("Demasiado lejos para recoger el ítem.");
    }

    private void OnDestroy()
    {
        if (tooltipShowing) TooltipSystem.Instance?.Hide();
    }

    private void Recoger()
    {
        if (itemData == null) return;

        // Oro — sin cambios
        if (InventoryManager.Instance != null && itemData == InventoryManager.Instance.itemOroReferencia)
        {
            InventoryManager.Instance.AddOro(100);
            Destroy(gameObject);
            return;
        }

        // ── CHEQUEO DE NIVEL REQUERIDO ──────────────────────────────────
        int nivelItem = (itemInstance != null) ? itemInstance.baseData.nivelRequerido : itemData.nivelRequerido;
        int nivelPlayer = (PlayerStats.Instance != null) ? PlayerStats.Instance.nivel : 1;

        if (nivelPlayer < nivelItem)
        {
            Debug.Log($"❌ Necesitás nivel {nivelItem} para recoger {itemData.nombreItem}. Tu nivel: {nivelPlayer}");
            return; // el ítem queda en el suelo
        }
        // ────────────────────────────────────────────────────────────────

        if (itemInstance != null)
        {
            if (InventoryManager.Instance.AddItemInstance(itemInstance))
            {
                Debug.Log("🔥 Recogido (INSTANCE): " + itemInstance.baseData.nombreItem +
                          " | Rareza: " + itemInstance.rareza +
                          " | Bonus: " + itemInstance.bonuses.Count);
                Destroy(gameObject);
            }
        }
        else
        {
            ItemData instancia = itemData.CreateInstance();
            if (InventoryManager.Instance.AddItem(instancia))
            {
                Debug.Log("Recogido: " + instancia.nombreItem);
                Destroy(gameObject);
            }
        }
    }

    System.Collections.IEnumerator DetenerFisicasLentamente(Rigidbody rb)
    {
        yield return new WaitForSeconds(2f);
        if (rb != null)
        {
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;
        }
    }
}