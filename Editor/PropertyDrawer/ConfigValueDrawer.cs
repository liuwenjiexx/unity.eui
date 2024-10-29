using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity;

namespace UnityEditor.UIElements.Extension
{
    [CustomPropertyDrawer(typeof(ConfigValue<>), true)]
    public class ConfigValueDrawer : PropertyDrawer
    {
        public ConfigValueDrawer()
        {
        }
        public ConfigValueDrawer(FieldInfo fieldInfo)
        {
            GetType().GetField("m_FieldInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, fieldInfo);
        }

        private static Dictionary<Type, ConfigValueDrawer> instances;
        protected static TDrawer GetInstance<TDrawer>()
            where TDrawer : ConfigValueDrawer, new()
        {
            if (instances == null)
            {
                instances = new();
            }

            Type type = typeof(TDrawer);
            if (!instances.TryGetValue(type, out var drawer))
            {
                drawer = new TDrawer();
                instances[type] = drawer;
            }

            return (TDrawer)drawer;
        }

        public static object ToConfigValue(SerializedProperty property)
        {
            var value = property.GetObjectOfProperty();
            return value;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var configValue = property.GetObjectOfProperty();
            
            Type valueType = configValue.GetType().GetGenericArguments()[0];
            var container = (VisualElement)typeof(ConfigValueDrawer).GetMethod(nameof(CreateUI), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(valueType)
                .Invoke(this,
                new object[] { property });

            var textField = container.Q<TextField>();
            if (textField != null)
            {
                var multilineAttr = fieldInfo.GetCustomAttribute<MultiLineAttribute>();
                if (multilineAttr != null)
                {
                    textField.multiline = true;
                    //var h= textField.MeasureTextSize(new string('\n', multilineAttr.Lines)+"a", 100, VisualElement.MeasureMode.Exactly, 100, VisualElement.MeasureMode.Exactly).y;
                    //var h = textField.style.unityFont.value.lineHeight * multilineAttr.Lines;
                    var h = (EditorGUIUtility.singleLineHeight - 3) * multilineAttr.Lines;
                    textField.style.height = h;
                    textField.style.maxHeight = h;
                }
            }
            return container;
        }

        VisualElement CreateUI<T>(SerializedProperty property)
        {

            var valueField = new ConfigValueField<T>(this);
            valueField.label = property.displayName;
            valueField.tooltip = property.tooltip;
            valueField.IsDelayed = true;
            valueField.bindingPath = property.propertyPath;
            valueField.binding = new SerializedPropertyBinding<ConfigValue<T>>(valueField, property);
            return valueField;
        }

        public virtual VisualElement CreateInputUI<T>()
        {
            VisualElement inputField = null;
            Type valueType = typeof(T);

            var drawerAttr = fieldInfo.GetCustomAttribute<ItemDrawerAttribute>();
            if (drawerAttr != null)
            {
                if (drawerAttr.DrawerType != null)
                {
                    if (typeof(VisualElement).IsAssignableFrom(drawerAttr.DrawerType))
                    {
                        inputField = Activator.CreateInstance(drawerAttr.DrawerType) as VisualElement;

                    }
                }
            }

            if (inputField == null)
            {
                inputField = EditorUIUtility.CreateInputField(valueType);
            }

            if (inputField == null)
                throw new NotImplementedException("ConfigValue drawer " + typeof(T));

            if (drawerAttr != null)
            {
                if (drawerAttr.InitalizeMethod != null)
                {
                    foreach (var methodInfo in fieldInfo.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        if (methodInfo.Name == drawerAttr.InitalizeMethod)
                        {
                            var ps = methodInfo.GetParameters();
                            if (ps.Length == 2 && ps[0].ParameterType.IsAssignableFrom(inputField.GetType()) && ps[1].ParameterType == typeof(string))
                            {
                                methodInfo.Invoke(null, new object[] { inputField, fieldInfo.Name });
                                break;
                            }
                        }
                    }
                }
            }

            if (inputField is EnumField)
            {
                var enumField = inputField as EnumField;
                enumField.Init((Enum)(object)default(T));
            }

            inputField.AddToClassList(BaseField<string>.inputUssClassName);

            inputField.style.marginLeft = 0;
            inputField.style.marginRight = 0;

            return inputField;
        }

    }

}
