using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Es el corazón del sistema. Recibe un corte (línea) y divide las caras de papel.
/// También evalúa si se alcanzó la forma objetivo.
/// </summary>
public class FoldManager : MonoBehaviour
{
    public static FoldManager Instance { get; private set; }

    [Header("Configuración del Papel")]
    public Material paperMaterial;
    public List<PaperFace> activeFaces = new List<PaperFace>();

    [Header("Objetivos del Nivel (Scriptable Object)")]
    public Material targetLineMaterial;

    [Tooltip("Arrastra aquí tu archivo de figura (ej. Grulla.asset, Rana.asset)")]
    public OrigamiFigure currentFigure;

    private int currentObjectiveIndex = 0;
    private List<Vector2> currentTargetShape = new List<Vector2>();

    // Renderers separados para mostrar los dos límites de forma visual
    private LineRenderer maxLimitRenderer;
    private LineRenderer minLimitRenderer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. Inicializar el papel cuadrado
        if (activeFaces.Count == 0)
        {
            CreateInitialPaper(new Vector2(-5, -5), new Vector2(5, 5));
        }

        // 2. Fallback de seguridad: Si olvidaste asignar un ScriptableObject, lo creamos en memoria.
        if (currentFigure == null || currentFigure.steps.Count == 0)
        {
            Debug.LogWarning("No se asignó ninguna figura en el Inspector. Cargando Diamante por defecto.");
            currentFigure = ScriptableObject.CreateInstance<OrigamiFigure>();
            currentFigure.figureName = "Diamante por defecto";

            OrigamiObjective defaultDiamond = new OrigamiObjective();
            defaultDiamond.stepName = "Paso 1: El Diamante";
            defaultDiamond.targetShape = new List<Vector2>()
            {
                new Vector2(0, 5),   new Vector2(-5, 0),
                new Vector2(0, -5),  new Vector2(5, 0)
            };
            currentFigure.steps.Add(defaultDiamond);
        }

        // 3. Crear los renderers visuales (vacíos por ahora)
        SetupVisualRenderers();

        // 4. Cargar el primer objetivo
        LoadObjective(0);
    }

    private void SetupVisualRenderers()
    {
        GameObject maxVisual = new GameObject("TargetMaxVisual");
        maxVisual.transform.SetParent(transform);
        maxLimitRenderer = maxVisual.AddComponent<LineRenderer>();
        maxLimitRenderer.loop = true;
        maxLimitRenderer.startWidth = 0.12f;
        maxLimitRenderer.endWidth = 0.12f;
        if (targetLineMaterial != null) maxLimitRenderer.material = targetLineMaterial;

        GameObject minVisual = new GameObject("TargetMinVisual");
        minVisual.transform.SetParent(transform);
        minLimitRenderer = minVisual.AddComponent<LineRenderer>();
        minLimitRenderer.loop = true;
        minLimitRenderer.startWidth = 0.05f;
        minLimitRenderer.endWidth = 0.05f;
        if (targetLineMaterial != null) minLimitRenderer.material = targetLineMaterial;
    }

    private void LoadObjective(int index)
    {
        currentObjectiveIndex = index;
        currentTargetShape = currentFigure.steps[index].targetShape;

        Debug.Log($"<color=cyan>Figura: {currentFigure.figureName} | Cargando Paso {index + 1}: {currentFigure.steps[index].stepName}</color>");

        maxLimitRenderer.positionCount = currentTargetShape.Count;
        minLimitRenderer.positionCount = currentTargetShape.Count;

        for (int i = 0; i < currentTargetShape.Count; i++)
        {
            maxLimitRenderer.SetPosition(i, new Vector3(currentTargetShape[i].x, currentTargetShape[i].y, -0.5f));
            minLimitRenderer.SetPosition(i, new Vector3(currentTargetShape[i].x, currentTargetShape[i].y, 0.4f));
        }

        maxLimitRenderer.startColor = Color.red; maxLimitRenderer.endColor = Color.red;
        minLimitRenderer.startColor = Color.red; minLimitRenderer.endColor = Color.red;
    }

    private void CreateInitialPaper(Vector2 bottomLeft, Vector2 topRight)
    {
        GameObject basePaper = new GameObject("BasePaper");
        basePaper.transform.SetParent(transform);
        basePaper.AddComponent<MeshRenderer>().material = paperMaterial;

        PaperFace face = basePaper.AddComponent<PaperFace>();
        face.vertices = new List<Vector2>()
        {
            bottomLeft,
            new Vector2(bottomLeft.x, topRight.y),
            topRight,
            new Vector2(topRight.x, bottomLeft.y)
        };
        face.UpdateMesh();
        activeFaces.Add(face);
    }

    public void ProcessFold(Vector2 startLine, Vector2 endLine)
    {
        List<PaperFace> newFaces = new List<PaperFace>();
        List<PaperFace> facesToRemove = new List<PaperFace>();

        foreach (PaperFace face in activeFaces)
        {
            List<Vector2> leftVertices = new List<Vector2>();
            List<Vector2> rightVertices = new List<Vector2>();

            for (int i = 0; i < face.vertices.Count; i++)
            {
                Vector2 currentPoint = face.vertices[i];
                Vector2 nextPoint = face.vertices[(i + 1) % face.vertices.Count];

                float side = GeometryMath.PointLinePosition(currentPoint, startLine, endLine);

                if (side >= 0) leftVertices.Add(currentPoint);
                else rightVertices.Add(currentPoint);

                Vector2 intersection;
                Vector2 lineDir = (endLine - startLine).normalized;
                Vector2 p3 = startLine - lineDir * 1000f;
                Vector2 p4 = endLine + lineDir * 1000f;

                if (GeometryMath.GetIntersection(currentPoint, nextPoint, p3, p4, out intersection))
                {
                    leftVertices.Add(intersection);
                    rightVertices.Add(intersection);
                }
            }

            if (leftVertices.Count >= 3 && rightVertices.Count >= 3)
            {
                facesToRemove.Add(face);
                PaperFace leftFace = CreateNewFace(leftVertices, "Paper_Left");
                newFaces.Add(leftFace);

                List<Vector2> foldedVertices = new List<Vector2>();
                foreach (Vector2 v in rightVertices)
                {
                    foldedVertices.Add(GeometryMath.ReflectPoint(v, startLine, endLine));
                }
                foldedVertices.Reverse();

                PaperFace rightFace = CreateNewFace(foldedVertices, "Paper_Right_Folded");
                rightFace.transform.position = new Vector3(0, 0, -0.01f * (activeFaces.Count + newFaces.Count + 1));
                newFaces.Add(rightFace);
            }
            else if (rightVertices.Count >= 3 && leftVertices.Count < 3)
            {
                facesToRemove.Add(face);
                List<Vector2> foldedVertices = new List<Vector2>();
                foreach (Vector2 v in rightVertices)
                {
                    foldedVertices.Add(GeometryMath.ReflectPoint(v, startLine, endLine));
                }
                foldedVertices.Reverse();

                PaperFace rightFace = CreateNewFace(foldedVertices, face.gameObject.name + "_Folded");
                rightFace.transform.position = new Vector3(0, 0, -0.01f * (activeFaces.Count + newFaces.Count + 1));
                newFaces.Add(rightFace);
            }
        }

        foreach (PaperFace oldFace in facesToRemove)
        {
            activeFaces.Remove(oldFace);
            Destroy(oldFace.gameObject);
        }
        activeFaces.AddRange(newFaces);

        CheckWinCondition();
    }

    private PaperFace CreateNewFace(List<Vector2> verts, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.AddComponent<MeshRenderer>().material = paperMaterial;
        PaperFace face = go.AddComponent<PaperFace>();
        face.vertices = verts;
        face.UpdateMesh();
        return face;
    }

    private void CheckWinCondition()
    {
        if (currentObjectiveIndex >= currentFigure.steps.Count) return;

        float tolerance = 0.2f;
        bool hasFailedUpperLimit = false;

        foreach (PaperFace face in activeFaces)
        {
            foreach (Vector2 vertex in face.vertices)
            {
                if (!IsPointInsideConvexPolygon(vertex, currentTargetShape, tolerance))
                {
                    hasFailedUpperLimit = true;
                    break;
                }
            }
            if (hasFailedUpperLimit) break;
        }

        bool hasFailedLowerLimit = false;
        foreach (Vector2 targetCorner in currentTargetShape)
        {
            bool cornerIsCovered = false;
            foreach (PaperFace face in activeFaces)
            {
                if (IsPointInsideConvexPolygon(targetCorner, face.vertices, tolerance))
                {
                    cornerIsCovered = true;
                    break;
                }
            }

            if (!cornerIsCovered)
            {
                hasFailedLowerLimit = true;
                break;
            }
        }

        if (maxLimitRenderer != null)
        {
            maxLimitRenderer.startColor = hasFailedUpperLimit ? Color.red : Color.green;
            maxLimitRenderer.endColor = hasFailedUpperLimit ? Color.red : Color.green;
        }
        if (minLimitRenderer != null)
        {
            minLimitRenderer.startColor = hasFailedLowerLimit ? Color.red : Color.blue;
            minLimitRenderer.endColor = hasFailedLowerLimit ? Color.red : Color.blue;
        }

        if (!hasFailedUpperLimit && !hasFailedLowerLimit)
        {
            Debug.Log($"<color=green>¡ÉXITO! {currentFigure.steps[currentObjectiveIndex].stepName} completado.</color>");

            currentObjectiveIndex++;
            if (currentObjectiveIndex < currentFigure.steps.Count)
            {
                LoadObjective(currentObjectiveIndex);
            }
            else
            {
                Debug.Log($"<color=yellow>¡VICTORIA FINAL! ¡Figura '{currentFigure.figureName}' terminada!</color>");
                minLimitRenderer.startColor = Color.green; minLimitRenderer.endColor = Color.green;
            }
        }
    }

    public List<Vector2> GetAllVertices()
    {
        List<Vector2> allVerts = new List<Vector2>();
        foreach (PaperFace face in activeFaces)
        {
            foreach (Vector2 v in face.vertices)
            {
                allVerts.Add(v);
            }
        }
        return allVerts;
    }

    private bool IsPointInsideConvexPolygon(Vector2 point, List<Vector2> polygon, float tolerance)
    {
        if (polygon.Count < 3) return false;
        bool hasPositive = false;
        bool hasNegative = false;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            float side = GeometryMath.PointLinePosition(point, a, b);

            if (side > tolerance) hasPositive = true;
            else if (side < -tolerance) hasNegative = true;

            if (hasPositive && hasNegative) return false;
        }
        return true;
    }
}