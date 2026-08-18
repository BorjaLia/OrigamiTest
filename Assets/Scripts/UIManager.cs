using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conecta los botones de la interfaz con los sistemas del juego.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject winPanel; // Un panel con el texto "¡Felicidades!"
    public Text toggleModeButtonText; // Texto del botón para cambiar entre Fold/View

    private InputController inputController;
    private CameraController cameraController;

    private void Start()
    {
        inputController = FindObjectOfType<InputController>();
        cameraController = FindObjectOfType<CameraController>();

        if (winPanel != null) winPanel.SetActive(false);
    }

    public void OnResetButtonClicked()
    {
        GameManager.Instance.ResetGame();
        if (cameraController != null) cameraController.ResetCamera();
        if (winPanel != null) winPanel.SetActive(false);

        // Asegurarse de volver a modo Pliegue
        //if (inputController.isViewMode) ToggleMode();
    }

    public void ToggleMode()
    {
        if (inputController == null) return;

        //bool isCurrentlyViewMode = inputController.isViewMode;
        //inputController.SetViewMode(!isCurrentlyViewMode);

        if (toggleModeButtonText != null)
        {
            //toggleModeButtonText.text = !isCurrentlyViewMode ? "Modo: Observar (Orbitar)" : "Modo: Plegar (Interaccionar)";
        }
    }

    /// <summary>
    /// Este método debe ser enlazado al evento OnGameWon del GameManager en el Inspector.
    /// </summary>
    public void ShowWinScreen()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }
}