using UnityEngine;

public class PowerSlideZone : MonoBehaviour
{
    [SerializeField] private float speedBoost = 20f;  // How much the max speed increases
    [SerializeField] private float boostDuration = 3f; // How long the boost lasts

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CarController car = other.GetComponent<CarController>();

            if (car != null)
            {
                car.TriggerBoostSlide(speedBoost, boostDuration);
            }
        }
    }
}
