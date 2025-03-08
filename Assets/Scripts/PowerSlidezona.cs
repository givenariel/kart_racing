using UnityEngine;

public class PowerSlideZone : MonoBehaviour
{
    public float boost = 200f; // Bisa diatur dari Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Pastikan kart punya tag "Player"
        {
            KartController kart = other.GetComponent<KartController>();

            if (kart != null)
            {
                Rigidbody kartRb = kart.GetComponent<Rigidbody>();

                if (kartRb != null)
                {
                    // Arah zona ke sumbu X (horizontal)
                    Vector3 zoneDirection = transform.right.normalized;
                    Vector3 kartDirection = kartRb.linearVelocity.normalized; // Perbaikan di sini

                    float alignment = Vector3.Dot(kartDirection, zoneDirection);

                    if (alignment > 0) // Jika kart bergerak searah dengan zona
                    {
                        kart.TriggerBoostZone(boost, zoneDirection); // Memanggil fungsi Boost baru
                    }
                }
            }
        }
    }
}
