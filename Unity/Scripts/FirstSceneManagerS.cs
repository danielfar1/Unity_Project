using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneManagerS : MonoBehaviour
{
    public Texture2D _cursor;
    public void LoginPress()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void SignUpPress()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
}
