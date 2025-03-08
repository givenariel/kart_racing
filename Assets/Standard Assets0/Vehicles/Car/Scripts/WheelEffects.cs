using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        public Transform SkidTrailPrefab;
        public static Transform skidTrailsDetachedParent;
        public ParticleSystem skidParticles;
        public bool skidding { get; private set; }
        public bool PlayingAudio { get; private set; }

        [SerializeField] private DriftVFXControl driftControl;
        private AudioSource m_AudioSource;
        private Transform m_SkidTrail;
        private Transform m_DriftVFX;
        private WheelCollider m_WheelCollider;

        [SerializeField] private Transform driftVFXPrefab;


        private void Start()
        {
            skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();

            if (skidParticles == null)
            {
                Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
            }
            else
            {
                skidParticles.Stop();
            }

            m_WheelCollider = GetComponent<WheelCollider>();
            m_AudioSource = GetComponent<AudioSource>();
            PlayingAudio = false;

            if (skidTrailsDetachedParent == null)
            {
                skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }


        public void EmitTyreSmoke()
        {
            skidParticles.transform.position = transform.position - transform.up*m_WheelCollider.radius;
            skidParticles.Emit(1);
            if (!skidding)
            {
                StartCoroutine(StartSkidTrail());
            }
        }


        public void PlayAudio()
        {
            m_AudioSource.Play();
            PlayingAudio = true;
        }


        public void StopAudio()
        {
            m_AudioSource.Stop();
            PlayingAudio = false;
        }


        public IEnumerator StartSkidTrail()
        {
            skidding = true;
            m_SkidTrail = Instantiate(SkidTrailPrefab);
            //driftControl.OnPlayDriftVFX();
            if (driftVFXPrefab != null && m_DriftVFX == null)
            {
                m_DriftVFX = Instantiate(driftVFXPrefab);
            }

            while (m_SkidTrail == null)
            {
                yield return null;
            }

            
            m_SkidTrail.parent = transform;
            
            m_SkidTrail.localPosition = -Vector3.up*m_WheelCollider.radius;
            if (m_DriftVFX != null)
            {
                m_DriftVFX.parent = transform;
                m_DriftVFX.localPosition = -Vector3.up * m_WheelCollider.radius - new Vector3(0, 0.05f, 0);
            }
            
        }


        public void EndSkidTrail()
        {
            if (!skidding)
            {
                return;
            }
            skidding = false;
            m_SkidTrail.parent = skidTrailsDetachedParent;
            
            Destroy(m_SkidTrail.gameObject, 10);
            
            if (m_DriftVFX != null)
            {
                m_DriftVFX.parent = skidTrailsDetachedParent;
                Destroy(m_DriftVFX.gameObject, 10);
                m_DriftVFX = null;
            }
            //driftControl.OnStopDriftVFX();
        }
    }
}
