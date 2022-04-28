using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Fusion.XR;
using UnityEngine.Events;

public class Guns : MonoBehaviour
{
    public bool isAutomatic;
    public bool debug;
    public Rigidbody rb;
    public InputActionReference magEjectButton;
    public Transform firePoint;
    public Transform ejectPoint;
    public GameObject fireAudio;
    public GameObject fakeMag;
    public GameObject newMag;
    public float damage;
    public float forceOfBullet;
    public float slideBackDistance;
    public float slideUpDistance;
    public float fireRate = 1f;
    private float nextRate = 0f;
    public Vector3 recoil;
    public Vector3 twoHandedRecoil;
    public Grabbable grabScript;
    public GrabPoint rightHandPoint;
    public GrabPoint leftHandPoint;
    public DistanceReader slideDistanceReader;
    public bool hasMag;
    private bool objectGrabbed;
    private bool shoot;
    private bool canShoot;
    private bool hasBullets;
    private bool back;
    private bool wasBack;
    private bool bulletLoaded;
    public float bullets;
    [SerializeField] private UnityEvent onFire;
    // Start is called before the first frame update
    public void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(slideDistanceReader.distance >= slideBackDistance)
        {
            back = true;
            wasBack = true;
        }
        else
        {
            back = false;
        }
        if(back == false && bullets <= 0)
        {
            wasBack = false;
        }
        if(slideDistanceReader.distance <= slideUpDistance && wasBack == true && bullets > 0) 
        {
            bulletLoaded = true;
            wasBack = false;
        }
        if(hasMag == true)
        {
            fakeMag.SetActive(true);
        }
        else
        {
            fakeMag.SetActive(false);
        }
        if (magEjectButton.action.triggered && debug == true)
        {
            Debug.Log("Mag Eject Pressed");
        }
        objectGrabbed = grabScript.isGrabbed;
        Ray fireRay = new Ray(firePoint.position, firePoint.forward);
        RaycastHit shot;
        if(bullets <= 0)
        {
            hasBullets = false;
            bulletLoaded = false;
        }
        else
        {
            hasBullets = true;
        }
        if(objectGrabbed == true)
        {
            if (magEjectButton.action.triggered && hasMag == true)
            {
                Instantiate(newMag, ejectPoint.position, ejectPoint.rotation);
                hasMag = false;
                if(hasBullets == true)
                {
                    bullets = 1;
                    newMag.GetComponent<MagData>().bullets = bullets;
                }
                else
                {
                    bullets = 0;
                    newMag.GetComponent<MagData>().bullets = 0;
                }
            }
            if(shoot == true && canShoot == true && bullets > 0 && bulletLoaded == true && Time.time > nextRate)
            {
                nextRate = Time.time + fireRate;
                Instantiate(fireAudio, firePoint);
                if(grabScript.attachedHands.Count >= 2)
                {
                    rb.AddRelativeForce(twoHandedRecoil);
                }
                else
                {
                    rb.AddRelativeForce(recoil);
                }
                if(isAutomatic == false) 
                {
                    canShoot = false;
                }
                else
                {
                    canShoot = true;
                }
                bullets -= 1;
                if (Physics.Raycast(fireRay, out shot))
                {
                    if(shot.collider.gameObject.GetComponent<Rigidbody>() != null)
                    {
                        shot.collider.gameObject.GetComponent<Rigidbody>().AddForce(firePoint.forward * forceOfBullet, ForceMode.Impulse);
                    }
                    if (shot.collider.gameObject.GetComponent<health>() != null)
                    {
                        shot.collider.gameObject.GetComponent<health>().healthNumber -= damage;
                    }
                    
                }
                Debug.DrawLine(fireRay.origin, shot.point, Color.red);
                Debug.Log("Shot");
                onFire.Invoke();
            }
        }
    }
    public void fire()
    {
        shoot = true;
    }

    public void nofire()
    {
        shoot = false;
        canShoot = true;
    }
}
