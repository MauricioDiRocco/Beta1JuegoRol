using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float distance = 10f;
    public float height = 8f;

    public float rotationSpeed = 5f;
    public float smoothSpeed = 10f;

    public float minVerticalAngle = 20f;
    public float maxVerticalAngle = 70f;

    private float currentYaw = 0f;   // izquierda/derecha
    private float currentPitch = 45f; // arriba/abajo

    void LateUpdate()
    {
        if (target == null) return;

        // 🖱 ROTACIÓN CON CLICK DERECHO
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentYaw += mouseX * rotationSpeed * 100f * Time.deltaTime;

            currentPitch -= mouseY * rotationSpeed * 100f * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        }

        // 🎯 ROTACIÓN FINAL
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        Vector3 desiredPosition = target.position + offset + Vector3.up * height;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // 🎥 MIRAR AL PLAYER
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}