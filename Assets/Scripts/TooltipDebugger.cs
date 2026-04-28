using UnityEngine;

public class TooltipDebugger : MonoBehaviour
{
    void Start()
    {
        // Test 1: ver si tiene collider
        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError("❌ SIN COLLIDER en " + gameObject.name);
        else
            Debug.Log("✅ Collider OK: " + col.GetType().Name +
                      " | isTrigger: " + col.isTrigger);

        // Test 2: ver si existe el TooltipSystem
        if (TooltipSystem.Instance == null)
            Debug.LogError("❌ TooltipSystem.Instance es NULL");
        else
            Debug.Log("✅ TooltipSystem encontrado");

        // Test 3: ver si tiene LootItem con item asignado
        LootItem li = GetComponent<LootItem>();
        if (li == null)
            Debug.LogError("❌ No tiene componente LootItem");
        else if (li.item == null)
            Debug.LogError("❌ LootItem.item es NULL");
        else
            Debug.Log("✅ Item: " + li.item.nombreItem);

        // Test 4: buscar la camara
        if (Camera.main == null)
            Debug.LogError("❌ Camera.main es NULL - falta tag MainCamera");
        else
        {
            var raycaster = Camera.main.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
            if (raycaster == null)
                Debug.LogError("❌ Physics Raycaster NO está en la cámara");
            else
                Debug.Log("✅ Physics Raycaster OK");
        }
    }

    void OnMouseEnter()
    {
        Debug.Log("🖱️ OnMouseEnter disparado en: " + gameObject.name);
    }

    void OnMouseOver()
    {
        // Esto se llama cada frame mientras el mouse está encima
        // Si no ves este log, el problema es el collider o la cámara
        Debug.Log("🖱️ OnMouseOver en: " + gameObject.name);
    }

    void OnMouseExit()
    {
        Debug.Log("🖱️ OnMouseExit en: " + gameObject.name);
    }
}