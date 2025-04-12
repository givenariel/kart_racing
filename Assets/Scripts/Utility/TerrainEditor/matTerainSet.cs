using UnityEngine;

[ExecuteInEditMode]
public class AssignCustomTerrainMaterial : MonoBehaviour
{
    public Terrain targetTerrain;
    public Material customMaterial;

    void OnValidate()
    {
        if (targetTerrain == null)
        {
            targetTerrain = GetComponent<Terrain>();
        }

        if (targetTerrain != null && customMaterial != null)
        {
            targetTerrain.materialTemplate = customMaterial;
            Debug.Log($"Custom material '{customMaterial.name}' assigned to terrain.");
        }
        else
        {
            Debug.LogWarning("Terrain or Custom Material not set.");
        }
    }
}
