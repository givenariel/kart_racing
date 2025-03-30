using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class Missile : NetworkBehaviour
{
    public float speed = 30f;
    public float rotateSpeed = 500f;
    public float stunDuration = 2f;
    public float detectionRadius = 10f; // Radius deteksi musuh
    public GameObject explosionEffect;
    public GameObject targetIndicatorPrefab;

    private Rigidbody rb;
    private Transform target;
    private GameObject targetIndicator;
    public RouteHandler routeHandler;
    private List<Vector3> bezierPath = new List<Vector3>(); // Jalur Bezier
    private int currentBezierIndex = 0;
    private bool isFollowingBezier = true; // Misil masih mengikuti jalur Bezier?

    public Transform ownerTransform;
    public List<Transform> players = new List<Transform>();

    private int lap = 0;
    public bool[] checkpointsPassed;

    [SerializeField] private float sideOffset = 1.5f;   // 🔹 Amplitudo zigzag horizontal (X)
    [SerializeField] private float verticalOffset = 1.0f; // 🔹 Amplitudo zigzag vertikal (Y)
    [SerializeField] private float waveFrequency = 2.0f;   // 🔹 Frekuensi osilasi
    [SerializeField] private float noiseScale = 0.5f; // 🔹 Variasi random dengan Perlin Noise

    void Start()
    {
        if (IsOwner)
        {
            detectionRadius *= 10;
            var (progress, newLap, closestPoint, updatedCheckpoints, iClosest) = routeHandler.GetTrackProgress(transform.position, lap, checkpointsPassed);
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            //routeHandler = FindObjectOfType<RouteHandler>();
            currentBezierIndex = iClosest;

            if (routeHandler != null)
            {
                bezierPath = routeHandler.GetTrackPoints(); // Ambil jalur Bezier
            }
        }
        
    }

    void FixedUpdate()
    {
        if (isFollowingBezier)
        {
            FollowBezierPath();
        }
        else
        {
            HomeToTarget();
        }

        FindTargetInRadius(); // Tetap mencari target selama bergerak
    }

    void FollowBezierPath()
    {
        if (currentBezierIndex < bezierPath.Count - 2)
        {
            MoveTowards(bezierPath[currentBezierIndex] -Vector3.up * 10, bezierPath[currentBezierIndex + 2] - Vector3.up * 10);

            if (Vector3.Distance(transform.position, bezierPath[currentBezierIndex] - Vector3.up * 10) < 10f)
            {
                currentBezierIndex++;
            }
        }
        else
        {
            currentBezierIndex = 0;
        }
    }

    void HomeToTarget()
    {
        if (target != null)
        {
            MoveTowards(target.position + Vector3.up, (target.position + Vector3.up - transform.position) +  target.position + Vector3.up);
        }
    }

    void MoveTowards(Vector3 destination, Vector3 nextBezierPoint)
    {
        // 🔹 Hitung arah utama berdasarkan jalur Bezier yang sedang ditempuh
        Vector3 pathDirection = (nextBezierPoint - destination).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(pathDirection);

        // 🔹 Terapkan rotasi agar tetap smooth
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));

        // 🔹 Hitung basis sumbu kanan & atas berdasarkan arah utama
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        // 🔹 Waktu berjalan untuk osilasi
        float timeOffset = Time.timeSinceLevelLoad * waveFrequency;

        // 🔹 Gelombang zigzag menggunakan Sin/Cos
        float waveOffsetX = Mathf.Sin(timeOffset) * sideOffset;
        float waveOffsetY = Mathf.Cos(timeOffset * 1.5f) * verticalOffset;

        // 🔹 Terapkan offset langsung ke posisi misil (zigzag)
        Vector3 waveOffset = (right * waveOffsetX) + (up * waveOffsetY);
        Vector3 newPosition = destination + waveOffset;

        // 🔹 Gunakan velocity agar tetap smooth dan tidak terlalu berubah arah tiba-tiba
        rb.linearVelocity = (newPosition - transform.position).normalized * speed;
    }


    void FindTargetInRadius()
    {
        if (!IsOwner) return;
        if (target != null) return; // Jangan cari target lagi jika sudah ada

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player") && hit.transform != ownerTransform)
            {
                target = hit.transform;
                isFollowingBezier = false; // Beralih ke mode homing
                Debug.Log("targetdeteksi");
                return;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.transform != ownerTransform)
        {
            if (IsServer)
            {
                CarHandler playerKart = other.GetComponent<CarHandler>();
                if (playerKart != null)
                {
                    playerKart.Stun(stunDuration, "Missile");
                    ApplyStunClientRpc(other.GetComponent<NetworkObject>());
                }
            }

            Explode();
        }
    }

    [ClientRpc]
    void ApplyStunClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            CarHandler playerKart = targetObject.GetComponent<CarHandler>();
            if (playerKart != null)
            {
                playerKart.Stun(stunDuration, "Missile");
            }
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (IsOwner)
        {
            RequestDestroyServerRpc();
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
