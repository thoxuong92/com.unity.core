using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Unity.Core.Editor.FSM
{
    /// <summary>
    /// Quick creation tools for FSM State (Pure C#) and AIBehaviour scripts via Project Window context menu (Assets/Create/FSM/...)
    /// and from the FSM Visual Graph Editor.
    /// </summary>
    public static class FSMScriptCreator
    {
        [MenuItem("Assets/Create/FSM/State", false, 80)]
        public static void CreateStateMenuItem()
        {
            string dir = FSMEditorUtility.GetActiveProjectDirectory();
            CreateStateScriptInDirectory(dir, "NewCustomState.cs");
        }

        [MenuItem("Assets/Create/FSM/AIBehaviour", false, 81)]
        public static void CreateAIBehaviourMenuItem()
        {
            string dir = FSMEditorUtility.GetActiveProjectDirectory();
            CreateAIBehaviourScriptInDirectory(dir, "NewCustomAIBehaviour.cs");
        }

        [MenuItem("Assets/Create/FSM/Pure C# State (Non-Visual)", false, 82)]
        public static void CreatePureCSharpStateMenuItem()
        {
            string dir = FSMEditorUtility.GetActiveProjectDirectory();
            CreatePureCSharpStateScriptInDirectory(dir, "NewBaseState.cs");
        }

        /// <summary>
        /// Starts interactive script creation for a Pure C# VisualStateNode in the specified folder.
        /// </summary>
        public static void CreateStateScriptInDirectory(string targetDirectory, string defaultName = "NewCustomState.cs")
        {
            if (string.IsNullOrEmpty(targetDirectory)) targetDirectory = "Assets";
            string path = Path.Combine(targetDirectory, defaultName).Replace("\\", "/");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            var endAction = ScriptableObject.CreateInstance<DoCreateFSMScriptAsset>();

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                endAction,
                path,
                icon,
                GetStateTemplate()
            );
        }

        /// <summary>
        /// Starts interactive script creation for an AIBehaviour in the specified folder.
        /// </summary>
        public static void CreateAIBehaviourScriptInDirectory(string targetDirectory, string defaultName = "NewCustomAIBehaviour.cs")
        {
            if (string.IsNullOrEmpty(targetDirectory)) targetDirectory = "Assets";
            string path = Path.Combine(targetDirectory, defaultName).Replace("\\", "/");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            var endAction = ScriptableObject.CreateInstance<DoCreateFSMScriptAsset>();

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                endAction,
                path,
                icon,
                GetAIBehaviourTemplate()
            );
        }

        /// <summary>
        /// Starts interactive script creation for a Pure C# BaseState in the specified folder.
        /// </summary>
        public static void CreatePureCSharpStateScriptInDirectory(string targetDirectory, string defaultName = "NewBaseState.cs")
        {
            if (string.IsNullOrEmpty(targetDirectory)) targetDirectory = "Assets";
            string path = Path.Combine(targetDirectory, defaultName).Replace("\\", "/");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            var endAction = ScriptableObject.CreateInstance<DoCreateFSMScriptAsset>();

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                endAction,
                path,
                icon,
                GetPureCSharpStateTemplate()
            );
        }

        #region Script Templates

        public static string GetStateTemplate()
        {
            return @"using System;
using UnityEngine;
using Unity.Core.FSM;
using Unity.Core.FSM.Visual;

/// <summary>
/// Custom FSM State (Pure C# - Not a MonoBehaviour component).
/// Managed by AIBehaviour via [SerializeReference].
/// </summary>
[Serializable]
public class #SCRIPTNAME# : VisualStateNode
{
    public override void OnEnter(IState previousState)
    {
        base.OnEnter(previousState);
        // Code executed when entering this state
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        // Code executed every frame while active
    }

    public override void OnExit(IState nextState)
    {
        base.OnExit(nextState);
        // Code executed when exiting this state
    }
}
";
        }

        public static string GetAIBehaviourTemplate()
        {
            return @"using UnityEngine;
using Unity.Core.FSM.Visual;

/// <summary>
/// Custom AI Behaviour running an FSM without attaching state components to the GameObject.
/// </summary>
[SelectionBase]
public class #SCRIPTNAME# : AIBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        // Custom initialization
    }

    /// <summary>
    /// Custom condition evaluations for transitions configured in the Visual FSM Graph.
    /// </summary>
    protected override bool EvaluateCondition(VisualTransitionConfig transition)
    {
        // Example:
        // if (transition.ConditionType == TransitionConditionType.TargetDetected) return CheckTarget();

        return base.EvaluateCondition(transition);
    }
}
";
        }

        public static string GetPureCSharpStateTemplate()
        {
            return @"using UnityEngine;
using Unity.Core.FSM;

/// <summary>
/// Pure C# Finite State Machine state.
/// </summary>
public class #SCRIPTNAME# : BaseState
{
    public override void OnEnter(IState previousState)
    {
        base.OnEnter(previousState);
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
    }

    public override void OnExit(IState nextState)
    {
        base.OnExit(nextState);
    }
}
";
        }

        #endregion
    }

    /// <summary>
    /// EndNameEditAction that processes the chosen filename, replaces #SCRIPTNAME#, and imports the asset.
    /// </summary>
    internal class DoCreateFSMScriptAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            string className = System.Text.RegularExpressions.Regex.Replace(fileName, @"[^\w]", "");
            if (string.IsNullOrEmpty(className)) className = "NewScript";

            string content = resourceFile.Replace("#SCRIPTNAME#", className);

            string dir = Path.GetDirectoryName(pathName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(pathName, content, System.Text.Encoding.UTF8);
            AssetDatabase.ImportAsset(pathName, ImportAssetOptions.ForceUpdate);

            Object obj = AssetDatabase.LoadAssetAtPath<MonoScript>(pathName);
            if (obj != null)
            {
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }
        }
    }
}
