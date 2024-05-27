using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

    [SerializeField]
    float drag = 0;

    Vector3 mDirection = new Vector3();

#if DEBUG
    List<Vector3> trajectory = new List<Vector3>();
#endif

    float timer = 0;

    private void Start()
    {
        mDirection = new Vector3(velocity,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 targetDest = transform.position + (mDirection * Time.deltaTime);
            if (targetDest.x < end1.transform.position.x)
                transform.position = targetDest;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 targetDest = transform.position - (mDirection * Time.deltaTime);
            if (targetDest.x > end2.transform.position.x)
                transform.position = targetDest;
        }
        if (Input.GetKey(KeyCode.Space) && timer <= 0)
        {
            Vector3 imp = refineTargeting(1.5f);
            if (imp != Vector3.zero)
            {
                errorText.active = false;
                GameObject obj = Instantiate(bullet, canonExit.transform.position, Quaternion.identity);
                transform.forward = -imp;
                obj.GetComponent<Rigidbody>().AddForce(imp * bulletForce, ForceMode.Impulse);
                obj.GetComponent<Rigidbody>().drag = drag;
            }
            else
                errorText.active = true;
            timer = reloadTime;
        }
        if (timer > 0)
            timer -= Time.deltaTime;
#if DEBUG
        foreach (Vector3 point in trajectory)
        {
            Debug.DrawLine(point, point + Vector3.up * 0.1f, Color.red, 10f);
        }
#endif
    }

    Vector3 calculateFiringSolution()
    {
        Vector3 delta = target.transform.position - canonExit.transform.position;
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

    Vector3 refineTargeting(float margin)
    {
        Vector3 direction = calculateFiringSolution();
        if (drag == 0)
            return direction;

        float distance = distanceToTarget(direction);

        if(-margin < distance && distance < margin)
            return direction;

        float angle = Mathf.Asin(direction.y/ direction.magnitude);

        float minBound;
        float maxBound;
        if (distance > 0)
        {
            maxBound = angle;
            minBound = Mathf.PI / 2;
            pair p = checkAngle(minBound);
            direction = p.direction;
            distance = p.distance;
            if (-margin < distance && distance < margin)
                return direction;
        }
        else
        {
            minBound = angle;
            maxBound = Mathf.PI / 4;
            pair p = checkAngle(maxBound);
            direction = p.direction;
            distance = p.distance;
            if (-margin < distance && distance < margin)
                return direction;

            if (distance < 0)
                return Vector3.zero;
        }

        distance = Mathf.Infinity;
        while (Mathf.Abs(distance) >= margin)
        {
            angle = (maxBound - minBound) / 2;
            angle += minBound;
            pair p = checkAngle(angle);
            direction = p.direction;
            distance = p.distance;

            if (distance < 0)
                minBound = angle;
            else
                maxBound = angle;
        }

        return direction;
    }

    struct pair
    {
        public Vector3 direction;
        public float distance;
    }

    pair checkAngle(float angle)
    {
        Vector3 deltaPos = target.transform.position - canonExit.transform.position;
        pair p;
        p.direction = convertToDirection(deltaPos, angle);
        p.distance = distanceToTarget(p.direction);
        return p;
    }

    float distanceToTarget(Vector3 dir)
    {
        float sol;
        float timeStep = 0.1f;

        //calcular distancia, sacar el tiempo en llegar a su plano zy, hallar distancia en y
        float distance = Mathf.Infinity;
        Vector3 position = canonExit.transform.position;
        Vector3 velocity = bulletForce * dir;
        Vector3 closestPos = new Vector3();
#if DEBUG
        trajectory.Clear();
#endif

        for (int i = 0; i < 100; i++)
        {
            // Update position and velocity

            velocity += Physics.gravity * timeStep;
            velocity /= 1 + drag * timeStep;
            position += velocity * timeStep;
#if DEBUG
            trajectory.Add(position);
#endif

            float actDistance = Vector3.Distance(target.transform.position, position);

            if (distance > actDistance){
                distance = actDistance;
                closestPos = position;
            }
        }

        Vector3 targetZX = target.transform.position;
        targetZX.y = 0;
        Vector3 positionZX = canonExit.transform.position;
        positionZX.y = 0;

        sol = Mathf.Sign(Vector3.Distance(closestPos, positionZX) - Vector3.Distance(targetZX, positionZX));
        sol *= distance;

        return sol;
    }

    Vector3 convertToDirection(Vector3 deltaPos, float angle)
    {
        Vector3 direction = deltaPos;
        direction.y = 0;
        direction.Normalize();

        direction *= Mathf.Cos(angle);
        direction.y = Mathf.Sin(angle);

        return direction;
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

    public void changeDrag(float f)
    {
        drag = f;
    }

    public void changeGravity(float g)
    {
        Physics.gravity =new Vector3(0,-g,0);
    }
}
