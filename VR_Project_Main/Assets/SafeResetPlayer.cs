using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeResetPlayer : MonoBehaviour
{
    public Vector3 startPosition;
    public GameObject mainCamera;
    public float minY = -2f;
    public float maxY = 4;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y < minY || transform.position.y > maxY)
        {
            Debug.LogWarning("Fell out of the map!\nReturning back.");
            Vector3 newPosition = startPosition + new Vector3(0, 0.5f, 0);
            transform.position = newPosition;
            mainCamera.transform.position = new Vector3(0,0,0);
        }
    }
}
