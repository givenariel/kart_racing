using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Scriptable Objects/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private Character[] characters = new Character[0];

    public Character[] GetAllCharacter() => characters;

    public Character GetCharacterById(int id)
    { 
        foreach (var character in characters)
        {
            if (character.Id == id)
            {
                return character;
            }
        }

        return null;
    }

    public bool IsvalidCharacterId(int id)
    {
        return characters.Any(x => x.Id == id);
    }

}
