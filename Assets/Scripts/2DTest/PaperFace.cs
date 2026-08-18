using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa una pieza plana de papel. Se encarga de generar su propia malla 3D 
/// basándose en una lista de vértices 2D.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PaperFace : MonoBehaviour
{
    public List<Vector2> vertices = new List<Vector2>();

    private MeshFilter meshFilter;
    private Mesh mesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Reconstruye el modelo 3D (Mesh) usando los vértices actuales de la lista.
    /// </summary>
    public void UpdateMesh()
    {
        if (vertices.Count < 3) return;

        // 1. Convertir Vector2 a Vector3 (en el plano Z=0)
        Vector3[] vertices3D = new Vector3[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices3D[i] = new Vector3(vertices[i].x, vertices[i].y, 0f);
        }

        // 2. Triangulación Matemática
        int[] triangles = GeometryMath.TriangulateConvex(vertices);

        // 3. Generar coordenadas UV básicas (para texturas/colores simples)
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            // Normalizado rudimentario para que no de error
            uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
        }

        // 4. Asignar a la malla de Unity
        mesh.Clear();
        mesh.vertices = vertices3D;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Recalcular normales para que la iluminación funcione correctamente
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}