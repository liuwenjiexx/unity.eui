using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.Bindings;
using Unity;

namespace UnityEditor.UIElements.Extension
{

    [CustomPropertyDrawer(typeof(SerializableObject<Type>))]
    class SerializableTypeDrawer : PropertyDrawer, ICreateUIFromField
    {
        public VisualElement CreateUIFromField(object target, FieldInfo fieldInfo)
        {
            TextField inputField = new TextField();
            inputField.label = ObjectNames.NicifyVariableName(fieldInfo.Name);
            inputField.isDelayed = true;
            inputField.RegisterValueChangedCallback(e =>
            {
                Type type = Type.GetType(e.newValue);
                var oldValue = fieldInfo.GetValue(target) as SerializableObject<Type>;
                var newValue = new SerializableObject<Type>(type);
                fieldInfo.SetValue(target, newValue);
                using (var evt = ChangeEvent<SerializableObject<Type>>.GetPooled(oldValue, newValue))
                {
                    evt.target = inputField;
                    inputField.SendEvent(evt);
                }
            });
            var value = fieldInfo.GetValue(target) as SerializableObject<Type>;
            inputField.SetValueWithoutNotify(value?.Target?.FullName);
            //inputField.binding = new TargetFieldBinding<string>(inputField, target, fieldInfo)
            //{
            //    Converter = new SerializableTypeConverter(SerializableTypeConverter.StringTypeNameMode.FullName)
            //};
            return inputField;
        }

        class SerializableTypeConverter : IValueConverter
        {
            public SerializableTypeConverter()
            {
            }
            public SerializableTypeConverter(StringTypeNameMode mode)
            {
                this.mode = mode;
            }

            private StringTypeNameMode mode = StringTypeNameMode.AssemblyQualifiedName;

            public object Convert(object value, Type targetType, object parameter)
            {
                if (value == null)
                {
                    return null;
                }

                if (value.GetType() != targetType)
                {
                    if (value is string)
                    {
                        string str = (string)value;
                        if (targetType == typeof(SerializableObject<Type>))
                        {
                            if (string.IsNullOrEmpty(str))
                            {
                                return null;
                            }
                            return new SerializableObject<Type>(Type.GetType(str));
                        }
                    }
                    else if (value is SerializableObject<Type>)
                    {
                        var type = ((SerializableObject<Type>)value)?.Target;
                        if (targetType == typeof(string))
                        {
                            if (type == null)
                                return null;
                            if (mode == StringTypeNameMode.AssemblyQualifiedName)
                                return type.AssemblyQualifiedName;
                            return type.FullName;
                        }
                    }
                }
                return value;
            }

            public virtual object ConvertBack(object value, Type targetType, object parameter)
            {
                return Convert(value, targetType, parameter);
            }

            public enum StringTypeNameMode
            {
                AssemblyQualifiedName,
                FullName,
                Name,
            }
        }

    }

    class TypeConverter : IValueConverter
    {

        public TypeConverter()
        {
        }
        public TypeConverter(StringTypeNameMode mode)
        {
            this.mode = mode;
        }

        private StringTypeNameMode mode = StringTypeNameMode.AssemblyQualifiedName;

        public StringTypeNameMode Mode { get => mode; set => mode = value; }

        public virtual object Convert(object value, Type targetType, object parameter)
        {
            if (value == null)
            {
                return null;
            }

            if (value.GetType() != targetType)
            {
                if (value is string)
                {
                    string str = (string)value;
                    if (targetType == typeof(Type))
                    {
                        if (string.IsNullOrEmpty(str))
                        {
                            return null;
                        }
                        return Type.GetType(str);
                    }
                }
                else if (value is Type)
                {
                    Type type = (Type)value;
                    if (targetType == typeof(string))
                    {
                        if (mode == StringTypeNameMode.AssemblyQualifiedName)
                            return type.AssemblyQualifiedName;
                        return type.FullName;
                    }
                }
            }
            return value;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter)
        {
            return Convert(value, targetType, parameter);
        }

        public enum StringTypeNameMode
        {
            AssemblyQualifiedName,
            FullName,
            Name,
        }

    }

}
