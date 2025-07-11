using System;
using Electronics.ResistorComponent;
using Mirror;
using UnityEngine;

namespace Reconnect.Electronics.ResistorComponent
{
    public class ResistorColorManager : NetworkBehaviour
    {
        [SerializeField] private GameObject band0;
        [SerializeField] private GameObject band1;
        [SerializeField] private GameObject band2;
        [SerializeField] private GameObject band3;
        [SerializeField] private GameObject band4;
        [NonSerialized] [SyncVar(hook = nameof(OnChangeResistance))] public uint ResistanceValue;
        [NonSerialized] [SyncVar(hook = nameof(OnChangeTolerance))] public float Tolerance = 5f;

        private void OnChangeResistance(uint oldValue, uint newValue) => UpdateBandResistance();
        private void OnChangeTolerance(float oldValue, float newValue) => UpdateBandTolerance();
        
        private static (int, int, int, int) ExtractDigits(int resistance)
        {
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
            while (resistance >= 1000)
            {
                resistance /= 10;
                multiplierPower++;
            }

            // Extract digits
            int d1 = resistance / 100;
            int d2 = (resistance / 10) % 10;
            int d3 = resistance % 10;
            return (d1, d2, d3, multiplierPower);
        }

        private void UpdateBandTolerance() => SetBandColor(band4, ResistorColorCode.ToleranceToColor(Tolerance));

        private void UpdateBandResistance()
        {
            (int d1, int d2, int d3, int multiplierPower) = ExtractDigits((int)ResistanceValue);

            // Set band colors
            SetBandColor(band0, ResistorColorCode.DigitToColor(d1));
            SetBandColor(band1, ResistorColorCode.DigitToColor(d2));
            SetBandColor(band2, ResistorColorCode.DigitToColor(d3));
            SetBandColor(band3, ResistorColorCode.DigitToColor(multiplierPower));
        }
        public void UpdateBandColors()
        {
            UpdateBandResistance();
            UpdateBandTolerance();
        }

        private static void SetBandColor(GameObject band, Color color)
        {
            if (band.TryGetComponent(out Renderer rend))
                rend.material.color = color;
        }
    }
}
