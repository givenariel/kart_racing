using UnityEngine;

[ExecuteInEditMode]
public class NOSRaymarchEffect : MonoBehaviour
{
    public Shader nosShader;
    private Material nosMaterial;

    public Texture2D noiseTexture;
    public float timeSpeed = 1.0f;

    void Start()
    {
        if (!nosShader)
        {
            Debug.LogError("Shader belum diassign!");
            enabled = false;
            return;
        }

        nosMaterial = new Material(nosShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (nosMaterial)
        {
            Debug.Log("Shader NOSRaymarch sedang dijalankan!");
            nosMaterial.SetTexture("_NoiseTex", noiseTexture);
            nosMaterial.SetFloat("_TimeSpeed", timeSpeed);
            Graphics.Blit(src, dest, nosMaterial);
        }
        else
        {
            Debug.LogError("nosMaterial NULL! Pastikan shader sudah diassign.");
            Graphics.Blit(src, dest);
        }
    }
    void Update()
    {
        if (nosMaterial != null)
        {
            Graphics.Blit(null, (RenderTexture)null, nosMaterial);
            Debug.Log("Shader NOS berjalan dengan Graphics.Blit!");
        }
    }

}
