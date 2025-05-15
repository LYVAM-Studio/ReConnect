using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Mirror;
using Reconnect.Electronics.Breadboards;
using Reconnect.Electronics.Breadboards.NetworkSync;
using Reconnect.Electronics.Components;
using Reconnect.Utils;
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
        private static Vector2Int ShiftToDirection(Vector2Int sourcePoint, string direction, bool allowDiagonal = false)
        {
            return direction.ToLower().Trim() switch
            {
                "n"  => sourcePoint + new Vector2Int( 0, -1),
                "e"  => sourcePoint + new Vector2Int(+1,  0),
                "s"  => sourcePoint + new Vector2Int( 0, +1),
                "w"  => sourcePoint + new Vector2Int(-1,  0),
                "ne" when allowDiagonal => sourcePoint + new Vector2Int(+1, -1),
                "se" when allowDiagonal => sourcePoint + new Vector2Int(+1, +1),
                "sw" when allowDiagonal => sourcePoint + new Vector2Int(-1, +1),
                "nw" when allowDiagonal => sourcePoint + new Vector2Int(-1, -1),
                { } dir => throw new InvalidDataException(allowDiagonal
                    ? $"Invalid direction. Expected 'n', 'e', 's', 'w', 'ne', 'se', 'sw' or 'nw' but got {dir}."
                    : $"Invalid direction. Expected 'n', 'e', 's' or 'w' but got '{dir}'.")
            };
        }
        
        private static void SummonSwitchWire(GameObject wirePrefab, Breadboard breadboard, Vector3 localPos, Vector3 scale, Vector3 eulerAngle)
        {
            GameObject wire = UnityEngine.Object.Instantiate(wirePrefab, breadboard.switchHolder.transform, false);
            wire.transform.localPosition = localPos;
            wire.transform.localScale = scale;
            wire.transform.localEulerAngles = eulerAngle;
            if (!wire.TryGetComponent(out ComponentSync wireSync))
                throw new ComponentNotFoundException(
                    "The Switch wire prefab clone does not contain any ComponentSync script.");
            wireSync.breadboardNetIdentity = breadboard.netIdentity;
            NetworkServer.Spawn(wire);
        }
        private static void DrawSwitchWires(Breadboard breadboard)
        {
            GameObject wirePrefab = Resources.Load<GameObject>("Prefabs/Electronics/Components/SwitchWirePrefab");
            
            // wire input horizontal
            Vector3 inputPos = Breadboard.PointToLocalPos(breadboard.CircuitInfo.InputPoint);
            Vector3 inputOffsetPos = inputPos + new Vector3(0, 0.5f, 0);
            SummonSwitchWire(wirePrefab, breadboard,
                (inputOffsetPos + BreadboardHolder.SwitchWireInputCorner) / 2,
                new Vector3(1, (inputOffsetPos - BreadboardHolder.SwitchWireInputCorner).magnitude, 1),
                new Vector3(0, 0, 90));
            
            // wire input vertical
            Vector3 inputWireJoinPos = new Vector3(inputPos.x, BreadboardHolder.SwitchWireInputCorner.y, inputPos.z);
            SummonSwitchWire(wirePrefab, breadboard,
                (inputPos + inputWireJoinPos) / 2,
                new Vector3(1, (inputWireJoinPos - inputPos).magnitude, 1),
                Vector3.zero);
            
            // wire output horizontal
            Vector3 outputPos = Breadboard.PointToLocalPos(breadboard.CircuitInfo.OutputPoint) ;
            Vector3 outputOffsetPos = outputPos - new Vector3(0, 0.5f, 0);
            SummonSwitchWire(wirePrefab, breadboard,
                (outputOffsetPos + BreadboardHolder.SwitchWireOutputCorner) / 2,
                new Vector3(1, (outputOffsetPos - BreadboardHolder.SwitchWireOutputCorner).magnitude, 1),
                new Vector3(0, 0, 90));
            
            // wire input vertical
            Vector3 outputWireJoinPos = new Vector3(outputPos.x, BreadboardHolder.SwitchWireOutputCorner.y, outputPos.z);
            SummonSwitchWire(wirePrefab, breadboard,
                (outputPos + outputWireJoinPos) / 2,
                new Vector3(1, (outputWireJoinPos - outputPos).magnitude, 1),
                Vector3.zero);
        }

        /// <summary>
        /// Loads the given circuit. Cleans the breadboard and load all the components required for the circuit.
        /// </summary>
        /// <param name="breadboard">The breadboard on which the circuit must be loaded.</param>
        /// <param name="yamlAsset">The YAML Ressource of the ciruit to be loaded.</param>
        public static void LoadCircuit(Breadboard breadboard, TextAsset yamlAsset)
        {
            breadboard.Clean();
            
            //TextAsset yamlAsset = Resources.Load<TextAsset>($"CircuitsPresets/{circuitName}");

            using StringReader reader = new StringReader(yamlAsset.text);
            YamlStream yaml = new YamlStream();
            yaml.Load(reader);

            YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;

            breadboard.CircuitInfo = new CircuitInfo
            {
                Title = YamlGetScalarValue(root.Children, "title"),
                InputTension = float.Parse(YamlGetScalarValue(root.Children, "input-tension"), CultureInfo.InvariantCulture),
                InputIntensity = float.Parse(YamlGetScalarValue(root.Children, "input-intensity"), CultureInfo.InvariantCulture),
                TargetValue = float.Parse(YamlGetScalarValue(root.Children, "target-value"), CultureInfo.InvariantCulture),
                TargetQuantity = Enum.Parse<CircuitInfo.Quantity>(YamlGetScalarValue(root.Children, "target-quantity"), true),
                TargetTolerance = 0.05f, // default percentage value
                InputPoint = new Vector2Int(int.Parse(YamlGetScalarValue(root.Children, "input-x-pos"), CultureInfo.InvariantCulture), 0),
                OutputPoint = new Vector2Int(int.Parse(YamlGetScalarValue(root.Children, "output-x-pos"), CultureInfo.InvariantCulture), 7),
            };

            if (YamlTryGetScalarValue(root.Children, "target-tolerance", out string targetTolerance)
                && !float.TryParse(targetTolerance, NumberStyles.Float, CultureInfo.InvariantCulture, out breadboard.CircuitInfo.TargetTolerance))
                throw new InvalidDataException($"Yaml key 'tolerance' of the circuit should be a floating point number. Got {targetTolerance}.");
            
            DrawSwitchWires(breadboard);
            
            YamlSequenceNode componentsNode = (YamlSequenceNode)root.Children[new YamlScalarNode("components")];

            int componentId = 0;
            
            foreach (YamlNode item in componentsNode)
            {
                YamlMappingNode component = (YamlMappingNode)item;
                string type = ((YamlScalarNode)component.Children.First().Key).Value;
                    
                // Fields in common (for any type of component) 

                string name = $"{componentId}: ";
                int xPos = int.Parse(YamlGetScalarValue(component.Children, "x-pos"));
                int yPos = int.Parse(YamlGetScalarValue(component.Children, "y-pos"));
                string direction = YamlGetScalarValue(component.Children, "direction");
                bool isTarget = false;
                if (YamlTryGetScalarValue(component.Children, "target", out string targetValue)
                    && !bool.TryParse(targetValue, out isTarget))
                    throw new InvalidDataException($"Yaml key 'target' expect a boolean value but got {targetValue}.");
                bool isLocked = false;
                if (YamlTryGetScalarValue(component.Children, "locked", out string lockedValue)
                    && !bool.TryParse(lockedValue, out isLocked))
                    throw new InvalidDataException($"Yaml key 'locked' expect a boolean value but got {targetValue}.");
                
                Vector2Int sourcePoint = new Vector2Int(xPos, yPos);
                Vector2Int destinationPoint = ShiftToDirection(sourcePoint, direction, allowDiagonal: type == "wire");
                
                if (YamlTryGetScalarValue(component.Children , "name", out string nameValue))
                    name += $"{nameValue} ";
                name += $"{sourcePoint} <-> {destinationPoint}";

                // Component-specific fields
                
                if (type == "wire")
                {
                    breadboard.CreateWire(sourcePoint, destinationPoint, name, isLocked);
                    if (isTarget)
                        throw new InvalidDataException("Invalid target: a wire cannot be a circuit target.");
                }
                else if (type == "resistor")
                {
                    uint resistance = uint.Parse(YamlGetScalarValue(component.Children, "resistance"), CultureInfo.InvariantCulture);
                    float tolerance = 5f;
                    if (YamlTryGetScalarValue(component.Children, "tolerance", out string toleranceValue)
                        && !float.TryParse(toleranceValue, NumberStyles.Float, CultureInfo.InvariantCulture, out tolerance))
                        throw new InvalidDataException($"Yaml key 'tolerance' expect a floating point numeric value but got {toleranceValue}.");
                    
                    int resistorID = breadboard.CreateResistor(sourcePoint, destinationPoint, name, resistance, tolerance, isLocked);
                    if (isTarget) 
                    {
                        if (breadboard.TargetID != -1)
                            throw new InvalidDataException("Multiple targets found. A circuit should have only one target.");
                        breadboard.TargetID = resistorID;
                    }
                }
                else if (type == "lamp")
                {
                    uint resistance = uint.Parse(YamlGetScalarValue(component.Children, "resistance"), CultureInfo.InvariantCulture);
                    
                    int lampID = breadboard.CreateLamp(sourcePoint, destinationPoint, name, resistance, isLocked);
                    if (isTarget) 
                    {
                        if (breadboard.TargetID != -1)
                            throw new InvalidDataException("Multiple targets found. A circuit should have only one target.");
                        breadboard.TargetID = lampID;
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid component type. Expected wire, resistor or lamp but got {type}.");
                }

                componentId++;
            }

            if (breadboard.TargetID == -1)
                throw new FormatException("The loaded circuit does not contain any target.");
        }
    }
}