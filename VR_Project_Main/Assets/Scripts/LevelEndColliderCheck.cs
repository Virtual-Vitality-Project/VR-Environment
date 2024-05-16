using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndColliderCheck : MonoBehaviour
{
    // Public variable for the object to check collision with
    public GameObject objectToCheckCollisionWith;

    // Name of the scene to load
    public string sceneToLoad;

    // Method called when this object triggers another object
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger with " + other.gameObject.name);
        // Checking if we have encountered the right object
        if (other.gameObject == objectToCheckCollisionWith)
        {
            // Change the scene to the specified one
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
