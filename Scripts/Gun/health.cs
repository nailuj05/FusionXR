using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class health : MonoBehaviour
{
    public float healthNumber;
    public bool getDamaged;
    float damageNumber;
    [SerializeField] private UnityEvent onDeath;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (healthNumber <= 0)
        {
            onDeath.Invoke();
        }

        if(getDamaged == true)
        {
            //GetComponent<damageGiver>().damage = damageNumber;
            //healthNumber -= damageNumber;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<BluntDamage>() != null && collision.gameObject.GetComponent<BluntDamage>().minVel <= collision.gameObject.GetComponent<BluntDamage>().vel)
        {
            healthNumber -= collision.gameObject.GetComponent<BluntDamage>().damage;
        }
    }
}
