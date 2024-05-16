using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public GameObject requiredObject; // Reference to the object that should be placed in the special zone
    public GameObject door; // Reference to the door object
    public AudioSource sound; // Reference to the audio source for sound playback

    private Quaternion targetRotation = Quaternion.Euler(0, 90, 0); // Desired orientation for the door

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other + "entered the trigger");
        // Check if the object entering the zone matches the required object
        if (other.gameObject == requiredObject)
        {
            // Set the desired orientation for the door
            door.transform.rotation = targetRotation;

            // Play the sound
            sound.Play();
        }
    }
}
