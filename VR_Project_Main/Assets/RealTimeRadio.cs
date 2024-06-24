using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealTimeAudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips; // List of audio clips to be played
    private AudioSource audioSource;
    private int currentClipIndex;

    public TextMeshProUGUI audioInfo;
    public Slider progressBar;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Calculate the current audio clip and start time
        CalculateAndPlayAudio();

        // Start the coroutine to update the audio info
        StartCoroutine(UpdateAudioInfo());
    }

    private void CalculateAndPlayAudio()
    {
        // Calculate the total length of all audio clips
        float totalLength = 0;
        foreach (var clip in audioClips)
        {
            totalLength += clip.length;
        }

        // Get the current time in seconds since midnight
        float currentTimeSeconds = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        // Calculate the time in the audio cycle (loop)
        float audioTime = currentTimeSeconds % totalLength;

        Debug.Log("audioTime: " + audioTime);

        // Determine the current clip and starting time
        float accumulatedLength = 0;
        for (int i = 0; i < audioClips.Count; i++)
        {
            var clip = audioClips[i];
            if (accumulatedLength + clip.length > audioTime)
            {
                Debug.Log("clip: " + clip);
                audioSource.clip = clip;
                audioSource.time = audioTime - accumulatedLength;
                audioSource.Play();
                currentClipIndex = i;
                break;
            }
            accumulatedLength += clip.length;
        }

        // Start the coroutine to handle audio switching
        StartCoroutine(PlayNextClipWhenFinished());
    }

    private System.Collections.IEnumerator PlayNextClipWhenFinished()
    {
        while (true)
        {
            // Wait until the current clip finishes playing
            yield return new WaitForSeconds(audioSource.clip.length - audioSource.time);

            Debug.Log("Move to the next clip in the list.");
            currentClipIndex = (currentClipIndex + 1) % audioClips.Count;
            audioSource.clip = audioClips[currentClipIndex];
            audioSource.time = 0;
            audioSource.Play();
        }
    }

    private System.Collections.IEnumerator UpdateAudioInfo()
    {
        while (true)
        {
            // Update audio info every second
            yield return new WaitForSeconds(1);

            if (audioSource.isPlaying)
            {
                string clipName = audioSource.clip.name;
                int elapsedTime = (int)audioSource.time;
                int clipLength = (int)audioSource.clip.length;
                audioInfo.text = $"Now playing: {clipName} ({elapsedTime}/{clipLength})";

                // Update the progress bar
                progressBar.value = (float)elapsedTime / clipLength;
            }
            else
            {
                // Ensure the progress bar is reset when not playing
                progressBar.value = 0;
            }
        }
    }
}
