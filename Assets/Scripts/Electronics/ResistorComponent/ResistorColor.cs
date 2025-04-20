using Electronics.ResistorComponent;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Electronics.ResistorComponent
{
    public class ResistorColor : MonoBehaviour
    {
        [SerializeField] private float resistanceValue;
        [SerializeField] private float tolerance = 5f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SetResistorBands();
        }

        private (int, int, int, int) ExtractDigits(float resistance)
        {
            Debug.Assert(resistance > 0, $"Resistance value cannot be negative ! Got : {resistance}");
            int d1, d2, d3;
            int multiplierPower;
            // Normalize resistance to always have 3 significant digits
            // Example: 20 -> 200 with multiplier -1 (i.e. ×0.1)
    
            int scaledValue = 0;
            multiplierPower = 0;

            // Add padding 0 to get at least 3 significant digits
            while (resistance < 100)
            {
                resistance *= 10;
                multiplierPower--; // Going toward fractional multipliers
            }

            // Round to the nearest int and keep only the 3 most significant digits
            scaledValue = Mathf.RoundToInt(resistance);
            while (scaledValue >= 1000)
            {
                scaledValue /= 10;
                multiplierPower++;
            }

            // Extract digits
            d1 = scaledValue / 100;
            d2 = (scaledValue / 10) % 10;
            d3 = scaledValue % 10;
            return (d1, d2, d3, multiplierPower);
        }

        void SetResistorBands()
        {
            (int d1, int d2, int d3, int multiplierPower) = ExtractDigits(resistanceValue);
            
            if (!ResistorColorCode.DigitToColor.ContainsKey(multiplierPower))
                Debug.LogError($"The resistance value {resistanceValue} cannot be represented using the 5-band resistor color code");
            
            // Set band colors
            SetBandColor("Band1", ResistorColorCode.DigitToColor[d1]);
            SetBandColor("Band2", ResistorColorCode.DigitToColor[d2]);
            SetBandColor("Band3", ResistorColorCode.DigitToColor[d3]);
            SetBandColor("Band4", ResistorColorCode.DigitToColor[multiplierPower]);
            SetBandColor("Band5", ResistorColorCode.ToleranceToColor[tolerance]);
        }

        void SetBandColor(string bandName, Color color)
        {
            Transform band = transform.Find(bandName);
            if (band != null)
            {
                Renderer rend = band.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = color;
            }
            else
            {
                Debug.LogError($"Band '{bandName}' not found.");
            }
        }
    }
}
