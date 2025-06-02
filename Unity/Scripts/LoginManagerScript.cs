using UnityEngine;
using TMPro;
using System;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections;

/// Manages user login in the multiplayer game. 
/// Handles email/password authentication, error messaging, and password masking toggle.
public class LoginManagerScript : MonoBehaviour
{
    // Singleton instance for access across scenes if needed
    public LoginManagerScript instance;

    public bool isMasked = true;          // Controls whether password is hidden
    public string Email;                  // Stores the logged-in email globally

    [Header("ServerAPI")]

    [Header("EyeImage")]
    public SpriteRenderer eyeOpen;        // Icon shown when password is visible
    public SpriteRenderer eyeClose;       // Icon shown when password is hidden

    [Header("Error Messages")]
    public TMP_Text PwdEmailMismatchError;  // UI text shown on password/email mismatch
    public TMP_Text userConnectedError;     // UI text shown if user is already logged in elsewhere

    [Header("Input Fields")]
    public TMP_InputField passwordInput;  // Password input field
    public TMP_InputField emailInput;     // Email input field

    void Start()
    {
        // Initialize singleton
        if (instance == null)
        {
            instance = this;
        }

        // Attempt to connect to server at launch
        ServerScript.instance.ConnectToServer();

        // Disable error messages initially
        PwdEmailMismatchError.enabled = false;

        // Set password mask character
        passwordInput.asteriskChar = '●';

        // Reset cursor to default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
    }

    /// Toggles between showing and masking the password.
    public void ToggleMask()
    {
        isMasked = !isMasked;

        if (isMasked)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.asteriskChar = '●';
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
        }

        passwordInput.ForceLabelUpdate();
    }

    /// Switches the eye icon to reflect mask state.
    public void ChangeEyeImage()
    {
        eyeOpen.enabled = !eyeOpen.enabled;
        eyeClose.enabled = !eyeClose.enabled;
    }

    /// Triggered when user clicks the login button.
    /// Sends login data to the server and handles success/failure responses.
    public void LoginPressedButton()
    {
        string action = "check_login";
        string data = $"{{\"email\":\"{emailInput.text}\",\"password\":\"{passwordInput.text}\"}}";

        // Check if this user is already connected
        bool? alreadyConnected = ServerScript.instance.SendRequest("user_is_already_connected", $"{{\"email\":\"{emailInput.text}\"}}");

        if (alreadyConnected == true)
        {
            userConnectedError.enabled = false;

            // Send login request
            if (ServerScript.instance.SendRequest(action, data) == true)
            {
                // Login successful
                PwdEmailMismatchError.enabled = false;
                Email = emailInput.text;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2); // Go to main game scene
            }
            else
            {
                // Incorrect password
                PwdEmailMismatchError.enabled = true;
                passwordInput.text = "";
            }
        }
        else
        {
            // User is already connected
            userConnectedError.enabled = true;
            passwordInput.text = "";
        }
    }

    /// Navigates to the account creation scene.
    public void CreateAccout()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// Clean disconnection from server on application quit.
    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();
        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();

        Debug.Log("Disconnected from the server.");
    }
}
