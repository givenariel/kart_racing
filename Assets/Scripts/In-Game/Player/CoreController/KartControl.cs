using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody rb;
    private float CurrentSpeed = 0;
    public float MaxSpeed;
    public float boostSpeed;
    private float RealSpeed;

    [Header("Tires")]
    public Transform frontLeftTire;
    public Transform frontRightTire;
    public Transform backLeftTire;
    public Transform backRightTire;

    private float steerDirection;
    private float driftTime;
    private bool isDrifting = false;
    private float outwardsDriftForce = 50000;
    private bool touchingGround;

    [Header("Particles Drift Sparks")]
    public Transform driftParticles;
    public Color driftColor;

    [HideInInspector]
    public float BoostTime = 0;
    public Transform boostFire;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Move();
        TireSteer();
        Steer();
        GroundNormalRotation();
        Drift();
        Boosts();
    }

    private void Move()
    {
        RealSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;

        if (Input.GetKey(KeyCode.Space))
        {
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, MaxSpeed, Time.deltaTime * 0.5f);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, -MaxSpeed / 1.75f, 1f * Time.deltaTime);
        }
        else
        {
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0, Time.deltaTime * 1.5f);
        }

        Vector3 vel = transform.forward * CurrentSpeed;
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;
    }

    private void Steer()
    {
        steerDirection = Input.GetAxisRaw("Horizontal");
        float steerAmount = RealSpeed > 30 ? RealSpeed / 4 * steerDirection : RealSpeed / 1.5f * steerDirection;
        
        Vector3 steerDirVect = new Vector3(
            transform.eulerAngles.x,
            transform.eulerAngles.y + steerAmount,
            transform.eulerAngles.z
        );
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, steerDirVect, 3 * Time.deltaTime);
    }

    private void GroundNormalRotation()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 0.75f))
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation,
                7.5f * Time.deltaTime
            );
            touchingGround = true;
        }
        else
        {
            touchingGround = false;
        }
    }

    private void Drift()
    {
        if (Input.GetKeyDown(KeyCode.V) && touchingGround)
        {
            isDrifting = true;
        }

        if (Input.GetKey(KeyCode.V) && touchingGround && CurrentSpeed > 40)
        {
            driftTime += Time.deltaTime;
            UpdateDriftParticles(driftColor);
        }

        if (!Input.GetKey(KeyCode.V) || RealSpeed < 40)
        {
            isDrifting = false;

            if (driftTime > 1.5f && driftTime < 4)
                BoostTime = 0.75f;
            else if (driftTime >= 4 && driftTime < 7)
                BoostTime = 1.5f;
            else if (driftTime >= 7)
                BoostTime = 2.5f;

            driftTime = 0;
            StopDriftParticles();
        }
    }

    private void UpdateDriftParticles(Color color)
    {
        for (int i = 0; i < driftParticles.childCount; i++)
        {
            ParticleSystem ps = driftParticles.GetChild(i).GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            if (!ps.isPlaying) ps.Play();
        }
    }

    private void StopDriftParticles()
    {
        for (int i = 0; i < driftParticles.childCount; i++)
        {
            driftParticles.GetChild(i).GetComponent<ParticleSystem>().Stop();
        }
    }

    private void Boosts()
    {
        BoostTime -= Time.deltaTime;
        if (BoostTime > 0)
        {
            for (int i = 0; i < boostFire.childCount; i++)
            {
                if (!boostFire.GetChild(i).GetComponent<ParticleSystem>().isPlaying)
                {
                    boostFire.GetChild(i).GetComponent<ParticleSystem>().Play();
                }
            }
            MaxSpeed = boostSpeed;
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, MaxSpeed, Time.deltaTime);
        }
        else
        {
            for (int i = 0; i < boostFire.childCount; i++)
            {
                boostFire.GetChild(i).GetComponent<ParticleSystem>().Stop();
            }
            MaxSpeed = boostSpeed - 20;
        }
    }

    private void TireSteer()
    {
        float spinSpeed = CurrentSpeed > 30 ? CurrentSpeed : RealSpeed;
        if (frontLeftTire.childCount > 0)
            frontLeftTire.GetChild(0).Rotate(-90 * Time.deltaTime * spinSpeed * 0.5f, 0, 0);

        if (frontRightTire.childCount > 0)
            frontRightTire.GetChild(0).Rotate(-90 * Time.deltaTime * spinSpeed * 0.5f, 0, 0);

    }
}
