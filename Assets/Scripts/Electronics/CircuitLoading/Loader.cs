using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Components;
using UnityEngine;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;

namespace Reconnect.Electronics.CircuitLoading
{
    public static class Loader
    {
        /// <summary>
        /// Returns the value associated with the yaml node in the given children that has the argument `key` as key.
        /// </summary>
        /// <param name="children">The children of a node in which the element in searched.</param>
        /// <param name="key">The key of the sought yaml node.</param>
        /// <returns>The value of the node.</returns>
        /// <example>
        /// <code>
        /// node:
        ///     child1: value1
        ///     child2: value2
        /// </code>
        /// For instance, `value2` can be got calling this function with `node.Children` and `child2`.
        /// </example>
        /// <exception cref="Exception">Throws an exception if the sought key is not found.</exception>
        private static string YamlGetScalarValue(IOrderedDictionary<YamlNode, YamlNode> children, string key)
        {
            if (!children.TryGetValue(new YamlScalarNode(key), out YamlNode node))
                throw new InvalidDataException($"Sought key {key} not found in yaml document.");
            return ((YamlScalarNode)node).Value!;
        }
        
        private static bool YamlTryGetScalarValue(IOrderedDictionary<YamlNode, YamlNode> children, string key, out string value)
        {
            value = null;
            if (!children.TryGetValue(new YamlScalarNode(key), out YamlNode node))
                return false;
            value = ((YamlScalarNode)node).Value;
            return true;
        }
        
        /// <summary>
        /// Shifts the given point to the given direction by 1.
        /// </summary>
        /// <param name="sourcePoint"> The initial point.</param>
        /// <param name="direction">The direction of the shift. It is represented by cardinal points in a string (for instance "n", "se"...).</param>
        /// <param name="allowDiagonal">Whether the direction can be composed of two cardinal points ("ne", "sw"...) or only one ("n", "e"). By default, the diagonals are not allowed.</param>
        /// <returns>Returns a new point shifted by 1 in the given direction from the given point.</returns>
        /// <exception cref="Exception">Throws an exception if the direction is not valid.</exception>
        private static Point ShiftToDirection(Point sourcePoint, string direction, bool allowDiagonal = false)
        {
            return direction.ToLower().Trim() switch
            {
                "n"  => sourcePoint.Shift(-1,  0),
                "e"  => sourcePoint.Shift( 0, +1),
                "s"  => sourcePoint.Shift(+1,  0),
                "w"  => sourcePoint.Shift( 0, -1),
                "ne" when allowDiagonal => sourcePoint.Shift(-1, +1),
                "se" when allowDiagonal => sourcePoint.Shift(+1, +1),
                "sw" when allowDiagonal => sourcePoint.Shift(+1, -1),
                "nw" when allowDiagonal => sourcePoint.Shift(-1, -1),
                { } dir => throw new InvalidDataException(allowDiagonal
                    ? $"Invalid direction. Expected 'n', 'e', 's', 'w', 'ne', 'se', 'sw' or 'nw' but got {dir}."
                    : $"Invalid direction. Expected 'n', 'e', 's' or 'w' but got '{dir}'.")
            };
        }
        
        /// <summary>
        /// Loads the given circuit. Cleans the breadboard and load all the components required for the circuit.
        /// </summary>
        /// <param name="circuitName">The name of the circuit to be loaded. It must contain neither the extension (.yaml) nor the path (CircuitsPresets/).</param>
        public static void LoadCircuit(Breadboard breadboard, string circuitName)
        {
            breadboard.Clean();
            
            TextAsset yamlAsset = Resources.Load<TextAsset>($"CircuitsPresets/{circuitName}");

            using StringReader reader = new StringReader(yamlAsset.text);
            YamlStream yaml = new YamlStream();
            yaml.Load(reader);

            YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;

            breadboard.CircuitInfo = new CircuitInfo
            {
                Title = YamlGetScalarValue(root.Children, "title"),
                InputTension = float.Parse(YamlGetScalarValue(root.Children, "input-tension"), CultureInfo.InvariantCulture),
                InputIntensity = float.Parse(YamlGetScalarValue(root.Children, "input-intensity"), CultureInfo.InvariantCulture),
                TargetTension = float.Parse(YamlGetScalarValue(root.Children, "target-tension"), CultureInfo.InvariantCulture)
            };

            YamlSequenceNode componentsNode = (YamlSequenceNode)root.Children[new YamlScalarNode("components")];

            int componentId = 0;
            
            foreach (YamlNode item in componentsNode)
            {
                YamlMappingNode component = (YamlMappingNode)item;
                string type = ((YamlScalarNode)component.Children.First().Key).Value;
                    
                // Fields in common (for any type of component) 

                string name = $"{componentId}: ";
                int hPos = int.Parse(YamlGetScalarValue(component.Children, "h-pos"));
                int wPos = int.Parse(YamlGetScalarValue(component.Children, "w-pos"));
                string direction = YamlGetScalarValue(component.Children, "direction");
                bool isTarget = false;
                if (YamlTryGetScalarValue(component.Children, "target", out string targetValue)
                    && !bool.TryParse(targetValue, out isTarget))
                    throw new InvalidDataException($"Yaml key 'target' expect a boolean value but got {targetValue}.");
                bool isLocked = false;
                if (YamlTryGetScalarValue(component.Children, "locked", out string lockedValue)
                    && !bool.TryParse(lockedValue, out isLocked))
                    throw new InvalidDataException($"Yaml key 'locked' expect a boolean value but got {targetValue}.");
                
                Point sourcePoint = new Point(hPos, wPos);
                Point destinationPoint = ShiftToDirection(sourcePoint, direction, allowDiagonal: type == "wire");
                
                if (component.Children.TryGetValue(new YamlScalarNode("name"), out YamlNode nameNode))
                    name += $"{((YamlScalarNode)nameNode).Value} {sourcePoint} <-> {destinationPoint}";
                else
                    name += $"{sourcePoint} <-> {destinationPoint}";

                Vector3 sourcePos = Point.PointToVector(sourcePoint, 0);
                Vector3 destinationPos = Point.PointToVector(destinationPoint, 0);
                
                // Component-specific fields
                
                if (type == "wire")
                {
                    breadboard.CreateWire(sourcePos, destinationPos, name, sourcePoint, destinationPoint, isLocked);
                    if (isTarget)
                        throw new InvalidDataException("Invalid target: a wire cannot be a circuit target.");
                }
                else if (type == "resistor")
                {
                    float resistance = float.Parse(YamlGetScalarValue(component.Children, "resistance"), CultureInfo.InvariantCulture);
                    
                    Resistor resistor = breadboard.CreateResistor(sourcePos, destinationPos, name, resistance, isLocked);
                    if (isTarget) 
                    {
                        if (breadboard.Target != null) throw new InvalidDataException("Multiple targets found. A circuit should have only one target.");
                        breadboard.Target = resistor;
                    }
                }
                else if (type == "lamp")
                {
                    float resistance = float.Parse(YamlGetScalarValue(component.Children, "resistance"), CultureInfo.InvariantCulture);
                    float nominalTension = float.Parse(YamlGetScalarValue(component.Children, "nominal-tension"), CultureInfo.InvariantCulture);
                    
                    Lamp lamp = breadboard.CreateLamp(sourcePos, destinationPos, name, resistance, nominalTension, isLocked);
                    if (isTarget) 
                    {
                        if (breadboard.Target != null) throw new InvalidDataException("Multiple targets found. A circuit should have only one target.");
                        breadboard.Target = lamp;
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid component type. Expected wire, resistor or lamp but got {type}.");
                }

                componentId++;
            }

            if (breadboard.Target is null)
                throw new FormatException("The loaded circuit does not contain any target.");
        }

        
    }
}