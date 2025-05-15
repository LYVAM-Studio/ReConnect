using UnityEngine;

namespace Reconnect.Entities.Player
{
    using UnityEngine;

    public class PlayerStamina : MonoBehaviour
    {
        public float maxStamina = 100f;
        public float currentStamina;

        public float regenRate = 15f;       // Stamina regenerated per second
        public float regenDelay = 1f;       // Delay after using stamina before regen starts

        private float lastStaminaUseTime;

        void Start()
        {
            currentStamina = maxStamina;
        }

        void Update()
        {
            // If enough time has passed since last use, regenerate stamina
            if (Time.time - lastStaminaUseTime >= regenDelay && currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); // Clamp to max
            }

            // Example input (Shift key to sprint)
            if (Input.GetKey(KeyCode.LeftShift))
            {
                UseStamina(20f * Time.deltaTime); // 20 stamina per second
            }
        }

        public void UseStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                lastStaminaUseTime = Time.time;
            }
            else
            {
                // Not enough stamina – handle this case (e.g., stop sprinting)
            }
        }
    }

}