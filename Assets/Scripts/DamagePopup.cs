using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text;

    public float speed = 2f;
    public float lifeTime = 1f;

    void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>();

        if (text == null)
            Debug.LogError("❌ NO SE ENCONTRÓ TEXT EN EL PREFAB");
    }

    public void Setup(int damage)
    {
        if (text == null)
        {
            Debug.LogError("❌ TEXT ES NULL");
            return;
        }

        text.text = damage.ToString();
    }

    // 🔥 NUEVO: cambiar color sin romper nada
    public void SetColor(Color color)
    {
        if (text != null)
        {
            text.color = color;
        }
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
            Destroy(gameObject);
    }
}