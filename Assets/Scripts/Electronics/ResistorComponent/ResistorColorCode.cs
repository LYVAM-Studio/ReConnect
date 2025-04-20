using System.Collections.Generic;
using Reconnect.Electronics.ResistorComponent;
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
        public static readonly Dictionary<ResistorColor, Color> ColorValues = new Dictionary<ResistorColor, Color>
        {
            { ResistorColor.Gold,   new Color(211f / 255, 175f / 255, 55f / 255) },
            { ResistorColor.Silver, new Color(0.753f, 0.753f, 0.753f) },
            { ResistorColor.Brown,  new Color(0.545f, 0.271f, 0.075f) },
            { ResistorColor.Orange, new Color(1f, 0.647f, 0f) },
            { ResistorColor.Yellow, Color.yellow },
            { ResistorColor.Green,  Color.green },
            { ResistorColor.Blue,   Color.blue },
            { ResistorColor.Violet, new Color(0.502f, 0f, 0.502f) },
            { ResistorColor.Gray,   Color.gray },
            { ResistorColor.White,  Color.white },
            { ResistorColor.Black,  Color.black },
            { ResistorColor.Red,    Color.red }
        };

        public static readonly Dictionary<int, Color> DigitToColor = new Dictionary<int, Color>
        {
            { -2, ColorValues[ResistorColor.Silver] },
            { -1, ColorValues[ResistorColor.Gold] },
            {  0, ColorValues[ResistorColor.Black] },
            {  1, ColorValues[ResistorColor.Brown] },
            {  2, ColorValues[ResistorColor.Red] },
            {  3, ColorValues[ResistorColor.Orange] },
            {  4, ColorValues[ResistorColor.Yellow] },
            {  5, ColorValues[ResistorColor.Green] },
            {  6, ColorValues[ResistorColor.Blue] },
            {  7, ColorValues[ResistorColor.Violet] },
            {  8, ColorValues[ResistorColor.Gray] },
            {  9, ColorValues[ResistorColor.White] }
        };

        public static readonly Dictionary<float, Color> ToleranceToColor = new Dictionary<float, Color>
        {
            { 0.1f, ColorValues[ResistorColor.Violet] },
            { 0.5f, ColorValues[ResistorColor.Green] },
            { 1f,   ColorValues[ResistorColor.Brown] },
            { 5f,   ColorValues[ResistorColor.Gold] },
            { 10f,  ColorValues[ResistorColor.Silver] }
        };
    }

}