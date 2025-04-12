using UnityEngine;
using TMPro;

public class LapCounter : MonoBehaviour
{
    [SerializeField] private int totalLaps = 3; // Atur jumlah lap di Inspector
    [SerializeField] private TMP_Text lapText;  // Drag & Drop TMP Text dari Inspector

    private int currentLap = 0;
    private bool hasCrossedStart = false; // Cegah hitungan awal langsung bertambah

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("StartLine")) return;

        if (!hasCrossedStart)
        {
            hasCrossedStart = true; // Melewati garis start pertama kali, tidak dihitung
            return;
        }

        if (currentLap < totalLaps)
        {
            currentLap++;
            lapText.text = $"Lap: {currentLap}/{totalLaps}";
        }

        if (currentLap >= totalLaps)
            lapText.text = "Race Finished!";
    }
}
