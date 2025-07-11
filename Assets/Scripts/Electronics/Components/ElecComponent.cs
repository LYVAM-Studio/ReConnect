using System;
using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace Reconnect.Electronics.Components
{
    public abstract class ElecComponent : Vertex
    {
        public double Resistance { get; set; }
    
        public ElecComponent(string name, double resistance) : base(name)
        {
            if (resistance < 0)
                throw new ArgumentException("Resistance of a component cannot be negative");
            Resistance = resistance;
        }
        public ElecComponent(string name, List<Vertex> adjacentComponents, double resistance) : base(name, adjacentComponents)
        {
            if (resistance < 0)
                throw new ArgumentException("Resistance of a component cannot be negative");
            Resistance = resistance;
        }

        public double GetTension(double intensity)
        {
            return Resistance * intensity;
        }
        
        public double GetIntensity(double tension)
        {
            return tension / Resistance;
        }

        public virtual void DoAction() { }

        public virtual void UndoAction() { }
    }
}