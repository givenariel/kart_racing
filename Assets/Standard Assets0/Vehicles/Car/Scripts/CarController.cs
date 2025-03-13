using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

namespace UnityStandardAssets.Vehicles.Car
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        MPH,
        KPH
    }

    public class CarController : MonoBehaviour
    {
        [SerializeField] private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] private Vector3 m_CentreOfMassOffset;
        [SerializeField] private float m_MaximumSteerAngle;
        [Range(0, 1)] [SerializeField] private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [Range(0, 1)] [SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference
        [SerializeField] private float m_FullTorqueOverAllWheels;
        [SerializeField] private float m_ReverseTorque;
        [SerializeField] private float m_MaxHandbrakeTorque;
        [SerializeField] private float m_Downforce = 100f;
        [SerializeField] private SpeedType m_SpeedType;
        [SerializeField] private float m_Topspeed = 200;
        [SerializeField] private static int NoOfGears = 5;
        [SerializeField] private float m_RevRangeBoundary = 1f;
        [SerializeField] private float m_SlipLimit;
        [SerializeField] private float m_BrakeTorque;

        private Quaternion[] m_WheelMeshLocalRotations;
        private Vector3 m_Prevpos, m_Pos;
        private float m_SteerAngle;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_ReversingThreshold = 0.01f;

        [SerializeField] private float m_DriftSteerFactor = 1.5f;
        [SerializeField] private float m_DriftTractionFactor = 0.5f;
        [SerializeField] private Transform forcePointDrift;

        private bool isDrifting = false;
        private float originalTraction;
        private float originalSteerHelper;

        [SerializeField] private float boostMultiplier = 2f; // Faktor peningkatan kecepatan saat boost
        [SerializeField] private float boostDuration = 3f;   // Durasi boost dalam detik
        public bool isBoosting = false;
        float originalTopSpeed;             // Simpan top speed asli
        float originalTorque;
        private float originalDownforce;

        [SerializeField] private BoostFXControl boostFX;
        //[SerializeField] private DriftVFXControl driftVFXControl;

        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle{ get { return m_SteerAngle; }}
        public float CurrentSpeed{ get { return m_Rigidbody.linearVelocity.magnitude*2.23693629f; }}
        public float MaxSpeed{get { return m_Topspeed; }}
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        // Use this for initialization
        private void Start()
        {
            m_WheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

            m_MaxHandbrakeTorque = float.MaxValue;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl*m_FullTorqueOverAllWheels);
            originalTraction = m_TractionControl;
            originalTopSpeed = m_Topspeed;
            originalTorque = m_FullTorqueOverAllWheels;
            originalDownforce = m_Downforce;
            originalSteerHelper = m_SteerHelper;
        }

        public void DriftCheck()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                StartDrift();
            }
            if (UnityEngine.Input.GetKeyUp(KeyCode.Space))
            {
                StopDrift();
            }
        }

        private void StartDrift()
        {
            isDrifting = true;
            //originalSteerAngle = m_MaximumSteerAngle;
            //m_Rigidbody.AddForceAtPosition(transform.right * 10, transform.position - transform.forward * 2.5f, ForceMode.Impulse);
            //m_MaximumSteerAngle *= m_DriftSteerFactor;
            m_TractionControl *= m_DriftTractionFactor;
            //driftVFXControl.OnPlayDriftVFX();
            m_FullTorqueOverAllWheels *= 2.8f;
            m_Downforce *= 2.5f;
            m_SteerHelper *= 0.4f;
        }


        private void StopDrift()
        {
            isDrifting = false;
            //driftVFXControl.OnStopDriftVFX();
           // m_MaximumSteerAngle = originalSteerAngle;
            m_TractionControl = originalTraction;
            for (int i = 0; i < 4; i++)
            {
                
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque * 1;
                
            }
            m_FullTorqueOverAllWheels = originalTorque;
            m_Downforce = originalDownforce;
            m_SteerHelper = originalSteerHelper;
            m_Rigidbody.angularVelocity = Vector3.zero;
            /*
            float accel = 20;
            float thrustTorque;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

            }*/
            StartCoroutine(ReduceLateralVelocity());
        }

        private IEnumerator ReduceLateralVelocity()
        {
            
            float elapsedTime = 0f;
            float duration = 0.3f; // Waktu untuk mengurangi gaya samping
            float lateralReductionFactor = 1f; // 50% dari gaya samping akan dikurangi

            Vector3 startVelocity = m_Rigidbody.linearVelocity;
            Vector3 forwardVelocity =  transform.forward * 15;
            Vector3 forwardVelocityFact = transform.forward * Vector3.Dot(m_Rigidbody.linearVelocity, transform.forward); // Komponen maju
            Vector3 lateralVelocity = m_Rigidbody.linearVelocity - forwardVelocityFact; // Komponen samping

            // Target velocity: masih ada sebagian lateral velocity yang tersisa
            Vector3 targetVelocity = forwardVelocity + (lateralVelocity * (1 - lateralReductionFactor));

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
               
                // Reduksi velocity lateral dengan Lerp
                m_Rigidbody.linearVelocity = Vector3.Lerp(startVelocity, targetVelocity, t);
                //startVelocity = m_Rigidbody.linearVelocity;
                m_Rigidbody.AddForce(forwardVelocity, ForceMode.Acceleration);
                yield return null;
            }

            // Pastikan velocity hanya di komponen maju
           // m_Rigidbody.linearVelocity = targetVelocity;

           
        }

        public IEnumerator Boost()
        {
            if (isBoosting) yield break; // Jika sedang boost, abaikan


            boostFX.OnPlayBoostFX();
            isBoosting = true;
           ; // Simpan torque asli
            

            m_FullTorqueOverAllWheels *= boostMultiplier * 0.8f;  // Tingkatkan tenaga
            m_Topspeed *= boostMultiplier;                 // Tingkatkan kecepatan maksimal
            m_Rigidbody.linearVelocity += transform.forward * 15;

            
           

            yield return new WaitForSeconds(boostDuration); // Tunggu selama boost
            /*
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:

                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = 0;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = 0;
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = 0;
                    break;

            }
            // Kembalikan kecepatan ke nilai asli
            m_FullTorqueOverAllWheels = originalTorque;
            /*
            while (CurrentSpeed >= originalTopSpeed)
            {
                m_Rigidbody.linearVelocity -= Vector3.Lerp(transform.position, transform.forward, 0.01f * Time.deltaTime) * 0.001f;
                yield return null;
            }*/
            m_Topspeed = originalTopSpeed;
            boostFX.OnStopBoostFX();
            m_FullTorqueOverAllWheels = originalTorque;

            isBoosting = false;
        }



        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
            float upgearlimit = (1/(float) NoOfGears)*(m_GearNum + 1);
            float downgearlimit = (1/(float) NoOfGears)*m_GearNum;

            if (m_GearNum > 0 && f < downgearlimit)
            {
                m_GearNum--;
            }

            if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
            {
                m_GearNum++;
            }
        }


        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor)*(1 - factor);
        }


        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }


        private void CalculateGearFactor()
        {
            float f = (1/(float) NoOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f*m_GearNum, f*(m_GearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime*5f);
        }


        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = m_GearNum/(float) NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
        }


        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1*Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);
            if (isDrifting && CurrentSpeed > 10 && steering != 0)
            {
                steering *= 1.25f; // Tambah efek drift
                m_SteerAngle = steering * m_MaximumSteerAngle * m_DriftSteerFactor;
                Vector3 lateralForce = transform.right * -steering * 15;
                m_Rigidbody.AddForce(lateralForce, ForceMode.Acceleration);
                //m_Rigidbody.AddForce(transform.right * -steering * 45, ForceMode.Impulse);
                Vector3 rearDriftForce = transform.right * -steering * (1 - 0.2f) *1.5f* 0.0330f * CurrentSpeed + (transform.forward * 0.015f * CurrentSpeed * 35 ) ;
                //Vector3 rearDriftForce = transform.right * steering * -1;
                rearDriftForce.y = 0f;
               //m_Rigidbody.linearVelocity *= 0.98f;
                m_Rigidbody.AddForceAtPosition(rearDriftForce, forcePointDrift.position, ForceMode.Acceleration);
            }
            else
            {
                m_SteerAngle = steering * m_MaximumSteerAngle;
            }

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                var hbTorque = handbrake*m_MaxHandbrakeTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
                m_WheelColliders[3].brakeTorque = hbTorque;
            }


            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
        }


        private void CapSpeed()
        {
            float speed = m_Rigidbody.linearVelocity.magnitude;
            switch (m_SpeedType)
            {
                case SpeedType.MPH:

                    speed *= 2.23693629f;
                    if (speed > m_Topspeed)
                        m_Rigidbody.linearVelocity = (m_Topspeed/2.23693629f) * m_Rigidbody.linearVelocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= 3.6f;
                    if (speed > m_Topspeed)
                        m_Rigidbody.linearVelocity = (m_Topspeed/3.6f) * m_Rigidbody.linearVelocity.normalized;
                    break;
            }
        }


        private void ApplyDrive(float accel, float footbrake)
        {
            accel *=2;
            
            float thrustTorque;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 4f);
                    if (isBoosting)
                    {
                        thrustTorque *= boostMultiplier;
                    }

                    if (CurrentSpeed >= originalTopSpeed)
                    {
                        //return;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    if (isBoosting)
                    {
                        thrustTorque *= boostMultiplier;
                    }
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    if (isBoosting)
                    {
                        thrustTorque *= boostMultiplier;
                    }
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.linearVelocity) < 50f)
                {
                    m_WheelColliders[i].brakeTorque = m_BrakeTorque*footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque*footbrake;
                }
            }
        }


        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                m_Rigidbody.linearVelocity = velRotation * m_Rigidbody.linearVelocity;
            }
            m_OldRotation = transform.eulerAngles.y;
        }


        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up*m_Downforce*
                                                         m_WheelColliders[0].attachedRigidbody.linearVelocity.magnitude);
        }


        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
                {
                    m_WheelEffects[i].EmitTyreSmoke();

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }

        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
            {
                m_CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                m_CurrentTorque += 10 * m_TractionControl;
                if (m_CurrentTorque > m_FullTorqueOverAllWheels)
                {
                    m_CurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }


        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
