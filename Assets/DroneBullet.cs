using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBullet : MonoBehaviour
{
    public float movementSpeed;
    private float Timer = 0.1f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * movementSpeed);

        IEnumerator SelfDestruct()
        {
            yield return new WaitForSeconds(Timer);
            Destroy(this.gameObject);
        }
        
    }


}
