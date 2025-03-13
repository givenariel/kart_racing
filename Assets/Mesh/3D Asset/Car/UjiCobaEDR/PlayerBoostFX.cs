using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PlayerBoostFX : MonoBehaviour
{
    public ScriptableRendererFeature boostFeature;
    public Camera playerCamera;

    void Start()
    {
        boostFeature.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Contoh: Tekan SPACE untuk Boost
        {
            boostFeature.SetActive(true);
        }
    }
}
