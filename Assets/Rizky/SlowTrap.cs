using UnityEngine;

public class ThrowableSlowTrap : MonoBehaviour
{
    public float slowDuration = 3f; // Durasi efek slow
    public float slowMultiplier = 0.5f; // Seberapa lambat pemain jadi
    public GameObject slowTrapPrefab; // Prefab jebakan setelah terkena tanah
    private bool hasSpawnedTrap = false; // Cegah jebakan spawn lebih dari sekali

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger dengan: " + other.name); // Debug untuk cek trigger

        if (other.CompareTag("Player")) // Jika kena pemain, beri efek slow
        {
            CarController playerKart = other.GetComponent<CarController>();
            if (playerKart != null)
            {
                Debug.Log("Mengenai player, memberikan efek slow!");
                playerKart.ApplySlowEffect(slowMultiplier, slowDuration);
                Destroy(gameObject); // Hancurkan bola setelah mengenai player
            }
        }
        else if (other.CompareTag("Ground") && !hasSpawnedTrap) // Jika kena tanah, ubah jadi jebakan
        {
            hasSpawnedTrap = true;
            Debug.Log("Mengenai ground, mengubah menjadi jebakan!");

            if (slowTrapPrefab == null)
            {
                Debug.LogError("slowTrapPrefab belum diassign di Inspector!");
                return;
            }

            GameObject trap = Instantiate(slowTrapPrefab, transform.position, Quaternion.identity);
            ThrowableSlowTrap trapScript = trap.GetComponent<ThrowableSlowTrap>();

            if (trapScript != null)
            {
                trapScript.ActivateTrap();
                Debug.Log("Jebakan berhasil dibuat!");
            }
            else
            {
                Debug.LogError("Komponen ThrowableSlowTrap tidak ditemukan pada prefab jebakan!");
            }

            Destroy(gameObject); // Hancurkan bola setelah berubah jadi jebakan
        }
    }

    public void ActivateTrap()
    {
        Debug.Log("Jebakan aktif!");
        gameObject.tag = "Trap"; // Ubah tag agar tidak dianggap throwable lagi
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; // Jadikan jebakan trigger
    }

    private void OnTriggerStay(Collider other)
    {
        if (CompareTag("Trap") && other.CompareTag("Player")) // Jika pemain melewati jebakan
        {
            CarController playerKart = other.GetComponent<CarController>();
            if (playerKart != null)
            {
                Debug.Log("Pemain menginjak jebakan, memberikan efek slow!");
                playerKart.ApplySlowEffect(slowMultiplier, slowDuration);
            }
        }
    }
}
