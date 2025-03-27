using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Android.Gradle.Manifest;
using Unity.Netcode;

public class PlayerInventory : NetworkBehaviour
{
    [SerializeField] private List<GameObject> itemImages;
    public GameObject trapPrefab;
    public GameObject slowTrapPrefab;
    public GameObject missilePrefab;
    public Transform missileSpawn;
    private Dictionary<ItemType, GameObject> itemUIMap;
    [SerializeField] private ItemType currentItem = ItemType.None;
    private CarHandler KartController;
    private Shield shield;
    public Transform trapSpawn;
    public Transform throwSpawn;
    public float throwForce = 10f;
    public CarIdManager carIdManager;
    public NetworkVariable<NetworkObjectReference> carIdManagerRef = new NetworkVariable<NetworkObjectReference>();
    public int dirMove;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            // Coba ambil referensi jika sudah di-set
            if (carIdManagerRef.Value.TryGet(out NetworkObject netObj) && netObj != null)
            {
                carIdManager = netObj.GetComponent<CarIdManager>();
                Debug.Log($"[Client] carIdManager berhasil diperoleh: {carIdManager}");
            }
            else
            {
                Debug.LogWarning("[Client] carIdManagerRef belum valid atau belum di-set.");
            }
        }
    }

    // Server mengatur referensi
    [ServerRpc (RequireOwnership = false)]
    public void SetCarIdManagerRefServerRpc(NetworkObjectReference managerRef)
    {
        carIdManagerRef.Value = managerRef;
    }

    void Start()
    {
        KartController = GetComponent<CarHandler>();
        shield = GetComponent<Shield>();
        
        

        itemUIMap = new Dictionary<ItemType, GameObject>();
        ItemType[] itemTypes = (ItemType[])System.Enum.GetValues(typeof(ItemType));

        int index = 0;
        foreach (ItemType item in itemTypes)
        {
            if (item == ItemType.None) continue;

            if (index < itemImages.Count)
            {
                itemUIMap[item] = itemImages[index];
                index++;
            }
            else
            {
                break;
            }
        }

        UpdateUI();
    }

    public void CollectItem(ItemType item)
    {
        currentItem = item;
        UpdateUI();
    }

    public void UseItem()
    {
        Debug.Log(currentItem.ToString());
        if (currentItem != ItemType.None)
        {
            if (currentItem == ItemType.Boost && KartController != null)
            {
                KartController.AddImpulseBoost();
            }
            else if (currentItem == ItemType.Shield)
            {
                ActivateShield();
            }
            else if (currentItem == ItemType.Trap)
            {
                PlaceTrap(); // Menaruh jebakan di belakang mobil
            }
            else if (currentItem == ItemType.Slow)
            {
                ThrowSlowTrap(); // Melempar jebakan slow seperti granat
            }
            else if (currentItem == ItemType.Misille)
            {
                FireMissile();
            }

            currentItem = ItemType.None;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        foreach (var img in itemUIMap.Values)
        {
            img.SetActive(false);
        }

        if (currentItem != ItemType.None && itemUIMap.ContainsKey(currentItem))
        {
            itemUIMap[currentItem].SetActive(true);
        }
    }

    private void PlaceTrap()
    {
        if (trapPrefab != null && trapSpawn != null)
        {
            GameObject trap = Instantiate(trapPrefab, trapSpawn.position, trapSpawn.rotation);
            trap.GetComponent<Trap>().ownerTransform = transform;
            trap.transform.SetParent(null);
        }
    }

    private void ThrowSlowTrap()
    {
        if (slowTrapPrefab != null && throwSpawn != null)
        {
            GameObject slowTrap = Instantiate(slowTrapPrefab, throwSpawn.position, throwSpawn.rotation);
            ThrowableSlowTrap throwItemHandle = slowTrap.GetComponent<ThrowableSlowTrap>();
            throwItemHandle.ownerTransform = transform;
            Rigidbody slowTrapRb = slowTrap.GetComponent<Rigidbody>();

            if (slowTrapRb != null)
            {
                slowTrapRb.useGravity = true;
                slowTrapRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Arah lemparan dengan parabola
                Vector3 throwDirection = transform.forward *  dirMove + (transform.right * Random.Range(0.1f,-0.1f));

                // Menambahkan momentum pemain agar lemparan tidak tertinggal di belakang
                Rigidbody playerRb = GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    //throwDirection += playerRb.linearVelocity * 10f; // Bisa disesuaikan agar efeknya lebih natural
                }
                //Debug.Log(throwDirection);
                // Gunakan AddForce dengan mode Impulse untuk efek lemparan instan
                slowTrapRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
                throwDirection.y = Mathf.Tan(25 * Mathf.Deg2Rad); // Menyesuaikan sudut vertikal
                throwDirection.Normalize();

                Vector3 velocity = throwDirection * playerRb.linearVelocity.magnitude;
                StartCoroutine(ParabolicMotion(velocity, slowTrapRb, playerRb, throwItemHandle, dirMove));
            }
        }
    }

    private IEnumerator ParabolicMotion(Vector3 initialVelocity, Rigidbody rb, Rigidbody playerRb, ThrowableSlowTrap  itemHandle, int inputP = 1)
    {
        
        rb.linearVelocity = initialVelocity;
        Vector3 throwDirection = (transform.forward * 0.8f * inputP) + (transform.up * 0.01f) + (transform.right * Random.Range(0.05f, -0.05f));
        float elapsed = 0f;
        rb.AddForce(throwDirection * 15 * (15 + (playerRb.linearVelocity.magnitude * 0.05f)), ForceMode.Acceleration);
        while (elapsed < 0.3f)
        {
            if (itemHandle.hasSpawnedTrap)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            rb.AddForce(throwDirection + (15 + (playerRb.linearVelocity.magnitude * 0.05f)) * (transform.forward * inputP)* 0.5f, ForceMode.Acceleration);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < 0.8f)
        {
            if (itemHandle.hasSpawnedTrap)
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            rb.AddForce( (35 + (playerRb.linearVelocity.magnitude * 0.1f)) * -transform.up * 1f , ForceMode.Acceleration);
            yield return null;
        }
    }

    private void ActivateShield()
    {
        if (shield != null)
        {
            shield.ActivateShield();
        }
    }

    private void FireMissile()
    {
        if (IsOwner) // Pastikan hanya pemilik kendaraan yang bisa menembak
        {
            FireMissileServerRpc();
        }
    }

    [ServerRpc]
    private void FireMissileServerRpc()
    {
        if (missilePrefab != null && missileSpawn != null)
        {
            GameObject missile = Instantiate(missilePrefab, missileSpawn.position, missileSpawn.rotation);
            NetworkObject missileNetworkObject = missile.GetComponent<NetworkObject>();

            if (missileNetworkObject != null)
            {
                missileNetworkObject.Spawn();
            }

            missile.GetComponent<Missile>().ownerTransform = transform;
            missile.GetComponent<Missile>().players = new List<Transform>(carIdManager.players);

            Rigidbody missileRb = missile.GetComponent<Rigidbody>();

            if (missileRb != null)
            {
                missileRb.linearVelocity = transform.forward * 20f; // Kecepatan awal misil
            }
        }
    }
}
