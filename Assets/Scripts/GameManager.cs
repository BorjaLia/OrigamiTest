using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controla el estado global del juego y detecta la condición de victoria.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración del Nivel")]
    public FoldablePaper currentPaper;

    [Header("Eventos")]
    public UnityEvent OnGameWon;

    private bool gameWon = false;

    private void Awake()
    {
        // Configurar el Singleton para fácil acceso
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Llamado cada vez que un HingeNode hace "snap" a su posición final.
    /// </summary>
    public void CheckWinCondition()
    {
        if (gameWon) return;

        if (currentPaper != null && currentPaper.IsPaperComplete())
        {
            gameWon = true;
            Debug.Log("¡Pieza de Origami Completada!");
            OnGameWon?.Invoke();
        }
    }

    public void ResetGame()
    {
        gameWon = false;
        if (currentPaper != null)
        {
            currentPaper.ResetPaper();
        }
    }
}