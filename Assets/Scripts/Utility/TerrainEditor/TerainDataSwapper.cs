using UnityEngine;

[ExecuteInEditMode]
public class TerrainDataReplacer : MonoBehaviour
{
    public TerrainData newTerrainData;

    void Update()
    {
        if (!Application.isPlaying && newTerrainData != null)
        {
            Terrain terrain = GetComponent<Terrain>();
            if (terrain != null && terrain.terrainData != newTerrainData)
            {
                terrain.terrainData = newTerrainData;
                Debug.Log("TerrainData diganti permanen di Editor.");
            }
        }
    }
}
