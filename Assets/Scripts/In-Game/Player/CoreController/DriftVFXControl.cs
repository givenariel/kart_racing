using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class DriftVFXControl : MonoBehaviour
{
    [SerializeField] VisualEffect[] driftVFX = new VisualEffect[2];

    private void Start()
    {
        //StartCoroutine(SetDecal());
    }

    public void OnPlayDriftVFX()
    {
        for (int i = 0; i < driftVFX.Length; i++)
        {
            driftVFX[i].enabled = true;
            StartCoroutine(StartDecal(driftVFX[i]));
        }
    }

    public void OnStopDriftVFX()
    {
        for (int i = 0; i < driftVFX.Length; i++)
        {
            driftVFX[i].enabled = false;
            driftVFX[i].Stop();
        }
    }

    private IEnumerator StartDecal(VisualEffect driftFX)
    {
        driftFX.Stop();
        yield return new WaitForSeconds(0.25f);
        driftFX.Play();
    }

    private IEnumerator SetDecal()
    {

        OnPlayDriftVFX();
        yield return new WaitForSeconds(1f);
        OnStopDriftVFX();
        //yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < driftVFX.Length; i++)
        {
            driftVFX[i].enabled = false;
        }
    }
}
