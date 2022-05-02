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
    [Header("Gun Atributes")]
    public bool isAutomatic;
    public float damage;
    public float forceOfBullet;
    public float fireRate = 1f;
    public Vector3 recoil;
    public Vector3 twoHandedRecoil;
    [Header("Objects")]
    public Rigidbody rb;
    public Transform firePoint;
    public Transform ejectPoint;
    public GameObject fireAudio;
    public GameObject fakeMag;
    public GameObject newMag;
    public GameObject Slide;
    private GameObject newGoldMag; // not functional right now
    public Grabbable grabScript;
    public GrabPoint rightHandPoint;
    public GrabPoint leftHandPoint;
    public DistanceReader slideDistanceReader;
    [Header("Mag / Slide")]
    public int magId;
    public bool lockBackSlide;
    public float slideBackDistance;
    public float slideUpDistance;
    public InputActionReference magEjectButton;
    [Header("Debug")]
    public bool debug;
    private float nextRate = 0f;
    [ReadOnly] public bool hasMag;
    private bool objectGrabbed;
    private bool shoot;
    private bool canShoot;
    private bool hasBullets;
    private bool back;
    private bool wasBack;
    private bool bulletLoaded;
    private bool magIsGold;
    [ReadOnly] public float bullets;
    [ReadOnly] public float originalBullets;
    [SerializeField] private UnityEvent onFire;
    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        #region TonOfIfStatments
        // Locks back slide
        if (Slide.GetComponent<KinematicSlider>().attachedHands.Count <= 0 && lockBackSlide == true)
        {
            Slide.transform.localPosition = new Vector3(0, 0, 0);
        }
        // Detects if the slide is back
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
        // Loads the bullet
        if(slideDistanceReader.distance <= slideUpDistance && wasBack == true && bullets > 0) 
        {
            bulletLoaded = true;
            wasBack = false;
        }
        // Creates the fake mag inside the gun
        if(hasMag == true)
        {
            fakeMag.SetActive(true);
        }
        else
        {
            fakeMag.SetActive(false);
        }
        // Debug text for the mag eject button
        if (magEjectButton.action.triggered && debug == true)
        {
            Debug.Log("Mag Eject Pressed");
        }
        #endregion
        objectGrabbed = grabScript.isGrabbed;
        Ray fireRay = new Ray(firePoint.position, firePoint.forward);
        RaycastHit shot;
        // Checks if the bullets are at zero or below if they are at zero then set hasBullets to false
        if(bullets <= 0)
        {
            hasBullets = false;
            bulletLoaded = false;
        }
        else
        {
            hasBullets = true;
        }
        // Checks if the gun is grabbed
        if(objectGrabbed == true)
        {
            #region MagEject
            // Ejects the mag
            if (magEjectButton.action.triggered && hasMag == true)
            {
                hasMag = false;
                if (magIsGold == true)
                {
                    Instantiate(newMag, ejectPoint.position, ejectPoint.rotation);
                    if (hasBullets == true)
                    {
                        bullets = 1;
                        newGoldMag.GetComponent<MagData>().bullets = originalBullets;
                    } else
                    {
                        bullets = 0;
                        newGoldMag.GetComponent<MagData>().bullets = originalBullets;
                    }
                }
                else
                {
                    Instantiate(newMag, ejectPoint.position, ejectPoint.rotation);
                    if (hasBullets == true)
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
            }
            #endregion
            #region Firing
            // Checks if the trigger is pulled, bullets loaded, and if the trigger was pressed within the fire rate
            if (shoot == true && canShoot == true && bullets > 0 && bulletLoaded == true && Time.time > nextRate)
            {
                nextRate = Time.time + fireRate;
                // Creates the fire audio source
                Instantiate(fireAudio, firePoint.position, firePoint.rotation);
                // Checks the amount of hands grabbing the gun to determine the amount of recoil to be added
                if(grabScript.attachedHands.Count >= 2)
                {
                    rb.AddRelativeForce(twoHandedRecoil);
                }
                else
                {
                    rb.AddRelativeForce(recoil);
                }
                // Checks if the gun is Automatic or not
                if(isAutomatic == false) 
                {
                    canShoot = false;
                }
                else
                {
                    canShoot = true;
                }
                // Removes a bullet appon shooting once
                bullets -= 1;
                // Shoots a raycast
                if (Physics.Raycast(fireRay, out shot))
                {
                    // Null Check
                    if(shot.collider.gameObject.GetComponent<Rigidbody>() != null)
                    {
                        // Applies force to the shot rigidbody
                        shot.collider.gameObject.GetComponent<Rigidbody>().AddForce(firePoint.forward * forceOfBullet, ForceMode.Impulse);
                    }
                    // Null Check
                    if (shot.collider.gameObject.GetComponent<health>() != null)
                    {
                        // Damages the shot object
                        shot.collider.gameObject.GetComponent<health>().healthNumber -= damage;
                    }
                    
                }
                if(debug == true)
                {
                    Debug.Log("Shot");
                }
                // Invokes the onFire unity event
                onFire.Invoke();
            }
            #endregion
        }
    }
    // Used for the onPinch Unity Events to make firing on each hand easier
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
