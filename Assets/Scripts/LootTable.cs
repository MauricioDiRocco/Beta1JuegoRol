using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemData item;
    [Range(0f, 100f)] public float probabilidad;
}

[CreateAssetMenu(fileName = "Nueva Tabla de Botin", menuName = "Sistema Inventario/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootEntry> posiblesDrops = new List<LootEntry>();

    // Método original — sin cambios, para no romper nada
    public ItemData GetRandomItem()
    {
        return GetRandomItem(99); // sin filtro = acepta todo
    }

    // Método nuevo — filtra por nivel del enemigo
    // Solo dropea ítems cuyo nivelRequerido <= nivelMaximo
    public ItemData GetRandomItem(int nivelMaximo)
    {
        // Construimos lista filtrada
        List<LootEntry> filtradas = new List<LootEntry>();
        foreach (LootEntry entrada in posiblesDrops)
        {
            if (entrada.item != null && entrada.item.nivelRequerido <= nivelMaximo)
                filtradas.Add(entrada);
        }

        if (filtradas.Count == 0) return null;

        float azar = Random.Range(0f, 100f);
        float acumulado = 0;

        foreach (LootEntry entrada in filtradas)
        {
            acumulado += entrada.probabilidad;
            if (azar <= acumulado)
                return entrada.item;
        }

        return null;
    }
}