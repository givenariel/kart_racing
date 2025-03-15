using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ItemType
{
    None,
    Boost,
    Shield,
    Trap,
    Slow,
    Misille
}

public class ItemBox : MonoBehaviour
{
    [SerializeField] private List<ItemType> availableItems = new List<ItemType> { ItemType.Boost, ItemType.Shield, ItemType.Slow, ItemType.Trap, ItemType.Misille };
    [SerializeField] private float respawnTime = 3f;

    private bool isAvailable = true;
    private Renderer rend;
    private Collider col;

    void Start()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAvailable && other.CompareTag("Player"))
        {
            ItemType randomItem = GetRandomItem();

            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.CollectItem(randomItem);
            }

            StartCoroutine(RespawnItemBox());
        }
    }


    private ItemType GetRandomItem()
    {
        if (availableItems == null || availableItems.Count == 0)
        {
            availableItems.Add(ItemType.Boost); // Tambahkan default item agar tidak kosong
        }

        ItemType selectedItem = availableItems[Random.Range(0, availableItems.Count)];
        return selectedItem;
    }



    private IEnumerator RespawnItemBox()
    {
        isAvailable = false;
        rend.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        rend.enabled = true;
        col.enabled = true;
        isAvailable = true;
    }
}
    