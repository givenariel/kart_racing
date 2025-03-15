using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 30f; // Kecepatan misil
    public float rotateSpeed = 500f; // Kecepatan rotasi misil ke target
    public float stunDuration = 2f; // Lama stun akibat misil
    public GameObject explosionEffect;
    public GameObject targetIndicatorPrefab; // UI Target (Prefab)

    private Transform target;
    private Rigidbody rb;
    private GameObject targetIndicator; // Menyimpan indikator yang muncul

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        FindTarget();
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject, 3f); // Hancurkan jika tidak ada target setelah 3 detik
            return;
        }

        // Arahkan misil ke target
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed * Time.fixedDeltaTime));

        // Gerakkan misil maju ke arah target
        rb.linearVelocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Jika menyentuh target
        {
            KartController playerKart = other.GetComponent<KartController>();
            Shield kartShield = other.GetComponent<Shield>();

            if (kartShield != null && kartShield.IsShieldActive)
            {
                Debug.Log("Misil mengenai pemain, tetapi shield aktif!");
                return; // Tidak memberikan stun jika shield aktif
            }

            if (playerKart != null)
            {
                playerKart.Stun(stunDuration, "Missile"); // Stun dengan sumber "Missile"
            }

            Explode();
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Debug.Log("Misil meledak!");

        // Hapus UI target saat misil meledak
        if (targetIndicator != null)
        {
            Destroy(targetIndicator);
        }

        Destroy(gameObject);
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;

            if (targetIndicatorPrefab != null)
            {
                targetIndicator = Instantiate(targetIndicatorPrefab, target.position + new Vector3(0, 2f, 0), Quaternion.identity);
                targetIndicator.transform.SetParent(target);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
