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
            CarController playerKart = other.GetComponent<CarController>();
            Shield kartShield = other.GetComponent<Shield>();

            if (kartShield != null && kartShield.IsShieldActive)
            {
                return;
            }

            if (playerKart != null)
            {
                playerKart.Stun(disableDuration, "Trap");

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