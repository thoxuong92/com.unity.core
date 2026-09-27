using System;
using UnityEngine;

namespace Unity.Core.Attributes
{
    public enum Size
    {
        small,
        medium,
        big,
        max
    }

    public enum BaseColor
    {
        White,
        Black,
        Yellow,
        Red,
        Blue,
        Green
    }

    public enum QuationCircle
    {
        XZ,
        XY,
        YZ
    }

    public enum FixedColor
    {
        BabyBlue,
        Black,
        Blue,
        CherryRed,
        CloudWhite,
        Cyan,
        DarkGray,
        DustyBlue,
        Gray,
        Green,
        IceWhite,
        Magenta,
        Orange,
        PressedBlue,
        Purple,
        Red,
        Yellow,
        Transparent
    }

    public enum LabelStyle
    {
        NoLabel,
        EmptyLabel
    }

    public enum MessageBoxType
    {
        None,
        Info,
        Warning,
        Error
    }

    public enum BoolOperator
    {
        And,
        Or
    }

    public enum ComparisonOp
    {
        Equals,
        NotNull,
        Null
    }

    public enum EButtonEnableMode
    {
        Always,
        Editor,
        Playmode
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public static class AttributeStyleExtensions
    {
        public static Color ToColor(this FixedColor color)
        {
            return color switch
            {
                FixedColor.CloudWhite => new Color(0.93f, 0.93f, 0.93f, 1f),
                FixedColor.IceWhite => Color.white,
                FixedColor.Black => Color.black,
                FixedColor.Gray => Color.gray,
                FixedColor.DarkGray => new Color(0.1f, 0.1f, 0.1f, 1f),
                FixedColor.Blue => Color.blue,
                FixedColor.PressedBlue => new Color(0.27f, 0.38f, 0.49f, 1f),
                FixedColor.BabyBlue => new Color(0.73f, 0.89f, 0.96f, 1f),
                FixedColor.DustyBlue => new Color(0.31f, 0.4f, 0.5f, 1f),
                FixedColor.Purple => new Color(0.44f, 0.13f, 0.51f, 1f),
                FixedColor.Red => Color.red,
                FixedColor.CherryRed => new Color(0.8f, 0f, 0.1f, 1f),
                FixedColor.Orange => new Color(0.95f, 0.55f, 0.09f, 1f),
                FixedColor.Cyan => Color.cyan,
                FixedColor.Green => Color.green,
                FixedColor.Magenta => Color.magenta,
                FixedColor.Yellow => Color.yellow,
                FixedColor.Transparent => new Color(0f, 0f, 0f, 0f),
                _ => Color.white
            };
        }

        public static Color ToColor(this BaseColor color)
        {
            return color switch
            {
                BaseColor.White => Color.white,
                BaseColor.Black => Color.black,
                BaseColor.Yellow => Color.yellow,
                BaseColor.Red => Color.red,
                BaseColor.Blue => Color.blue,
                BaseColor.Green => Color.green,
                _ => Color.white
            };
        }
    }
}
