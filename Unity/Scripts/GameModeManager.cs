using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    private void Awake()
    {
        Application.runInBackground = true;
        ServerScript.instance.ConnectToServer();
    }
    public void SinglePlayerPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
    public void MultiPlayerPress()
    {
        string action = "check_multiplayer_users", data = "connected";
        if (ServerScript.instance.SendRequest(action, data) == true)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
        else
            Debug.Log("Game full");
    }
    public void BackButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
        private void OnApplicationQuit()
        {
            if (ServerScript.instance.stream != null)
                ServerScript.instance.stream.Close();
            if (ServerScript.instance.client != null)
                ServerScript.instance.client.Close();
            Debug.Log("Disconnected from the server.");
        }
}
