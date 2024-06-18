using System.Collections;
using TMPro;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public GameObject requiredObject; // Reference to the object that should be placed in the special zone
    public GameObject door; // Reference to the door object
    public AudioSource sound; // Reference to the audio source for sound playback

    private Quaternion targetRotationOpen = Quaternion.Euler(-90, 0, 180); // Desired orientation for the door when open
    private Quaternion targetRotationClosed = Quaternion.Euler(-90, 0, -90); // Desired orientation for the door when closed
    public float doorOpenSpeed = 2.0f; // Speed of door opening
    private bool isOpen = false; // Flag to track if the door is open
    private Coroutine doorMovementCoroutine; // Reference to the door movement coroutine

    private int objectsInsideCount = 0; // Counter to track how many objects are inside the trigger zone

    private static QuestionShelf questionShelfInstance;

    private void Start()
    {
        // Ensure you have a reference to the QuestionShelf instance
        questionShelfInstance = FindObjectOfType<QuestionShelf>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other + " entered the trigger");
        if (other.CompareTag("Answer"))
        {
            objectsInsideCount++; // Increment the count of objects inside

            int answerIndex = int.Parse(other.transform.Find("Text (number)").GetComponent<TextMeshPro>().text) - 1;
            Debug.Log("answerIndex = " + answerIndex);

            questionShelfInstance.AnswerTriggeredEnter(objectsInsideCount, answerIndex);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other + " exited the trigger");
        if (other.CompareTag("Answer"))
        {
            objectsInsideCount--; // Decrement the count of objects inside

            questionShelfInstance.AnswerTriggeredExit(objectsInsideCount);
        }
    }

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
