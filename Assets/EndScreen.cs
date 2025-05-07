using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public GameObject characterObject;
    public GameObject endCanvas;
    public bool hasEnded = true;
    public GameObject winCanvas;

    public GameObject fixedGarden;
    public GameObject fixedDiningRoom;
    public GameObject fixedLibrary;

    void Start()
    {
        if (endCanvas != null)
        {
            endCanvas.SetActive(false);
        }
        if (winCanvas != null)
        {
            winCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (!hasEnded && AreAllRoomsFixed())
        {
            hasEnded = true;
            ShowWinScreen();
        }
        if (hasEnded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                RestartGame();
            }
        }
    }

    private bool AreAllRoomsFixed()
    {
        return fixedGarden.activeSelf && fixedDiningRoom.activeSelf && fixedLibrary.activeSelf;
    }

    public void ShowWinScreen()
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }

        var playerHiding = FindFirstObjectByType<PlayerHiding>();
        if (playerHiding != null)
        {
            if (playerHiding.playerMovementScript != null)
                playerHiding.playerMovementScript.enabled = false;
            if (playerHiding.characterController != null)
                playerHiding.characterController.enabled = false;
        }

    
        var npcAgent = FindFirstObjectByType<HideAndSeekNPC>();
        if (npcAgent != null)
        {
            npcAgent.enabled = false;
        }

      
        var raycastTeleport = FindFirstObjectByType<RaycastTeleport>();
        if (raycastTeleport != null)
        {
            raycastTeleport.enabled = false;
        }

        Time.timeScale = 0f;
    }

    public void ShowEndScreen()
    {
        if (endCanvas != null)
        {
            endCanvas.SetActive(true);
        }

        var playerHiding = FindFirstObjectByType<PlayerHiding>();
        if (playerHiding != null)
        {
            if (playerHiding.playerMovementScript != null)
                playerHiding.playerMovementScript.enabled = false;
            if (playerHiding.characterController != null)
                playerHiding.characterController.enabled = false;
        }

        var npcAgent = FindFirstObjectByType<HideAndSeekNPC>();
        if (npcAgent != null)
        {
            npcAgent.enabled = false;
        }

        // Disable raycast teleport
        var raycastTeleport = FindFirstObjectByType<RaycastTeleport>();
        if (raycastTeleport != null)
        {
            raycastTeleport.enabled = false;
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
