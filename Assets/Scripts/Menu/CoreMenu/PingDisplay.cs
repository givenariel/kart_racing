using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class PingDisplay : MonoBehaviour
{
    public TMP_Text pingText; // Assign in Inspector (Optional: UI Text to Display Ping)

    private void Update()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            var networkTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            if (networkTransport is UnityTransport utp)
            {
                // GetCurrentRtt returns a ulong, so we need to cast it to int safely
                ulong rawPing = utp.GetCurrentRtt(NetworkManager.Singleton.LocalClientId);
                int ping = rawPing > int.MaxValue ? int.MaxValue : (int)rawPing; // Prevent overflow

                // Log to Console
                Debug.Log($"Ping: {ping} ms");

                // Update UI if assigned
                if (pingText != null)
                {
                    pingText.text = $"Ping: {ping} ms";
                }
            }
        }
    }
}
