//2022/6/1
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.ComponentModel;
using Unity.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Unity
{

    /// <summary>
    /// Runtime│Project │ClassName                 │Git  │Location
    /// No           Yes         EditorMySettings          Yes   ProjectSettings/Packages/[PackageName]/Settings.json
    /// No           No          EditorMyUserSettings  No    UserSettings/Packages/[PackageName]/Settings.json
    /// Yes          Yes          MySettings                   Yes   Assets/Resources/ProjectSettings/Packages/[PackageName]/Settings.json
    /// Yes          No           MyUserSettings           No    [persistentDataPath]/UserSettings/Packages/[PackageName]/Settings.json
    /// </summary>
    public class SettingsProvider
    {
        private Type type;
        private string packageName;
        private SettingsScope scope;
        private object settings;
        private Encoding encoding;

        private string baseDir;

        public Func<object> OnFirstCreateInstance;
        public Action<object> OnLoadAfter;
        //热更
        public static LoaderDelegate Loader;
        public delegate void LoaderDelegate(string assetPath, ref byte[] data, ref bool handle);

        static FileSystemWatcher fsw;
        static DateTime lastWriteTime;

        public event PropertyChangedEventHandler PropertyChanged;

        private int _playingChanged;

        public SettingsProvider(Type type, SettingsScope scope, string baseDir = null)
            : this(type, type.Assembly.GetUnityPackageName(), scope, baseDir)
        {
        }

        public SettingsProvider(Type type, string packageName, SettingsScope scope, string baseDir = null)
        {
            this.packageName = packageName;
            this.type = type;
            this.scope = scope;
            this.baseDir = baseDir;
            this.FileName = type.Name + ".json";
        }

        public object Settings
        {
            get
            {
#if UNITY_EDITOR
                if (_playingChanged != EditorSettingsProvider.playingChanged)
                {
                    _playingChanged = EditorSettingsProvider.playingChanged;
                    settings = default;
                }
#endif

                if (settings == null)
                {
                    Load();
                }
                return settings;
            }
        }

        public string PackageName => packageName;

        public bool IsRuntime => scope == SettingsScope.ProjectRuntime || scope == SettingsScope.UserRuntime;

        public bool IsProject => scope == SettingsScope.ProjectEditor || scope == SettingsScope.ProjectRuntime;

        public Encoding Encoding { get => encoding ?? new UTF8Encoding(false); set => encoding = value; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public bool PrettyPrint { get; set; } = true;



        public string GetFilePath()
        {
            if (!string.IsNullOrEmpty(FilePath))
                return FilePath;
            string filePath;
            filePath = string.Empty;
            if (!string.IsNullOrEmpty(baseDir))
            {
                filePath = baseDir;
            }
            else
            {
                if (IsRuntime)
                {
                    if (IsProject)
                        filePath = "Assets/Resources";
                    else
                        filePath = Application.persistentDataPath;
                }
            }


            if (IsProject)
            {
                filePath = Path.Combine(filePath, "ProjectSettings");
            }
            else
            {
                filePath = Path.Combine(filePath, "UserSettings");
            }
            filePath = Path.Combine(filePath, "Packages", packageName);

            if (!string.IsNullOrEmpty(FileName))
                filePath = Path.Combine(filePath, FileName);
            else
                filePath = Path.Combine(filePath, "Settings.json");
            filePath = filePath.Replace('\\', '/');
            return filePath;
        }

        bool GetResourcesPath(out string resPath)
        {
            string filePath = GetFilePath();
            if (filePath.StartsWith("Assets/Resources/"))
            {
                filePath = filePath.Substring("Assets/Resources/".Length);
                filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
                filePath = filePath.Replace('\\', '/');
                resPath = filePath;
                return true;
            }
            resPath = null;
            return false;
        }


        public bool Set<TValue>(string propertyName, ref TValue value, TValue newValue)
        {
            Type type = typeof(TValue);
            bool changed = false;
            if (type.IsArray || typeof(IList).IsAssignableFrom(type))
            {
                changed = !EqualElements(value as IList, newValue as IList);
            }
            else if (!object.Equals(value, newValue))
            {
                changed = true;
            }
            if (changed)
            {
                value = newValue;
                if (!Application.isPlaying)
                    Save();
                PropertyChanged?.Invoke(settings, new PropertyChangedEventArgs(propertyName));
            }
            return changed;
        }

        bool EqualElements(IList a, IList b)
        {
            if (object.Equals(a, b) || object.ReferenceEquals(a, b))
                return true;
            if (a == null)
            {
                if (b != null)
                    return false;
                if (b == null)
                    return true;
            }
            else
            {
                if (b == null)
                    return false;
            }
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!object.Equals(a[i], b[i]))
                    return false;
            }
            return true;
        }

        public void Load()
        {
            object oldSettings = this.settings;
            string filePath = null;
            try
            {
#if UNITY_EDITOR
                _playingChanged = EditorSettingsProvider.playingChanged;
#endif
                string json = null;
                //if(Application.isPlaying)
                //{
                //    if (!IsRuntime)
                //        throw new Exception($"can't  playing load runtime settings [{type.FullName}]");
                //}

                filePath = GetFilePath();

                bool handle = false;
                if (Loader != null)
                {
                    byte[] data = null;
                    Loader(filePath, ref data, ref handle);
                    if (handle)
                    {
                        if (data != null)
                        {
                            json = Encoding.GetString(data);
                            if (!string.IsNullOrEmpty(json))
                            {
                                this.settings = FromJson(json);
                            }
                        }

                        if (Application.isEditor)
                        {
                            if (File.Exists(filePath))
                            {
                                lastWriteTime = File.GetLastWriteTimeUtc(filePath);
                                EnableFileSystemWatcher();
                            }
                        }
                    }
                }

                if (!handle)
                {
                    if (GetResourcesPath(out var resPath))
                    {
                        TextAsset textAsset = Resources.Load<TextAsset>(resPath);
                        if (textAsset)
                        {
                            json = Encoding.GetString(textAsset.bytes);
                        }
                        if (!string.IsNullOrEmpty(json))
                        {
                            this.settings = FromJson(json);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (File.Exists(filePath))
                            {
                                json = File.ReadAllText(filePath, Encoding);
                                lastWriteTime = File.GetLastWriteTimeUtc(filePath);
                                EnableFileSystemWatcher();
                            }
                            if (!string.IsNullOrEmpty(json))
                            {
                                this.settings = FromJson(json);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"load file <{filePath}>", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (this.settings == null)
            {
                //Debug.Log("settings null, " + GetFilePath());
                if (OnFirstCreateInstance != null)
                {
                    try
                    {
                        this.settings = OnFirstCreateInstance();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    if (this.settings == null)
                        Debug.LogError("OnFirstCreateInstance return null");
                }

                if (this.settings == null)
                {
                    this.settings = Activator.CreateInstance(type);
                }

                if (settings != null)
                {
                    if (!Application.isPlaying)
                    {
                        if (!File.Exists(filePath))
                        {
                            Save();
                        }
                    }
                }
            }


            if (oldSettings != this.settings)
                OnLoadAfter?.Invoke(this.settings);
        }

        public void Save()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning($"is playing, runtime settings can't save [{type.FullName}]");
                return;
            }

#if UNITY_EDITOR
            if (_playingChanged != EditorSettingsProvider.playingChanged)
            {
                _playingChanged = EditorSettingsProvider.playingChanged;
                settings = null;
                return;
            }
#endif
            DisableFileSystemWatcher();

            string filePath = GetFilePath();
            string json = ToJson(Settings);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, json, Encoding);
            lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            EnableFileSystemWatcher();
        }






        public void EnableFileSystemWatcher()
        {

            if (!Application.isEditor)
                return;
            if (fsw != null)
                return;
            try
            {
                string filePath = GetFilePath();
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    return;
                string fileName = Path.GetFileName(filePath);
                fsw = new FileSystemWatcher();
                fsw.BeginInit();
                fsw.Path = dir;
                fsw.Filter = fileName;
                fsw.NotifyFilter = NotifyFilters.LastWrite;
                fsw.Changed += OnFileSystemWatcher;
                fsw.Deleted += OnFileSystemWatcher;
                fsw.Renamed += OnFileSystemWatcher;
                fsw.Created += OnFileSystemWatcher;
                fsw.IncludeSubdirectories = true;
                fsw.EnableRaisingEvents = true;
                fsw.EndInit();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void DisableFileSystemWatcher()
        {
            if (fsw != null)
            {
                fsw.Dispose();
                fsw = null;
            }
        }

        public void OnFileSystemWatcher(object sender, FileSystemEventArgs e)
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying)
                {
                    bool changed = true;
                    string filePath = GetFilePath();
                    if (File.Exists(filePath) && lastWriteTime == File.GetLastWriteTimeUtc(filePath))
                    {
                        changed = false;
                    }

                    if (changed)
                    {
                        settings = null;
                    }
                }
            };
#endif
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;
            Save();
            PropertyChanged?.Invoke(settings, new PropertyChangedEventArgs(propertyName));
        }

        object CreateInstance()
        {
            if (type.IsArray)
            {
                return Array.CreateInstance(type, 0);
            }
            else
            {
                var listType = type.FindByGenericTypeDefinition(typeof(IList<>));
                if (listType != null)
                {
                    return Activator.CreateInstance(typeof(List<>).MakeGenericType(listType.GetGenericArguments()[0]));
                }
            }
            return Activator.CreateInstance(type);
        }
        object FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return CreateInstance();

            if (type.IsArray || type.FindByGenericTypeDefinition(typeof(IList<>)) != null)
                return FromArrayJson(json, type);
            return JsonUtility.FromJson(json, type);
        }

        string ToJson(object obj)
        {
            if (type.IsArray || type.FindByGenericTypeDefinition(typeof(IList<>)) != null)
                return ToArrayJson(obj, PrettyPrint);
            return JsonUtility.ToJson(obj, PrettyPrint);
        }

        public static string ToArrayJson(object array, bool prettyPrint = false)
        {
            if (array == null) return null;
            Type type = array.GetType();
            Type itemType = null;

            if (type.IsArray)
            {
                itemType = type.GetElementType();
            }
            else
            {
                var listType = type.FindByGenericTypeDefinition(typeof(IList<>));
                if (listType != null)
                {
                    itemType = listType.GetGenericArguments()[0];
                    var list = (IList)array;
                    Array newArray = Array.CreateInstance(itemType, list.Count);
                    for (int i = 0; i < list.Count; i++)
                    {
                        newArray.SetValue(list[i], i);
                    }
                    array = newArray;
                }
            }

            if (itemType == null) throw new ArgumentException(nameof(array), $"'{type.Name}' not is Array or List<T> type");

            Type serializableType = typeof(SerializableArray<>).MakeGenericType(itemType);
            return serializableType.GetMethod(nameof(ToArrayJson))
                .Invoke(null, new object[] { array, prettyPrint }) as string;
        }

        public static object FromArrayJson(string arrayJson, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(arrayJson)) return type.DefaultValue();

            Type itemType = null;
            if (type.IsArray)
            {
                itemType = type.GetElementType();
            }
            else
            {
                var listType = type.FindByGenericTypeDefinition(typeof(IList<>));
                if (listType != null)
                {
                    itemType = listType.GetGenericArguments()[0];
                }
            }
            if (itemType == null) throw new ArgumentException(nameof(type), $"'{type.Name}' not is Array or List<T> type");

            Type serializableType = typeof(SerializableArray<>).MakeGenericType(itemType);
            Array array = serializableType.GetMethod(nameof(FromArrayJson))
                .Invoke(null, new object[] { arrayJson }) as Array;
            if (array == null) return null;
            if (!type.IsArray)
            {
                IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType)) as IList;
                foreach (var item in array)
                {
                    list.Add(item);
                }
                return list;
            }
            return array;
        }

        public override string ToString()
        {
            return $"package: {packageName}, runtime: {IsRuntime}, project: {IsProject}";
        }

        public enum SettingsScope
        {
            /// <summary>
            /// Localtion: Assets/Resources/ProjectSettings/Packages/[PackageName]/Settings.json
            /// </summary>
            ProjectRuntime = 1,
            RuntimeProject = ProjectRuntime,
            /// <summary>
            /// Localtion: [persistentDataPath]/UserSettings/Packages/[PackageName]/Settings.json
            /// </summary>
            UserRuntime,
            RuntimeUser = UserRuntime,
            /// <summary>
            /// ProjectSettings/Packages/[PackageName]/Settings.json
            /// </summary>
            ProjectEditor,
            EditorProject = ProjectEditor,
            /// <summary>
            /// UserSettings/Packages/[PackageName]/Settings.json
            /// </summary>
            UserEditor,
            EditorUser = UserEditor,
            /// <summary>
            /// 
            /// </summary>
            Editor,
        }

        /// <summary>
        /// 解决<see cref="JsonUtility"/>序列化数组 '[]'
        /// </summary>
        [Serializable]
        class SerializableArray<T>
        {
            [SerializeField]
            public T[] array;

            public static string ToArrayJson(T[] array, bool prettyPrint = false)
            {
                string json = JsonUtility.ToJson(new SerializableArray<T>() { array = array }, prettyPrint);
                int startIndex = json.IndexOf('[');
                return json.Substring(startIndex, json.LastIndexOf(']') - startIndex + 1);
            }
            public static T[] FromArrayJson(string arrayJson)
            {
                if (string.IsNullOrEmpty(arrayJson))
                    return new T[0];
                return JsonUtility.FromJson<SerializableArray<T>>($"{{\"array\":{arrayJson}}}").array;
            }
        }
    }

#if UNITY_EDITOR
    internal class EditorSettingsProvider
    {

        public static int playingChanged;

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            //丢弃运行时设置
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode)
            {
                playingChanged = (playingChanged + 1) % 1000;
            }
        }
    }
#endif



}