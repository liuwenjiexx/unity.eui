using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Unity.Editor
{
    public static class Extensions
    {
        #region Assembly Metadata

        static Dictionary<(Assembly, string), (string, bool)> assemblyMetadatas;

        public static string GetAssemblyMetadata(this Assembly assembly, string key)
        {
            if (!TryGetAssemblyMetadata(assembly, key, out var value))
                throw new Exception($"Not define AssemblyMetadataAttribute. key: {key}");
            return value;
        }

        public static string GetAssemblyMetadata(this Assembly assembly, string key, string defaultValue)
        {
            if (!TryGetAssemblyMetadata(assembly, key, out var value))
            {
                value = defaultValue;
            }
            return value;
        }

        public static bool TryGetAssemblyMetadata(this Assembly assembly, string key, out string value)
        {
            if (assemblyMetadatas == null)
                assemblyMetadatas = new Dictionary<(Assembly, string), (string, bool)>();

            if (!assemblyMetadatas.TryGetValue((assembly, key), out var item))
            {
                foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
                {
                    if (attr.Key == key)
                    {
                        item = new(attr.Value, true);
                        break;
                    }
                }

                assemblyMetadatas[(assembly, key)] = item;
            }

            if (item.Item2)
            {
                value = item.Item1;
                return true;
            }
            value = null;
            return false;
        }

        #endregion

        #region UnityPackage

        static Dictionary<string, string> unityPackageDirectories = new Dictionary<string, string>();


        public static string GetUnityPackageName(this Assembly assembly)
        {
            return GetAssemblyMetadata(assembly, "Unity.Package.Name");
        }

        public static string GetUnityPackageDirectory(this Assembly assembly)
        {
            return GetUnityPackageDirectory(GetUnityPackageName(assembly));
        }

        //2021/4/13
        internal static string GetUnityPackageDirectory(string packageName)
        {
            if (!unityPackageDirectories.TryGetValue(packageName, out var path))
            {
                var tmp = Path.Combine("Packages", packageName);
                if (Directory.Exists(tmp) && File.Exists(Path.Combine(tmp, "package.json")))
                {
                    path = tmp;
                }

                if (path == null)
                {
                    foreach (var dir in Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories))
                    {
                        if (string.Equals(Path.GetFileName(dir), packageName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (File.Exists(Path.Combine(dir, "package.json")))
                            {
                                path = dir;
                                break;
                            }
                        }
                    }
                }

                if (path == null)
                {
                    foreach (var pkgPath in Directory.GetFiles("Assets", "package.json", SearchOption.AllDirectories))
                    {
                        try
                        {
                            if (JsonUtility.FromJson<_UnityPackage>(File.ReadAllText(pkgPath, System.Text.Encoding.UTF8)).name == packageName)
                            {
                                path = Path.GetDirectoryName(pkgPath);
                                break;
                            }
                        }
                        catch { }
                    }
                }

                if (path != null)
                {
                    path = path.Replace('\\', '/');
                }
                unityPackageDirectories[packageName] = path;
            }
            return path;
        }

        [Serializable]
        class _UnityPackage
        {
            public string name;
        }

        #endregion

        #region Type

        public static object DefaultValue(this Type type)
        {
            if (type == null || type == typeof(string))
                return null;
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static object CreateInstance(this Type type)
        {
            if (type == typeof(string))
                return string.Empty;
            if (type.IsArray)
            {
                return Array.CreateInstance(type.GetElementType(), 0);
            }
            return Activator.CreateInstance(type);
        }

        public static Type FindByGenericTypeDefinition(this Type type, Type genericTypeDefinition)
        {
            Type result = null;

            if (genericTypeDefinition.IsInterface)
            {
                foreach (var t in type.GetInterfaces())
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
                    {
                        result = t;
                        break;
                    }
                }
            }
            else
            {
                Type t = type;
                while (t != null)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
                    {
                        result = t;
                        break;
                    }
                    t = t.BaseType;
                }
            }
            return result;
        }
        #endregion


    }
}

