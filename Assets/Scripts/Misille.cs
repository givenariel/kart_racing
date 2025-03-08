using UnityEngine;

public class Misille : MonoBehaviour
{
    public float speed = 30f; // Kecepatan misil
    public float rotateSpeed = 500f; // Kecepatan rotasi misil ke target
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
            Destroy(gameObject, 3f); // Hancurkan jika tidak ada target
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

            // Buat UI indikator target di atas kepala pemain
            if (targetIndicatorPrefab != null)
            {
                targetIndicator = Instantiate(targetIndicatorPrefab, target.position + new Vector3(0, 2f, 0), Quaternion.identity);
                targetIndicator.transform.SetParent(target); // Indikator ikut pemain
            }
        }
    }
}
