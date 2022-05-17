using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

    public int resource = 50;
    public bool isBeingForaged = false;
    public int mined=0;

    public GameObject deathEffect;
    public GameObject deathSound;

    // Start is called before the first frame update
    void Start()
    {
        resource = Random.Range(10, 100);
    }

    // Update is called once per frame
    void Update()
    {
        if(mined > resource)
        {
            Destroy(this.gameObject);
            Instantiate(deathEffect, transform.position, transform.rotation);
            Instantiate(deathSound, transform.position, transform.rotation);
        }
    }
}
