using UnityEngine;

namespace Reconnect.Electronics.CircuitLoading
{
    public struct CircuitInfo
    {
        public enum Quantity { Tension, Intensity }

        /// <summary>
        /// The title of the circuit level. 
        /// </summary>
        public string Title;
        /// <summary>
        /// The input tension of the circuit in Volts.
        /// </summary>
        public float InputTension;
        /// <summary>
        /// The input intensity of the circuit in Amperes.
        /// </summary>
        public float InputIntensity;
        /// <summary>
        /// The value of the tension or intensity through the target necessary to solve the circuit. The quantity is determined by the TargetQuantity field.
        /// </summary>
        public float TargetValue;
        /// <summary>
        /// Whether the TargetValue refers to the tension or the intensity through the target.
        /// </summary>
        public Quantity TargetQuantity;
        /// <summary>
        /// The tolerance of the TargetValue field for the circuit to be validated.
        /// </summary>
        public float TargetTolerance;
        /// <summary>
        /// The position of the input of the circuit on the breadboard. The y component should always be 0.
        /// </summary>
        public Vector2Int InputPoint;
        /// <summary>
        /// The position of the output of the circuit on the breadboard. The y component should always be 7.
        /// </summary>
        public Vector2Int OutputPoint;
    }
}