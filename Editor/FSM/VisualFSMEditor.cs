using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Core.FSM.Samples;
using Unity.Core.FSM.Visual;

namespace Unity.Core.Editor.FSM
{
    [CustomEditor(typeof(AIBehaviour), true)]
    public class VisualFSMEditor : UnityEditor.Editor
    {
        private AIBehaviour _runner;
        private bool _showTransitionRules = true;
        private bool _showAddTransitionSection = false;
        private int _newFromStateIndex = 0;
        private int _newToStateIndex = 0;
        private TransitionConditionType _newConditionType = TransitionConditionType.TargetDetected;
        private ConditionLogicMode _newLogicMode = ConditionLogicMode.All;

        private void OnEnable()
        {
            _runner = (AIBehaviour)target;
        }

        public override void OnInspectorGUI()
        {
            // Big Visual Graph Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.7f, 1.0f);
            if (GUILayout.Button("⚡ OPEN VISUAL FSM GRAPH EDITOR", GUILayout.Height(36)))
            {
                FSMGraphEditorWindow.OpenForRunner(_runner);
            }
            GUI.backgroundColor = oldColor;

            EditorGUILayout.Space(6);
            DrawLiveStateHeader();

            EditorGUILayout.Space(6);
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            DrawInteractiveTransitionList();

            EditorGUILayout.Space(10);
            DrawQuickSetupSection();

            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void DrawLiveStateHeader()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("FSM Ready. Chạy Play Mode để theo dõi trực tiếp Active State và thử nghiệm chuyển cảnh.", MessageType.Info);
                return;
            }

            var currentState = _runner.CurrentVisualState;
            string stateName = currentState != null ? currentState.StateName : "None";
            float stateTime = currentState != null ? currentState.StateTime : 0f;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var oldColor = GUI.color;
            GUI.color = new Color(0.2f, 0.9f, 0.3f);
            GUILayout.Label("● ACTIVE STATE:", EditorStyles.boldLabel, GUILayout.Width(110));
            GUI.color = oldColor;

            EditorGUILayout.LabelField($"{stateName} ({stateTime:F1}s)", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // Quick State Switchers
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Manual State Switch (Debug):", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            foreach (var state in _runner.States)
            {
                if (state == null) continue;
                bool isCurrent = state == currentState;
                if (GUILayout.Button(state.StateName, isCurrent ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                {
                    _runner.ForceTransition(state);
                }
            }
            EditorGUILayout.EndHorizontal();

            // Enemy Damage simulation if runner is an EnemyAIBehaviour
            if (_runner is EnemyAIBehaviour enemy)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Damage Enemy (-25 HP)", EditorStyles.miniButton))
                {
                    enemy.TakeDamage(25f);
                }
                if (GUILayout.Button("Kill Enemy (0 HP)", EditorStyles.miniButton))
                {
                    enemy.TakeDamage(enemy.CurrentHealth);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawInteractiveTransitionList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            int count = _runner.Transitions != null ? _runner.Transitions.Count : 0;

            _showTransitionRules = EditorGUILayout.Foldout(_showTransitionRules, $"Visual Transition Rules ({count})", true, EditorStyles.foldoutHeader);

            if (_showTransitionRules && GUILayout.Button("+ Thêm Transition", EditorStyles.miniButton, GUILayout.Width(130)))
            {
                _showAddTransitionSection = !_showAddTransitionSection;
            }
            EditorGUILayout.EndHorizontal();

            if (!_showTransitionRules)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            // Add new transition sub-panel
            if (_showAddTransitionSection)
            {
                DrawAddNewTransitionForm();
            }

            EditorGUILayout.Space(4);

            if (count == 0)
            {
                EditorGUILayout.HelpBox("Chưa có quy tắc chuyển cảnh nào. Hãy bấm '+ Thêm Transition' ở trên hoặc bấm '⚡ Auto-Setup' bên dưới.", MessageType.None);
            }
            else
            {
                var so = new SerializedObject(_runner);
                var transArrayProp = so.FindProperty("_transitions");

                for (int i = 0; i < transArrayProp.arraySize; i++)
                {
                    var elem = transArrayProp.GetArrayElementAtIndex(i);
                    var t = _runner.Transitions[i];
                    if (t == null) continue;

                    string from = string.IsNullOrEmpty(t.FromState) ? "Any State" : t.FromState;
                    string to = string.IsNullOrEmpty(t.ToState) ? "None" : t.ToState;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // Row 1: Source & Target + Delete button
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"<b>[{from}]</b> ➔ <b>[{to}]</b>", new GUIStyle(EditorStyles.label) { richText = true, fontSize = 11 });
                    GUILayout.FlexibleSpace();

                    var oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(22)))
                    {
                        Undo.RecordObject(_runner, "Delete Transition");
                        transArrayProp.DeleteArrayElementAtIndex(i);
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_runner);
                        GUIUtility.ExitGUI();
                        return;
                    }
                    GUI.backgroundColor = oldBg;
                    EditorGUILayout.EndHorizontal();

                    // Row 2: Logic Mode (AND / OR) + Add Condition Button
                    var logicModeProp = elem.FindPropertyRelative("LogicMode");
                    var condsArrayProp = elem.FindPropertyRelative("Conditions");

                    // Ensure at least 1 condition
                    if (condsArrayProp.arraySize == 0)
                    {
                        condsArrayProp.InsertArrayElementAtIndex(0);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Chế độ kết hợp:", EditorStyles.miniBoldLabel, GUILayout.Width(95));
                    logicModeProp.enumValueIndex = (int)(ConditionLogicMode)EditorGUILayout.EnumPopup((ConditionLogicMode)logicModeProp.enumValueIndex, GUILayout.Width(110));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+ Thêm Điều Kiện", EditorStyles.miniButton))
                    {
                        int newIdx = condsArrayProp.arraySize;
                        condsArrayProp.InsertArrayElementAtIndex(newIdx);
                        var newElem = condsArrayProp.GetArrayElementAtIndex(newIdx);
                        newElem.FindPropertyRelative("ConditionType").enumValueIndex = (int)TransitionConditionType.TargetDetected;
                        newElem.FindPropertyRelative("DurationThreshold").floatValue = 1.0f;
                        newElem.FindPropertyRelative("NumericThreshold").floatValue = 5.0f;
                        newElem.FindPropertyRelative("CustomConditionName").stringValue = string.Empty;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(2);

                    // Render each Condition Item
                    for (int c = 0; c < condsArrayProp.arraySize; c++)
                    {
                        var condElem = condsArrayProp.GetArrayElementAtIndex(c);
                        var condTypeProp = condElem.FindPropertyRelative("ConditionType");

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.BeginHorizontal();
                        condTypeProp.enumValueIndex = (int)(TransitionConditionType)EditorGUILayout.EnumPopup($"Điều kiện #{c + 1}:", (TransitionConditionType)condTypeProp.enumValueIndex);

                        if (condsArrayProp.arraySize > 1)
                        {
                            var oldBg2 = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                            if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(20)))
                            {
                                condsArrayProp.DeleteArrayElementAtIndex(c);
                                break;
                            }
                            GUI.backgroundColor = oldBg2;
                        }
                        EditorGUILayout.EndHorizontal();

                        var curType = (TransitionConditionType)condTypeProp.enumValueIndex;

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
                            numProp.floatValue = EditorGUILayout.FloatField("Ngưỡng khoảng cách (m):", numProp.floatValue);
                        }
                        else if (curType == TransitionConditionType.CustomCondition)
                        {
                            var tagProp = condElem.FindPropertyRelative("CustomConditionName");
                            var available = FSMEditorUtility.GetAvailableCustomConditions(_runner);
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
                                    if (GUILayout.Button("✎ Edit Code", EditorStyles.miniButton, GUILayout.Width(80)))
                                    {
                                        FSMEditorUtility.OpenCustomConditionScript(_runner, available[newSel].MethodName);
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
                    }

                    // Detailed description
                    EditorGUILayout.LabelField($"ℹ {t.GetDetailedDescription()}", EditorStyles.wordWrappedMiniLabel);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }

                if (so.hasModifiedProperties)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_runner);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAddNewTransitionForm()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("<b>Tạo Mới Transition:</b>", new GUIStyle(EditorStyles.label) { richText = true });

            var stateNames = new List<string> { "(Any State)" };
            foreach (var s in _runner.States)
            {
                if (s != null) stateNames.Add(s.StateName);
            }

            var toStateNames = new List<string>();
            foreach (var s in _runner.States)
            {
                if (s != null) toStateNames.Add(s.StateName);
            }

            if (toStateNames.Count == 0)
            {
                EditorGUILayout.HelpBox("Cần ít nhất 1 State trong AIBehaviour trước khi tạo Transition.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            _newFromStateIndex = Mathf.Clamp(_newFromStateIndex, 0, stateNames.Count - 1);
            _newToStateIndex = Mathf.Clamp(_newToStateIndex, 0, toStateNames.Count - 1);

            _newFromStateIndex = EditorGUILayout.Popup("Từ State (From):", _newFromStateIndex, stateNames.ToArray());
            _newToStateIndex = EditorGUILayout.Popup("Sang State (To):", _newToStateIndex, toStateNames.ToArray());
            _newConditionType = (TransitionConditionType)EditorGUILayout.EnumPopup("Điều kiện ban đầu:", _newConditionType);
            _newLogicMode = (ConditionLogicMode)EditorGUILayout.EnumPopup("Chế độ kết hợp:", _newLogicMode);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Tạo Transition", GUILayout.Width(130)))
            {
                string from = _newFromStateIndex == 0 ? string.Empty : stateNames[_newFromStateIndex];
                string to = toStateNames[_newToStateIndex];

                Undo.RecordObject(_runner, "Add Transition");
                var so = new SerializedObject(_runner);
                var transProp = so.FindProperty("_transitions");
                int index = transProp.arraySize;
                transProp.InsertArrayElementAtIndex(index);

                var elem = transProp.GetArrayElementAtIndex(index);
                elem.FindPropertyRelative("FromState").stringValue = from;
                elem.FindPropertyRelative("ToState").stringValue = to;
                elem.FindPropertyRelative("LogicMode").enumValueIndex = (int)_newLogicMode;

                var condsProp = elem.FindPropertyRelative("Conditions");
                condsProp.ClearArray();
                condsProp.InsertArrayElementAtIndex(0);
                var condElem = condsProp.GetArrayElementAtIndex(0);
                condElem.FindPropertyRelative("ConditionType").enumValueIndex = (int)_newConditionType;
                condElem.FindPropertyRelative("DurationThreshold").floatValue = 1.0f;
                condElem.FindPropertyRelative("NumericThreshold").floatValue = 5.0f;
                condElem.FindPropertyRelative("CustomConditionName").stringValue = string.Empty;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(_runner);
                _showAddTransitionSection = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickSetupSection()
        {
            if (Application.isPlaying) return;

            string folder = FSMEditorUtility.GetAIBehaviourScriptDirectory(_runner);
            string folderName = !string.IsNullOrEmpty(folder) ? Path.GetFileName(folder) : "Assets";
            var statesInFolder = FSMEditorUtility.GetStatesInDirectory(folder);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Scoped Folder: {folderName} ({statesInFolder.Count} pure C# states found)", EditorStyles.boldLabel);

            if (_runner is EnemyAIBehaviour enemy)
            {
                EditorGUILayout.LabelField("Tự động cấu hình 4 State (Idle, Move, Attack, Dead) và toàn bộ chuyển cảnh mặc định (không gắn component rời rạc vào GameObject).", EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("Auto-Setup Enemy AI Preset", GUILayout.Height(30)))
                {
                    SetupEnemyPreset(enemy);
                }
            }
            else if (statesInFolder.Count > 0)
            {
                EditorGUILayout.LabelField($"Tự động khởi tạo {statesInFolder.Count} Pure C# State từ thư mục '{folderName}' vào AIBehaviour này.", EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button($"Auto-Setup {statesInFolder.Count} States from Folder", GUILayout.Height(30)))
                {
                    SetupStatesFromFolder(statesInFolder);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Chưa có class State nào trong thư mục '{folderName}'. Bạn có thể tạo State mới nhanh bên dưới.", EditorStyles.wordWrappedMiniLabel);
            }

            if (!string.IsNullOrEmpty(folder))
            {
                EditorGUILayout.Space(2);
                if (GUILayout.Button($"+ Create New State Script in '{folderName}'", EditorStyles.miniButton))
                {
                    FSMScriptCreator.CreateStateScriptInDirectory(folder, "NewCustomState.cs");
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void SetupStatesFromFolder(List<Type> stateTypes)
        {
            if (_runner == null || stateTypes == null || stateTypes.Count == 0) return;

            Undo.RecordObject(_runner, "Auto Setup States from Folder");

            var so = new SerializedObject(_runner);
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
            EditorUtility.SetDirty(_runner);

            Debug.Log($"[UnityCore FSM] Đã cấu hình {stateTypes.Count} Pure C# states từ thư mục vào {_runner.gameObject.name}!");
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

            AddTransitionSerialized(transProp, "Idle", "Move", TransitionConditionType.TargetDetected);
            AddTransitionSerialized(transProp, "Move", "Attack", TransitionConditionType.InAttackRange);
            AddTransitionSerialized(transProp, "Attack", "Move", TransitionConditionType.TargetDetected);
            AddTransitionSerialized(transProp, "Move", "Idle", TransitionConditionType.TargetLost);
            AddTransitionSerialized(transProp, string.Empty, "Dead", TransitionConditionType.HealthZero);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(enemy);

            Debug.Log("[UnityCore FSM] Đã cấu hình thành công Enemy AIBehaviour với 4 States (Idle, Move, Attack, Dead) và các chuyển cảnh mặc định!");
        }

        private void AddTransitionSerialized(SerializedProperty arrayProp, string fromState, string toState, TransitionConditionType condition)
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
    }
}
