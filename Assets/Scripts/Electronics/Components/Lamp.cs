using System.Collections.Generic;
using Reconnect.Electronics.Graphs;

namespace Reconnect.Electronics.Components
{
    public class Lamp : Resistor
    {
        public LightBulb LightBulb { get; set; }

        public Lamp(string name, uint resistance, LightBulb lightBulb) : base(name, resistance)
        {
            LightBulb = lightBulb;
        }

        public Lamp(string name, List<Vertex> adjacentComponents, uint resistance, LightBulb lightBulb) : base(name, adjacentComponents, resistance)
        {
            LightBulb = lightBulb;
        }

        public override void DoAction()
        {
            LightBulb.Set(true);
        }
        
        public override void UndoAction()
        {
            LightBulb.Set(false);
        }
    }
}