using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using UnityEngine.Networking;
using System.Collections;

public class SignUpManager : MonoBehaviour
{
    private const string NamePattern = @"^[a-zA-Z]+$";
    private const string EmailPattern = @"^([A-Za-z][A-Za-z0-9]+)([\._]\w+)?@(\w+)(\.\w+)(\.\w+)?$";
    private const string PasswordPattern = @"^(?=.*\d)(?=.*[A-Za-z])[A-Za-z\d]+$";
    public ServerScript connectToServer;
    private bool[] validationResults = new bool[6];
    public bool isMasked = true;
    [Header("EyeImage")]
    public SpriteRenderer eyeOpen;
    public SpriteRenderer eyeClose;

    [Header("Error Messages")]
    public TMP_Text firstNameError;
    public TMP_Text lastNameError;
    public TMP_Text usernameExistError;
    public TMP_Text usernameLengthError;
    public TMP_Text usernameSpaceError;
    public TMP_Text emailExistError;
    public TMP_Text emailError;
    public TMP_Text passwordLengthError;
    public TMP_Text passwordFormatError;
    public TMP_Text confirmPasswordError;

    [Header("Input Fields")]
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField[] passwordsArray;

    private void Awake()
    {
        Application.runInBackground = true;
        connectToServer.ConnectToServer();
        passwordsArray[0].asteriskChar = '●';
        passwordsArray[1].asteriskChar = '●';
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        DisableAllErrors();
    }
    public void ToggleMask()
    {
        isMasked = !isMasked;
        for (int i = 0; i < passwordsArray.Length; i++)
        {
            if (isMasked)
            {
                passwordsArray[i].contentType = TMP_InputField.ContentType.Password;
                passwordsArray[i].asteriskChar = '●';
            }
            else
            {
                passwordsArray[i].contentType = TMP_InputField.ContentType.Standard;
            }
            passwordsArray[i].ForceLabelUpdate();
        }
    }
    public void ChangeEyeImage()
    {
        if(eyeOpen.enabled == eyeOpen.enabled)
        {
           eyeOpen.enabled = !eyeOpen.enabled;
           eyeClose.enabled = !eyeClose.enabled;
        }
    }
    public void ProceedToLobby()
    {
        if (AllFieldsValid())
        {
            string action = "create_user";
            User user = new User(usernameInput.text.Trim(), passwordsArray[0].text.Trim(), emailInput.text.Trim(), firstNameInput.text.Trim(), lastNameInput.text.Trim());
            ServerScript.instance.SendRequest(action, user);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //lobby scene
        }
    }

    private bool AllFieldsValid()
    {
        foreach (bool isValid in validationResults)
        {
            if (!isValid) return false;
        }
        return true;
    }

    private void DisableAllErrors()
    {
        firstNameError.enabled = false;
        lastNameError.enabled = false;
        usernameExistError.enabled = false;
        usernameLengthError.enabled = false;
        usernameSpaceError.enabled = false;
        emailError.enabled = false;
        emailExistError.enabled = false;
        passwordLengthError.enabled = false;
        passwordFormatError.enabled = false;
        confirmPasswordError.enabled = false;
    }

    private void ValidateField(int index, bool condition, TMP_Text errorText)
    {
        validationResults[index] = condition;
        errorText.enabled = !condition;
    }

    public void ValidateFirstName()
    {
        ValidateField(0, firstNameInput.text.Length >= 2 && Regex.IsMatch(firstNameInput.text, NamePattern), firstNameError);
    }

    public void ValidateLastName()
    {
        ValidateField(1, lastNameInput.text.Length >= 2 && Regex.IsMatch(lastNameInput.text, NamePattern), lastNameError);
    }
    private void CheckIfValueAlreadyExist(int index,string inputField)
    {
        bool result;
        if (index==0)
        {
            if (ServerScript.instance.SendRequest("check_username", $"{{\"username\":\"{inputField}\"}}") == true)
                result = true;
            else
                result = false;
        }
        else
        {
            if (ServerScript.instance.SendRequest("check_email", $"{{\"email\":\"{inputField}\"}}") == true)
                result = true;
            else
                result = false;
        }
        ValidateUsernameAndEmail(result, index);
    }
    public void ValidateUsernameAndEmail(bool responseText,int index)
    {

        if(index==0) //ValidateUsername
        {
            bool usernameExist = responseText;
            if (usernameExist)
            {
                ValidateField(2, false, usernameExistError);
                usernameSpaceError.enabled = false;
                usernameLengthError.enabled = false;
            }
            else if (usernameInput.text.Length < 5)
            {
                ValidateField(2, false, usernameLengthError);
                usernameSpaceError.enabled = false;
                usernameExistError.enabled = false;
            }
            else if (usernameInput.text.Contains(' '))
            {
                ValidateField(2, false, usernameSpaceError);
                usernameLengthError.enabled = false;
                usernameExistError.enabled = false;
            }
            else
            {
                ValidateField(2, true, usernameLengthError);
                usernameSpaceError.enabled = false;
                usernameExistError.enabled = false;
            }
        }
        else       //ValidateEmail
        {
            bool emailExist = responseText;
            if (emailExist)
            {
                ValidateField(3, false, emailExistError);
                emailError.enabled = false;
            }
            else
            {
                ValidateField(3, Regex.IsMatch(emailInput.text, EmailPattern), emailError);
                emailExistError.enabled = false;
            }
        }
    }
    public void ValidateUsername()
    {
        CheckIfValueAlreadyExist(0, usernameInput.text);
    }

    public void ValidateEmail()
    {
        CheckIfValueAlreadyExist(1, emailInput.text);
    }

    public void ValidatePassword()
    {
        if (passwordsArray[0].text.Length < 8)
        {
            ValidateField(4, false, passwordLengthError);
            passwordFormatError.enabled = false;
        }
        else if (!Regex.IsMatch(passwordsArray[0].text, PasswordPattern))
        {
            ValidateField(4, false, passwordFormatError);
            passwordLengthError.enabled = false;
        }
        else
        {
            ValidateField(4, true, passwordLengthError);
            passwordFormatError.enabled = false;
        }
    }

    public void ValidateConfirmPassword()
    {
        ValidateField(5, passwordsArray[0].text == passwordsArray[1].text, confirmPasswordError);
    }
    public void AlreadyHaveAnACC()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex -1);
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

public class User
{
    public string username;
    public string password;
    public string email;
    public string firstname;
    public string lastname;

    public User(string username, string password,string email,string firstname,string lastname)
    {
        this.username = username;
        this.password = password;
        this.email = email;
        this.firstname = firstname;
        this.lastname = lastname;
    }
}

