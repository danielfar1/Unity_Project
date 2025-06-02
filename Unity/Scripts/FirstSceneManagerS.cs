using UnityEngine;
using UnityEngine.SceneManagement;

/// Handles the logic for the first screen of the application,
/// including login and sign-up navigation, and initializing the server connection.
public class FirstSceneManagerS : MonoBehaviour
{
    public ServerScript connectToServer;  // Reference to ServerScript (set in Inspector)


    /// Called on script initialization. Establishes server connection.

    private void Awake()
    {
        connectToServer.ConnectToServer();
    }


    /// Triggered when the Login button is pressed.
    /// Navigates to the login scene (next in build index).

    public void LoginPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    /// Triggered when the Sign Up button is pressed.
    /// Navigates to the signup scene (2 scenes ahead in build index).

    public void SignUpPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }


    /// Ensures clean disconnection from the server when the app is closed.

    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();

        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();

        Debug.Log("Disconnected from the server.");
    }
}
