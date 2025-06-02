using UnityEngine;
using UnityEngine.SceneManagement;

/// Manages navigation between game modes (Single Player and Multiplayer).
/// Also handles server connection and disconnection lifecycle events.
public class GameModeManager : MonoBehaviour
{
    private void Awake()
    {
        Application.runInBackground = true;

        // Ensure the server connection is established
        ServerScript.instance.ConnectToServer();
    }

    public void SinglePlayerPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    /// Sends a request to the server to join the multiplayer queue.
    /// Loads the multiplayer lobby if accepted, logs a message if full.
    public void MultiPlayerPress()
    {
        string action = "check_multiplayer_users";
        string data = "connected";

        // Attempt to register the user for multiplayer mode
        if (ServerScript.instance.SendRequest(action, data) == true)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
        }
        else
        {
            Debug.Log("Game full"); // Server already has 2 players
        }
    }

    public void BackButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    /// Gracefully disconnects from the server when the application exits.
    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();

        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();

        Debug.Log("Disconnected from the server.");
    }
}
