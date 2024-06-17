using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class QuestionShelf : MonoBehaviour
{
    public GameObject KofferShelf_floor_shelf_prefab; // Object for each floor to create vertical elevator
    public GameObject Grab_Interactable_prefab; // Prefab object for each answer
    public TextMeshPro textQuestion; // set to current question string .text
    public GameObject kofferShelf;
    public TextMeshPro textCurrentQuestion;

    public List<GameObject> allowedObjectsList;
    public List<GameObject> spawnedObjects;

    public float yOffset = 0.5f;
    private int currentQuestion = 0; // Start from the first question
    private List<GameObject> shelfInstances = new List<GameObject>();

    private Dictionary<string, (List<string> answers, int correctIndex)> questionsAndAnswers = new Dictionary<string, (List<string>, int)>()
    {
        {"What color is an apple?", (new List<string>{"Red", "Green", "Yellow", "Purple"}, 0)},
        {"Which animal says 'meow'?", (new List<string>{"Dog", "Cat", "Cow", "Bird"}, 1)},
        {"What color is the sky?", (new List<string>{"Purple", "Blue", "Gray", "White"}, 1)},
        {"What is the capital of France?", (new List<string>{"London", "Madrid", "Paris", "\"F\""}, 2)},
     
    };

    void Start()
    {
        // Create copies of KofferShelf_floor_shelf_prefab for each question
        CreateShelfInstances();

        // Display the initial question and its answers
        DisplayQuestionAndAnswers();
    }

    void CreateShelfInstances()
    {
        // Add the original shelf to the list
        shelfInstances.Add(KofferShelf_floor_shelf_prefab);

        // Create copies for each question
        for (int i = 1; i < questionsAndAnswers.Count; i++)
        {
            Vector3 newPosition = KofferShelf_floor_shelf_prefab.transform.position + new Vector3(0, yOffset * i, 0);
            GameObject newShelf = Instantiate(KofferShelf_floor_shelf_prefab, newPosition, Quaternion.Euler(0, 180, 0), kofferShelf.transform);
            shelfInstances.Add(newShelf);
        }
    }

    void DisplayQuestionAndAnswers()
    {
        // Get the current question
        string question = questionsAndAnswers.Keys.ElementAt(currentQuestion);

        // Display the question
        textQuestion.text = (currentQuestion + 1).ToString() +". "+ question;
        textCurrentQuestion.text = (currentQuestion + 1).ToString() + " / " + questionsAndAnswers.Count;

        // Destroy existing answer objects if any
        DestroyAnswers();

        // Get the tuple containing answers and correct index for the current question
        var tuple = questionsAndAnswers[question];

        // Pass the answers and correct index to the SpawnAnswers function
        SpawnAnswers(tuple.answers, tuple.correctIndex);

        // Move the shelves to reflect the current question index
        MoveShelves();
    }

    void MoveShelves()
    {
        StartCoroutine(MoveShelvesCoroutine());
    }

    IEnumerator MoveShelvesCoroutine()
    {
        float targetYPosition = -yOffset * currentQuestion;
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3[] startPositions = shelfInstances.Select(shelf => shelf.transform.position).ToArray();

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            for (int i = 0; i < shelfInstances.Count; i++)
            {
                shelfInstances[i].transform.position = Vector3.Lerp(startPositions[i], new Vector3(0, targetYPosition - yOffset * i, 0), t);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    // Configurable size parameters
    public float minSize = 0.12f;
    public float maxSize = 0.2f;
    public int maxQuestionsForMinSize = 10;

    // Function to spawn answer options for the current question
    public void SpawnAnswers(List<string> answers, int correctIndex)
    {
        float minX = -0.6f;
        float maxX = 0.6f;
        float stepX = (maxX - minX) / (answers.Count - 1);

        float size = Mathf.Lerp(maxSize, minSize, (answers.Count - 1) / (float)(maxQuestionsForMinSize - 1));

        List<GameObject> spawnedAnswerObjects = new List<GameObject>();

        // Spawn all answer objects and store their references
        for (int i = 0; i < answers.Count; i++)
        {
            float xPos = minX + stepX * i;

            GameObject newAnswer = Instantiate(Grab_Interactable_prefab, new Vector3(xPos, 0.3f, 0.4f), Quaternion.identity);

            // Set the number and answer texts
            newAnswer.transform.SetParent(kofferShelf.transform);
            newAnswer.transform.localPosition = new Vector3(xPos, 0.3f, 0.4f); // Set local position after setting the parent

            newAnswer.transform.Find("Text (number)").GetComponent<TextMeshPro>().text = (i + 1).ToString(); // Text (number)
            newAnswer.transform.Find("Text (answer)").GetComponent<TextMeshPro>().text = answers[i]; // Text (answer)

            // Scale the prefab based on the number of answers
            newAnswer.transform.localScale = Vector3.one * size;

            // Indicate the correct answer by changing its color or adding an icon
            if (i == correctIndex)
            {
                newAnswer.transform.Find("Text (answer)").GetComponent<TextMeshPro>().color = Color.green;
            }

            // Add the new answer object to the list of spawned answer objects
            spawnedAnswerObjects.Add(newAnswer);

            // Set the name of the new answer object with an ID at the end
            newAnswer.name = $"{Grab_Interactable_prefab.name}_{i}";
        }

        // Update the allowedObjects list for each KofferReturn component
        foreach (GameObject answerObject in spawnedAnswerObjects)
        {
            KofferReturn kofferReturnComponent = answerObject.GetComponent<KofferReturn>();
            if (kofferReturnComponent != null)
            {
                kofferReturnComponent.allowedObjects = new List<GameObject>(allowedObjectsList);
                kofferReturnComponent.allowedObjects.AddRange(spawnedAnswerObjects);
            }
        }
    }

    // Function to destroy existing answer objects
    void DestroyAnswers()
    {
        // Find all children of kofferShelf with the tag "Answer" and destroy them
        foreach (Transform child in kofferShelf.transform)
        {
            if (child.CompareTag("Answer"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void TopButtonClicked()
    {
        currentQuestion--;
        if (currentQuestion < 0)
        {
            currentQuestion = questionsAndAnswers.Count - 1;
        }
        DisplayQuestionAndAnswers();
    }

    public void BottomButtonClicked()
    {
        currentQuestion++;
        if (currentQuestion >= questionsAndAnswers.Count)
        {
            currentQuestion = 0;
        }
        DisplayQuestionAndAnswers();
    }
}
