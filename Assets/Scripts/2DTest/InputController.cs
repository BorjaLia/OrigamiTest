using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Permite al jugador dibujar una línea (Swipe) para doblar el papel.
/// Usa LineRenderer para dar feedback visual e incorpora Snapping Inteligente y visual.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class InputController : MonoBehaviour
{
    [Header("Snapping Global (Tapete de Corte)")]
    [Tooltip("Activa una cuadrícula de puntos fijos en el fondo.")]
    [SerializeField] private bool enableGridSnap = true;
    [Tooltip("Separación entre los puntos de la cuadrícula global.")]
    [SerializeField] private float gridSpacing = 1f;
    [Tooltip("Tamaño de la cuadrícula desde el centro (Ej: 5 cubre un área de 10x10).")]
    [SerializeField] private float gridSize = 5f;

    [Header("Snapping de Geometría (Vértices y Aristas)")]
    [Tooltip("Activa o desactiva el imán hacia los puntos del propio papel.")]
    [SerializeField] private bool enablePointSnap = true;
    [Tooltip("Cantidad de subdivisiones en las aristas. (Ej: 2 = esquinas y centro).")]
    [Min(1)]
    [SerializeField] private int edgeDivisions = 3;

    [Header("Filtros de Snapping")]
    [Tooltip("Distancia máxima en unidades para que el imán atrape el ratón.")]
    [SerializeField] private float snapRadius = 0.5f;
    [Tooltip("¡LA SOLUCIÓN A LA EXPLOSIÓN! Distancia mínima permitida entre dos puntos de snap.")]
    [SerializeField] private float minPointDistance = 0.2f;

    [Header("Snapping de Ángulos (Ortogonal)")]
    [Tooltip("Activa o desactiva forzar el doblez a ángulos rectos/diagonales.")]
    [SerializeField] private bool enableAngleSnap = true;
    [Tooltip("El múltiplo de ángulo permitido (Ej. 15 permitirá 15, 30, 45, 60...).")]
    [SerializeField] private float angleSnapStep = 15f;
    [Tooltip("Grados de tolerancia para que el doblez se ajuste automáticamente al múltiplo.")]
    [SerializeField] private float angleSnapTolerance = 10f;

    [Header("Feedback Visual (Debug)")]
    [Tooltip("Muestra puntitos magenta en todas las posiciones válidas de snapping.")]
    [SerializeField] private bool showSnapPoints = true;

    private Camera mainCam;
    private LineRenderer lineRenderer;

    private Vector2 startPos;
    private bool isDragging = false;

    // --- Feedback Visual ---
    private GameObject startRealMarker;
    private GameObject startSnapMarker;
    private GameObject endRealMarker;
    private GameObject endSnapMarker;

    private List<GameObject> magentaSnapMarkers = new List<GameObject>();

    private void Start()
    {
        mainCam = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();

        // Configurar el estilo de la línea
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Crear marcadores visuales (Esferas) por código
        startRealMarker = CreateMarker("StartReal", Color.blue);
        startSnapMarker = CreateMarker("StartSnap", Color.green);
        endRealMarker = CreateMarker("EndReal", Color.blue);
        endSnapMarker = CreateMarker("EndSnap", Color.green);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // Actualizar la visualización de los puntos magenta en los bordes y cuadrícula
        UpdateMagentaMarkers();

        // 1. Iniciar el agarre (Punto de inicio)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;

            // Posiciones
            Vector2 rawStart = GetMouseWorldPosition();
            startPos = GetSnappedPosition(rawStart);

            // Ocultar marcadores del final (nuevo trazo)
            endRealMarker.SetActive(false);
            endSnapMarker.SetActive(false);

            // Mostrar y posicionar marcadores de inicio (Z = -2 para que queden por encima del papel)
            startRealMarker.transform.position = new Vector3(rawStart.x, rawStart.y, -2f);
            startSnapMarker.transform.position = new Vector3(startPos.x, startPos.y, -2f);

            startRealMarker.SetActive(true);
            startSnapMarker.SetActive(true);
        }

        // 2. Arrastrar (calcular y dibujar línea perpendicular/mediatriz)
        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            Vector2 currentPos = GetSnappedPosition(GetMouseWorldPosition());

            // Mostrar línea solo si se ha arrastrado un poco para evitar parpadeos
            if (Vector2.Distance(startPos, currentPos) > 0.2f)
            {
                Vector2 foldStart, foldEnd;
                CalculateFoldLine(startPos, currentPos, out foldStart, out foldEnd);

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, new Vector3(foldStart.x, foldStart.y, -1f));
                lineRenderer.SetPosition(1, new Vector3(foldEnd.x, foldEnd.y, -1f));
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }

        // 3. Soltar (Ejecutar el doblez)
        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;

            // Posiciones
            Vector2 rawEnd = GetMouseWorldPosition();
            Vector2 endPos = GetSnappedPosition(rawEnd);

            // Mostrar y posicionar marcadores del final
            endRealMarker.transform.position = new Vector3(rawEnd.x, rawEnd.y, -2f);
            endSnapMarker.transform.position = new Vector3(endPos.x, endPos.y, -2f);

            endRealMarker.SetActive(true);
            endSnapMarker.SetActive(true);

            // Ocultar línea
            lineRenderer.positionCount = 0;

            // Solo doblar si el trazo tiene una longitud mínima
            if (Vector2.Distance(startPos, endPos) > 0.2f)
            {
                Vector2 foldStart, foldEnd;
                CalculateFoldLine(startPos, endPos, out foldStart, out foldEnd);

                Debug.Log("Enviando orden de doblez perpendicular al FoldManager...");
                FoldManager.Instance.ProcessFold(foldStart, foldEnd);
            }
        }
    }

    /// <summary>
    /// Calcula la mediatriz (línea perpendicular en el punto medio) entre el inicio y el fin del arrastre.
    /// </summary>
    private void CalculateFoldLine(Vector2 dragStart, Vector2 dragEnd, out Vector2 lineStart, out Vector2 lineEnd)
    {
        Vector2 midPoint = (dragStart + dragEnd) / 2f;
        Vector2 dragDir = (dragEnd - dragStart).normalized;
        dragDir = SnapDirection(dragDir);

        Vector2 perpDir = new Vector2(dragDir.y, -dragDir.x);
        float lineLength = 20f;
        lineStart = midPoint - perpDir * lineLength;
        lineEnd = midPoint + perpDir * lineLength;
    }

    /// <summary>
    /// Recopila todos los puntos de snapping válidos (Cuadrícula Global + Geometría filtrada).
    /// </summary>
    private List<Vector2> GetAllValidSnapPoints()
    {
        List<Vector2> points = new List<Vector2>();

        // 1. Añadir Cuadrícula Global (Grid)
        if (enableGridSnap)
        {
            for (float x = -gridSize; x <= gridSize; x += gridSpacing)
            {
                for (float y = -gridSize; y <= gridSize; y += gridSpacing)
                {
                    TryAddSnapPoint(points, new Vector2(x, y));
                }
            }
        }

        // 2. Añadir Geometría del Papel (Vértices y Aristas)
        if (enablePointSnap && FoldManager.Instance != null)
        {
            foreach (var face in FoldManager.Instance.activeFaces)
            {
                int vertCount = face.vertices.Count;
                for (int i = 0; i < vertCount; i++)
                {
                    Vector2 p1 = face.vertices[i];
                    Vector2 p2 = face.vertices[(i + 1) % vertCount];

                    // Añadimos el vértice principal (t = 0) y los puntos intermedios
                    for (int d = 0; d < edgeDivisions; d++)
                    {
                        float t = (float)d / edgeDivisions;
                        TryAddSnapPoint(points, Vector2.Lerp(p1, p2, t));
                    }
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Función auxiliar que actúa como filtro: Solo añade el punto si no hay otro muy cerca.
    /// Resuelve el problema de la "explosión de puntos".
    /// </summary>
    private void TryAddSnapPoint(List<Vector2> list, Vector2 newPoint)
    {
        foreach (Vector2 existingPoint in list)
        {
            // Si el punto está muy cerca de uno ya registrado, lo descartamos
            if (Vector2.Distance(existingPoint, newPoint) < minPointDistance)
            {
                return;
            }
        }
        // Si sobrevivió al chequeo, lo añadimos
        list.Add(newPoint);
    }

    /// <summary>
    /// Busca entre los puntos filtrados y atrae el cursor hacia el más cercano.
    /// </summary>
    private Vector2 GetSnappedPosition(Vector2 originalPos)
    {
        if (!enablePointSnap && !enableGridSnap) return originalPos;

        Vector2 closestPoint = originalPos;
        float closestDistance = snapRadius;
        bool foundSnap = false;

        List<Vector2> validPoints = GetAllValidSnapPoints();

        foreach (Vector2 snapCandidate in validPoints)
        {
            float dist = Vector2.Distance(originalPos, snapCandidate);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = snapCandidate;
                foundSnap = true;
            }
        }

        return foundSnap ? closestPoint : originalPos;
    }

    /// <summary>
    /// Actualiza la posición y visibilidad de los puntos magenta de debug.
    /// </summary>
    private void UpdateMagentaMarkers()
    {
        if (!showSnapPoints)
        {
            foreach (var m in magentaSnapMarkers) m.SetActive(false);
            return;
        }

        List<Vector2> validPoints = GetAllValidSnapPoints();

        // Crear más marcadores si nos faltan en la "pool"
        while (magentaSnapMarkers.Count < validPoints.Count)
        {
            // Puntos más pequeños (0.15f)
            magentaSnapMarkers.Add(CreateMarker("MagentaSnap", Color.magenta, 0.15f));
        }

        // Posicionar y activar las necesarias, ocultar las que sobran
        for (int i = 0; i < magentaSnapMarkers.Count; i++)
        {
            if (i < validPoints.Count)
            {
                // Z = -1.5f para que queden flotando
                magentaSnapMarkers[i].transform.position = new Vector3(validPoints[i].x, validPoints[i].y, -1.5f);
                magentaSnapMarkers[i].SetActive(true);
            }
            else
            {
                magentaSnapMarkers[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Fuerza un vector de dirección a encajar en un múltiplo configurado si está dentro de la tolerancia.
    /// </summary>
    private Vector2 SnapDirection(Vector2 dir)
    {
        if (!enableAngleSnap || angleSnapStep <= 0f) return dir;

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(currentAngle / angleSnapStep) * angleSnapStep;

        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, snappedAngle)) <= angleSnapTolerance)
        {
            float rad = snappedAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }

        return dir;
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(mainCam.transform.position.z)));
        return new Vector2(worldPos.x, worldPos.y);
    }

    private GameObject CreateMarker(string name, Color color, float size = 0.25f)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = name;
        Destroy(marker.GetComponent<Collider>());
        marker.transform.localScale = new Vector3(size, size, size);

        Renderer rend = marker.GetComponent<Renderer>();
        Shader unlit = Shader.Find("Sprites/Default");
        if (unlit != null) rend.material.shader = unlit;
        rend.material.color = color;

        marker.SetActive(false);
        return marker;
    }
}