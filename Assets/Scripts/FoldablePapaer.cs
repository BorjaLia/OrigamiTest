using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestiona todas las bisagras del papel actual.
/// </summary>
public class FoldablePaper : MonoBehaviour
{
    private List<HingeNode> hinges = new List<HingeNode>();

    private void Start()
    {
        // Busca todos los componentes HingeNode en los hijos (la jerarquía del papel)
        hinges.AddRange(GetComponentsInChildren<HingeNode>());
    }

    /// <summary>
    /// Revisa si absolutamente todos los dobleces están en su posición final (snapped).
    /// </summary>
    public bool IsPaperComplete()
    {
        if (hinges.Count == 0) return false;

        foreach (HingeNode hinge in hinges)
        {
            // Si el objetivo es 0, significa que esa pieza no debe moverse.
            // Para el prototipo básico asumiremos que solo evaluamos las que sí se doblan.
            if (Mathf.Abs(hinge.targetAngle) > 1f && !hinge.isSnapped)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Desdobla el papel.
    /// </summary>
    public void ResetPaper()
    {
        foreach (HingeNode hinge in hinges)
        {
            hinge.ResetFold();
        }
    }
}