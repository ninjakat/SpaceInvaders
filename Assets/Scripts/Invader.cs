using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Invader : MonoBehaviour
{
    [SerializeField] GameObject m_BulletPrefab = null;

    public event UnityAction<GameObject> onDeath;
    public event UnityAction<GameObject> onReachBottom;

    float m_Time;
    Vector3 m_StartPosition;

    private void Start()
    {
        m_Time = 0f;
        m_StartPosition = transform.position;
    }

    private void Update()
    {
        m_Time += Time.deltaTime * GameManager.invaderSpeed;
        transform.position = GetPositionAtTime(m_Time);

        if (transform.position.z <= 0f)
        {
            onReachBottom?.Invoke(gameObject);
        }
    }

    public void Shoot()
    {
        GameObject bullet = GameObject.Instantiate(m_BulletPrefab);
        bullet.transform.position = transform.position;
        bullet.transform.forward = Vector3.back;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // It can happen that we get hit by 2 bullets on the same frame,
        // so we check if we haven't been killed yet
        if (enabled)
        {
            enabled = false;

            // De-activate root physics
            GetComponent<Collider>().enabled = false;

            // Apply some force
            foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>())
            {
                body.isKinematic = false;
                // Accentuate the angle and randomize it
                float angleOffset = Vector3.SignedAngle(Vector3.forward, collision.relativeVelocity, Vector3.up);
                angleOffset *= 10f + Random.Range(-10f, 10f);
                Vector3 force = Quaternion.Euler(0f, angleOffset, 0f) * collision.relativeVelocity;
                force = Random.Range(1f, 20f) * force;
                Vector3 torque = Random.Range(-10f, 10f) * Random.insideUnitCircle;
                body.AddForce(force);
                body.AddTorque(torque);
            }

            // Clean after some time
            Destroy(gameObject, 2f);
            onDeath?.Invoke(gameObject);
        }
    }

    private Vector3 GetPositionAtTime(float t)
    {
        // Funky maths to have some less predictible sinusoidal patterns
        float dz = -t;
        float dx = Mathf.Sin(t / GameManager.invaderPeriod);
        dx = Mathf.Sign(dx) * Mathf.Pow(Mathf.Abs(dx), GameManager.invaderExponent);
        dx *= GameManager.invaderAmplitude;
        return m_StartPosition + new Vector3(dx, 0, dz);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the path the invader is going to take
        const int numSteps = 100;
        const float length = 10;

        for (int i = 0; i < numSteps; ++i)
        {
            float t0 = length * i / numSteps;
            float t1 = length * (i + 1) / numSteps;

            Vector3 p0 = GetPositionAtTime(t0);
            Vector3 p1 = GetPositionAtTime(t1);

            if (!Application.isPlaying)
            {
                p0 += transform.position;
                p1 += transform.position;
            }

            Gizmos.color = i % 2 == 0 ? Color.red : Color.green;
            Gizmos.DrawLine(p0, p1);
        }
    }
}
