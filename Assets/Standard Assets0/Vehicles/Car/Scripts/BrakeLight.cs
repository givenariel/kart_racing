using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    public class BrakeLight : MonoBehaviour
    {
        public CarController car; // reference to the car controller, must be dragged in inspector

        private Renderer m_Renderer;

        [SerializeField] Light[] lampBlk = new Light[4];


        private void Start()
        {
            m_Renderer = GetComponent<Renderer>();
        }


        private void Update()
        {
            // enable the Renderer when the car is braking, disable it otherwise.
            for  (int i = 0; i < lampBlk.Length; i++)
            {
                lampBlk[i].enabled = car.BrakeInput > 0f;
            }
            //m_Renderer.enabled = car.BrakeInput > 0f;
        }


    }
}
