using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    [SerializeField] private float disableDuration = 0.5f;
    public GameObject stunVFX;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            KartController playerKart = other.GetComponent<KartController>();
            Shield kartShield = other.GetComponent<Shield>();

            // Jika pemain memiliki shield, jebakan tidak berpengaruh
            if (kartShield != null && kartShield.IsShieldActive)
            {
                return;
            }

            if (playerKart != null)
            {
                playerKart.Stun(disableDuration);

                if (stunVFX != null)
                {
                    Vector3 spawnPosition = other.transform.position + Vector3.up * 1.5f; 
                    GameObject effect = Instantiate(stunVFX, spawnPosition, Quaternion.identity);
                    effect.transform.SetParent(other.transform);
                    Destroy(effect, disableDuration);
                }
            }

            Destroy(gameObject);
        }
    }
}
