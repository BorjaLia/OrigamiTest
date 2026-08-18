//using UnityEngine;
//using UnityEngine.InputSystem; // Importante: Añadimos la librería del nuevo Input System

///// <summary>
///// Lee los clics del jugador para doblar el papel o rotar la cámara usando el nuevo Input System.
///// </summary>
//public class InputController : MonoBehaviour
//{
//    [Header("Configuración de Interacción")]
//    // Reducimos la sensibilidad por defecto porque Mouse.current.delta lee píxeles de pantalla
//    public float foldSensitivity = 0.5f;
//    public LayerMask paperLayer;

//    [Header("Estado Actual")]
//    public bool isViewMode = false;

//    private HingeNode selectedHinge = null;
//    private Camera mainCam;

//    private void Start()
//    {
//        mainCam = Camera.main;
//    }

//    private void Update()
//    {
//        // Si estamos en modo observación, el InputController no dobla papel.
//        if (isViewMode) return;

//        // Seguridad: Verificar que hay un ratón conectado
//        if (Mouse.current == null) return;

//        HandleFoldingInput();
//    }

//    private void HandleFoldingInput()
//    {
//        // 1. Al hacer clic (se presionó este frame), lanzar Raycast
//        if (Mouse.current.leftButton.wasPressedThisFrame)
//        {
//            Vector2 mousePos = Mouse.current.position.ReadValue();
//            Ray ray = mainCam.ScreenPointToRay(mousePos);
//            RaycastHit hit;

//            if (Physics.Raycast(ray, out hit, 100f, paperLayer))
//            {
//                // Buscamos el HingeNode en el objeto que tocamos o en su padre (el pivote)
//                selectedHinge = hit.collider.GetComponentInParent<HingeNode>();
//            }
//        }

//        // 2. Mientras mantenemos el clic presionado, doblar
//        if (Mouse.current.leftButton.isPressed && selectedHinge != null)
//        {
//            // Usamos el delta vertical (movimiento Y del ratón) para doblar
//            float mouseDelta = Mouse.current.delta.ReadValue().y * foldSensitivity;
//            selectedHinge.Fold(mouseDelta);
//        }

//        // 3. Al soltar el clic, intentar hacer Snap
//        if (Mouse.current.leftButton.wasReleasedThisFrame && selectedHinge != null)
//        {
//            selectedHinge.CheckSnap();
//            selectedHinge = null; // Soltar la solapa
//        }
//    }

//    public void SetViewMode(bool viewModeEnabled)
//    {
//        isViewMode = viewModeEnabled;
//    }
//}