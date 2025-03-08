using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;
    private KartController KartController; // Perbaikan nama class
    public Vector3 boostCamPos; // Posisi kamera boost akan selalu aktif

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("? Player belum di-assign ke CameraFollow!");
            return;
        }

        KartController = player.GetComponent<KartController>(); // Perbaikan nama class

        if (KartController == null)
        {
            Debug.LogError("? KartController tidak ditemukan di Player!");
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Mengikuti posisi pemain dengan offset
        transform.position = player.position + offset;

        // Rotasi kamera mengikuti rotasi pemain
        transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, 3 * Time.deltaTime);

        // Posisi kamera selalu dalam mode boost
        transform.GetChild(0).localPosition = boostCamPos;
    }
}
