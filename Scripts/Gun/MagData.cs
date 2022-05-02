using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagData : MonoBehaviour
{
    public int magId;
    public float bullets;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Guns>() != null && other.gameObject.GetComponent<Guns>().hasMag == false && other.gameObject.GetComponent<Guns>().magId == magId)
        {
            gameObject.SetActive(false);
            other.gameObject.GetComponent<Guns>().bullets += bullets;
            other.gameObject.GetComponent<Guns>().originalBullets = bullets;
            other.gameObject.GetComponent<Guns>().hasMag = true;
        }
    }
}
