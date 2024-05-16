using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalSceneManager : MonoBehaviour
{
    public TMP_Text timeSpent;
    public Button repeatButton;
    public Button goBackButton;

    void Start()
    {
        repeatButton.onClick.AddListener(repeatLevel);
        goBackButton.onClick.AddListener(goBack);
        GlobalVariableStorage.stopLevelTime();
        float currentTime = GlobalVariableStorage.getFullLevelTime();
        int minutes = (int)(currentTime / 60);
        int seconds = (int)(currentTime % 60);

        timeSpent.text = "Time spent: " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    void repeatLevel()
    {
        SceneManager.LoadScene(GlobalVariableStorage.SceneName);
    }

    void goBack()
    {
        SceneManager.LoadScene("SituationSelectionHub");
    }
}
