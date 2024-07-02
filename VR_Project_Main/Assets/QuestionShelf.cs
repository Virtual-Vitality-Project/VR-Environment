using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class QuestionShelf : MonoBehaviour
{
    public GameObject KofferShelf_floor_shelf_prefab; // Object for each floor to create vertical elevator
    public GameObject KofferShelf_floor_shelf_top;
    public GameObject Grab_Interactable_prefab; // Prefab object for each answer
    public TextMeshPro textQuestion; // set to current question string .text
    public GameObject kofferShelf;
    public GameObject collisionDetector;
    public TextMeshPro textCurrentQuestion;
    public TextMeshProUGUI textDebug;

    public List<GameObject> allowedObjectsList;
    public List<GameObject> spawnedObjects;

    public bool playerWon = false;

    public float yOffset = 0.5f;
    private int previousQuestion = 1;
    private int currentQuestion = 0; // Start from the first question
    private int amountOfObjectInsideTrigger = 0;
    private List<GameObject> shelfInstances = new List<GameObject>();



    public Dictionary<string, (List<string> answers, int correctIndex)> questionsAndAnswers = new Dictionary<string, (List<string>, int)>()
    {
        {"How many legs does a dog have?", (new List<string>{"Four", "Two", "Five", "Three"}, 0)},
        {"Which animal says 'meow'?", (new List<string>{"Dog", "Cat", "Cow", "Bird"}, 1)},
        {"What color is the sky?", (new List<string>{"Green", "Blue", "Yellow", "Red"}, 1)},
        {"What is the capital of France?", (new List<string>{"London", "Madrid", "Paris", "\"F\""}, 2)},

    };

    public Dictionary<int, int> selectedAnswers = new Dictionary<int, int>();

    void Start()
    {
        for (int i = 0; i < questionsAndAnswers.Count; i++)
        {
            selectedAnswers[i] = -1;
        }

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
            GameObject newShelf = Instantiate(KofferShelf_floor_shelf_prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 180, 0), kofferShelf.transform);
            newShelf.transform.localPosition = new Vector3(1.404f, yOffset * i, 0.392f);

            allowedObjectsList.Add(newShelf);

            // Set the text for "Text (Answer)"
            TextMeshPro answerText = newShelf.transform.Find("Text (Answer)").GetComponent<TextMeshPro>();
            answerText.text = questionsAndAnswers.Keys.ElementAt(i);

            // Adjust the size of "Shelf_cover"
            Transform shelfCover = newShelf.transform.Find("Shelf_cover");
            shelfCover.localScale = Vector3.one;

            shelfInstances.Add(newShelf);
        }

        // Set the text for the first shelf's "Text (Answer)"
        TextMeshPro firstShelfAnswerText = KofferShelf_floor_shelf_prefab.transform.Find("Text (Answer)").GetComponent<TextMeshPro>();
        firstShelfAnswerText.text = questionsAndAnswers.Keys.ElementAt(0);

        // Adjust the size of "Shelf_cover" for the first shelf
        Transform firstShelfCover = KofferShelf_floor_shelf_prefab.transform.Find("Shelf_cover");
        firstShelfCover.localScale = Vector3.one;
    }

    void DisplayQuestionAndAnswers()
    {
        // Get the current question
        string question = questionsAndAnswers.Keys.ElementAt(currentQuestion);

        // Display the question
        textQuestion.text = (currentQuestion + 1).ToString() + ". " + question;
        textCurrentQuestion.text = (currentQuestion + 1).ToString() + " / " + questionsAndAnswers.Count;

        // Destroy existing answer objects if any
        DestroyAnswers();

        // Get the tuple containing answers and correct index for the current question
        var tuple = questionsAndAnswers[question];

        // Pass the answers and correct index to the SpawnAnswers function
        SpawnAnswers(tuple.answers, tuple.correctIndex);

        // Move the shelves to reflect the current question index
        StartCoroutine(MoveShelvesCoroutine());
    }

    IEnumerator MoveShelvesCoroutine()
    {
        float targetYPosition = yOffset * currentQuestion - 0.55f;

        // Calculate the number of questions scrolled
        int questionsScrolled = Mathf.Abs(currentQuestion - previousQuestion);

        // Adjust the duration based on the number of questions scrolled
        float duration = 0.5f * questionsScrolled;

        float elapsedTime = 0f;
        Vector3[] startPositions = shelfInstances.Select(shelf => shelf.transform.localPosition).ToArray();

        // Get the initial position of the top shelf
        Vector3 topStartPosition = KofferShelf_floor_shelf_top.transform.localPosition;
        Vector3 topTargetPosition = new Vector3(topStartPosition.x, targetYPosition, topStartPosition.z);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothStep = t * t * (3f - 2f * t);

            // Calculate the target position for the top shelf
           
            KofferShelf_floor_shelf_top.transform.localPosition = Vector3.Lerp(topStartPosition, topTargetPosition, smoothStep);

            for (int i = 0; i < shelfInstances.Count; i++)
            {
                shelfInstances[i].transform.localPosition = Vector3.Lerp(startPositions[i], new Vector3(startPositions[i].x, targetYPosition - yOffset * i, startPositions[i].z), smoothStep);

                Transform shelfCover = shelfInstances[i].transform.Find("Shelf_cover");
                if (i == currentQuestion)
                {
                    shelfCover.localScale = Vector3.Lerp(shelfCover.localScale, new Vector3(1, 0.3f, 1), smoothStep);
                    shelfCover.localRotation = Quaternion.Lerp(shelfCover.localRotation, Quaternion.Euler(90f, 0f, 0f), smoothStep);

                }
                else
                {
                    shelfCover.localScale = Vector3.Lerp(shelfCover.localScale, Vector3.one, smoothStep);
                    shelfCover.localRotation = Quaternion.Lerp(shelfCover.localRotation, Quaternion.Euler(0f, 0f, 0f), smoothStep);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position and size are set correctly
        for (int i = 0; i < shelfInstances.Count; i++)
        {
            shelfInstances[i].transform.localPosition = new Vector3(startPositions[i].x, targetYPosition - yOffset * i, startPositions[i].z);
            KofferShelf_floor_shelf_top.transform.localPosition = topTargetPosition;

            Transform shelfCover = shelfInstances[i].transform.Find("Shelf_cover");
            if (i == currentQuestion)
            {
                shelfCover.localScale = new Vector3(1, 0.3f, 1);
                shelfCover.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                shelfCover.localScale = Vector3.one;
                shelfCover.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        // Update the previous question index
        previousQuestion = currentQuestion;
    }

    // Configurable size parameters
    public float minSize = 0.12f;
    public float maxSize = 0.25f;
    public int maxQuestionsForMinSize = 10;

    // Function to spawn answer options for the current question
    public void SpawnAnswers(List<string> answers, int correctIndex)
    {
        UpdateDebugText();
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
            // if (i == correctIndex)
            // {
            //     newAnswer.transform.Find("Text (answer)").GetComponent<TextMeshPro>().color = Color.green;
            // }

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

        // Check if there was a previously selected answer for the current question
        if (selectedAnswers.ContainsKey(currentQuestion))
        {
            int selectedAnswerIndex = selectedAnswers[currentQuestion];
            if (selectedAnswerIndex >= 0)
            {
                Debug.Log("Found selectedAnswerIndex for this question: " + selectedAnswerIndex);
                GameObject selectedAnswer = spawnedAnswerObjects[selectedAnswerIndex];
                Debug.Log(selectedAnswer + "Answer gameobject");
                // Move the selected answer to the collisionDetector position
                selectedAnswer.transform.position = collisionDetector.transform.position;
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

    public void AnswerTriggeredEnter(int objectsInsideCount, int answerIndex)
    {
        amountOfObjectInsideTrigger = objectsInsideCount;
        selectedAnswers[currentQuestion] = answerIndex;
        UpdateDebugText();
    }

    public void AnswerTriggeredExit(int objectsInsideCount)
    {
        amountOfObjectInsideTrigger = objectsInsideCount;
        selectedAnswers[currentQuestion] = -1;
        UpdateDebugText();
    }

    public void UpdateDebugText() {
        string result = string.Join("\n", selectedAnswers.Select(answer => $"Question {answer.Key}: Selected Answer Index = {answer.Value}"));
        textDebug.text = "currentQuestion: " + (currentQuestion+1) + "\nAmount: " + amountOfObjectInsideTrigger + "\n\nselectedAnswers:\n" + result;
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


    void ValidateAnswers()
    {
        int questionIndex = 0;
        foreach (var question in questionsAndAnswers)
        {
            if (selectedAnswers.ContainsKey(questionIndex))
            {
                int selectedAnswerIndex = selectedAnswers[questionIndex];
                int correctAnswerIndex = question.Value.correctIndex;

                if (selectedAnswerIndex != correctAnswerIndex)
                {
                    selectedAnswers[questionIndex] = -1; // Set wrong answers to -1
                }
            }
            questionIndex++;
        }

        // Debugging output to verify the results
        foreach (var answer in selectedAnswers)
        {
            Debug.Log($"Question {answer.Key}: Selected Answer Index = {answer.Value}");
        }
    }


    public void SetPlayerWon()
    {
        playerWon = true;
    }

    public void KeypadAccessDenied()
    {
        ValidateAnswers();
        currentQuestion = 0;
        DisplayQuestionAndAnswers();
    }
}


