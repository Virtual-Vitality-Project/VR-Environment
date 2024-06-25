using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomInputField : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button submitButton;


    public void EmailInputReturned(){
        passwordInputField.Select();
    }
    public void PasswordInputReturned(){
        submitButton.onClick.Invoke();
    }   

  
}
