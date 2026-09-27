using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.UI
{
    [CreateAssetMenu(fileName = "PanelConfig", menuName = "Unity Core/UI/Panel Config")]
    public class PanelConfig : ScriptableObject
    {
        public List<BaseUI> Panels = new List<BaseUI>();
    }
}
