using UnityEngine;
using TMPro;
using Mono.Data.Sqlite;
using UnityEngine.SceneManagement;

public class LoginManagerScript : MonoBehaviour
{
    public Texture2D _cursor;
    private string realText, textWithoutPoints = "";
    public bool isMasked = true;
    private string dbPath;

    [Header("EyeImage")]
    public SpriteRenderer eyeOpen;
    public SpriteRenderer eyeClose;

    [Header("Error Messages")]
    public TMP_Text PwdEmailMismatchError;

    [Header("Input Fields")]
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
    void Awake()
    {
        PwdEmailMismatchError.enabled = false;
        dbPath = "URI=file:C:/FinalProject/1vs1Football.db";
        passwordInput.onValueChanged.AddListener(OnTextChanged);
        UpdateMaskedText();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware); 
    }
    private void OnTextChanged(string userInput)
    {
        realText = userInput;
        if (userInput == "")
            textWithoutPoints = "";
        else
            if (userInput.Length - 1 == textWithoutPoints.Length)
            textWithoutPoints += userInput[userInput.Length - 1];
        if (isMasked)
        {
            UpdateMaskedText();
        }
    }
    private void UpdateMaskedText()
    {
        if (passwordInput.text != "")
        {
            passwordInput.text = new string('●', realText.Length);
            passwordInput.caretPosition = realText.Length;
        }
    }
    public void ToggleMask()
    {
        isMasked = !isMasked;
        if (isMasked)
        {
            UpdateMaskedText();
        }
        else
        {
            if (textWithoutPoints != "")
               passwordInput.text = textWithoutPoints.Replace("●", "");
        }
            passwordInput.caretPosition = passwordInput.text.Length;
    }
    public void ChangeEyeImage()
    {
        if (eyeOpen.enabled == eyeOpen.enabled)
        {
            eyeOpen.enabled = !eyeOpen.enabled;
            eyeClose.enabled = !eyeClose.enabled;
        }
    }
    public void LoginCheckProceedToLobby()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT email,password FROM users WHERE email = @email AND password = @password";
                command.Parameters.AddWithValue("@password", textWithoutPoints.Replace("●", ""));
                command.Parameters.AddWithValue("@email",emailInput.text );
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows == true)
                    {
                        PwdEmailMismatchError.enabled = false;
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
                    }
                    else
                    {
                        emailInput.text = "";
                        passwordInput.text = "";
                        PwdEmailMismatchError.enabled = true;
                    }
                }
            }
        }
    }
    public void CreateAccout()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


}
