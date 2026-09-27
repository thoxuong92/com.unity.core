using System;
using UnityEngine;

namespace Unity.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class HorizontalLineAttribute : PropertyAttribute
    {
        public string Message { get; set; }
        public float Thickness { get; set; }
        public FixedColor Color { get; set; }
        public float Spacing { get; set; }
        public float GapSize { get; set; }

        public HorizontalLineAttribute(float thickness = 1f, FixedColor color = FixedColor.Gray, float spacing = 8f, float gapSize = 0f)
        {
            Thickness = thickness;
            Color = color;
            Spacing = spacing;
            GapSize = gapSize;
        }

        public HorizontalLineAttribute(string message, float thickness = 1f, FixedColor color = FixedColor.Orange, float spacing = 12f, float gapSize = 0f)
        {
            Message = message;
            Thickness = thickness;
            Color = color;
            Spacing = spacing;
            GapSize = gapSize;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ProgressAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }
        public string MethodName { get; }
        public bool UseSlider { get; set; }

        public ProgressAttribute(float min, float max, string methodName = null)
        {
            Min = min;
            Max = max;
            MethodName = methodName;
            UseSlider = false;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class TagAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class LayerAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SceneAttribute : PropertyAttribute
    {
        public bool UseFullPath { get; }

        public SceneAttribute(bool useFullPath = false)
        {
            UseFullPath = useFullPath;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ForceFillAttribute : PropertyAttribute
    {
        public string[] NotAllowed { get; }
        public string ErrorMessage { get; set; }
        public bool OnlyTestInPlayMode { get; set; }

        public ForceFillAttribute(params string[] notAllowed)
        {
            NotAllowed = notAllowed;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class PreviewAttribute : PropertyAttribute
    {
        public float ThumbnailSize { get; }

        public PreviewAttribute(Size size = Size.medium)
        {
            ThumbnailSize = size switch
            {
                Size.small => 32f,
                Size.medium => 64f,
                Size.big => 96f,
                Size.max => 128f,
                _ => 64f
            };
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class URLAttribute : PropertyAttribute
    {
        public string Link { get; }
        public string Label { get; set; }

        public URLAttribute(string link, string label = null)
        {
            Link = link;
            Label = label;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class StaticAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MessageBoxAttribute : PropertyAttribute
    {
        public string Content { get; }
        public MessageBoxType Type { get; }

        public MessageBoxAttribute(string content, MessageBoxType type = MessageBoxType.Info)
        {
            Content = content;
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class HideFieldAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DrawCircleAttribute : Attribute
    {
        public BaseColor Color { get; set; }
        public float Radius { get; set; } = 0.5f;
        public QuationCircle Rotation { get; set; } = QuationCircle.XZ;

        public DrawCircleAttribute(BaseColor color = BaseColor.Green, float radius = 0.5f, QuationCircle rotation = QuationCircle.XZ)
        {
            Color = color;
            Radius = radius;
            Rotation = rotation;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DrawCubeAttribute : Attribute
    {
        public BaseColor Color { get; set; }
        public float Size { get; set; } = 1f;

        public DrawCubeAttribute(BaseColor color = BaseColor.Yellow, float size = 1f)
        {
            Color = color;
            Size = size;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DrawLineAttribute : Attribute
    {
        public BaseColor Color { get; set; }
        public DrawLineAttribute(BaseColor color = BaseColor.Red)
        {
            Color = color;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DrawSphereAttribute : Attribute
    {
        public BaseColor Color { get; set; }
        public float Radius { get; set; }

        public DrawSphereAttribute(BaseColor color = BaseColor.Blue, float radius = 0.5f)
        {
            Color = color;
            Radius = radius;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SceneButtonAttribute : Attribute
    {
        public string Text { get; }

        public SceneButtonAttribute(string text = null)
        {
            Text = text;
        }
    }
}
