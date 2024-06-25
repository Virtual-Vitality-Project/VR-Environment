using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelEndColliderCheck : MonoBehaviour
{
    // Public variable for the object to check collision with
    public GameObject objectToCheckCollisionWith;

    [SerializeField] private UnityEvent inTheBoxWithoutPermission;

    // Name of the scene to load
    public string sceneToLoad;
    private bool isAccessGranted = false;

    // Method called when this object triggers another object
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger with " + other.gameObject.name);
        // Checking if we have encountered the right object
        if (other.gameObject == objectToCheckCollisionWith && isAccessGranted)
        {
            // Change the scene to the specified one
            SceneManager.LoadScene(sceneToLoad);
        }
        else {
            inTheBoxWithoutPermission?.Invoke();
        }
    }

    public void AccessGranted()
    {
        isAccessGranted = true;
    }
}
