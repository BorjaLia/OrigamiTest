using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contiene todas las funciones matemáticas puras para cortar y reflejar polígonos.
/// </summary>
public static class GeometryMath
{
    /// <summary>
    /// Determina de qué lado de una línea (definida por a y b) está un punto (p).
    /// Devuelve > 0 (izquierda), < 0 (derecha) o 0 (sobre la línea).
    /// </summary>
    public static float PointLinePosition(Vector2 p, Vector2 a, Vector2 b)
    {
        return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
    }

    /// <summary>
    /// Refleja un punto 2D como en un espejo a través de una línea definida por a y b.
    /// </summary>
    public static Vector2 ReflectPoint(Vector2 p, Vector2 a, Vector2 b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;

        if (dx == 0 && dy == 0) return p;

        float a_param = (dx * dx - dy * dy) / (dx * dx + dy * dy);
        float b_param = 2 * dx * dy / (dx * dx + dy * dy);

        float x2 = a_param * (p.x - a.x) + b_param * (p.y - a.y) + a.x;
        float y2 = b_param * (p.x - a.x) - a_param * (p.y - a.y) + a.y;

        return new Vector2(x2, y2);
    }

    /// <summary>
    /// Encuentra el punto de intersección entre dos segmentos de línea (p1-p2 y p3-p4).
    /// Devuelve true si se intersectan, junto con el punto exacto.
    /// </summary>
    public static bool GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float denominator = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);

        // Si el denominador es 0, las líneas son paralelas
        if (Mathf.Abs(denominator) < 0.0001f) return false;

        float t = ((p1.x - p3.x) * (p3.y - p4.y) - (p1.y - p3.y) * (p3.x - p4.x)) / denominator;
        float u = -((p1.x - p2.x) * (p1.y - p3.y) - (p1.y - p2.y) * (p1.x - p3.x)) / denominator;

        // Si t y u están entre 0 y 1, los segmentos se tocan
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            intersection = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Convierte un polígono convexo en un arreglo de triángulos para que Unity lo pueda dibujar.
    /// Usa "Fan Triangulation" (Triangulación en abanico), ideal y rápida para figuras convexas.
    /// </summary>
    public static int[] TriangulateConvex(List<Vector2> vertices)
    {
        if (vertices.Count < 3) return new int[0];

        int[] triangles = new int[(vertices.Count - 2) * 3];
        int triIndex = 0;

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i;
            triangles[triIndex + 2] = i + 1;
            triIndex += 3;
        }

        return triangles;
    }
}