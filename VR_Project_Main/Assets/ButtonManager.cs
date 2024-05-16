using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ButtonManager : MonoBehaviour
{
    public GameObject content;
    public Button buttonPrefab;

    // Array containing scene names for loading
    private string[] sceneNames = { "MyScene", "Car Parking", "Heart Surgery", "Rotate Patient", "Learn First Aid", "Drive the PickUp",
                                    "Fire Range Safety Rules", "Matrix crashing againg", "Why do you even read this?", "Hospital room", "Calm and cozy lobby" };

    void Start()
    {
        // Creating buttons and setting their text
        for (int i = 0; i < sceneNames.Length; i++)
        {
            int sceneIndex = i; // Create a local variable to capture the current index
            Button button = Instantiate(buttonPrefab, content.transform);
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = sceneNames[i];

            // Adding click listener to the button
            button.onClick.AddListener(() => SetSceneAndLoad(sceneIndex));
        }
    }

    void SetSceneAndLoad(int sceneIndex)
    {
        // Set the selected scene index in the global storage
        GlobalVariableStorage.SceneName = sceneNames[sceneIndex];

        // Load the specified scene
        SceneManager.LoadScene("StudentLogin");
    }




    // Method for loading scene by its name
    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
