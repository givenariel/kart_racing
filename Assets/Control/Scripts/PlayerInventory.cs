using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<GameObject> itemImages; // Tambahkan dari Inspector
    private Dictionary<ItemType, GameObject> itemUIMap;
    private ItemType currentItem = ItemType.None;
    private kartController kart;

    void Start()
    {
        kart = GetComponent<kartController>();
        itemUIMap = new Dictionary<ItemType, GameObject>();

        ItemType[] itemTypes = (ItemType[])System.Enum.GetValues(typeof(ItemType));

        int index = 0; // Untuk itemImages
        foreach (ItemType item in itemTypes)
        {
            if (item == ItemType.None) continue; // Jangan masukkan None ke dictionary

            if (index < itemImages.Count)
            {
                itemUIMap[item] = itemImages[index];
                index++;
            }
            else
            {
                Debug.LogWarning("Jumlah itemImages lebih sedikit dari jumlah ItemType yang tersedia.");
                break;
            }
        }

        UpdateUI();
    }


    public void CollectItem(ItemType item)
    {
        currentItem = item;
        Debug.Log("Item diperoleh: " + item);
        UpdateUI();
    }

    public void UseItem()
    {
        if (currentItem != ItemType.None)
        {
            Debug.Log("Menggunakan item: " + currentItem);

            if (currentItem == ItemType.Boost && kart != null)
            {
                kart.AddImpulseBoost();
            }
            else if (currentItem == ItemType.Skill)
            {
                Debug.Log("Skill digunakan, tetapi belum memiliki efek.");
            }

            currentItem = ItemType.None;
            UpdateUI();
        }
        else
        {
            Debug.Log("Tidak ada item untuk digunakan!");
        }
    }

    private void UpdateUI()
    {
        Debug.Log("Memperbarui UI, item saat ini: " + currentItem);

        foreach (var img in itemUIMap.Values)
        {
            img.SetActive(false);
        }

        if (currentItem != ItemType.None && itemUIMap.ContainsKey(currentItem))
        {
            Debug.Log("Menampilkan UI untuk: " + currentItem);
            itemUIMap[currentItem].SetActive(true);
        }
    }

}
