using UnityEngine;
using TMPro;

public class GoldDrop : MonoBehaviour
{
    [Header("Ajustes de Valor")]
    public int cantidadOro;

    [Header("Interfaz en el Suelo")]
    public TMP_Text textoCantidad;
    public SpriteRenderer iconoOro;

    [Header("Audio")]
    public AudioClip sonidoRecogida; // Arrastra tu archivo de audio aquí
    [Range(0f, 1f)] public float volumenSonido = 1f;

    private bool yaRecogido = false;

    private void Start()
    {
        if (textoCantidad != null)
        {
            textoCantidad.text = cantidadOro.ToString();
        }

        // Autodestruir en 2 minutos para limpiar el mapa
        Destroy(gameObject, 120f);
    }

    private void Update()
    {
        // Esto hace que la interfaz (Texto e Icono) siempre mire a la cámara
        if (Camera.main != null)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Camera.main.transform.forward);

            if (textoCantidad != null)
            {
                textoCantidad.transform.rotation = lookRotation;
            }

            if (iconoOro != null)
            {
                iconoOro.transform.rotation = lookRotation;
            }
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Clic detectado en moneda");
        RecogerOro();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RecogerOro();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RecogerOro();
        }
    }

    void RecogerOro()
    {
        if (yaRecogido) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distancia = Vector3.Distance(transform.position, player.transform.position);

        if (distancia <= 4.0f)
        {
            yaRecogido = true;

            // --- Lógica de Sonido ---
            if (sonidoRecogida != null)
            {
                // Reproduce el sonido en la posición actual antes de destruir el objeto
                AudioSource.PlayClipAtPoint(sonidoRecogida, transform.position, volumenSonido);
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddOro(cantidadOro);
                Debug.Log("Oro sumado: " + cantidadOro);
            }
            else
            {
                Debug.LogError("¡ERROR: No se encontró el InventoryManager.Instance!");
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Estás muy lejos para recoger: " + distancia);
        }
    }
}