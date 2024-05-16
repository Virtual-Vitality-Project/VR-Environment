using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GlobalVariableStorage : MonoBehaviour
{
    public static string TeacherEmail { get; set; }
    public static string TeacherName { get; set; }
    public static string StudentEmail { get; set; }
    public static string StudentName { get; set; }
    public static string SceneName { get; set; }
    public static float StartTime { get; set; }
    public static float EndTime { get; set; }
    public static bool IsTiming { get; set; }

    public static void startLevelTime()
    {
        StartTime = Time.time;
        IsTiming = true;
    }

    public static void stopLevelTime()
    {
        if (IsTiming)
        {
            EndTime = Time.time;
            IsTiming = false;
        }
        else
        {
            Debug.LogWarning("Trying to stop level time without starting it first.");
        }
    }

    public static float getFullLevelTime()
    {
        if (IsTiming)
        {
            Debug.LogWarning("Level time is still running. Stop it before getting the full time.");
            return 0f;
        }
        return EndTime - StartTime;
    }

    public static float getCurrentLevelTime()
    {
        return Time.time - StartTime;
    }



}
