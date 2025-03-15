using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    public float shieldDuration = 5f;
    public GameObject shieldVFX;
    private bool isShieldActive = false;

    public bool IsShieldActive => isShieldActive; // Properti lebih ringkas

    public void ActivateShield()
    {
        if (isShieldActive) return; // Jika sudah aktif, tidak perlu diaktifkan lagi

        isShieldActive = true;
        Debug.Log("Shield Aktif!");

        if (shieldVFX != null)
        {
            shieldVFX.SetActive(true);
            ParticleSystem ps = shieldVFX.GetComponent<ParticleSystem>();
            ps?.Play(); // Memulai efek jika ada ParticleSystem
        }

        StartCoroutine(DisableShieldAfterTime());
    }

    private IEnumerator DisableShieldAfterTime()
    {
        yield return new WaitForSeconds(shieldDuration - 1f); // Kurangi 1 detik sebelum habis

        if (shieldVFX != null)
        {
            ParticleSystem ps = shieldVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = new Color(1, 1, 1, 0.3f); // Atur transparansi ke 30%
            }
        }

        yield return new WaitForSeconds(1f); // Tambah 1 detik delay untuk transparansi

        if (shieldVFX != null)
        {
            ParticleSystem ps = shieldVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                yield return new WaitUntil(() => ps.particleCount == 0);
            }

            shieldVFX.SetActive(false);
        }

        isShieldActive = false;
        Debug.Log("Shield Habis!");
    }

}
