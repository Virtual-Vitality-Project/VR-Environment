using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KofferReturn : MonoBehaviour
{
    public List<GameObject> allowedObjects;
    public ParticleSystem teleportEffectBefore;
    public ParticleSystem teleportEffectAfter;
    public float moveDuration = 2f;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void Update()
    {
        if(transform.position.y < -2f)
        {
            StartCoroutine(TeleportBackCoroutine());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        MeshCollider meshCollider = collision.collider as MeshCollider;
        if (meshCollider != null)
        {
            GameObject collidedObject = meshCollider.gameObject;
            if (!IsAllowed(collidedObject))
            {
                StartCoroutine(TeleportBackCoroutine());
            }
        }
        else
        {
            StartCoroutine(TeleportBackCoroutine());
        }
    }

    bool IsAllowed(GameObject collidedObject)
    {
        // Check if the collided object is in the allowedObjects list
        if (allowedObjects.Contains(collidedObject))
        {
            return true;
        }

        // Check if the collided object is a child of any object in the allowedObjects list
        foreach (GameObject allowedObject in allowedObjects)
        {
            if (collidedObject.transform.IsChildOf(allowedObject.transform))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator TeleportBackCoroutine()
    {
        // Disable XR Grab Interactable
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Disable Rigidbody
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Move and rotate the object back to its initial position over 1 second
        
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, elapsedTime / moveDuration);
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Enable Rigidbody and XR Grab Interactable
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
    }
}
