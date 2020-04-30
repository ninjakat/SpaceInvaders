using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float m_Speed = 1f;

    private void Start()
    {
        // Destroy automatically after a certain distance
        const float maxTravelDistance = 30f;
        Destroy(gameObject, maxTravelDistance / m_Speed);

        GetComponent<Rigidbody>().velocity = m_Speed * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject);
    }
}
