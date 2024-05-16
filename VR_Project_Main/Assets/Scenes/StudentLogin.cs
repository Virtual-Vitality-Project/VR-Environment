using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StudentLogin : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorOutput;
    public Button loginButton;
    public Button exitButton;

    ArrayList credentials;

    void Start()
    {
        loginButton.onClick.AddListener(login);
        exitButton.onClick.AddListener(BackToSceneHub);
    }

    void login()
    {

        string loginAPIfilePath = Application.dataPath + "/APIConfigSettings/loginAPI.ini";
        LoginAPI test = new LoginAPI(usernameInput.text, passwordInput.text, "student", loginAPIfilePath);

        int loginResult = test.isValidLogin();//Of using the obj, execute this function first.

        //Test with obj
        LoginAPI.OkJsonResponse LoginAPIResponse = test.getJsonObj();

        if (loginResult == 2)
        {//Config file not found.
            DisplayError("Config file for LoginAPI not found.\n");
        }
        else if (loginResult == 3)
        {//LoginAPI not active or endpoint url incorrect. (Offline)
            DisplayError("LoginAPI not reachable or offline.\n");
        }
        else if (LoginAPIResponse.Error.Active == "false" && LoginAPIResponse.Error.Stat == "ok" && LoginAPIResponse.Data.Is_valid_login == "true")
        {
            Debug.Log("Welkom, " + LoginAPIResponse.Data.Name + " " + LoginAPIResponse.Data.Nickname + "!\n");

            Debug.Log($"Logging in '{usernameInput.text}'");
            GlobalVariableStorage.StudentEmail = usernameInput.text;
            GlobalVariableStorage.StudentName = LoginAPIResponse.Data.Name;

            SceneManager.LoadScene(GlobalVariableStorage.SceneName); // go to selected scene
        }
        else if (LoginAPIResponse.Error.Active == "false" && LoginAPIResponse.Error.Stat == "ok" && LoginAPIResponse.Data.Is_valid_login == "false")
        {
            DisplayError("Invalid login details. Please try again.\n");
        }
        else if (LoginAPIResponse.Data.Is_valid_login != "true")
        {
            DisplayError("Error: " + LoginAPIResponse.Error.Stat + "\n");
        }



    }

    void BackToSceneHub()
    {
        Debug.Log("User went back to the scenes hub.");
        GlobalVariableStorage.SceneName = null;
        SceneManager.LoadScene("SituationSelectionHub");
    }

    void DisplayError(string message)
    {
        Debug.LogError(message);
        errorOutput.text = message;
    }

}