using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Core.FSM.Samples;
using Unity.Core.FSM.Visual;

namespace Unity.Core.Editor.FSM
{
    /// <summary>
    /// Professional Unity Animator-Style Visual State Machine Graph Editor.
    /// Manages Pure C# Visual State Nodes serialized directly inside AIBehaviour via [SerializeReference].
    /// Features Multi-Condition support (AND / OR logic), interactive Condition Badges on arrows,
    /// in-graph Transition Inspector sidebar, and smooth mouse wheel zoom.
    /// </summary>
    public class FSMGraphEditorWindow : EditorWindow
    {
        private AIBehaviour _targetRunner;
        private Vector2 _panOffset = new Vector2(60, 60);
        private float _zoom = 1.0f;
        private Vector2 _dragStartPos;
        private bool _isPanning;

        private VisualStateNode _selectedNode;
        private VisualStateNode _draggingNode;
        private Vector2 _nodeDragStartGraphPos;
        private Vector2 _mouseDragStartScreenPos;

        private VisualStateNode _transitionSourceNode;
        private bool _isAnyStateSource;
        private bool _isMakingTransition;
        private int _selectedTransitionIndex = -1;
        private Vector2 _inspectorScrollPos;

        private const float NodeWidth = 145f;
        private const float NodeHeight = 42f;

        private static readonly Color StartNodeColor = new Color(0.12f, 0.55f, 0.15f);
        private static readonly Color AnyNodeColor = new Color(0.0f, 0.45f, 0.50f);
        private static readonly Color InitialDefaultStateColor = new Color(0.85f, 0.52f, 0.12f);
        private static readonly Color DefaultStateColor = new Color(0.38f, 0.38f, 0.40f);
        private static readonly Color ActiveStateColor = new Color(0.0f, 0.85f, 0.35f);
        private static readonly Color SelectedStateColor = new Color(0.2f, 0.6f, 1.0f);

        // Cached transition badge rects for hit-testing
        private readonly List<Rect> _transitionBadgeRects = new List<Rect>();

        [MenuItem("Unity Core/FSM Visual Node Graph", false, 10)]
        public static void OpenWindow()
        {
            var window = GetWindow<FSMGraphEditorWindow>("Animator FSM");
            window.minSize = new Vector2(750, 480);
            window.Show();
        }

        public static void OpenForRunner(AIBehaviour runner)
        {
            var window = GetWindow<FSMGraphEditorWindow>("Animator FSM");
            window._targetRunner = runner;
            window.minSize = new Vector2(750, 480);
            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject != null)
            {
                var runner = Selection.activeGameObject.GetComponent<AIBehaviour>();
                if (runner != null)
                {
                    _targetRunner = runner;
                    _selectedTransitionIndex = -1;
                    _selectedNode = null;
                    Repaint();
                }
            }
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_targetRunner == null)
            {
                DrawEmptyNotice();
                return;
            }

            DrawGrid(20 * _zoom, 0.12f, Color.gray);
            DrawGrid(100 * _zoom, 0.25f, Color.gray);

            DrawSpecialNodesAndTransitions();
            DrawStateTransitions();
            DrawStateNodes();

            DrawActiveTransitionArrow();
            DrawTransitionInspectorPanel();

            HandleInputEvents(Event.current);

            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            string runnerName = _targetRunner != null ? _targetRunner.gameObject.name : "None (Select AI in Hierarchy)";
            string folder = _targetRunner != null ? FSMEditorUtility.GetAIBehaviourScriptDirectory(_targetRunner) : "";
            string folderName = !string.IsNullOrEmpty(folder) ? Path.GetFileName(folder) : "";

            string title = !string.IsNullOrEmpty(folderName)
                ? $"<b>Animator FSM:</b> {runnerName} <color=#88ccff>[📁 {folderName}]</color>"
                : $"<b>Animator FSM:</b> {runnerName}";
            GUILayout.Label(title, new GUIStyle(EditorStyles.label) { richText = true });

            if (_isMakingTransition)
            {
                GUILayout.Space(10);
                string fromName = _isAnyStateSource ? "Any State" : (_transitionSourceNode != null ? _transitionSourceNode.StateName : "");
                GUILayout.Label($"➔ Chọn Node đích để kết nối từ: <b>[{fromName}]</b> (Bấm ESC để hủy)", new GUIStyle(EditorStyles.helpBox) { richText = true });
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label($"Zoom: {Mathf.RoundToInt(_zoom * 100)}%", EditorStyles.miniLabel);

            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton))
            {
                _zoom = 1.0f;
                _panOffset = new Vector2(60, 60);
            }

            if (_targetRunner != null)
            {
                if (!string.IsNullOrEmpty(folder) && GUILayout.Button("+ New State Script", EditorStyles.toolbarButton))
                {
                    FSMScriptCreator.CreateStateScriptInDirectory(folder, "NewCustomState.cs");
                }

                if (_targetRunner is EnemyAIBehaviour enemy)
                {
                    if (GUILayout.Button("⚡ Setup Enemy Preset", EditorStyles.toolbarButton))
                    {
                        SetupEnemyPreset(enemy);
                    }
                }
                else
                {
                    if (GUILayout.Button("⚡ Auto-Setup Folder States", EditorStyles.toolbarButton))
                    {
                        SetupAllStatesInFolder();
                    }
                }

                if (GUILayout.Button("+ Auto Layout", EditorStyles.toolbarButton))
                {
                    AutoLayoutNodes();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyNotice()
        {
            EditorGUILayout.Space(50);
            EditorGUILayout.HelpBox("Chưa chọn Actor có AIBehaviour. Hãy chọn GameObject có gắn component AIBehaviour / EnemyAIBehaviour trong Hierarchy.", MessageType.Info);
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            if (gridSpacing <= 2f) return;

            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 offset = new Vector3(_panOffset.x % gridSpacing, _panOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + offset, new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + offset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + offset, new Vector3(position.width + gridSpacing, gridSpacing * j, 0f) + offset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        #region Coordinate Conversion

        private Vector2 GraphToScreen(Vector2 graphPos) => (graphPos * _zoom) + _panOffset;
        private Vector2 ScreenToGraph(Vector2 screenPos) => (screenPos - _panOffset) / _zoom;

        private Rect GetStartNodeScreenRect() => new Rect(GraphToScreen(new Vector2(30, 80)), new Vector2(110 * _zoom, NodeHeight * _zoom));
        private Rect GetAnyNodeScreenRect() => new Rect(GraphToScreen(new Vector2(30, 240)), new Vector2(110 * _zoom, NodeHeight * _zoom));

        private Rect GetNodeScreenRect(VisualStateNode state)
        {
            if (state == null) return Rect.zero;
            Vector2 screenPos = GraphToScreen(state.NodePosition);
            return new Rect(screenPos.x, screenPos.y, NodeWidth * _zoom, NodeHeight * _zoom);
        }

        #endregion

        #region Node & Transition Rendering

        private void DrawSpecialNodesAndTransitions()
        {
            // 1. Start Node
            Rect startRect = GetStartNodeScreenRect();
            DrawPillNode(startRect, "Start", StartNodeColor, false);

            var defaultState = _targetRunner != null ? _targetRunner.InitialState : null;
            if (defaultState != null)
            {
                Rect targetRect = GetNodeScreenRect(defaultState);
                DrawSolidArrowBetweenRects(startRect, targetRect, new Color(0.3f, 0.9f, 0.3f), 0f, null, -1);
            }

            // 2. Any Node
            Rect anyRect = GetAnyNodeScreenRect();
            DrawPillNode(anyRect, "Any", AnyNodeColor, false);
        }

        private void DrawStateNodes()
        {
            var states = _targetRunner.States;
            if (states == null) return;

            var defaultState = _targetRunner.InitialState;

            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state == null) continue;

                Rect rect = GetNodeScreenRect(state);
                bool isDefault = state == defaultState;
                bool isActive = Application.isPlaying && _targetRunner.CurrentVisualState == state;
                bool isSelected = _selectedNode == state;

                Color nodeColor = isDefault ? InitialDefaultStateColor : DefaultStateColor;
                if (isActive) nodeColor = ActiveStateColor;
                else if (isSelected) nodeColor = SelectedStateColor;

                DrawPillNode(rect, state.StateName, nodeColor, isSelected);
            }
        }

        private void DrawPillNode(Rect rect, string text, Color color, bool isSelected)
        {
            Handles.BeginGUI();

            // Background solid rect
            EditorGUI.DrawRect(rect, color);

            // Border line
            Handles.color = isSelected ? Color.white : new Color(0.18f, 0.18f, 0.18f, 0.9f);
            Vector3[] outline = new Vector3[]
            {
                new Vector3(rect.xMin, rect.yMin),
                new Vector3(rect.xMax, rect.yMin),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.xMin, rect.yMax),
                new Vector3(rect.xMin, rect.yMin)
            };
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, isSelected ? 2.5f : 1.5f, outline);

            Handles.EndGUI();

            // Label Text
            int fontSize = Mathf.RoundToInt(12 * Mathf.Clamp(_zoom, 0.7f, 1.4f));
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontSize = fontSize
            };
            GUI.Label(rect, text, labelStyle);
        }

        private void DrawStateTransitions()
        {
            _transitionBadgeRects.Clear();
            var transitions = _targetRunner.Transitions;
            if (transitions == null) return;

            for (int i = 0; i < transitions.Count; i++)
            {
                var t = transitions[i];
                if (t == null) continue;

                var toState = _targetRunner.GetState(t.ToState);
                if (toState == null) continue;

                var fromState = !string.IsNullOrEmpty(t.FromState) ? _targetRunner.GetState(t.FromState) : null;

                Rect startRect = fromState != null ? GetNodeScreenRect(fromState) : GetAnyNodeScreenRect();
                Rect endRect = GetNodeScreenRect(toState);

                bool isSelected = _selectedTransitionIndex == i;
                bool isRunning = Application.isPlaying && _targetRunner.CurrentVisualState == fromState;

                Color arrowColor = new Color(0.8f, 0.8f, 0.8f);
                if (isSelected) arrowColor = Color.cyan;
                else if (isRunning) arrowColor = ActiveStateColor;
                else if (fromState == null) arrowColor = new Color(0.3f, 0.85f, 0.9f);

                // Offset parallel reverse lines so they don't overlap
                float offset = HasReverseTransition(t) ? (8f * _zoom) : 0f;
                DrawSolidArrowBetweenRects(startRect, endRect, arrowColor, offset, t, i);
            }
        }

        private bool HasReverseTransition(VisualTransitionConfig transition)
        {
            if (string.IsNullOrEmpty(transition.FromState) || string.IsNullOrEmpty(transition.ToState)) return false;
            foreach (var t in _targetRunner.Transitions)
            {
                if (t != null && t.FromState.Equals(transition.ToState, StringComparison.OrdinalIgnoreCase) &&
                    t.ToState.Equals(transition.FromState, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void DrawSolidArrowBetweenRects(Rect startRect, Rect endRect, Color color, float perpendicularOffset, VisualTransitionConfig transition, int transitionIndex)
        {
            Vector2 startCenter = startRect.center;
            Vector2 endCenter = endRect.center;
            if (Vector2.Distance(startCenter, endCenter) < 2f) return;

            Vector2 dir = (endCenter - startCenter).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);

            Vector2 startEdge = GetRectEdgeIntersection(startRect, endCenter) + (normal * perpendicularOffset);
            Vector2 endEdge = GetRectEdgeIntersection(endRect, startCenter) + (normal * perpendicularOffset);

            Handles.BeginGUI();
            Handles.color = color;

            float lineWidth = (transitionIndex == _selectedTransitionIndex) ? 3.5f : 2.5f;
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, lineWidth, new Vector3[] { startEdge, endEdge });

            Vector2 mid = (startEdge + endEdge) * 0.5f;
            float arrowSize = 7f * Mathf.Clamp(_zoom, 0.8f, 1.3f);

            Vector2 tip = mid + dir * arrowSize;
            Vector2 left = mid - dir * arrowSize + normal * (arrowSize * 0.8f);
            Vector2 right = mid - dir * arrowSize - normal * (arrowSize * 0.8f);

            Handles.DrawAAConvexPolygon(tip, left, right);
            Handles.EndGUI();

            // Draw Condition Badge on the transition arrow
            if (transition != null && transitionIndex >= 0)
            {
                string summaryText = transition.GetSummary();
                int badgeFontSize = Mathf.RoundToInt(10 * Mathf.Clamp(_zoom, 0.7f, 1.2f));

                var badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = badgeFontSize,
                    normal = { textColor = transitionIndex == _selectedTransitionIndex ? Color.cyan : Color.white },
                    fontStyle = FontStyle.Bold
                };

                Vector2 textSize = badgeStyle.CalcSize(new GUIContent(summaryText));
                float badgePaddingX = 8f * _zoom;
                float badgePaddingY = 3f * _zoom;
                float badgeW = textSize.x + badgePaddingX * 2;
                float badgeH = textSize.y + badgePaddingY * 2;

                Vector2 badgeCenter = mid + (normal * (14f * _zoom));
                Rect badgeRect = new Rect(badgeCenter.x - badgeW * 0.5f, badgeCenter.y - badgeH * 0.5f, badgeW, badgeH);

                _transitionBadgeRects.Add(badgeRect);

                Handles.BeginGUI();
                // Badge background
                Color badgeBgColor = (transitionIndex == _selectedTransitionIndex)
                    ? new Color(0.1f, 0.35f, 0.5f, 0.95f)
                    : new Color(0.18f, 0.18f, 0.2f, 0.9f);
                EditorGUI.DrawRect(badgeRect, badgeBgColor);

                // Badge border
                Handles.color = (transitionIndex == _selectedTransitionIndex) ? Color.cyan : new Color(0.4f, 0.4f, 0.45f, 0.8f);
                Vector3[] badgeOutline = new Vector3[]
                {
                    new Vector3(badgeRect.xMin, badgeRect.yMin),
                    new Vector3(badgeRect.xMax, badgeRect.yMin),
                    new Vector3(badgeRect.xMax, badgeRect.yMax),
                    new Vector3(badgeRect.xMin, badgeRect.yMax),
                    new Vector3(badgeRect.xMin, badgeRect.yMin)
                };
                Handles.DrawAAPolyLine(Texture2D.whiteTexture, 1.2f, badgeOutline);
                Handles.EndGUI();

                GUI.Label(badgeRect, summaryText, badgeStyle);
            }
        }

        private Vector2 GetRectEdgeIntersection(Rect rect, Vector2 targetPos)
        {
            Vector2 center = rect.center;
            Vector2 dir = targetPos - center;
            if (dir.sqrMagnitude < 0.0001f) return center;

            float halfW = rect.width * 0.5f;
            float halfH = rect.height * 0.5f;

            float tX = dir.x != 0f ? Mathf.Abs(halfW / dir.x) : float.MaxValue;
            float tY = dir.y != 0f ? Mathf.Abs(halfH / dir.y) : float.MaxValue;

            float t = Mathf.Min(tX, tY);
            return center + dir * t;
        }

        private void DrawActiveTransitionArrow()
        {
            if (_isMakingTransition)
            {
                Rect startRect = _isAnyStateSource
                    ? GetAnyNodeScreenRect()
                    : (_transitionSourceNode != null ? GetNodeScreenRect(_transitionSourceNode) : new Rect(Event.current.mousePosition, Vector2.zero));

                Vector2 startCenter = startRect.center;
                Vector2 mousePos = Event.current.mousePosition;

                if (Vector2.Distance(startCenter, mousePos) > 5f)
                {
                    Vector2 dir = (mousePos - startCenter).normalized;
                    Vector2 normal = new Vector2(-dir.y, dir.x);
                    Vector2 startEdge = GetRectEdgeIntersection(startRect, mousePos);

                    Handles.BeginGUI();
                    Handles.color = Color.yellow;
                    Handles.DrawAAPolyLine(Texture2D.whiteTexture, 2.5f, new Vector3[] { startEdge, mousePos });

                    Vector2 mid = (startEdge + mousePos) * 0.5f;
                    float arrowSize = 7f;
                    Vector2 tip = mid + dir * arrowSize;
                    Vector2 left = mid - dir * arrowSize + normal * (arrowSize * 0.8f);
                    Vector2 right = mid - dir * arrowSize - normal * (arrowSize * 0.8f);
                    Handles.DrawAAConvexPolygon(tip, left, right);

                    Handles.EndGUI();
                }

                Repaint();
            }
        }

        #endregion

        #region Transition Inspector Sidebar (Multi-Condition Support)

        private void DrawTransitionInspectorPanel()
        {
            if (_selectedTransitionIndex < 0 || _targetRunner == null || _targetRunner.Transitions == null ||
                _selectedTransitionIndex >= _targetRunner.Transitions.Count)
            {
                return;
            }

            var transition = _targetRunner.Transitions[_selectedTransitionIndex];
            if (transition == null) return;

            float panelWidth = 320f;
            float panelHeight = Mathf.Min(420f, position.height - 60f);
            Rect panelRect = new Rect(position.width - panelWidth - 10, 40, panelWidth, panelHeight);

            // Dark semi-transparent background
            Handles.BeginGUI();
            EditorGUI.DrawRect(panelRect, new Color(0.14f, 0.14f, 0.16f, 0.97f));
            Handles.color = new Color(0.25f, 0.6f, 1.0f, 0.85f);
            Vector3[] outline = new Vector3[]
            {
                new Vector3(panelRect.xMin, panelRect.yMin),
                new Vector3(panelRect.xMax, panelRect.yMin),
                new Vector3(panelRect.xMax, panelRect.yMax),
                new Vector3(panelRect.xMin, panelRect.yMax),
                new Vector3(panelRect.xMin, panelRect.yMin)
            };
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 1.5f, outline);
            Handles.EndGUI();

            GUILayout.BeginArea(new Rect(panelRect.x + 8, panelRect.y + 8, panelRect.width - 16, panelRect.height - 16));

            // Header
            EditorGUILayout.BeginHorizontal();
            string from = string.IsNullOrEmpty(transition.FromState) ? "Any State" : transition.FromState;
            string to = string.IsNullOrEmpty(transition.ToState) ? "None" : transition.ToState;
            GUILayout.Label($"<b>➔ Transition: [{from}] ➔ [{to}]</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true, fontSize = 12 });
            if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                _selectedTransitionIndex = -1;
                Repaint();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            _inspectorScrollPos = EditorGUILayout.BeginScrollView(_inspectorScrollPos);

            var so = new SerializedObject(_targetRunner);
            var transProp = so.FindProperty("_transitions").GetArrayElementAtIndex(_selectedTransitionIndex);
            var logicModeProp = transProp.FindPropertyRelative("LogicMode");
            var conditionsArrayProp = transProp.FindPropertyRelative("Conditions");

            // Logic Mode Selector (AND / OR)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Chế độ kết hợp:", EditorStyles.boldLabel, GUILayout.Width(110));
            logicModeProp.enumValueIndex = (int)(ConditionLogicMode)EditorGUILayout.EnumPopup((ConditionLogicMode)logicModeProp.enumValueIndex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Conditions List Header + Add Button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"<b>Danh sách điều kiện ({conditionsArrayProp.arraySize}):</b>", new GUIStyle(EditorStyles.label) { richText = true });
            if (GUILayout.Button("+ Thêm Điều Kiện", EditorStyles.miniButton, GUILayout.Width(120)))
            {
                int newIdx = conditionsArrayProp.arraySize;
                conditionsArrayProp.InsertArrayElementAtIndex(newIdx);
                var newElem = conditionsArrayProp.GetArrayElementAtIndex(newIdx);
                newElem.FindPropertyRelative("ConditionType").enumValueIndex = (int)TransitionConditionType.TargetDetected;
                newElem.FindPropertyRelative("DurationThreshold").floatValue = 1.0f;
                newElem.FindPropertyRelative("NumericThreshold").floatValue = 5.0f;
                newElem.FindPropertyRelative("CustomConditionName").stringValue = string.Empty;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Render each Condition Item
            for (int c = 0; c < conditionsArrayProp.arraySize; c++)
            {
                var condElem = conditionsArrayProp.GetArrayElementAtIndex(c);
                var condTypeProp = condElem.FindPropertyRelative("ConditionType");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Row: Condition Header + Remove button
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"<b>Điều kiện #{c + 1}:</b>", new GUIStyle(EditorStyles.label) { richText = true });
                GUILayout.FlexibleSpace();
                if (conditionsArrayProp.arraySize > 1)
                {
                    var oldBgCol = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        conditionsArrayProp.DeleteArrayElementAtIndex(c);
                        break;
                    }
                    GUI.backgroundColor = oldBgCol;
                }
                EditorGUILayout.EndHorizontal();

                // Condition Type Dropdown
                condTypeProp.enumValueIndex = (int)(TransitionConditionType)EditorGUILayout.EnumPopup("Loại điều kiện:", (TransitionConditionType)condTypeProp.enumValueIndex);

                var curType = (TransitionConditionType)condTypeProp.enumValueIndex;

                // Parameter fields
                if (curType == TransitionConditionType.TimerExpired)
                {
                    var durProp = condElem.FindPropertyRelative("DurationThreshold");
                    durProp.floatValue = Mathf.Max(0.05f, EditorGUILayout.FloatField("Thời gian chờ (s):", durProp.floatValue));
                }
                else if (curType == TransitionConditionType.HealthBelowThreshold)
                {
                    var numProp = condElem.FindPropertyRelative("NumericThreshold");
                    numProp.floatValue = EditorGUILayout.FloatField("Ngưỡng máu (HP):", numProp.floatValue);
                }
                else if (curType == TransitionConditionType.DistanceLessThan || curType == TransitionConditionType.DistanceGreaterThan)
                {
                    var numProp = condElem.FindPropertyRelative("NumericThreshold");
                    numProp.floatValue = EditorGUILayout.FloatField("Khoảng cách (m):", numProp.floatValue);
                }
                else if (curType == TransitionConditionType.CustomCondition)
                {
                    var tagProp = condElem.FindPropertyRelative("CustomConditionName");
                    var available = FSMEditorUtility.GetAvailableCustomConditions(_targetRunner);
                    if (available.Count > 0)
                    {
                        var options = new List<string>();
                        int selectedIndex = -1;
                        for (int k = 0; k < available.Count; k++)
                        {
                            options.Add(available[k].DisplayName);
                            if (available[k].MethodName.Equals(tagProp.stringValue, StringComparison.OrdinalIgnoreCase) ||
                                available[k].DisplayName.Equals(tagProp.stringValue, StringComparison.OrdinalIgnoreCase))
                            {
                                selectedIndex = k;
                            }
                        }
                        options.Add("(Tự nhập tên khác...)");

                        if (selectedIndex == -1)
                        {
                            selectedIndex = available.Count;
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginChangeCheck();
                        int newSel = EditorGUILayout.Popup("Hàm điều kiện:", selectedIndex, options.ToArray());
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (newSel < available.Count)
                            {
                                tagProp.stringValue = available[newSel].MethodName;
                            }
                            else
                            {
                                tagProp.stringValue = string.Empty;
                            }
                        }

                        if (newSel < available.Count)
                        {
                            if (GUILayout.Button("✎ Edit Code", EditorStyles.miniButton, GUILayout.Width(75)))
                            {
                                FSMEditorUtility.OpenCustomConditionScript(_targetRunner, available[newSel].MethodName);
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (newSel >= available.Count)
                        {
                            tagProp.stringValue = EditorGUILayout.TextField("Tên hàm / Tag tự nhập:", tagProp.stringValue);
                        }
                    }
                    else
                    {
                        tagProp.stringValue = EditorGUILayout.TextField("Tên điều kiện (Method / Tag):", tagProp.stringValue);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (so.hasModifiedProperties)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(_targetRunner);
                Repaint();
            }

            EditorGUILayout.Space(4);

            // Detailed condition description
            EditorGUILayout.HelpBox(transition.GetDetailedDescription(), MessageType.Info);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);

            // Bottom Action Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⇄ Reverse", EditorStyles.miniButton))
            {
                if (!string.IsNullOrEmpty(transition.FromState) && !string.IsNullOrEmpty(transition.ToState))
                {
                    var fromNode = _targetRunner.GetState(transition.FromState);
                    var toNode = _targetRunner.GetState(transition.ToState);
                    if (fromNode != null && toNode != null)
                    {
                        AddTransitionSerialized(toNode, fromNode);
                    }
                }
            }

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑 Delete Transition", EditorStyles.miniButton))
            {
                DeleteTransitionAtIndex(_selectedTransitionIndex);
                _selectedTransitionIndex = -1;
                Repaint();
            }
            GUI.backgroundColor = oldBg;
            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        #endregion

        #region Input & Event Handling

        private void HandleInputEvents(Event e)
        {
            Vector2 mousePos = e.mousePosition;

            if (e.type == EventType.ScrollWheel)
            {
                float prevZoom = _zoom;
                _zoom = Mathf.Clamp(_zoom - e.delta.y * 0.04f, 0.4f, 2.0f);
                _panOffset = mousePos - (mousePos - _panOffset) * (_zoom / prevZoom);
                Repaint();
                e.Use();
                return;
            }

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0) // Left Click
                {
                    if (_isMakingTransition)
                    {
                        var clickedNode = GetNodeAtScreenPos(mousePos);
                        if (clickedNode != null)
                        {
                            CompleteTransition(clickedNode);
                        }
                        else
                        {
                            CancelMakingTransition();
                        }
                        e.Use();
                        return;
                    }

                    // Check if clicked node
                    var node = GetNodeAtScreenPos(mousePos);
                    if (node != null)
                    {
                        _selectedNode = node;
                        _draggingNode = node;
                        _nodeDragStartGraphPos = node.NodePosition;
                        _mouseDragStartScreenPos = mousePos;
                        _selectedTransitionIndex = -1;
                        e.Use();
                        return;
                    }

                    // Check if clicked Any Node
                    if (GetAnyNodeScreenRect().Contains(mousePos))
                    {
                        _selectedNode = null;
                        _selectedTransitionIndex = -1;
                        e.Use();
                        return;
                    }

                    // Check if clicked transition badge or arrow
                    int clickedTrans = GetTransitionAtScreenPos(mousePos);
                    if (clickedTrans >= 0)
                    {
                        _selectedTransitionIndex = clickedTrans;
                        _selectedNode = null;
                        Repaint();
                        e.Use();
                        return;
                    }

                    // Clicked empty background
                    _selectedNode = null;
                    _selectedTransitionIndex = -1;
                    Repaint();
                }
                else if (e.button == 1) // Right Click
                {
                    var node = GetNodeAtScreenPos(mousePos);
                    if (node != null)
                    {
                        ShowNodeContextMenu(node);
                    }
                    else if (GetAnyNodeScreenRect().Contains(mousePos))
                    {
                        ShowAnyNodeContextMenu();
                    }
                    else
                    {
                        int clickedTrans = GetTransitionAtScreenPos(mousePos);
                        if (clickedTrans >= 0)
                        {
                            _selectedTransitionIndex = clickedTrans;
                            ShowTransitionContextMenu(clickedTrans);
                        }
                        else
                        {
                            ShowCanvasContextMenu(mousePos);
                        }
                    }
                    e.Use();
                }
                else if (e.button == 2 || (e.button == 0 && e.alt)) // Middle Click or Alt+Drag Pan
                {
                    _isPanning = true;
                    _dragStartPos = mousePos;
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (_draggingNode != null)
                {
                    Undo.RecordObject(_targetRunner, "Move FSM Node");
                    Vector2 screenDiff = mousePos - _mouseDragStartScreenPos;
                    _draggingNode.NodePosition = _nodeDragStartGraphPos + (screenDiff / _zoom);
                    EditorUtility.SetDirty(_targetRunner);
                    Repaint();
                    e.Use();
                }
                else if (_isPanning)
                {
                    _panOffset += mousePos - _dragStartPos;
                    _dragStartPos = mousePos;
                    Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                _draggingNode = null;
                _isPanning = false;
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    CancelMakingTransition();
                    _selectedTransitionIndex = -1;
                    _selectedNode = null;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
                {
                    if (_selectedNode != null)
                    {
                        DeleteState(_selectedNode);
                        e.Use();
                    }
                    else if (_selectedTransitionIndex >= 0)
                    {
                        DeleteTransitionAtIndex(_selectedTransitionIndex);
                        _selectedTransitionIndex = -1;
                        e.Use();
                    }
                }
            }
        }

        private VisualStateNode GetNodeAtScreenPos(Vector2 screenPos)
        {
            if (_targetRunner == null || _targetRunner.States == null) return null;
            foreach (var state in _targetRunner.States)
            {
                if (state != null && GetNodeScreenRect(state).Contains(screenPos))
                {
                    return state;
                }
            }
            return null;
        }

        private int GetTransitionAtScreenPos(Vector2 screenPos)
        {
            if (_targetRunner == null || _targetRunner.Transitions == null) return -1;

            // 1. Check badge rects first (exact hit)
            for (int i = 0; i < _transitionBadgeRects.Count && i < _targetRunner.Transitions.Count; i++)
            {
                if (_transitionBadgeRects[i].Contains(screenPos))
                {
                    return i;
                }
            }

            // 2. Check distance to transition arrow line segment
            for (int i = 0; i < _targetRunner.Transitions.Count; i++)
            {
                var t = _targetRunner.Transitions[i];
                if (t == null) continue;

                var toState = _targetRunner.GetState(t.ToState);
                if (toState == null) continue;
                var fromState = !string.IsNullOrEmpty(t.FromState) ? _targetRunner.GetState(t.FromState) : null;

                Rect startRect = fromState != null ? GetNodeScreenRect(fromState) : GetAnyNodeScreenRect();
                Rect endRect = GetNodeScreenRect(toState);

                Vector2 p1 = GetRectEdgeIntersection(startRect, endRect.center);
                Vector2 p2 = GetRectEdgeIntersection(endRect, startRect.center);

                if (DistancePointToSegment(screenPos, p1, p2) < (10f * _zoom))
                {
                    return i;
                }
            }

            return -1;
        }

        private float DistancePointToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.0001f) return Vector2.Distance(point, a);

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq);
            Vector2 projection = a + t * ab;
            return Vector2.Distance(point, projection);
        }

        #endregion

        #region Context Menus

        private void ShowNodeContextMenu(VisualStateNode node)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Make Transition"), false, () =>
            {
                _transitionSourceNode = node;
                _isAnyStateSource = false;
                _isMakingTransition = true;
            });

            menu.AddItem(new GUIContent("Set as Layer Default State"), false, () =>
            {
                Undo.RecordObject(_targetRunner, "Set Default State");
                var so = new SerializedObject(_targetRunner);
                so.FindProperty("_initialStateName").stringValue = node.StateName;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(_targetRunner);
                Repaint();
            });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete State"), false, () =>
            {
                DeleteState(node);
            });

            menu.ShowAsContext();
        }

        private void ShowAnyNodeContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Make Transition (From Any State)"), false, () =>
            {
                _transitionSourceNode = null;
                _isAnyStateSource = true;
                _isMakingTransition = true;
            });
            menu.ShowAsContext();
        }

        private void ShowTransitionContextMenu(int transitionIndex)
        {
            if (transitionIndex < 0 || transitionIndex >= _targetRunner.Transitions.Count) return;
            var t = _targetRunner.Transitions[transitionIndex];
            if (t == null) return;

            GenericMenu menu = new GenericMenu();
            string from = string.IsNullOrEmpty(t.FromState) ? "Any State" : t.FromState;
            string to = string.IsNullOrEmpty(t.ToState) ? "None" : t.ToState;

            menu.AddDisabledItem(new GUIContent($"Transition: [{from}] ➔ [{to}]"));
            menu.AddSeparator("");

            // Quick condition switchers
            foreach (TransitionConditionType cond in Enum.GetValues(typeof(TransitionConditionType)))
            {
                TransitionConditionType c = cond;
                bool isCurrent = t.ConditionType == c;
                menu.AddItem(new GUIContent($"Set First Condition/{c}"), isCurrent, () =>
                {
                    Undo.RecordObject(_targetRunner, "Change Transition Condition");
                    var so = new SerializedObject(_targetRunner);
                    var elem = so.FindProperty("_transitions").GetArrayElementAtIndex(transitionIndex);
                    var condsArray = elem.FindPropertyRelative("Conditions");
                    if (condsArray.arraySize == 0) condsArray.InsertArrayElementAtIndex(0);
                    condsArray.GetArrayElementAtIndex(0).FindPropertyRelative("ConditionType").enumValueIndex = (int)c;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_targetRunner);
                    Repaint();
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete Transition"), false, () =>
            {
                DeleteTransitionAtIndex(transitionIndex);
                _selectedTransitionIndex = -1;
            });

            menu.ShowAsContext();
        }

        private void ShowCanvasContextMenu(Vector2 mouseScreenPos)
        {
            GenericMenu menu = new GenericMenu();
            Vector2 spawnGraphPos = ScreenToGraph(mouseScreenPos);

            if (_targetRunner != null)
            {
                string folder = FSMEditorUtility.GetAIBehaviourScriptDirectory(_targetRunner);
                var stateTypes = FSMEditorUtility.GetStatesInDirectory(folder);
                string folderName = !string.IsNullOrEmpty(folder) ? Path.GetFileName(folder) : "Assets";

                menu.AddDisabledItem(new GUIContent($"📁 Scoped Folder: {folderName}"));
                menu.AddSeparator("");

                if (stateTypes.Count > 0)
                {
                    foreach (var stateType in stateTypes)
                    {
                        Type currentType = stateType;
                        menu.AddItem(new GUIContent($"Add State/{currentType.Name}"), false, () => AddStateInstance(currentType, spawnGraphPos));
                    }
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Add State/[No States Found in Folder]"));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent($"✨ Create New State in '{folderName}'..."), false, () =>
                {
                    FSMScriptCreator.CreateStateScriptInDirectory(folder, "NewCustomState.cs");
                });

                if (stateTypes.Count > 0)
                {
                    menu.AddItem(new GUIContent("⚡ Auto-Setup All States in Folder"), false, SetupAllStatesInFolder);
                }
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Reset Canvas View"), false, () => { _panOffset = new Vector2(60, 60); _zoom = 1.0f; Repaint(); });
            if (_targetRunner != null && _targetRunner.States != null && _targetRunner.States.Count > 0)
            {
                menu.AddItem(new GUIContent("Auto Layout Nodes"), false, AutoLayoutNodes);
            }

            menu.ShowAsContext();
        }

        #endregion

        #region Logic Helpers

        private void CompleteTransition(VisualStateNode targetNode)
        {
            if (_isAnyStateSource)
            {
                AddTransitionSerialized(null, targetNode);
            }
            else if (_transitionSourceNode != null && _transitionSourceNode != targetNode)
            {
                AddTransitionSerialized(_transitionSourceNode, targetNode);
            }

            CancelMakingTransition();
        }

        private void CancelMakingTransition()
        {
            _isMakingTransition = false;
            _transitionSourceNode = null;
            _isAnyStateSource = false;
            Repaint();
        }

        private void AddTransitionSerialized(VisualStateNode from, VisualStateNode to)
        {
            if (_targetRunner == null || to == null) return;

            Undo.RecordObject(_targetRunner, "Add FSM Transition");

            var so = new SerializedObject(_targetRunner);
            var transProp = so.FindProperty("_transitions");
            int index = transProp.arraySize;
            transProp.InsertArrayElementAtIndex(index);

            var elem = transProp.GetArrayElementAtIndex(index);
            elem.FindPropertyRelative("FromState").stringValue = from != null ? from.StateName : string.Empty;
            elem.FindPropertyRelative("ToState").stringValue = to.StateName;
            elem.FindPropertyRelative("LogicMode").enumValueIndex = (int)ConditionLogicMode.All;

            var condsProp = elem.FindPropertyRelative("Conditions");
            condsProp.ClearArray();
            condsProp.InsertArrayElementAtIndex(0);
            var firstCond = condsProp.GetArrayElementAtIndex(0);
            firstCond.FindPropertyRelative("ConditionType").enumValueIndex = (int)TransitionConditionType.AlwaysTrue;
            firstCond.FindPropertyRelative("DurationThreshold").floatValue = 1.0f;
            firstCond.FindPropertyRelative("NumericThreshold").floatValue = 5.0f;
            firstCond.FindPropertyRelative("CustomConditionName").stringValue = string.Empty;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_targetRunner);

            _selectedTransitionIndex = index;
            Debug.Log($"[Animator FSM] Created transition: [{(from != null ? from.StateName : "Any")}] ──► [{to.StateName}]");
            Repaint();
        }

        private void DeleteTransitionAtIndex(int index)
        {
            if (_targetRunner == null || _targetRunner.Transitions == null || index < 0 || index >= _targetRunner.Transitions.Count) return;

            Undo.RecordObject(_targetRunner, "Delete Transition");
            var so = new SerializedObject(_targetRunner);
            var transProp = so.FindProperty("_transitions");
            transProp.DeleteArrayElementAtIndex(index);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_targetRunner);
            Repaint();
        }

        private void DeleteState(VisualStateNode node)
        {
            if (_targetRunner == null || node == null) return;

            Undo.RecordObject(_targetRunner, "Delete State Node");

            var so = new SerializedObject(_targetRunner);
            var transProp = so.FindProperty("_transitions");
            for (int i = transProp.arraySize - 1; i >= 0; i--)
            {
                var elem = transProp.GetArrayElementAtIndex(i);
                string from = elem.FindPropertyRelative("FromState").stringValue;
                string to = elem.FindPropertyRelative("ToState").stringValue;
                if (from.Equals(node.StateName, StringComparison.OrdinalIgnoreCase) ||
                    to.Equals(node.StateName, StringComparison.OrdinalIgnoreCase))
                {
                    transProp.DeleteArrayElementAtIndex(i);
                }
            }

            var statesProp = so.FindProperty("_states");
            for (int i = statesProp.arraySize - 1; i >= 0; i--)
            {
                var elem = statesProp.GetArrayElementAtIndex(i);
                if (elem.managedReferenceValue == node)
                {
                    statesProp.DeleteArrayElementAtIndex(i);
                }
            }

            var initialProp = so.FindProperty("_initialStateName");
            if (initialProp.stringValue.Equals(node.StateName, StringComparison.OrdinalIgnoreCase))
            {
                initialProp.stringValue = statesProp.arraySize > 0
                    ? (statesProp.GetArrayElementAtIndex(0).managedReferenceValue as VisualStateNode)?.StateName ?? string.Empty
                    : string.Empty;
            }

            so.ApplyModifiedProperties();
            _selectedNode = null;
            EditorUtility.SetDirty(_targetRunner);
            Repaint();
        }

        private void AddStateInstance(Type stateType, Vector2 position)
        {
            if (_targetRunner == null || stateType == null || !typeof(VisualStateNode).IsAssignableFrom(stateType)) return;

            Undo.RecordObject(_targetRunner, $"Add {stateType.Name} State");

            var stateInstance = Activator.CreateInstance(stateType) as VisualStateNode;
            if (stateInstance == null) return;

            stateInstance.NodePosition = position;
            stateInstance.StateName = stateType.Name;

            var so = new SerializedObject(_targetRunner);
            var statesProp = so.FindProperty("_states");
            int index = statesProp.arraySize;
            statesProp.InsertArrayElementAtIndex(index);
            statesProp.GetArrayElementAtIndex(index).managedReferenceValue = stateInstance;

            var initialProp = so.FindProperty("_initialStateName");
            if (string.IsNullOrEmpty(initialProp.stringValue))
            {
                initialProp.stringValue = stateInstance.StateName;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_targetRunner);
            Repaint();
        }

        private void SetupAllStatesInFolder()
        {
            if (_targetRunner == null) return;
            string folder = FSMEditorUtility.GetAIBehaviourScriptDirectory(_targetRunner);
            var stateTypes = FSMEditorUtility.GetStatesInDirectory(folder);
            if (stateTypes.Count == 0)
            {
                EditorUtility.DisplayDialog("FSM Setup", $"Không tìm thấy class State nào kế thừa VisualStateNode trong thư mục:\n{folder}", "OK");
                return;
            }

            Undo.RecordObject(_targetRunner, "Setup All States In Folder");

            var so = new SerializedObject(_targetRunner);
            var statesProp = so.FindProperty("_states");
            statesProp.ClearArray();

            string firstStateName = string.Empty;

            for (int i = 0; i < stateTypes.Count; i++)
            {
                var type = stateTypes[i];
                var stateInstance = Activator.CreateInstance(type) as VisualStateNode;
                if (stateInstance == null) continue;

                stateInstance.NodePosition = new Vector2(180 + (i % 3 * 200), 80 + (i / 3 * 120));
                stateInstance.StateName = type.Name;

                if (i == 0) firstStateName = stateInstance.StateName;

                statesProp.InsertArrayElementAtIndex(i);
                statesProp.GetArrayElementAtIndex(i).managedReferenceValue = stateInstance;
            }

            so.FindProperty("_initialStateName").stringValue = firstStateName;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_targetRunner);
            Repaint();
            Debug.Log($"[Animator FSM] Đã cấu hình thành công {stateTypes.Count} Pure C# States từ thư mục '{folder}' vào {_targetRunner.gameObject.name}!");
        }

        private void AutoLayoutNodes()
        {
            if (_targetRunner == null || _targetRunner.States == null) return;
            var states = _targetRunner.States;

            Undo.RecordObject(_targetRunner, "Auto Layout Nodes");
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] != null)
                {
                    states[i].NodePosition = new Vector2(200 + (i % 3 * 180), 80 + (i / 3 * 100));
                }
            }
            EditorUtility.SetDirty(_targetRunner);
            Repaint();
        }

        private void SetupEnemyPreset(EnemyAIBehaviour enemy)
        {
            Undo.RecordObject(enemy, "Setup Enemy Preset");

            var idle = new EnemyIdleState { NodePosition = new Vector2(180, 80), StateName = "Idle" };
            var move = new EnemyMoveState { NodePosition = new Vector2(380, 80), StateName = "Move" };
            var attack = new EnemyAttackState { NodePosition = new Vector2(580, 80), StateName = "Attack" };
            var dead = new EnemyDeadState { NodePosition = new Vector2(380, 240), StateName = "Dead" };

            var so = new SerializedObject(enemy);
            var statesProp = so.FindProperty("_states");
            statesProp.ClearArray();

            statesProp.InsertArrayElementAtIndex(0);
            statesProp.GetArrayElementAtIndex(0).managedReferenceValue = idle;

            statesProp.InsertArrayElementAtIndex(1);
            statesProp.GetArrayElementAtIndex(1).managedReferenceValue = move;

            statesProp.InsertArrayElementAtIndex(2);
            statesProp.GetArrayElementAtIndex(2).managedReferenceValue = attack;

            statesProp.InsertArrayElementAtIndex(3);
            statesProp.GetArrayElementAtIndex(3).managedReferenceValue = dead;

            so.FindProperty("_initialStateName").stringValue = "Idle";

            var transProp = so.FindProperty("_transitions");
            transProp.ClearArray();

            AddTransitionSerializedProperty(transProp, "Idle", "Move", TransitionConditionType.TargetDetected);
            AddTransitionSerializedProperty(transProp, "Move", "Attack", TransitionConditionType.InAttackRange);
            AddTransitionSerializedProperty(transProp, "Attack", "Move", TransitionConditionType.TargetDetected);
            AddTransitionSerializedProperty(transProp, "Move", "Idle", TransitionConditionType.TargetLost);
            AddTransitionSerializedProperty(transProp, string.Empty, "Dead", TransitionConditionType.HealthZero);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(enemy);
            Repaint();
            Debug.Log($"[Animator FSM] Đã thiết lập hoàn chỉnh Enemy AI Preset (4 States: Idle, Move, Attack, Dead) trên {enemy.gameObject.name}!");
        }

        private void AddTransitionSerializedProperty(SerializedProperty arrayProp, string fromState, string toState, TransitionConditionType condition)
        {
            int index = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(index);
            var elem = arrayProp.GetArrayElementAtIndex(index);
            elem.FindPropertyRelative("FromState").stringValue = fromState;
            elem.FindPropertyRelative("ToState").stringValue = toState;
            elem.FindPropertyRelative("LogicMode").enumValueIndex = (int)ConditionLogicMode.All;

            var condsProp = elem.FindPropertyRelative("Conditions");
            condsProp.ClearArray();
            condsProp.InsertArrayElementAtIndex(0);
            var condElem = condsProp.GetArrayElementAtIndex(0);
            condElem.FindPropertyRelative("ConditionType").enumValueIndex = (int)condition;
            condElem.FindPropertyRelative("DurationThreshold").floatValue = 1.0f;
            condElem.FindPropertyRelative("NumericThreshold").floatValue = 5.0f;
            condElem.FindPropertyRelative("CustomConditionName").stringValue = string.Empty;
        }

        #endregion
    }
}
