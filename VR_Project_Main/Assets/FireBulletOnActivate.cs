using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;

public class FireBulletOnActivate : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnPoint;
    public float shootForce, upwardForce;
    public float fireRate = 0.5f;
    public AudioSource audioSource;
    public AudioClip clip;
    public float volume = 0.5f;
    private float lastFireTime;
    private bool isFiring;


    void Start()
    {
        XRGrabInteractable grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(StartFiring);
        grabbable.deactivated.AddListener(StopFiring);
    }

    void Update()
    {
        if (isFiring)
        {
            if (Time.time - lastFireTime > fireRate)
            {
                FireBullet();
                lastFireTime = Time.time;
            }
        }
    }

    public void StartFiring(ActivateEventArgs arg)
    {
        isFiring = true;
        lastFireTime = Time.time;
    }

    public void StopFiring(DeactivateEventArgs arg)
    {
        isFiring = false;
    }

    public void FireBullet()
    {
        audioSource.PlayOneShot(clip, volume);
        GameObject spawnedBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
        spawnedBullet.GetComponent<Rigidbody>().velocity = spawnPoint.forward * shootForce * Time.deltaTime;
        Destroy(spawnedBullet, 5);
    }
}








