using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class BoostFXControl : MonoBehaviour
{
    [SerializeField] private VisualEffect[] flameFX = new VisualEffect[2];
    [SerializeField] ScriptableRendererFeature boostFullScreen;

    private void Start()
    {
        boostFullScreen.SetActive(false);
    }
    public void OnPlayBoostFX()
    {
        for (int i = 0; i < flameFX.Length; i++)
        {
            flameFX[i].gameObject.SetActive(true);
            flameFX[i].Play();
            
        }
        boostFullScreen.SetActive(true);
    }

    public void OnStopBoostFX()
    {
        for (int i = 0; i < flameFX.Length; i++)
        {
            flameFX[i].gameObject.SetActive(false);
            flameFX[i].Stop();
        }
        boostFullScreen.SetActive(false);
    }


}
