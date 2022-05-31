using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBullet : MonoBehaviour
{
    public float movementSpeed;
    private float Timer = 3f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * movementSpeed);

        //Debug.Log("Bullet destory");

        StartCoroutine(SelfDestruct());
        
        IEnumerator SelfDestruct()
        {
            Debug.Log("Bullet destory");
            yield return new WaitForSeconds(Timer);
            Destroy(this.gameObject);
            
        }
        
    }


}
