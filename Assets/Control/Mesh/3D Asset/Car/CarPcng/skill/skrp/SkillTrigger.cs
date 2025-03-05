using UnityEngine;
using System.Collections;

public class SkillTrigger : MonoBehaviour
{
    public Material skillMaterial;
    public float scrollSpeed = 0.1f; // Kecepatan scroll bertahap
    private float scrollProgress = 0.5f; // Mulai dari 0.5
    private bool isScrolling = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isScrolling)
        {
            StartCoroutine(ScrollTexture());
        }
    }

    IEnumerator ScrollTexture()
    {
        isScrolling = true;

        // Jika sudah di 1, langsung lompat ke 0.5 lalu mulai scroll ke 1 lagi
        if (scrollProgress >= 1.0f)
        {
            scrollProgress = 0.5f;
            skillMaterial.SetFloat("_ScrollProgress", scrollProgress);
            yield return new WaitForSeconds(0.1f); // Delay kecil agar terlihat perubahannya
        }

        float target = 1.0f;
        while (Mathf.Abs(scrollProgress - target) > 0.01f)
        {
            scrollProgress = Mathf.MoveTowards(scrollProgress, target, scrollSpeed * Time.deltaTime);
            skillMaterial.SetFloat("_ScrollProgress", scrollProgress);
            yield return null;
        }

        scrollProgress = target; // Pastikan nilai akhir sesuai target
        isScrolling = false;
    }
}
