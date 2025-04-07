using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class Missile : NetworkBehaviour
{
    public float speed = 30f; // Kecepatan misil
    public float rotateSpeed = 500f; // Kecepatan rotasi misil ke target
    public float stunDuration = 2f; // Lama stun akibat misil
    public GameObject explosionEffect;
    public GameObject targetIndicatorPrefab; // UI Target (Prefab)

    private Transform target;
    private Rigidbody rb;
    private GameObject targetIndicator; // Menyimpan indikator yang muncul
    public Transform ownerTransform;

    public List<Transform> players = new List<Transform>();

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
            if (IsOwner)
            {
                RequestDestroyServerRpc();
            } // Hancurkan jika tidak ada target setelah 3 detik
            return;
        }

        // Arahkan misil ke target
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.RotateTowards(transform.localRotation, lookRotation, rotateSpeed * Time.fixedDeltaTime));

        // Gerakkan misil maju ke arah target
        rb.linearVelocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Jika menyentuh target
        {
            if (ownerTransform != null && other.transform != ownerTransform)
            {
                if (IsOwner)
                {
                    Shield kartShield = other.GetComponent<Shield>();
                    if (kartShield != null && kartShield.IsShieldActive)
                    {
                        Debug.Log("Misil mengenai pemain, tetapi shield aktif!");
                        Explode();
                        return; // Tidak memberikan stun jika shield aktif
                    }
                    Debug.Log("pp");
                }
                

                if (IsServer) // Pastikan stun hanya dilakukan oleh server
                {
                    CarHandler playerKart = other.GetComponent<CarHandler>();
                    if (playerKart != null)
                    {
                        playerKart.Stun(stunDuration, "Missile");
                        ApplyStunClientRpc(other.GetComponent<NetworkObject>());
                    }

                    Explode();
                }
                else
                {
                    // Jika client mendeteksi tabrakan, kirim permintaan ke server
                    RequestStunServerRpc(other.GetComponent<NetworkObject>());
                }
            }
        }
    }

    // Client meminta server untuk memberikan stun
    [ServerRpc]
    void RequestStunServerRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            CarHandler playerKart = targetObject.GetComponent<CarHandler>();
            if (playerKart != null)
            {
                playerKart.Stun(stunDuration, "Missile");
                ApplyStunClientRpc(target);
            }
        }

        Explode();
    }

    // Memberikan efek stun ke semua client (misalnya untuk efek visual)
    [ClientRpc]
    void ApplyStunClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            CarHandler playerKart = targetObject.GetComponent<CarHandler>();
            if (playerKart != null)
            {
                playerKart.Stun(stunDuration, "Missile");
                // Jika ada efek visual khusus, tambahkan di sini
                Debug.Log("Efek stun diterapkan di client.");
            }
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
            if (IsOwner)
            {
                RequestDestroyServerRpc();
            }
        }

        if (IsOwner)
        {
            RequestDestroyServerRpc();
        }
    }

    private void FindTarget()
    {
        if (!IsOwner) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        players.Remove(ownerTransform);
        Transform targetMisille = players[Random.Range(0, players.Count - 1)];
        target = targetMisille.transform;
        if (targetMisille != null)
        {
            //target = player.transform;

            if (targetIndicatorPrefab != null)
            {
                targetIndicator = Instantiate(targetIndicatorPrefab, target.position + new Vector3(0, 2f, 0), Quaternion.identity);
                targetIndicator.transform.SetParent(targetMisille);
            }
        }
        else
        {
            if (IsOwner)
            {
                RequestDestroyServerRpc();
            }
        }
    }

    [ServerRpc]
    void RequestDestroyServerRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}
