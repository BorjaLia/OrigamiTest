using UnityEngine;
using UnityEngine.InputSystem; // Añadido para el nuevo Input System

/// <summary>
/// Controla la cámara. En "View Mode", permite orbitar alrededor del papel.
/// </summary>
public class CameraController : MonoBehaviour
{
    public Transform target; // El centro del papel o de la mesa
    public float orbitSpeed = 15f; // Reducido ligeramente para el nuevo delta del ratón

    private InputController inputController;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        inputController = FindObjectOfType<InputController>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        //if (inputController == null || !inputController.isViewMode) return;
        if (Mouse.current == null) return;

        // Solo orbitar si se mantiene el clic izquierdo presionado en modo View
        if (Mouse.current.leftButton.isPressed)
        {
            // Leer el movimiento X e Y del ratón en este frame
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float mouseX = mouseDelta.x;
            float mouseY = mouseDelta.y;

            if (target != null)
            {
                // Orbitar horizontalmente
                transform.RotateAround(target.position, Vector3.up, mouseX * orbitSpeed * Time.deltaTime);

                // Orbitar verticalmente
                transform.RotateAround(target.position, transform.right, -mouseY * orbitSpeed * Time.deltaTime);

                transform.LookAt(target); // Mantener la mirada en el centro
            }
        }
    }

    /// <summary>
    /// Vuelve la cámara a la posición de doblaje original
    /// </summary>
    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}