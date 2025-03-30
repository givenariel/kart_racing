using System.Collections.Generic;
using UnityEngine;

public class RouteHandler : MonoBehaviour
{
    [SerializeField] private Transform[] routes; // Array dari tiap segmen jalur
    [SerializeField] private int resolution = 20; // Berapa banyak titik sampling per segmen

    [SerializeField] private List<Vector3> trackPoints = new List<Vector3>(); // Semua titik gabungan
    private List<float> trackDistances = new List<float>(); // Jarak kumulatif dari titik awal
    private List<float> checkpoints = new List<float>(); // Checkpoint jarak

    public float totalTrackLength = 0f;

    private void Start()
    {
        GenerateTrackPoints();
    }

    private void GenerateTrackPoints()
    {
        trackPoints.Clear();
        trackDistances.Clear();
        checkpoints.Clear();
        totalTrackLength = 0f;

        Vector3 lastPoint = Vector3.zero;

        for (int segment = 0; segment < routes.Length; segment++)
        {
            Transform route = routes[segment];

            Vector3 p0 = route.GetChild(0).position;
            Vector3 p1 = route.GetChild(1).position;
            Vector3 p2 = route.GetChild(2).position;
            Vector3 p3 = route.GetChild(3).position;

            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 bezierPoint = GetBezierPoint(t, p0, p1, p2, p3);
                trackPoints.Add(bezierPoint);

                if (trackDistances.Count == 0)
                {
                    trackDistances.Add(0f);
                }
                else
                {
                    float segmentLength = Vector3.Distance(lastPoint, bezierPoint);
                    totalTrackLength += segmentLength;
                    trackDistances.Add(totalTrackLength);
                }

                lastPoint = bezierPoint;
            }
        }

        // Buat 3 checkpoint di lintasan
        checkpoints.Add(totalTrackLength * 0.25f);
        checkpoints.Add(totalTrackLength * 0.50f);
        checkpoints.Add(totalTrackLength * 0.75f);
    }

    private Vector3 GetBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Mathf.Pow(1 - t, 3) * p0 +
               3 * Mathf.Pow(1 - t, 2) * t * p1 +
               3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
               Mathf.Pow(t, 3) * p3;
    }

    public List<Vector3> GetTrackPoints()
    {
        return new List<Vector3>(trackPoints);
    }

    public (float progress, int lap, Vector3 closestPoint, bool[] checkpointStatus, int index) GetTrackProgress(Vector3 carPosition, float currentLap, bool[] checkpointsPassed)
    {
        float minDistance = Mathf.Infinity;
        float closestProgress = 0f;
        Vector3 closestPoint = Vector3.zero;
        int indexClosest = 0;

        for (int i = 0; i < trackPoints.Count; i++)
        {
            float distance = Vector3.Distance(carPosition, trackPoints[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestProgress = trackDistances[i];
                closestPoint = trackPoints[i];
                indexClosest = i;
            }
        }

        int lap = (int)(closestProgress / totalTrackLength);
        float normalizedProgress = closestProgress % totalTrackLength;

        // Update checkpoint status
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (Mathf.Abs(checkpoints[i] - closestProgress) <  10f)
            {
                checkpointsPassed[i] = true;
            }
        }

        return (normalizedProgress, lap, closestPoint, checkpointsPassed, indexClosest);
    }

    public bool IsLapValid(bool[] checkpointsPassed)
    {
        foreach (bool passed in checkpointsPassed)
        {
            if (!passed) return false;
        }
        return true;
    }
}
