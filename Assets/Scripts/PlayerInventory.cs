using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<GameObject> itemImages;
    public GameObject trapPrefab;       // Objek jebakan biasa
    public GameObject slowTrapPrefab;   // Objek jebakan slow yang dilempar
    public GameObject missilePrefab; // Prefab misil
    public Transform missileSpawn;
    private Dictionary<ItemType, GameObject> itemUIMap;
    private ItemType currentItem = ItemType.None;
    private KartController KartController;
    private Shield shield;
    public Transform trapSpawn;
    public Transform throwSpawn;
    public float throwForce = 10f; // Kecepatan lemparan slow trap

    void Start()
    {
        KartController = GetComponent<KartController>();
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
            trap.transform.SetParent(null);
        }
    }

    private void ThrowSlowTrap()
    {
        if (slowTrapPrefab != null && throwSpawn != null)
        {
            GameObject slowTrap = Instantiate(slowTrapPrefab, throwSpawn.position, throwSpawn.rotation);
            Rigidbody slowTrapRb = slowTrap.GetComponent<Rigidbody>();

            if (slowTrapRb != null)
            {
                slowTrapRb.useGravity = true;
                slowTrapRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Arah lemparan dengan parabola
                Vector3 throwDirection = (transform.forward * 1.5f) + (transform.up * 2f);

                // Menambahkan momentum pemain agar lemparan tidak tertinggal di belakang
                Rigidbody playerRb = GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    throwDirection += playerRb.linearVelocity * 1f; // Bisa disesuaikan agar efeknya lebih natural
                }

                // Gunakan AddForce dengan mode Impulse untuk efek lemparan instan
                slowTrapRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
            }
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
        if (missilePrefab != null && missileSpawn != null)
        {
            GameObject missile = Instantiate(missilePrefab, missileSpawn.position, missileSpawn.rotation);
            Rigidbody missileRb = missile.GetComponent<Rigidbody>();

            if (missileRb != null)
            {
                missileRb.linearVelocity = transform.forward * 20f; // Kecepatan awal misil
            }
        }
    }
}
