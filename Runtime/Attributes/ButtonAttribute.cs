using System;

namespace Unity.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : Attribute
    {
        public string Text { get; private set; }
        public EButtonEnableMode SelectedEnableMode { get; private set; }

        public ButtonAttribute(string text = null, EButtonEnableMode enableMode = EButtonEnableMode.Always)
        {
            Text = text;
            SelectedEnableMode = enableMode;
        }
    }
}
