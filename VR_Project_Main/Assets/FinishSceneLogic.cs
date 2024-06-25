using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using UnityEngine;

public class FinishSceneLogic : MonoBehaviour
{
    private Quaternion targetRotationOpen = Quaternion.Euler(-90, 0, 180); // Desired orientation for the door when open
    private Quaternion targetRotationClosed = Quaternion.Euler(-90, 0, -90); // Desired orientation for the door when closed
    public float doorOpenSpeed = 2.0f; // Speed of door opening

    public GameObject door; // Reference to the door object
    public AudioSource sound; // Reference to the audio source for sound playback

    private bool isOpen = false; // Flag to track if the door is open
    private Coroutine doorMovementCoroutine; // Reference to the door movement coroutine

    public void OpenDoor()
    {
        // Set the flag indicating that the door is open
        isOpen = true;
        Debug.Log("Door is open: " + isOpen);

        // Play the sound
        sound.Play();

        // Stop any ongoing door movement coroutine
        if (doorMovementCoroutine != null)
        {
            StopCoroutine(doorMovementCoroutine);
        }

        // Start the coroutine for door opening
        doorMovementCoroutine = StartCoroutine(InterpolateRotation(door.transform, targetRotationOpen, doorOpenSpeed));
    }

    private void CloseDoor()
    {
        // Set the flag indicating that the door is closed
        isOpen = false;
        Debug.Log("Door is closed");

        // Play the sound
        sound.Play();

        // Stop any ongoing door movement coroutine
        if (doorMovementCoroutine != null)
        {
            StopCoroutine(doorMovementCoroutine);
        }

        // Start the coroutine for door closing
        doorMovementCoroutine = StartCoroutine(InterpolateRotation(door.transform, targetRotationClosed, doorOpenSpeed));
    }

    private IEnumerator InterpolateRotation(Transform objTransform, Quaternion targetRotation, float duration)
    {
        float timeElapsed = 0f;
        Quaternion startRotation = objTransform.rotation;

        while (timeElapsed < duration)
        {
            objTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        objTransform.rotation = targetRotation; // Ensure final rotation is exact
    }

}
