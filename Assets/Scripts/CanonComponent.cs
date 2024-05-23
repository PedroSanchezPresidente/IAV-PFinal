using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
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
    GameObject target;

    [SerializeField]
    GameObject errorText;

    [SerializeField]
    float reloadTime = 1f;

    [SerializeField]
    float bulletForce = 5f;

    [SerializeField]
    float velocity = 0.5f;

    [SerializeField]
    bool paraboleIfPosible = false;

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
            Vector3 imp = calculateFiringSolution(target.transform.position);
            if (imp != Vector3.zero)
            {
                errorText.active = false;
                GameObject obj = Instantiate(bullet, canonExit.transform.position, Quaternion.identity);
                transform.forward = -imp;
                obj.GetComponent<Rigidbody>().AddForce(imp * bulletForce, ForceMode.Impulse);
            }
            else
                errorText.active = true;
            timer = reloadTime;
        }
        if (timer > 0)
            timer -= Time.deltaTime;
    }

    Vector3 calculateFiringSolution(Vector3 end)
    {
        Vector3 delta = end - canonExit.transform.position;
        ;
        float ttt;

        float a = Physics.gravity.sqrMagnitude;
        float b = -4 * (Vector3.Dot(Physics.gravity, delta) + bulletForce * bulletForce);
        float c = 4 * delta.sqrMagnitude;

        float b2minus4ac = b * b - 4 * a * c;
        if(b2minus4ac < 0)
            return Vector3.zero;

        float time0 = Mathf.Sqrt(((-b + Mathf.Sqrt(b2minus4ac))) / (2 * a));
        float time1 = Mathf.Sqrt(((-b - Mathf.Sqrt(b2minus4ac))) / (2 * a));

        if (time0 < 0)
        {
            if (time1 < 0)
                return Vector3.zero;
            else
                ttt = time1;
        }
        else
        {
            if (time1 < 0)
                ttt = time0;
            else
            {
                if(paraboleIfPosible)
                    ttt = Mathf.Max(time0, time1);
                else
                    ttt = Mathf.Min(time0, time1);
            }
        }

        return (2 * delta - Physics.gravity * (ttt * ttt)) / (2 * bulletForce * ttt);
    }

    public void changeTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    public void changeBulletForce(float newForce)
    {
        bulletForce = newForce;
    }

    public void changeParaboleIfPosible(bool b)
    {
        paraboleIfPosible = b;
    }
}
