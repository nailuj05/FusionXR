using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontCollide : MonoBehaviour
{
    public Collider SlideCollidor;
    public Collider[] GunCollidor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Physics.IgnoreCollision(SlideCollidor, GunCollidor[0], true);
        Physics.IgnoreCollision(SlideCollidor, GunCollidor[1], true);
        Physics.IgnoreCollision(SlideCollidor, GunCollidor[2], true);
        Physics.IgnoreCollision(SlideCollidor, GunCollidor[3], true);
        Physics.IgnoreCollision(SlideCollidor, GunCollidor[4], true);
        Physics.IgnoreCollision(SlideCollidor, GunCollidor[5], true);
    }
}
