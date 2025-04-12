using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

namespace Player.CoreController.Car
{
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : NetworkBehaviour
    {
        //public Transform SkidTrailPrefab;
        //public Transform skidTrailsDetachedParent;
        public ParticleSystem skidParticles;
        public bool skidding { get; private set; }
        public bool PlayingAudio { get; private set; }

        [SerializeField] private DriftVFXControl driftControl;
        private AudioSource m_AudioSource;
        [SerializeField] private TrailRenderer m_SkidTrail;
        [SerializeField] private VisualEffect m_DriftVFX;
        private WheelCollider m_WheelCollider;

        //[SerializeField] private Transform driftVFXPrefab;


        private void Start()
        {
            //skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();

            if (m_DriftVFX != null)
            {
                m_DriftVFX.Stop();
            }
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

            //SpawnSkidParent();
            //skidTrailsDetachedParent.GetComponent<NetworkObject>().Spawn();
        }

        void SpawnSkidParent()
        {
            if (IsOwner )
            {
                GameObject skidPref = new GameObject("skidTrailDetached");
                
                //skidTrailsDetachedParent = Instantiate(skidPref.transform);
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
            m_SkidTrail.enabled = true;
            //m_SkidTrail.GetComponent<NetworkObject>().Spawn();
            //driftControl.OnPlayDriftVFX();
            if (m_DriftVFX != null)
            {
                m_DriftVFX.Play();
                //m_DriftVFX.GetComponent<NetworkObject>().Spawn();
            }

            while (m_SkidTrail == null)
            {
                yield return null;
            }

            
            //m_SkidTrail.parent = transform;
            
            //m_SkidTrail.transform.localPosition = -Vector3.up*m_WheelCollider.radius;
            if (m_DriftVFX != null)
            {
                //m_DriftVFX.parent = transform;
                //m_DriftVFX.transform.localPosition = -Vector3.up * m_WheelCollider.radius - new Vector3(0, 0.05f, 0);
            }
            
        }


        public IEnumerator EndSkidTrail()
        {
            if (!skidding)
            {
                yield break;
            }
            if (m_DriftVFX != null)
            {
                //m_DriftVFX.parent = skidTrailsDetachedParent;
                //Destroy(m_DriftVFX.gameObject, 10);
                //m_DriftVFX = null;
                m_DriftVFX.Stop();
            }
            yield return new WaitForSeconds(5);
            
            //m_SkidTrail.GetComponent <NetworkObject>().Spawn();
            //m_SkidTrail.parent = skidTrailsDetachedParent;

            //Destroy(m_SkidTrail.gameObject, 10);
            m_SkidTrail.Clear();
            m_SkidTrail.enabled = false;
            
            
            //driftControl.OnStopDriftVFX();
            skidding = false;
        }
    }
}
