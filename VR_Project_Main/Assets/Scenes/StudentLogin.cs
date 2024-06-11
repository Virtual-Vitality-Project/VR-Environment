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


        string email = usernameInput.text;
        string password = passwordInput.text;

        LoginAPI test = new LoginAPI(email, password, loginAPIfilePath, "00080E2255A4CDC4210DCC5AABB574CCoi2hro32j@#I$o32h432n");
        int loginResult = test.isValidLogin();//Login function
        LoginAPI.OkJsonResponse LoginAPIResponse = test.getJsonObj();//Get response obj

        if (loginResult == 2)
        {//Config file not found.
            DisplayError("Config file for LoginAPI not found.\n");
        }
        else if (loginResult == 3)
        {//LoginAPI not active or endpoint url incorrect. (Offline)
            DisplayError("LoginAPI not reachable or offline.\n");
        }
        else if (LoginAPIResponse.Error.Num == "0")
        {
            DisplayError("You are blacklisted!\n");
        }
        else if (LoginAPIResponse.Error.Num == "1" || LoginAPIResponse.Error.Num == "2" || LoginAPIResponse.Error.Num == "3")
        {
            DisplayError("Something went wrong!\n");//1: Missing or invalid POST data | 2: Invalid jellyfish | 3: Department doesn't exist
        }
        else if (LoginAPIResponse.Error.Num == "4" || LoginAPIResponse.Error.Num == "5" || LoginAPIResponse.Error.Num == "6")
        {
            DisplayError("Wrong login details or no access, please try again.\n");//4: invalid email format | 5: user doesn't exist | 6: no matching role
        }
        else if (LoginAPIResponse.Error.Active == "false" && LoginAPIResponse.Error.Stat == "ok")
        {
            Debug.Log("Welcome, " + LoginAPIResponse.Data.Firstname + " " + LoginAPIResponse.Data.Lastname + "!\n");
            Debug.Log(test.getJsonResponse() + "\nFirst right the user has for this application: " + test.getJsonObj().Data.RightArr[0]);
            Debug.Log($"Logging in '{usernameInput.text}'");
            GlobalVariableStorage.StudentEmail = usernameInput.text;
            GlobalVariableStorage.StudentName = LoginAPIResponse.Data.Firstname;

            SceneManager.LoadScene(GlobalVariableStorage.SceneName); // go to selected scene
        }
        else
        {
            DisplayError("Unknown error: " + LoginAPIResponse.Error.Stat + "\n");
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