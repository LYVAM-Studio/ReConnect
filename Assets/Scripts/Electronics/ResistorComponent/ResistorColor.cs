using System;
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
            if (resistance < 0)
                throw new ArgumentException($"Resistance value cannot be negative ! Got : {resistance}");

            // Normalize resistance to always have 3 significant digits
            // Example: 20 -> 200 with multiplier -1 (i.e. x0.1)
            
            int multiplierPower = 0;

            // Add padding 0 to get at least 3 significant digits
            while (resistance < 100)
            {
                resistance *= 10;
                multiplierPower--; // Going toward fractional multipliers
            }

            // Round to the nearest int and keep only the 3 most significant digits
            int scaledValue = Mathf.RoundToInt(resistance);
            while (scaledValue >= 1000)
            {
                scaledValue /= 10;
                multiplierPower++;
            }

            // Extract digits
            int d1 = scaledValue / 100;
            int d2 = (scaledValue / 10) % 10;
            int d3 = scaledValue % 10;
            return (d1, d2, d3, multiplierPower);
        }

        private void SetResistorBands()
        {
            (int d1, int d2, int d3, int multiplierPower) = ExtractDigits(resistanceValue);

            if (!ResistorColorCode.DigitToColor.ContainsKey(multiplierPower))
                throw new ArgumentOutOfRangeException(
                    nameof(resistanceValue),
                    $"The resistance value {resistanceValue} cannot be represented using the 5-band resistor color code"); 
            
            // Set band colors
            SetBandColor("Band1", ResistorColorCode.DigitToColor[d1]);
            SetBandColor("Band2", ResistorColorCode.DigitToColor[d2]);
            SetBandColor("Band3", ResistorColorCode.DigitToColor[d3]);
            SetBandColor("Band4", ResistorColorCode.DigitToColor[multiplierPower]);
            SetBandColor("Band5", ResistorColorCode.ToleranceToColor[tolerance]);
        }

        private void SetBandColor(string bandName, Color color)
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
                throw new ArgumentException($"Band '{bandName}' not found.");
            }
        }
    }
}
