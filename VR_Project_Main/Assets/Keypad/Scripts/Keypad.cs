using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NavKeypad
{
    public class Keypad : MonoBehaviour
    {
        [Header("Collision components")]
        public Transform playerHead; // Set this to the player's head/camera transform in the Inspector
        public Vector3 offsetFromHead = new Vector3(0, 0, 4f); // Offset from the player's head position
        public float moveSpeed = 5f; // Speed at which the keypad moves to the target position
        private bool isMoving = false;
        private Vector3 initialHeadPosition;
        private float deactivateDistance = 1f; // Distance threshold to deactivate the keyboard
        public GameObject Keyboard;


        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;
        [Header("Combination Code (9 Numbers Max)")]
        [SerializeField] private int keypadCombo = 12345;

        public UnityEvent OnAccessGranted => onAccessGranted;
        public UnityEvent OnAccessDenied => onAccessDenied;

        [Header("Settings")]
        [SerializeField] private string accessGrantedText = "Granted";
        [SerializeField] private string accessDeniedText = "Denied";

        [Header("Visuals")]
        [SerializeField] private float displayResultTime = 1f;
        [Range(0, 5)]
        [SerializeField] private float screenIntensity = 2.5f;
        [Header("Colors")]
        [SerializeField] private Color screenNormalColor = new Color(0.98f, 0.50f, 0.032f, 1f); //orangy
        [SerializeField] private Color screenDeniedColor = new Color(1f, 0f, 0f, 1f); //red
        [SerializeField] private Color screenGrantedColor = new Color(0f, 0.62f, 0.07f); //greenish
        [Header("SoundFx")]
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;
        [Header("Component References")]
        [SerializeField] private Renderer panelMesh;
        [SerializeField] private TMP_Text keypadDisplayText;
        [SerializeField] private AudioSource audioSource;
        // Find the GameObject that has the ExampleScript attached
        [SerializeField]private GameObject Questionshelf;


        private string currentInput;
        private bool displayingResult = false;
        private bool accessWasGranted = false;

        private Dictionary<int, int> selectedAnswers = new Dictionary<int, int>();
        public Dictionary<string, (List<string> answers, int correctIndex)> questionsAndAnswers = new Dictionary<string, (List<string>, int)>() { };

        public void Start()
        {
            // Get the QuestionShelf component from the GameObject
            QuestionShelf questionShelf = Questionshelf.GetComponent<QuestionShelf>();

            // Access and store the questionsAndAnswers variable
            questionsAndAnswers = questionShelf.questionsAndAnswers;

            // Print the questionsAndAnswers to the console
            foreach (var question in questionsAndAnswers)
            {
                Debug.Log($"Question: {question.Key}, Answers: {string.Join(", ", question.Value.answers)}, Correct Index: {question.Value.correctIndex}");
            }

            // Create a list to store the correct indexes
            List<int> correctIndexes = new List<int>();

            // Loop through the questionsAndAnswers to extract the correct indexes
            foreach (var question in questionsAndAnswers)
            {
                correctIndexes.Add(question.Value.correctIndex + 1);
            }

            // Print the correct indexes to the console
            Debug.Log("The correct indexes are: " + string.Join(", ", correctIndexes));
            // Convert the list of correct indexes to a single integer by concatenation
            string concatenatedIndexes = string.Join("", correctIndexes);
            keypadCombo = int.Parse(concatenatedIndexes);
        }

        //Debug only take away in production
        // Update method to capture keyboard input
        private void Update()
        {
            if (displayingResult || accessWasGranted) return;

            foreach (char c in Input.inputString)
            {
                if (char.IsDigit(c))
                {
                    AddInput(c.ToString());
                }
                else if (c == '\n' || c == '\r') // Enter/Return key
                {
                    CheckCombo();
                }
                else if (c == '\b' && currentInput.Length > 0) // Backspace key
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    keypadDisplayText.text = currentInput;
                }
            }

            if (isMoving)
            {



                

                // Check if the player has moved beyond the deactivate distance
                if (Vector3.Distance(playerHead.position, initialHeadPosition) > deactivateDistance)
                {
                    Debug.Log("Deactivated");
                    Keyboard.SetActive(false);
                    isMoving = false;
                }
            }
        }

        private void Awake()
        {
            ClearInput();
            panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);
        }


        //Gets value from pressedbutton
        public void AddInput(string input)
        {
            Debug.Log(input);
            audioSource.PlayOneShot(buttonClickedSfx);
            if (displayingResult || accessWasGranted) return;
            switch (input)
            {
                case "enter":
                    CheckCombo();
                    break;
                default:
                    if (currentInput != null && currentInput.Length == 9) // 9 max passcode size 
                    {
                        return;
                    }
                    currentInput += input;
                    keypadDisplayText.text = currentInput;
                    break;
            }

        }

        public void recieveStringFromKeyboard(string input)
        {
            switch (input)
            {
                case "enter":
                    CheckCombo();
                    break;
                default:
                    if (currentInput != null && currentInput.Length == 9) // 9 max passcode size 
                    {
                        return;
                    }
                    currentInput = input;
                    keypadDisplayText.text = currentInput;
                    Debug.Log(currentInput);
                    break;
            }
        }

        public void CheckCombo()
        {
            // Get the QuestionShelf component from the GameObject
            QuestionShelf questionShelf = Questionshelf.GetComponent<QuestionShelf>();

            // Access and store the selectedAnswers variable
            selectedAnswers = questionShelf.selectedAnswers;

            // Print the selectedAnswers to the console
            Debug.Log("The value of selectedAnswers is: " + string.Join(", ", selectedAnswers.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
            // Create a list to store the correct indexes
            List<int> correctIndexes = new List<int>();

            // Loop through the questionsAndAnswers to extract the correct indexes
            foreach (var question in selectedAnswers)
            {
                correctIndexes.Add(question.Value + 1);
            }
            // Convert the list of correct indexes to a single integer by concatenation
            string concatenatedIndexes = string.Join("", correctIndexes);
            Debug.Log(concatenatedIndexes);
            //If currentInput matches briefcases
            if (currentInput == concatenatedIndexes)
            {
                //If currentInput is actually correct
                if (currentInput == keypadCombo.ToString())
                {
                    if (!displayingResult)
                    {
                        StartCoroutine(DisplayResultRoutine(true));
                    }
                }                
            } else
            {
                StartCoroutine(DisplayResultRoutine(false));
                Debug.Log("The code doesnt match the briefcases");
            }

            if (isMoving)
            {
                Keyboard.SetActive(false);
            }

        }

        //mainly for animations 
        private IEnumerator DisplayResultRoutine(bool granted)
        {
            displayingResult = true;

            if (granted) AccessGranted();
            else AccessDenied();

            yield return new WaitForSeconds(displayResultTime);
            displayingResult = false;
            if (granted) yield break;
            ClearInput();
            panelMesh.material.SetVector("_EmissionColor", screenNormalColor * screenIntensity);

        }

        private void AccessDenied()
        {
            keypadDisplayText.text = accessDeniedText;
            onAccessDenied?.Invoke();
            panelMesh.material.SetVector("_EmissionColor", screenDeniedColor * screenIntensity);
            audioSource.PlayOneShot(accessDeniedSfx);
        }

        private void ClearInput()
        {
            currentInput = "";
            keypadDisplayText.text = currentInput;
        }

        private void AccessGranted()
        {
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            onAccessGranted?.Invoke();
            panelMesh.material.SetVector("_EmissionColor", screenGrantedColor * screenIntensity);
            audioSource.PlayOneShot(accessGrantedSfx);
        }

        // Collision detection with VR hand or player
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("ON");
            isMoving = true;

            // Activate and position the keyboard relative to the player's head
            Keyboard.SetActive(true);
            Keyboard.transform.SetParent(playerHead);
            Keyboard.transform.localPosition = offsetFromHead;
            /*// Smoothly move the keypad towards the target position
            Keyboard.transform.position = Vector3.Lerp(transform.position, Keyboard.transform.position, 2);*/
            // Track the initial position of the player's head
            initialHeadPosition = playerHead.position;
        }
    }
}