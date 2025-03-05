using UnityEngine;

public class SphereController : MonoBehaviour
{
    public Rigidbody sphereRB;  // Sphere utama yang bergerak
    public Transform rodaDepanKiri;  // Roda depan kiri
    public Transform rodaDepanKanan; // Roda depan kanan
    public float kecepatan = 10f;  // Kecepatan gerak
    public float kecepatanBelok = 5f; // Kecepatan belok
    public float maxSteerAngle = 30f; // Maksimum sudut belok roda depan

    private float steerInput;
    private float steerAngle; // ? Tambahkan variabel ini agar tidak error

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        // Gerakkan sphere utama
        sphereRB.AddForce(transform.forward * moveInput * kecepatan, ForceMode.Acceleration);

        // Beri torque agar sphere bisa belok
        sphereRB.AddTorque(Vector3.up * steerInput * kecepatanBelok, ForceMode.Acceleration);

        // Hitung steer angle
        steerAngle = maxSteerAngle * steerInput;

        // ? Rotasi hanya pada sumbu Y, tanpa mengubah X/Z
        rodaDepanKiri.localRotation = Quaternion.Euler(rodaDepanKiri.localRotation.eulerAngles.x, steerAngle, rodaDepanKiri.localRotation.eulerAngles.z);
        rodaDepanKanan.localRotation = Quaternion.Euler(rodaDepanKanan.localRotation.eulerAngles.x, steerAngle, rodaDepanKanan.localRotation.eulerAngles.z);
    }
}
