using UnityEngine;
using TMPro;
using System;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections;

public class LoginManagerScript : MonoBehaviour
{
    public LoginManagerScript instance;
    public Texture2D _cursor;
    public bool isMasked = true;
    public string Email;
    [Header("ServerAPI")]

    [Header("EyeImage")]
    public SpriteRenderer eyeOpen;
    public SpriteRenderer eyeClose;

    [Header("Error Messages")]
    public TMP_Text PwdEmailMismatchError;
    public TMP_Text userConnectedError;

    [Header("Input Fields")]
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        ServerScript.instance.ConnectToServer();
        PwdEmailMismatchError.enabled = false;
        passwordInput.asteriskChar = '●';
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
    }
    
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
            passwordInput.contentType = TMP_InputField.ContentType.Standard; ;
        }
        passwordInput.ForceLabelUpdate();
    }
    public void ChangeEyeImage()
    {
        if (eyeOpen.enabled == eyeOpen.enabled)
        {
            eyeOpen.enabled = !eyeOpen.enabled;
            eyeClose.enabled = !eyeClose.enabled;
        }
    }
    public void LoginPressedButton()
    {
        string action = "check_login", data = $"{{\"email\":\"{emailInput.text}\",\"password\":\"{passwordInput.text}\"}}";
        if (ServerScript.instance.SendRequest("user_is_already_connected", $"{{\"email\":\"{emailInput.text}\"}}") == true)
        {
            userConnectedError.enabled = false;
            if (ServerScript.instance.SendRequest(action, data) == true)
            {
                PwdEmailMismatchError.enabled = false;
                Email = emailInput.text;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
            }
            else
            {
                PwdEmailMismatchError.enabled = true;
                passwordInput.text = "";
            }
        }
        else
        {
            userConnectedError.enabled = true;
            passwordInput.text = "";
        }
    }


    
    public void CreateAccout()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
