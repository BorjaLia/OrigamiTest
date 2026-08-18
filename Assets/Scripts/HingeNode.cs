using UnityEngine;
using System.Collections;

/// <summary>
/// Se adjunta al objeto "Pivote" (bisagra) de cada solapa de papel.
/// Controla la rotación, los límites y el snapping.
/// </summary>
public class HingeNode : MonoBehaviour
{
    [Header("Configuración del Doblez")]
    [Tooltip("El eje local sobre el cual rota esta bisagra. Ej: (1,0,0) para X")]
    public Vector3 rotationAxis = Vector3.right;

    [Tooltip("El ángulo final al que debe llegar para ganar. (Usa 179.5 en vez de 180 para evitar Z-Fighting)")]
    public float targetAngle = 179.5f;

    [Tooltip("Grados de tolerancia para que el doblez se ajuste solo (Snap)")]
    public float snapTolerance = 15f;

    [Tooltip("Velocidad de la animación del Snap o Reset")]
    public float animationSpeed = 5f;

    [Header("Estado Actual (Solo Lectura)")]
    public float currentAngle = 0f;
    public bool isSnapped = false;

    private Quaternion initialRotation;
    private Coroutine animationCoroutine;

    private void Start()
    {
        initialRotation = transform.localRotation;
    }

    /// <summary>
    /// Aplica una rotación basada en el movimiento del ratón.
    /// </summary>
    public void Fold(float angleDelta)
    {
        if (isSnapped) return; // No dejar mover si ya está fijo

        currentAngle += angleDelta;

        // Limitar para que no se doble infinitamente. Asumimos dobleces de 0 a 180 o -180.
        // Esto puede variar según tu diseño, aquí lo limitamos entre 0 y el objetivo (+ padding)
        float maxLimit = Mathf.Max(0, targetAngle + 20f);
        float minLimit = Mathf.Min(0, targetAngle - 20f);
        currentAngle = Mathf.Clamp(currentAngle, minLimit, maxLimit);

        ApplyRotation(currentAngle);
    }

    /// <summary>
    /// Se llama al soltar el clic. Revisa si estamos cerca de la meta.
    /// </summary>
    public void CheckSnap()
    {
        if (isSnapped) return;

        if (Mathf.Abs(currentAngle - targetAngle) <= snapTolerance)
        {
            isSnapped = true;
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateToAngle(targetAngle, true));
        }
    }

    public void ResetFold()
    {
        isSnapped = false;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateToAngle(0f, false));
    }

    private void ApplyRotation(float angle)
    {
        transform.localRotation = initialRotation * Quaternion.AngleAxis(angle, rotationAxis);
    }

    private IEnumerator AnimateToAngle(float endAngle, bool checkWinAfter)
    {
        float startAngle = currentAngle;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            ApplyRotation(currentAngle);
            yield return null;
        }

        currentAngle = endAngle;
        ApplyRotation(currentAngle);

        if (checkWinAfter)
        {
            GameManager.Instance.CheckWinCondition();
        }
    }
}