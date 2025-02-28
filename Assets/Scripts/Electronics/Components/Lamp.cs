using System;
using System.Collections.Generic;
using System.ComponentModel;
using Reconnect.Electronics.Graphs;

namespace TestGraph.Components
{
    public class Lamp : Resistor
    {
        public int NominalTension;
        public Lamp(string name, int resistance, int nominalTension) : base(name, resistance)
        {
            NominalTension = nominalTension;
        }

        public Lamp(string name, List<Vertice> adjacentComponents, int resistance, int nominalTension) : base(name, adjacentComponents, resistance)
        {
            NominalTension = nominalTension;
        }

        /// <summary>
        /// Determines whether the lamp is on or off when the given intensity is applied to it.
        /// The lamps is turned on if the tension to its terminals is its <see cref="NominalTension"/>.
        /// </summary>
        /// <remarks>There is a tolerance of 0.1 Volts</remarks>
        /// <param name="intensity">The intensity applied to the lamp</param>
        /// <returns>true if the lamp turns on, false otherwise</returns>
        public bool isLampOn(double intensity) => Math.Abs(GetVoltage(intensity) - NominalTension) < 0.1;
    }
}