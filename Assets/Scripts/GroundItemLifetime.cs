using UnityEngine;
using System.Collections;

public class GroundItemLifetime : MonoBehaviour
{
    [Header("Configuración")]
    public float lifetime = 30f;
    public float blinkTime = 5f;
    public float blinkSpeed = 0.2f;

    private float timer;
    private bool isBlinking = false;

    private Renderer[] renderers;

    void OnEnable()
    {
        // 🔥 RESET TOTAL cada vez que aparece
        timer = 0f;
        isBlinking = false;

        renderers = GetComponentsInChildren<Renderer>();

        SetVisible(true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 🔥 Empezar a parpadear
        if (!isBlinking && timer >= (lifetime - blinkTime))
        {
            isBlinking = true;
            StartCoroutine(Blink());
        }

        // 🔥 Destruir
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Blink()
    {
        while (true)
        {
            SetVisible(false);
            yield return new WaitForSeconds(blinkSpeed);

            SetVisible(true);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    void SetVisible(bool value)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = value;
        }
    }
}