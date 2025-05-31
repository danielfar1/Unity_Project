using UnityEngine;
using UnityEngine.SceneManagement;

public class SkinsManager : MonoBehaviour
{
    public void BackButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }
}
