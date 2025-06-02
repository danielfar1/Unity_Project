using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Handles the skin selection screen where players choose between different characters (e.g., Messi and Ronaldo).
/// Manages UI feedback, selection state, and saves selected skin via server.
public class SkinsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject selectedFrame;  // Frame that highlights selected skin
    public Image selected;            // "Selected" visual indicator
    public Image select;              // "Select" button visual
    public Image messiBig;            // Large image of Messi
    public Image ronaldoBig;          // Large image of Ronaldo
    public GameObject messiBackround; // Background area for Messi
    public GameObject ronaldoBackround; // Background area for Ronaldo

    private int skinNum; // Tracks the selected skin ID (1 = Messi, 2 = Ronaldo)


    /// Initializes the UI elements at scene start.

    private void Start()
    {
        select.enabled = false;
        selected.enabled = false;
        selectedFrame.SetActive(false);
        ronaldoBig.enabled = false;
        messiBig.enabled = false;
    }


    /// Handles selection of Ronaldo skin.
    /// Updates UI and checks if this skin is already selected via server.

    public void RonaldoClicked()
    {
        messiBig.enabled = false;
        ronaldoBig.enabled = true;

        selectedFrame.SetActive(true);
        selectedFrame.transform.position = ronaldoBackround.transform.position;

        skinNum = 2;

        bool? isSelected = ServerScript.instance.SendRequest("get_player_skin", $"{{\"skinId\":\"{skinNum}\"}}");
        Debug.Log(isSelected);

        if (isSelected == true)
        {
            selected.enabled = true;
            select.enabled = false;
        }
        else
        {
            selected.enabled = false;
            select.enabled = true;
        }
    }


    /// Handles selection of Messi skin.
    /// Updates UI and checks if this skin is already selected via server.

    public void MessiClicked()
    {
        ronaldoBig.enabled = false;
        messiBig.enabled = true;

        selectedFrame.SetActive(true);
        selectedFrame.transform.position = messiBackround.transform.position;

        skinNum = 1;

        bool? isSelected = ServerScript.instance.SendRequest("get_player_skin", $"{{\"skinId\":\"{skinNum}\"}}");
        Debug.Log(isSelected);

        if (isSelected == true)
        {
            selected.enabled = true;
            select.enabled = false;
        }
        else
        {
            selected.enabled = false;
            select.enabled = true;
        }
    }


    /// Called when the "Select" button is pressed.
    /// Sends selected skin ID to server and updates UI state.

    public void SelectPressed()
    {
        select.enabled = false;
        selected.enabled = true;

        ServerScript.instance.SendRequest("set_player_skin", $"{{\"skinId\":\"{skinNum}\"}}");
    }


    /// Navigates back to the previous scene (usually game menu).

    public void BackButtonPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }


    /// Ensures server socket is cleanly closed when the app exits.

    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();

        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();

        Debug.Log("Disconnected from the server.");
    }
}
