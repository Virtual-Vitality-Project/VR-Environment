using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KofferReturn : MonoBehaviour
{
    public List<GameObject> allowedObjects;
    public ParticleSystem teleportEffectBefore;
    public ParticleSystem teleportEffectAfter;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!allowedObjects.Contains(collision.gameObject))
        {
            TeleportBack();
        }
    }

    void TeleportBack()
    {
        if (teleportEffectBefore != null)
        {
            Instantiate(teleportEffectBefore, transform.position, Quaternion.identity);
        }

        transform.position = initialPosition;

        if (teleportEffectAfter != null)
        {
            Instantiate(teleportEffectAfter, initialPosition, Quaternion.identity);
        }
    }
}
