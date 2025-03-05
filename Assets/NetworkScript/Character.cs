using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject introPrefabs;
    [SerializeField] private NetworkObject gameplayPrefab;
   


    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public GameObject IntroPrefabs => introPrefabs;
    public NetworkObject GameplayPrefab => gameplayPrefab;
}
