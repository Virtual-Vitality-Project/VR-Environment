using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SituationSelectionManager : MonoBehaviour
{

    public TMP_Text logCredentials;
    public TMP_Text errorOutput;
    public Button goToLoginButton;
    public Button exitButton;

    void Start()
    {
        goToLoginButton.onClick.AddListener(goToLogin);
        exitButton.onClick.AddListener(Exit);
        logCredentials.text = "Logged in as: " + GlobalVariableStorage.TeacherEmail;
    }

    void DisplayError(string message)
    {
        Debug.LogError(message);
        errorOutput.text = message;
    }

    void goToLogin()
    {
        GlobalVariableStorage.TeacherEmail = null;
        GlobalVariableStorage.TeacherName = null;
        Debug.Log("User went to Login scene. Teacher email: " + GlobalVariableStorage.TeacherEmail);
        SceneManager.LoadScene("Login");
    }

    void Exit()
    {
        Debug.Log("User exitted the application!");
        // UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
