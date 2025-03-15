using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;
    private KartController KartController;
    public Vector3 boostCamPos;

    void Start()
    {
        if (player == null)
        {
            return;
        }

        KartController = player.GetComponent<KartController>(); 

        if (KartController == null)
        {
        }
    }

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.position + offset;
        transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, 3 * Time.deltaTime);
        transform.GetChild(0).localPosition = boostCamPos;
    }
}
