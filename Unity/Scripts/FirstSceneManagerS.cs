using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneManagerS : MonoBehaviour
{
    public ServerScript connectToServer;
    private void Awake()
    {
        connectToServer.ConnectToServer();
    }
    public void LoginPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void SignUpPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
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
