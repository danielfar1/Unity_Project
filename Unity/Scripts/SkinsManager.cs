using UnityEngine;
using UnityEngine.SceneManagement;

public class SkinsManager : MonoBehaviour
{
    public void BackButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
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
