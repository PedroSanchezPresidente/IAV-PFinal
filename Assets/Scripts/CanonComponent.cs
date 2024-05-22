using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CanonComponent : MonoBehaviour
{
    [SerializeField]
    GameObject end1;

    [SerializeField]
    GameObject end2;

    [SerializeField] 
    GameObject bullet;

    [SerializeField]
    GameObject canonExit;

    [SerializeField]
    float reloadTime = 1f;

    [SerializeField]
    float bulletForce = 5f;

    [SerializeField]
    float velocity = 0.5f;

    Vector3 direction = new Vector3();

    float timer = 0;

    private void Start()
    {
        direction = new Vector3(velocity,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 targetDest = transform.position + (direction * Time.deltaTime);
            if (targetDest.x < end1.transform.position.x)
                transform.position = targetDest;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 targetDest = transform.position - (direction * Time.deltaTime);
            if (targetDest.x > end2.transform.position.x)
                transform.position = targetDest;
        }
        if (Input.GetKey(KeyCode.Space) && timer <= 0)
        {
            GameObject obj = Instantiate(bullet, canonExit.transform.position, Quaternion.identity);
            obj.GetComponent<Rigidbody>().AddForce(new Vector3(0, bulletForce, -bulletForce), ForceMode.Impulse);
            timer = reloadTime;
        }
        if (timer > 0)
            timer -= Time.deltaTime;
    }
}
