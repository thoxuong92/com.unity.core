using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

public class PlayerPrefEditorWindow : EditorWindow
{
    [MenuItem("Unity Core/Tools/PlayerPrefs Manager #P", false, 20)]
    static void Init()
    {
        var window = EditorWindow.GetWindow(typeof(PlayerPrefEditorWindow));
        window.titleContent = new GUIContent("PlayerPrefs Editor");
        window.minSize = new Vector2(800, 400);
    }

    private const string UNIQUE_STRING = "0987654321qwertyuiopasdfghjklzxcvbnm[];,.";
    private const int UNIQUE_INT = int.MinValue;
    private const float UNIQUE_FLOAT = Mathf.NegativeInfinity;

    private const string UNITY_GRAPHICS_QUALITY = "UnityGraphicsQuality";

    private const float UpdateIntervalInSeconds = 1.0F;

    private bool waitTillPlistHasBeenWritten = false;
    private FileInfo tmpPlistFile;

    private List<PlayerPrefsEntry> ppeList = new List<PlayerPrefsEntry>();
    private Vector2 scrollPos;
    private string newKey = "";
    private string newValueString = "";
    private int newValueInt = 0;
    private float newValueFloat = 0;
    private float rotation = 0;
    private ValueType selectedType = ValueType.String;

    private bool showNewEntryBox = false;
    private bool isOneSelected = false;
    private bool autoRefresh = false;
    private bool sortAscending = true;   //Ascending is A-Z or 1-2. Descending is Z-A or 2-1.

    private float oldTime = 0;

    private string _searchString = string.Empty;
    private SearchFilterType _searchFilter = SearchFilterType.All;

    private List<PlayerPrefsEntry> filteredPpeList = new List<PlayerPrefsEntry>();

    private Texture2D _refreshIcon;
    private Texture2D _deleteIcon;
    private Texture2D _addIcon;
    private Texture2D _undoIcon;
    private Texture2D _saveIcon;

    List<string> listIgnore = new List<string>() {
    "unity.cloud_userid",
    "unity.player_session_count",
    "unity.player_sessionid"
    };
    void OnEnable()
    {
        if (!IsUnityWritingToPlist())
            RefreshKeys();

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        waitTillPlistHasBeenWritten = IsUnityWritingToPlist();

        if (!waitTillPlistHasBeenWritten)
            RefreshKeys();
    }

    void Update()
    {
        //Auto refresh on Windows. On Mac this would be annoying because it takes longer so the user must manually refresh.
        if (autoRefresh && Application.platform == RuntimePlatform.WindowsEditor
            && EditorApplication.isPlaying)
        {
            float newtime = Mathf.Repeat(Time.timeSinceLevelLoad, UpdateIntervalInSeconds);
            if (newtime < oldTime)
                RefreshKeys();

            oldTime = newtime;
        }

        if (waitTillPlistHasBeenWritten)
        {
            if (new FileInfo(tmpPlistFile.FullName).Exists)
            {

            }
            else
            {
                RefreshKeys();
                waitTillPlistHasBeenWritten = false;
            }

            rotation += 0.05F;
            Repaint();
        }

        //Only enable certain options when atleast one is selected
        isOneSelected = false;
        foreach (PlayerPrefsEntry item in filteredPpeList)
        {
            if (item.IsSelected)
            {
                isOneSelected = true;
                break;
            }
        }
    }

    void OnGUI()
    {
        GUIStyle boldNumberFieldStyle = new GUIStyle(EditorStyles.numberField);
        boldNumberFieldStyle.font = EditorStyles.boldFont;

        GUIStyle boldToggleStyle = new GUIStyle(EditorStyles.toggle);
        boldToggleStyle.font = EditorStyles.boldFont;

        GUI.enabled = !waitTillPlistHasBeenWritten;

        //Toolbar
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            Rect optionsRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent("Sort   " + (sortAscending ? "?" : "?"), "Change sorting to " + (sortAscending ? "descending" : "ascending")), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                OnChangeSortModeClicked();
            }

            if (GUILayout.Button(new GUIContent("Options", "Contains additional functionality like \"Add new entry\" and \"Delete all entries\" "), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false)))
            {
                GenericMenu options = new GenericMenu();
                options.AddItem(new GUIContent("New Entry..."), false, OnNewEntryClicked);

                options.AddItem(new GUIContent("Delete Selected Entries"), false, OnDeleteSelectedClicked);
                options.AddItem(new GUIContent("Delete All Entries"), false, OnDeleteAllClicked);
                options.DropDown(optionsRect);
            }

            GUILayout.Space(5);

            //Searchfield
            Rect position = GUILayoutUtility.GetRect(50, 250, 10, 50, EditorStyles.toolbarTextField);
            position.width -= 16;
            position.x += 56;
            SearchString = GUI.TextField(position, SearchString, EditorStyles.toolbarTextField);

            position.x = position.x - 56;
            position.width = 50;
            if (GUI.Button(position, "Filter"))
            {
                GenericMenu options = new GenericMenu();
                options.AddItem(new GUIContent("All"), SearchFilter == SearchFilterType.All, OnSearchAllClicked);
                options.AddItem(new GUIContent("Key"), SearchFilter == SearchFilterType.Key, OnSearchKeyClicked);
                options.AddItem(new GUIContent("Value (Strings only)"), SearchFilter == SearchFilterType.Value, OnSearchValueClicked);
                options.DropDown(position);
            }

            position = GUILayoutUtility.GetRect(20, 20);
            position.x += 35;
            if (GUI.Button(position, "x"))
            {
                SearchString = string.Empty;
            }

            GUILayout.FlexibleSpace();

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string refreshTooltip = "Should all entries be automaticly refreshed every " + UpdateIntervalInSeconds + " seconds?";
                autoRefresh = GUILayout.Toggle(autoRefresh, new GUIContent("Auto Refresh ", refreshTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.MinWidth(75));
            }

            if (GUILayout.Button(new GUIContent("Refresh", "Force a refresh, could take a few seconds."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
            {
                if (Application.platform == RuntimePlatform.OSXEditor)
                    waitTillPlistHasBeenWritten = IsUnityWritingToPlist();

                RefreshKeys();
            }

        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = !waitTillPlistHasBeenWritten;

        //Show new entry box
        if (showNewEntryBox)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    newKey = EditorGUILayout.TextField("Key", newKey);

                    switch (selectedType)
                    {
                        default:
                        case ValueType.String:
                            newValueString = EditorGUILayout.TextField("Value", newValueString);
                            break;
                        case ValueType.Float:
                            newValueFloat = EditorGUILayout.FloatField("Value", newValueFloat);
                            break;
                        case ValueType.Integer:
                            newValueInt = EditorGUILayout.IntField("Value", newValueInt);
                            break;
                    }

                    selectedType = (ValueType)EditorGUILayout.EnumPopup("Type", selectedType);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Width(1));
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(new GUIContent("X", "Close"), EditorStyles.boldLabel, GUILayout.ExpandWidth(false)))
                        {
                            showNewEntryBox = false;
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button(new GUIContent("Add", "Add a new key-value.")))
                    {
                        if (!string.IsNullOrEmpty(newKey))
                        {
                            switch (selectedType)
                            {
                                case ValueType.Integer:
                                    PlayerPrefs.SetInt(newKey, newValueInt);
                                    ppeList.Add(new PlayerPrefsEntry(newKey, newValueInt));
                                    break;
                                case ValueType.Float:
                                    PlayerPrefs.SetFloat(newKey, newValueFloat);
                                    ppeList.Add(new PlayerPrefsEntry(newKey, newValueFloat));
                                    break;
                                default:
                                case ValueType.String:
                                    PlayerPrefs.SetString(newKey, newValueString);
                                    ppeList.Add(new PlayerPrefsEntry(newKey, newValueString));
                                    break;
                            }
                            PlayerPrefs.Save();
                            Sort();
                        }

                        newKey = newValueString = "";
                        newValueInt = 0;
                        newValueFloat = 0;
                        EditorGUIUtility.keyboardControl = 0;	//move focus from textfield, else the text won't be cleared
                        showNewEntryBox = false;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(2);

        GUI.backgroundColor = Color.white;

        EditorGUI.indentLevel++;

        //Show all PlayerPrefs
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        {
            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < filteredPpeList.Count; i++)
                {
                    if (filteredPpeList[i] == null)
                    {
                        continue;
                    }
                    if (filteredPpeList[i].Value != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            filteredPpeList[i].IsSelected = GUILayout.Toggle(filteredPpeList[i].IsSelected, new GUIContent("", "Toggle selection."), filteredPpeList[i].HasChanged ? boldToggleStyle : EditorStyles.toggle, GUILayout.MaxWidth(20), GUILayout.MinWidth(20), GUILayout.ExpandWidth(false));
                            filteredPpeList[i].Key = EditorGUILayout.TextField(filteredPpeList[i].Key, filteredPpeList[i].HasChanged ? boldNumberFieldStyle : EditorStyles.numberField, GUILayout.MaxWidth(250), GUILayout.MinWidth(100), GUILayout.ExpandWidth(false));

                            GUIStyle numberFieldStyle = filteredPpeList[i].HasChanged ? boldNumberFieldStyle : EditorStyles.numberField;
                            string _type = "?";
                            EditorGUIUtility.labelWidth = 80;
                            switch (filteredPpeList[i].Type)
                            {
                                default:
                                case ValueType.String:
                                    _type = "STRING";
                                    EditorGUILayout.LabelField(_type, GUILayout.ExpandWidth(false), GUILayout.Width(65));
                                    filteredPpeList[i].Value = EditorGUILayout.TextArea((string)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
                                    break;
                                case ValueType.Float:
                                    _type = "FLOAT";
                                    filteredPpeList[i].Value = EditorGUILayout.FloatField(_type, (float)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
                                    break;
                                case ValueType.Integer:
                                    _type = "INT";
                                    filteredPpeList[i].Value = EditorGUILayout.IntField(_type, (int)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
                                    break;
                            }

                            // GUILayout.Label(new GUIContent(_type, filteredPpeList[i].Type.ToString()), GUILayout.ExpandWidth(false), GUILayout.Width(100));

                            GUI.enabled = filteredPpeList[i].HasChanged && !waitTillPlistHasBeenWritten;
                            if (GUILayout.Button(new GUIContent("Save", "Save changes made to this value."), GUILayout.ExpandWidth(false)))
                            {
                                filteredPpeList[i].SaveChanges();
                            }

                            if (GUILayout.Button(new GUIContent("Undo", "Discard changes made to this value."), GUILayout.ExpandWidth(false)))
                            {
                                filteredPpeList[i].RevertChanges();
                            }

                            GUI.enabled = !waitTillPlistHasBeenWritten;

                            if (GUILayout.Button(new GUIContent("Delete", "Delete this key-value."), GUILayout.ExpandWidth(false)))
                            {
                                PlayerPrefs.DeleteKey(filteredPpeList[i].Key);
                                ppeList.Remove(filteredPpeList[i]);
                                PlayerPrefs.Save();

                                UpdateFilteredList();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        EditorGUI.indentLevel--;
    }

    #region Menu Actions

    private void OnChangeSortModeClicked()
    {
        sortAscending = !sortAscending;
        Sort();
    }

    private void OnNewEntryClicked()
    {
        showNewEntryBox = true;
    }

    private void OnDeleteSelectedClicked()
    {
        if (isOneSelected)
        {
            if (!waitTillPlistHasBeenWritten)
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to delete the selected keys? There is no undo!", "Delete", "Cancel"))
                {
                    int count = filteredPpeList.Count - 1;
                    for (int i = count; i >= 0; i--)
                    {
                        if (filteredPpeList[i].IsSelected)
                        {
                            PlayerPrefs.DeleteKey(filteredPpeList[i].Key);
                            ppeList.Remove(filteredPpeList[i]);
                        }
                    }

                    PlayerPrefs.Save();
                    UpdateFilteredList();
                }
            }
            else
                Debug.LogError("Cannot delete PlayerPrefs entries because it is still loading.");
        }
        else
            Debug.LogError("Cannot delete PlayerPrefs entries because no entries has been selected.");
    }

    private void OnDeleteAllClicked()
    {
        for (int i = 0; i < ppeList.Count; i++)
        {
            ppeList[i].IsSelected = true;
        }
        isOneSelected = true;

        OnDeleteSelectedClicked();
    }

    #endregion

    #region SearchMenu Actions

    private void OnSearchAllClicked()
    {
        SearchFilter = SearchFilterType.All;
    }

    private void OnSearchKeyClicked()
    {
        SearchFilter = SearchFilterType.Key;
    }

    private void OnSearchValueClicked()
    {
        SearchFilter = SearchFilterType.Value;
    }

    private void UpdateFilteredList()
    {
        filteredPpeList.Clear();

        if (!string.IsNullOrEmpty(SearchString))
        {
            for (int i = 0; i < ppeList.Count; i++)
            {
                if (SearchFilter == SearchFilterType.Key || SearchFilter == SearchFilterType.All)
                {
                    if (ppeList[i].Key.ToLowerInvariant().Contains(SearchString.Trim().ToLowerInvariant()))
                    {
                        filteredPpeList.Add(ppeList[i]);
                    }
                }

                //For the future: Figure out how to filter also on int/float and deal with dot '.' and comma ',' for different cultures
                if ((SearchFilter == SearchFilterType.Value || SearchFilter == SearchFilterType.All) && ppeList[i].Type == ValueType.String)
                {
                    if (!filteredPpeList.Contains(ppeList[i]))  //Prevent duplicates when 'All' is selected
                    {
                        if (((string)ppeList[i].Value).ToLowerInvariant().Contains(SearchString.Trim().ToLowerInvariant()))
                        {
                            filteredPpeList.Add(ppeList[i]);
                        }
                    }
                }
            }
        }
        else
        {
            filteredPpeList.AddRange(ppeList);
        }
    }

    #endregion

    private void Sort()
    {
        if (sortAscending)
            ppeList.Sort(PlayerPrefsEntry.SortByNameAscending);
        else
            ppeList.Sort(PlayerPrefsEntry.SortByNameDescending);

        UpdateFilteredList();
    }

    private void RefreshKeys()
    {
        ppeList.Clear();
        string[] allKeys = GetAllKeys();

        for (int i = 0; i < allKeys.Length; i++)
        {
            ppeList.Add(new PlayerPrefsEntry(allKeys[i]));
        }

        //Updating of filtered list happens in Sort()
        Sort();
        Repaint();
    }

    private string[] GetAllKeys()
    {
        List<string> result = new List<string>();

        if (Application.platform == RuntimePlatform.WindowsEditor)
            result.AddRange(GetAllWindowsKeys());
        else if (Application.platform == RuntimePlatform.OSXEditor)
            result.AddRange(GetAllMacKeys());
        else
        {
            Debug.LogError("Unsupported platform detected, please contact support@rejected-games.com and let us know.");
        }

        //Remove UnityGraphicsQuality, thats something Unity always saves in your PlayerPrefs, apparently
        if (result.Contains(UNITY_GRAPHICS_QUALITY))
            result.Remove(UNITY_GRAPHICS_QUALITY);

        return result.ToArray();
    }

    /// <summary>
    /// On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players. 
    /// </summary>
    private string[] GetAllMacKeys()
    {
        string plistPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/unity." + PlayerSettings.companyName + "." + PlayerSettings.productName + ".plist";
        string[] keys = new string[0];

        if (File.Exists(plistPath))
        {
            FileInfo fi = new FileInfo(plistPath);
            Dictionary<string, object> plist = (Dictionary<string, object>)PlistCS.Plist.ReadPlist(fi.FullName);

            keys = new string[plist.Count];
            plist.Keys.CopyTo(keys, 0);
        }

        return keys;
    }

    /// <summary>
    /// On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
    /// </summary>
    private string[] GetAllWindowsKeys()
    {
        List<string> result = new List<string>();
#if !NETSTANDARD2_1
        RegistryKey cuKey = Registry.CurrentUser;
        RegistryKey unityKey = cuKey.CreateSubKey(@"SOFTWARE\Unity\UnityEditor\" +
           PlayerSettings.companyName + @"\" + PlayerSettings.productName);

        string[] values = unityKey.GetValueNames();
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Substring(0, values[i].LastIndexOf("_"));
            if (!listIgnore.Contains(values[i]))
            {
                result.Add(values[i]);
            }
        }
#else
        Debug.LogError("Registry access is not supported in .NET Standard 2.1");
#endif

        return result.ToArray();
    }

    private bool IsUnityWritingToPlist()
    {
        bool result = false;

        //Mac specific, unfortunately no editor_mac preprocessor is available
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            //Find the tempPlistFile, while it exists we know Unity is still busy writen the last version of PlayerPrefs
            FileInfo plistFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/unity." + PlayerSettings.companyName + "." + PlayerSettings.productName + ".plist");
            DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/");
            FileInfo[] allPreferenceFiles = di.GetFiles();

            foreach (FileInfo fi in allPreferenceFiles)
            {
                if (fi.FullName.Contains(plistFile.FullName))
                {
                    if (!fi.FullName.EndsWith(".plist"))
                    {
                        tmpPlistFile = fi;
                        result = true;
                    }
                }
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
            result = false;

        return result;
    }

    private string SearchString
    {
        get { return _searchString; }
        set
        {
            if (_searchString != value)
            {
                _searchString = value;

                UpdateFilteredList();
            }
        }
    }

    private SearchFilterType SearchFilter
    {
        get { return _searchFilter; }
        set
        {
            if (_searchFilter != value)
            {
                _searchFilter = value;
                UpdateFilteredList();
            }
        }
    }

    private class PlayerPrefsEntry
    {
        private string key;
        private object value;

        public ValueType Type;
        public bool IsSelected = false;
        public bool HasChanged = false;

        private string oldKey;

        public PlayerPrefsEntry(string key)
        {
            this.key = key;
            oldKey = key;

            RetrieveValue();
        }

        public PlayerPrefsEntry(string key, string value)
        {
            this.key = key;
            this.value = value;
            this.Type = ValueType.String;
        }

        public PlayerPrefsEntry(string key, float value)
        {
            this.key = key;
            this.value = value;
            this.Type = ValueType.Float;
        }

        public PlayerPrefsEntry(string key, int value)
        {
            this.key = key;
            this.value = value;
            this.Type = ValueType.Integer;
        }

        public void SaveChanges()
        {
            switch (Type)
            {
                default:
                case ValueType.String:
                    PlayerPrefs.SetString(Key, (string)Value);
                    break;
                case ValueType.Float:
                    PlayerPrefs.SetFloat(Key, (float)Value);
                    break;
                case ValueType.Integer:
                    PlayerPrefs.SetInt(Key, (int)Value);
                    break;
            }

            if (oldKey != Key)
            {
                PlayerPrefs.DeleteKey(oldKey);
                oldKey = Key;
            }

            HasChanged = false;

            //Incase the user exits without saving project
            PlayerPrefs.Save();
        }

        public void RevertChanges()
        {
            RetrieveValue();
        }

        public void RetrieveValue()
        {
            Key = oldKey;

            if (PlayerPrefs.GetString(Key, UNIQUE_STRING) != UNIQUE_STRING)
            {
                Type = ValueType.String;
                value = PlayerPrefs.GetString(Key);
            }
            else if (PlayerPrefs.GetInt(Key, UNIQUE_INT) != UNIQUE_INT)
            {
                Type = ValueType.Integer;
                value = PlayerPrefs.GetInt(Key);
            }
            else if (PlayerPrefs.GetFloat(Key, UNIQUE_FLOAT) != UNIQUE_FLOAT)
            {
                Type = ValueType.Float;
                value = PlayerPrefs.GetFloat(Key);
            }

            oldKey = Key;

            //Don't mark the first set Value as changed
            HasChanged = false;
        }

        public string Key
        {
            get
            {
                return key;
            }

            set
            {
                if (value != key)
                {
                    HasChanged = true;
                    key = value;
                }
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (!value.Equals(this.value))
                {
                    this.value = value;

                    HasChanged = true;
                }
            }
        }

        public static int SortByNameAscending(PlayerPrefsEntry a, PlayerPrefsEntry b)
        {
            return string.Compare(a.Key, b.Key);
        }

        public static int SortByNameDescending(PlayerPrefsEntry a, PlayerPrefsEntry b)
        {
            return string.Compare(b.Key, a.Key);
        }
    }

    private enum ValueType
    {
        String,
        Float,
        Integer
    }

    private enum SearchFilterType
    {
        All,
        Key,
        Value
    }

    public static void Separator(Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        Rect lineRect = GUILayoutUtility.GetRect(10, 1);
        GUI.DrawTexture(new Rect(lineRect.x, lineRect.y, lineRect.width, 1), EditorGUIUtility.whiteTexture);
        GUI.color = old;
    }


    private GUIStyle GetStyle(string styleName)
    {
        GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        if (guiStyle == null)
        {
            Debug.LogError((object)("Missing built-in guistyle " + styleName));
            guiStyle = GUI.skin.button;
        }
        return guiStyle;
    }
}
