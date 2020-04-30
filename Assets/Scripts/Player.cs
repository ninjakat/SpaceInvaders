using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Player : MonoBehaviour
{
    [SerializeField] GameObject m_BulletPrefab = null;
    [SerializeField] float m_Speed = 1f;
    [SerializeField] Transform m_ShootingPoint = null;
    [SerializeField] float m_xMax = 10f;
    [SerializeField] GameObject[] m_Bullets = null;
    [SerializeField] float m_ReloadTime = 1f;
    [SerializeField] float m_Fire1Dispersion = 1f;
    [SerializeField] float m_Fire2Dispersion = 10f;

    int m_NumBullets;

    public event UnityAction<GameObject> onDeath;

    private void Start()
    {
        m_NumBullets = m_Bullets.Length;
    }
    private void Update()
    {
        if (GameManager.playerCanShoot)
        {
            HandleFire();
        }
        if (GameManager.playerCanMove)
        {
            HandleMove();
        }
    }

    private void HandleFire()
    {
        if (m_NumBullets == 0)
        {
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            GameObject bullet = GameObject.Instantiate(m_BulletPrefab);
            bullet.transform.position = m_ShootingPoint.position;
            float angle = Random.Range(-m_Fire1Dispersion, m_Fire1Dispersion);
            bullet.transform.forward = Quaternion.Euler(0, angle, 0) * m_ShootingPoint.forward;

            --m_NumBullets;
            if (m_NumBullets == 0)
            {
                StartCoroutine(Reload());
            }

            m_Bullets[m_NumBullets].transform.localScale = Vector3.zero;
        }

        if (Input.GetButtonDown("Fire2"))
        {

            // Shotgun mechanics
            for (int i = 0; i < m_NumBullets; ++i)
            {
                GameObject bullet = GameObject.Instantiate(m_BulletPrefab);
                float dispersion = m_Fire2Dispersion / m_Bullets.Length;
                float angle = Random.Range(-dispersion, dispersion);
                angle = Mathf.Sign(angle) * (Mathf.Abs(angle) + i * dispersion);
                Vector3 forward = Quaternion.Euler(0, angle, 0) * m_ShootingPoint.forward;
                Vector3 offset = forward * i * 0.2f;
                bullet.transform.forward = forward;
                bullet.transform.position = m_ShootingPoint.position + offset;

                m_Bullets[i].transform.localScale = Vector3.zero;
            }

            m_NumBullets = 0;
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        const float reloadPeriod = 0.1f;
        float t = 0;
        while (t < m_ReloadTime)
        {
            yield return new WaitForSeconds(reloadPeriod);

            t += reloadPeriod;
            foreach (GameObject bullet in m_Bullets)
            {
                bullet.transform.localScale = t / m_ReloadTime * Vector3.one;
            }
        }

        foreach (GameObject bullet in m_Bullets)
        {
            bullet.transform.localScale = Vector3.one;
        }
        m_NumBullets = m_Bullets.Length;
    }

    private void HandleMove()
    {
        float horizontal = Input.GetAxis("Horizontal");
        Vector3 pos = transform.position;
        pos.x += horizontal * m_Speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -m_xMax, m_xMax);
        transform.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        onDeath?.Invoke(gameObject);
        StartCoroutine(Death());
    }

    private IEnumerator Death()
    {
        enabled = false;

        Rigidbody body = GetComponentInChildren<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.AddTorque(0f, Random.Range(-10f, 10f), 0f);
        body.AddForce(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));

        Light light = GetComponentInChildren<Light>();
        light.color = Color.red;

        // Flicker until the game restarts
        bool on = true;
        while (true)
        {
            float waitTime = Random.Range(0f, 1f);
            waitTime = Mathf.Pow(waitTime, 2f);
            waitTime *= 0.5f;

            yield return new WaitForSeconds(waitTime);

            on = !on;
            light.color = on ? Color.red : Color.black;
        }
    }

    // The following function is unused now. It was an attempt to allow the player
    // to aim with the mouse, but proved less interesting for the gameplay
    private void HandleFireToCursor()
    {
        float m_AimAngle = 45f;
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100f, LayerMask.GetMask("Floor")))
            {
                Vector3 aimDirection = hitInfo.point + Vector3.up * m_ShootingPoint.position.y - m_ShootingPoint.position;
                float aimAngle = Vector3.SignedAngle(Vector3.forward, aimDirection, Vector3.up);
                aimAngle = Mathf.Clamp(aimAngle, -m_AimAngle, m_AimAngle);

                GameObject bullet = GameObject.Instantiate(m_BulletPrefab);
                bullet.transform.position = m_ShootingPoint.position;
                bullet.transform.forward = Quaternion.Euler(0, aimAngle, 0) * Vector3.forward;
            }
        }
    }
}
