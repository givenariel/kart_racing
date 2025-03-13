using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BoostCamEffect : MonoBehaviour
{
    public Material boostMaterial; // Masukkan Material dengan Fullscreen Shader
    public  bool isBoosting = false;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Debug.Log("test");
        if (isBoosting)
        {
            Graphics.Blit(src, dest, boostMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    public void ActivateBoost(bool active)
    {
        isBoosting = active;
    }
}
