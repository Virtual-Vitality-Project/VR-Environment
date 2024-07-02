using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CollisionDetection : MonoBehaviour
{  
    private int objectsInsideCount = 0; // Counter to track how many objects are inside the trigger zone

    private static QuestionShelf questionShelfInstance;

    [SerializeField] private UnityEvent TriggerEntered;

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

        TriggerEntered?.Invoke();
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
}
