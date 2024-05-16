using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class ProjectileGun : MonoBehaviour
{
    public GameObject bullet;
    public float shootForce, upwardForce;
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    // bools
    bool shooting, readyToShoot, reloading;
    private float reloadStartTime;
    private float shootStartTime;
    private bool isFiringPressed;

    // Reference
    public Transform attackPoint;
    public ParticleSystem muzzleFlash;
    public TextMeshPro ammunitionDisplay;
    public AudioSource audioSource;
    public AudioClip clip;
    public float volume = 0.5f;

    // Bug fixing :D
    public bool allowInvoke = true;

    // Public references
    public GameObject reloadObject;
    public Collider reloadCollider;

    void Start()
    {
        XRGrabInteractable grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(StartFiring);
        grabbable.deactivated.AddListener(StopFiring);
    }

    public void StartFiring(ActivateEventArgs arg)
    {
        isFiringPressed = true;
    }

    public void StopFiring(DeactivateEventArgs arg)
    {
        isFiringPressed = false;
    }

    private void Awake()
    {
        // make sure magazine is full
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    void Update()
    {
        MyInput();

        // set ammo display, if it exists :D
        if (ammunitionDisplay != null)
        {
            ammunitionDisplay.text = (bulletsLeft / bulletsPerTap) + " / " + (magazineSize / bulletsPerTap); // Changed to use TextMeshPro properties
            if ((float)bulletsLeft / magazineSize <= 0.2)
                ammunitionDisplay.color = new Color32(255, 0, 0, 255);
            else
                ammunitionDisplay.color = new Color32(0, 0, 0, 255);
        }

        if (reloading)
        {
            ammunitionDisplay.text = "Reloading..."; // Changed to use TextMeshPro properties
            RotateDuringReload();
        }

        if (!allowInvoke)
        {
            RotateDuringShoot();
        }
    }

    private void RotateDuringReload()
    {
        float reloadProgress = Mathf.Clamp01((Time.time - reloadStartTime) / reloadTime);
        float rotationAngle;

        if (reloadProgress < 0.5f)
        {
            rotationAngle = Mathf.SmoothStep(0f, 45f, reloadProgress * 2f);
        }
        else
        {
            rotationAngle = Mathf.SmoothStep(45f, 0f, (reloadProgress - 0.5f) * 2f);
        }

        transform.localRotation = Quaternion.Euler(rotationAngle, 0f, 0f);
    }

    private void MyInput()
    {
        // Check if allowed to hold down button and take corresponding input
        if (allowButtonHold) shooting = isFiringPressed;
        else shooting = isFiringPressed;

        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading && reloadCollider != null && reloadObject != null && reloadObject.activeInHierarchy) Reload();
        
        // Reload automatically when trying to shoot without ammo
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        // Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        audioSource.PlayOneShot(clip, volume);
        shootStartTime = Time.time;

        readyToShoot = false;

        // Calculate direction from attackPoint
        Vector3 directionWithoutSpread = attackPoint.forward;
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        // Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        // Rotate bullet to the shoot direction
        currentBullet.transform.forward = directionWithSpread;

        // Add forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(transform.up * upwardForce, ForceMode.Impulse);
        Destroy(currentBullet, 5);

        // Instantiate muzzle flash, if you have one
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        bulletsLeft--;
        bulletsShot++;

        // Invoke resetShot function
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        // if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void RotateDuringShoot()
    {
        float reloadProgress = 0;

        if (timeBetweenShots > 0.2)
        {
            reloadProgress = Mathf.Clamp01((Time.time - shootStartTime) / timeBetweenShots);
        }
        else
        {
            reloadProgress = Mathf.Clamp01((Time.time - shootStartTime) / timeBetweenShooting * 2);
        }


        float rotationAngle;

        if (reloadProgress < 0.5f)
        {
            rotationAngle = Mathf.SmoothStep(0f, -10f, reloadProgress * 2f);
        }
        else
        {
            rotationAngle = Mathf.SmoothStep(-10f, 0f, (reloadProgress - 0.5f) * 2f);
        }

        transform.localRotation = Quaternion.Euler(rotationAngle, 0f, 0f);
    }

    private void ResetShot()
    {
        // Allow shooting and invoking again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        reloadStartTime = Time.time;
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        while (reloadObject.activeInHierarchy)
        {
            yield return null;
        }

        reloading = false;
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
}
