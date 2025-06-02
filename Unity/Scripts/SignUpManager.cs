using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using UnityEngine.Networking;
using System.Collections;

/// Manages the signup process, including input validation, error handling,
/// and communication with the server to create new users.
public class SignUpManager : MonoBehaviour
{
    // Validation regex patterns
    private const string NamePattern = @"^[a-zA-Z]+$";
    private const string EmailPattern = @"^([A-Za-z][A-Za-z0-9]+)([\._]\w+)?@(\w+)(\.\w+)(\.\w+)?$";
    private const string PasswordPattern = @"^(?=.*\d)(?=.*[A-Za-z])[A-Za-z\d]+$";

    private bool[] validationResults = new bool[6]; // Track valid states of fields: [first, last, username, email, pwd, confirmPwd]
    public bool isMasked = true;

    [Header("Eye Image")]
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
    public TMP_InputField[] passwordsArray; // [0] = password, [1] = confirm password

    private void Start()
    {
        Application.runInBackground = true;
        ServerScript.instance.ConnectToServer();
        DisableAllErrors();

        // Mask password fields by default
        passwordsArray[0].asteriskChar = '●';
        passwordsArray[1].asteriskChar = '●';

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// Toggles password visibility in input fields.
    public void ToggleMask()
    {
        isMasked = !isMasked;

        foreach (var field in passwordsArray)
        {
            field.contentType = isMasked ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            field.asteriskChar = '●';
            field.ForceLabelUpdate();
        }
    }

    /// Toggles eye icon visibility to reflect password masking state.
    public void ChangeEyeImage()
    {
        eyeOpen.enabled = !eyeOpen.enabled;
        eyeClose.enabled = !eyeClose.enabled;
    }

    /// Proceeds to the lobby if all fields are valid.
    public void ProceedToLobby()
    {
        if (AllFieldsValid())
        {
            string action = "create_user";
            User user = new User(
                usernameInput.text.Trim(),
                passwordsArray[0].text.Trim(),
                emailInput.text.Trim(),
                firstNameInput.text.Trim(),
                lastNameInput.text.Trim()
            );

            ServerScript.instance.SendRequest(action, user);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Load lobby scene
        }
    }

    /// Checks if all validation flags are true.
    private bool AllFieldsValid()
    {
        foreach (bool isValid in validationResults)
        {
            if (!isValid) return false;
        }
        return true;
    }

    /// Disables all error UI elements.
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

    /// Updates validation state and toggles error visibility.
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

    /// Sends a request to check if username/email already exists.
    private void CheckIfValueAlreadyExist(int index, string inputField)
    {
        bool exists = index == 0
            ? ServerScript.instance.SendRequest("check_username", $"{{\"username\":\"{inputField}\"}}") == true
            : ServerScript.instance.SendRequest("check_email", $"{{\"email\":\"{inputField}\"}}") == true;

        ValidateUsernameAndEmail(exists, index);
    }

    public void ValidateUsernameAndEmail(bool exists, int index)
    {
        if (index == 0) // Username
        {
            if (exists)
            {
                ValidateField(2, false, usernameExistError);
                usernameLengthError.enabled = false;
                usernameSpaceError.enabled = false;
            }
            else if (usernameInput.text.Length < 5)
            {
                ValidateField(2, false, usernameLengthError);
                usernameExistError.enabled = false;
                usernameSpaceError.enabled = false;
            }
            else if (usernameInput.text.Contains(" "))
            {
                ValidateField(2, false, usernameSpaceError);
                usernameExistError.enabled = false;
                usernameLengthError.enabled = false;
            }
            else
            {
                ValidateField(2, true, usernameLengthError);
                usernameExistError.enabled = false;
                usernameSpaceError.enabled = false;
            }
        }
        else // Email
        {
            if (exists)
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

    public void ValidateUsername() => CheckIfValueAlreadyExist(0, usernameInput.text);

    public void ValidateEmail() => CheckIfValueAlreadyExist(1, emailInput.text);

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
        bool match = passwordsArray[0].text == passwordsArray[1].text;
        ValidateField(5, match, confirmPasswordError);

        if (!match)
        {
            passwordsArray[0].text = "";
            passwordsArray[1].text = "";
        }
    }

    /// Navigates back to the login scene.
    public void AlreadyHaveAnACC()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    /// Clean disconnection from the server when application exits.
    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();

        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();

        Debug.Log("Disconnected from the server.");
    }
}

/// Serializable user object for signup submission.
public class User
{
    public string username;
    public string password;
    public string email;
    public string firstname;
    public string lastname;

    public User(string username, string password, string email, string firstname, string lastname)
    {
        this.username = username;
        this.password = password;
        this.email = email;
        this.firstname = firstname;
        this.lastname = lastname;
    }
}
