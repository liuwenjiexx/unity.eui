using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Extension
{
    internal static partial class InternalExtensions
    {
  
        public static bool IsIndexer(this PropertyInfo property)
        {
            if (property.GetIndexParameters().Length > 0)
            {
                return true;
            }
            return false;
        }


    

        #region SerializedProperty

        public static object[] PropertyPathMembers(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var members = path.Split('.');
            List<object> result = new List<object>();
            foreach (var member in members)
            {
                if (member.Contains("["))
                {
                    var elementName = member.Substring(0, member.IndexOf("["));
                    result.Add(elementName);

                    var index = System.Convert.ToInt32(member.Substring(member.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    result.Add(index);
                }
                else
                {
                    result.Add(member);
                }
            }
            return result.ToArray();
        }

        public static object GetObjectOfProperty(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }
        private static MemberInfo GetValue_ImpMember(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f;

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p;

                type = type.BaseType;
            }
            return null;
        }
        public static void SetObjectOfProperty(this SerializedProperty prop, object value)
        {
            //var owner = GetObjectOfPropertyOwner(prop);
            //if (owner == null)
            //    return;
            //var member = GetValue_ImpMember(owner, prop.name);
            //if (member is FieldInfo)
            //    ((FieldInfo)member).SetValue(owner, value);
            //else
            //    ((PropertyInfo)member).SetValue(owner, value, null);

            List<(object, MemberInfo, int)> values = new List<(object, MemberInfo, int)>();
            object obj = prop.serializedObject.targetObject;
            object owner = obj;
            var members = prop.PropertyPathMembers();
            for (int i = 0; i < members.Length - 1; i++)
            {
                var member = members[i];
                if (member is string)
                {
                    string name = (string)member;

                    var memberInfo = GetValue_ImpMember(obj, name);
                    if (memberInfo is FieldInfo)
                        obj = ((FieldInfo)memberInfo).GetValue(obj);
                    else
                        obj = ((PropertyInfo)memberInfo).GetValue(obj, null);
                    values.Add(new(obj, memberInfo, -1));
                }
                else if (member is int)
                {
                    int index = (int)member;
                    var enumerable = members[i - 1] as IEnumerable;

                    if (enumerable == null)
                    {
                        obj = null;
                        break;
                    }
                    var enm = enumerable.GetEnumerator();
                    //while (index-- >= 0)
                    //    enm.MoveNext();
                    //return enm.Current;

                    for (int j = 0; j <= index; j++)
                    {
                        if (!enm.MoveNext())
                        {
                            obj = null;
                            break;
                        }
                    }
                    if (obj != null)
                    {
                        obj = enm.Current;
                        values.Add(new(obj, null, index));
                    }
                }

            }

            if (obj == null)
                return;
            {
                var member = GetValue_ImpMember(obj, prop.name);
                if (member is FieldInfo)
                    ((FieldInfo)member).SetValue(obj, value);
                else
                    ((PropertyInfo)member).SetValue(obj, value, null);
                //if (obj.GetType().IsValueType)
                //{
                //    if (values.Count > 0)
                //    {
                //        var last = values[values.Count - 1];
                //        last.Item1 = obj;
                //        values[values.Count - 1] = last;
                //    }
                //}
            }

            //check owner value type
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (values[i].Item2 == null || !values[i].Item1.GetType().IsValueType)
                {
                    continue;
                }
                var _owner = i == 0 ? owner : values[i - 1].Item1;
                var _value = values[i].Item1;
                var _member = values[i].Item2;
                if (_member is FieldInfo)
                    ((FieldInfo)_member).SetValue(_owner, _value);
                else
                    ((PropertyInfo)_member).SetValue(_owner, _value, null);
            }

            //var path = prop.propertyPath.Replace(".Array.data[", "[");
            //object obj = prop.serializedObject.targetObject;
            //var elements = path.Split('.');
            //for (int i = 0; i < elements.Length; i++)
            //{
            //    string element = elements[i];
            //    if (element.Contains("["))
            //    {
            //        var elementName = element.Substring(0, element.IndexOf("["));
            //        var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

            //        obj = GetValue_Imp(obj, elementName, index);

            //    }
            //    else
            //    {
            //        if (i == elements.Length - 1)
            //        {
            //            var member = GetValue_ImpMember(obj, element);
            //            if (member is FieldInfo)
            //                ((FieldInfo)member).SetValue(obj, value);
            //            else
            //                ((PropertyInfo)member).SetValue(obj, value, null);
            //        }
            //        else
            //            obj = GetValue_Imp(obj, element);
            //    }
            //}
            //return obj;
        }

        public static object GetObjectOfPropertyOwner(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            for (int i = 0, len = elements.Length - 1; i < len; i++)
            {

                string element = elements[i];
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        public static GUIContent LabelContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        #endregion

 

    }


}