using UnityEngine;

public class CarTracker : MonoBehaviour
{
    [SerializeField] private RouteHandler track;

    private int lap = 0;
    public bool[] checkpointsPassed;

    private void Start()
    {
        checkpointsPassed = new bool[3]; // 3 checkpoint (25%, 50%, 75%)
    }

    private void Update()
    {
        var (progress, newLap, closestPoint, updatedCheckpoints) = track.GetTrackProgress(transform.position, lap, checkpointsPassed);
        checkpointsPassed = updatedCheckpoints;

        Debug.Log($"Progres: {progress} / {track.totalTrackLength}, Lap: {lap}");

        // Jika sudah mencapai garis finish dan semua checkpoint telah dilewati
        if (progress >= track.totalTrackLength - 15 && track.IsLapValid(checkpointsPassed))
        {
            lap++; // Tambah lap
            checkpointsPassed = new bool[3]; // Reset checkpoint untuk lap baru
            progress = 0; // Reset progres ke awal lintasan
        }

        Debug.DrawLine(transform.position, closestPoint, Color.green); // Garis ke titik terdekat di lintasan
    }
}
