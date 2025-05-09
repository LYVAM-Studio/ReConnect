using System;
using Reconnect.Utils;
using UnityEngine;

namespace Electronics.ResistorComponent
{
    public enum ResistorColor
    {
        Black,
        Brown,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Violet,
        Gray,
        White,
        Gold,
        Silver
    }

    public static class ResistorColorCode
    {
        public static Color GetValue(ResistorColor resistorColor)
            => resistorColor switch
            {
                ResistorColor.Gold => new Color(211f / 255, 175f / 255, 55f / 255),
                ResistorColor.Silver => new Color(0.753f, 0.753f, 0.753f),
                ResistorColor.Brown => new Color(0.545f, 0.271f, 0.075f),
                ResistorColor.Orange => new Color(1f, 0.647f, 0f),
                ResistorColor.Yellow => Color.yellow,
                ResistorColor.Green => Color.green,
                ResistorColor.Blue => Color.blue,
                ResistorColor.Violet => new Color(0.502f, 0f, 0.502f),
                ResistorColor.Gray => Color.gray,
                ResistorColor.White => Color.white,
                ResistorColor.Black => Color.black,
                ResistorColor.Red => Color.red,
                _ => throw new UnreachableCaseException("Impossible case")
            };

        public static Color DigitToColor(int digit)
            => digit switch
            {
                -2 => GetValue(ResistorColor.Silver),
                -1 => GetValue(ResistorColor.Gold),
                0 => GetValue(ResistorColor.Black),
                1 => GetValue(ResistorColor.Brown),
                2 => GetValue(ResistorColor.Red),
                3 => GetValue(ResistorColor.Orange),
                4 => GetValue(ResistorColor.Yellow),
                5 => GetValue(ResistorColor.Green),
                6 => GetValue(ResistorColor.Blue),
                7 => GetValue(ResistorColor.Violet),
                8 => GetValue(ResistorColor.Gray),
                9 => GetValue(ResistorColor.White),
                _ => throw new ArgumentOutOfRangeException(nameof(digit), digit,
                    "Cannot convert the given digit into color.")
            };

        public static Color ToleranceToColor(float tolerance)
            => tolerance switch
            {
                0.1f => GetValue(ResistorColor.Violet),
                0.5f => GetValue(ResistorColor.Green),
                1f => GetValue(ResistorColor.Brown),
                5f => GetValue(ResistorColor.Gold),
                10f => GetValue(ResistorColor.Silver),
                _ => throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance,
                    "Cannot convert the given tolerance into color.")
            };
    }
}