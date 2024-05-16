using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public TMP_Text sceneName;
    public TMP_Text studentName;
    public TMP_Text teacherName;
    public TMP_Text timerText;

    void Start()
    {
        sceneName.text = GlobalVariableStorage.SceneName;
        studentName.text = $"Student: {GlobalVariableStorage.StudentName} ({GlobalVariableStorage.StudentEmail})";
        teacherName.text = $"Docent: {GlobalVariableStorage.TeacherName} ({GlobalVariableStorage.TeacherEmail})";
        GlobalVariableStorage.startLevelTime();
    }

    private void Update()
    {
        float currentTime = GlobalVariableStorage.getCurrentLevelTime();
        int minutes = (int)(currentTime / 60);
        int seconds = (int)(currentTime % 60); 

        timerText.text = "Elapsed time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
