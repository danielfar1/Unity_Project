using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;

public class SignUpManager : MonoBehaviour
{
    private const string NamePattern = @"^[a-zA-Z]+$";
    private const string EmailPattern = @"^([A-Za-z][A-Za-z0-9]+)([\._]\w+)?@(\w+)(\.\w+)(\.\w+)?$";
    private const string PasswordPattern = @"^(?=.*\d)(?=.*[A-Za-z])[A-Za-z\d]+$";

    private bool[] validationResults = new bool[6];
    private string[] realTexts;
    private string[] textWithoutPoints = { "", "" };
    public bool isMasked = true;
    private string dbPath;

    [Header("EyeImage")]
    public SpriteRenderer eyeOpen;
    public SpriteRenderer eyeClose;

    [Header("Cursor")]
    public Texture2D customCursor;

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
        dbPath = "URI=file:C:/FinalProject/1vs1Football.db";
        realTexts = new string[passwordsArray.Length];
        passwordsArray[0].onValueChanged.AddListener((value) => OnTextChanged(0, value));
        UpdateMaskedText(0);
        passwordsArray[1].onValueChanged.AddListener((value) => OnTextChanged(1, value));
        UpdateMaskedText(1);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        DisableAllErrors();
    }
    private void OnTextChanged(int index, string userInput)
    {
        realTexts[index] = userInput;
        if (userInput == "")
            textWithoutPoints[index] = "";
        else
            if (userInput.Length-1 == textWithoutPoints[index].Length)
                textWithoutPoints[index] += userInput[userInput.Length-1]; 
        if (isMasked)
        {
            UpdateMaskedText(index);
        }
    }
    private void UpdateMaskedText(int index)
    {
        if(passwordsArray[index].text != "")
        {
            passwordsArray[index].text = new string('●', realTexts[index].Length);
            passwordsArray[index].caretPosition = realTexts[index].Length;
        }
        
    }
    public void ToggleMask()
    {
        isMasked = !isMasked;
        for (int i = 0; i < passwordsArray.Length; i++)
        {
            if (isMasked)
            {
                UpdateMaskedText(i);
            }
            else
            {
                if(textWithoutPoints[i]!="")
                    passwordsArray[i].text = textWithoutPoints[i].Replace("●", "");
            }
            passwordsArray[i].caretPosition = passwordsArray[i].text.Length;
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
            string[] newdata = {usernameInput.text.Trim(),textWithoutPoints[0].Replace("●", "").Trim(), emailInput.text.Trim(), firstNameInput.text.Trim(), lastNameInput.text.Trim() };
            using (var connection = new SqliteConnection(dbPath))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT INTO users (id, username, password, email, firstname, lastname) 
                        VALUES (NULL, @username, @password, @email, @firstname, @lastname);";
                        command.Parameters.AddWithValue("@username", newdata[0]);
                        command.Parameters.AddWithValue("@password", newdata[1]);
                        command.Parameters.AddWithValue("@email", newdata[2]);
                        command.Parameters.AddWithValue("@firstname", newdata[3]);
                        command.Parameters.AddWithValue("@lastname", newdata[4]);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.ForceSoftware);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
    private bool CheckIfValueAlreadyExist(int index,string inputField)
    {
        string[] checks = { "@username", "@email" };
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT "+checks[index].Substring(1)+" FROM users WHERE " + checks[index].Substring(1) +"=" + checks[index];
                command.Parameters.AddWithValue(checks[index], inputField);
                using (var reader = command.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
    }
    public void ValidateUsername()
    {
        bool usernameExist = CheckIfValueAlreadyExist(0, usernameInput.text);
        if(usernameExist)
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

    public void ValidateEmail()
    {
        bool emailExist = CheckIfValueAlreadyExist(1, emailInput.text);
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

    public void ValidatePassword()
    {
        if (textWithoutPoints[0].Replace("●", "").Length < 8)
        {
            ValidateField(4, false, passwordLengthError);
            passwordFormatError.enabled = false;
        }
        else if (!Regex.IsMatch(textWithoutPoints[0].Replace("●", ""), PasswordPattern))
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
        ValidateField(5, textWithoutPoints[0].Replace("●", "") == textWithoutPoints[1].Replace("●", ""), confirmPasswordError);
    }
    public void AlreadyHaveAnACC()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex -1);
    }

}
