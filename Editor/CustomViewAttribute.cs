using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.UIElements.Extension
{
   public class CustomViewAttribute : Attribute
    {
        private static Dictionary<Type, Type> typeMapViewTypes;

        public CustomViewAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        public Type TargetType { get; set; }
  
        public static Type GetViewType(Type dataType)
        {
            Type viewType = null;
            if (typeMapViewTypes == null)
            {
                typeMapViewTypes = new();
                foreach (var type in TypeCache.GetTypesWithAttribute(typeof(CustomViewAttribute)))
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;
                    var viewAttr = type.GetCustomAttribute<CustomViewAttribute>();
                    var targetType = viewAttr.TargetType;
                    if (targetType == null) continue;
                    typeMapViewTypes[targetType] = type;
                }
            }
            typeMapViewTypes.TryGetValue(dataType, out viewType);
            return viewType;
        }
    }
}