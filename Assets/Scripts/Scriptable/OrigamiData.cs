using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estructura de datos para definir cada paso de nuestro nivel de Origami.
/// </summary>
[System.Serializable]
public class OrigamiObjective
{
    public string stepName = "Nuevo Paso";
    public List<Vector2> targetShape = new List<Vector2>();
}

/// <summary>
/// ScriptableObject que actúa como una "receta" de origami.
/// Puedes crear múltiples de estos en tu carpeta de proyecto.
/// </summary>
[CreateAssetMenu(fileName = "NewOrigamiFigure", menuName = "Origami/Figure Data")]
public class OrigamiFigure : ScriptableObject
{
    [Header("Información de la Figura")]
    public string figureName = "Nueva Figura";

    [Header("Pasos para completarla")]
    public List<OrigamiObjective> steps = new List<OrigamiObjective>();
}