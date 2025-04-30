using System;
using Electronics.ResistorComponent;
using UnityEngine;

namespace Reconnect.Electronics.ResistorComponent
{
    public class ResistorColorManager : MonoBehaviour
    {
        [SerializeField] private GameObject band0;
        [SerializeField] private GameObject band1;
        [SerializeField] private GameObject band2;
        [SerializeField] private GameObject band3;
        [SerializeField] private GameObject band4;
        [NonSerialized] public float ResistanceValue;
        [NonSerialized] public float Tolerance = 5f;
        
        private static (int, int, int, int) ExtractDigits(float resistance)
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

        public void UpdateBandColors()
        {
            (int d1, int d2, int d3, int multiplierPower) = ExtractDigits(ResistanceValue);

            // Set band colors
            SetBandColor(band0, ResistorColorCode.DigitToColor(d1));
            SetBandColor(band1, ResistorColorCode.DigitToColor(d2));
            SetBandColor(band2, ResistorColorCode.DigitToColor(d3));
            SetBandColor(band3, ResistorColorCode.DigitToColor(multiplierPower));
            SetBandColor(band4, ResistorColorCode.ToleranceToColor(Tolerance));
        }

        private void SetBandColor(GameObject band, Color color)
        {
            Renderer rend = band.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = color;
        }
    }
}
