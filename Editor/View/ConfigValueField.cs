using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity;

namespace Unity.UI.Editor
{
    public class ConfigValueField<T> : BaseField<ConfigValue<T>>
    {
        private VisualElement inputField;

        public ConfigValueField()
        : this(null, new ConfigValueDrawer()) { }
        public ConfigValueField(FieldInfo fieldInfo)
            : this(null, new ConfigValueDrawer(fieldInfo)) { }
        internal ConfigValueField(ConfigValueDrawer drawer)
            : this(null, drawer)
        {
        }

        private ConfigValueField(string label, ConfigValueDrawer drawer)
            : this(label, drawer.CreateInputUI<T>())
        {
        }

        public ConfigValueField(VisualElement visualInput)
         : this(null, visualInput)
        {
        }

        public ConfigValueField(string label, VisualElement visualInput)
        : base(label, visualInput)
        {
            if (visualInput != null)
            {
                visualInput.style.marginLeft = 0;
                visualInput.style.marginRight = 0;
            }
            inputField = Children().First(o => !(o is Label));
            SetDelayed();

            labelElement.AddManipulator(new MenuManipulator(e =>
            {
                e.menu.AppendAction("Used", act =>
                {
                    if (SetUsedValue != null)
                    {
                        SetValueWithoutNotify(SetUsedValue(GetValueState));
                    }
                    else
                    {
                        value = new ConfigValue<T>(ConfigValueKeyword.Undefined, value.Value);
                    }

                }, act =>
                {
                    if (value.Keyword == ConfigValueKeyword.Undefined)
                        return DropdownMenuAction.Status.Checked | DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                });

                e.menu.AppendAction("Unused", act =>
                {
                    //T _value;
                    //if (GetUnusedValue != null)
                    //{
                    //    _value = GetUnusedValue(GetValueState);
                    //}
                    //else
                    //{
                    //    _value = default;
                    //}

                    if (SetUnusedValue != null)
                    {
                        SetValueWithoutNotify(SetUnusedValue(GetValueState));
                    }
                    else
                    {
                        value = new ConfigValue<T>(ConfigValueKeyword.Null);
                    }

                }, act =>
                {
                    if (value.Keyword == ConfigValueKeyword.Null)
                        return DropdownMenuAction.Status.Checked | DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                });
            }));

            if (typeof(T).IsEnum)
            {
                inputField.RegisterCallback<ChangeEvent<Enum>>(e =>
                {
                    value = new ConfigValue<T>((T)(object)e.newValue);
                });
            }
            else
            {
                inputField.RegisterCallback<ChangeEvent<T>>(e =>
                {
                    value = e.newValue;
                });
            }

            UpdateView();
        }

        public VisualElement InputField => inputField;

        public override ConfigValue<T> value
        {
            get
            {
                return base.value;
            }
            set
            {
                if (!Equals(value, this.value))
                {
                    base.value = value;
                }
            }
        }

        private bool isDelayed;
        public bool IsDelayed
        {
            get => isDelayed;
            set
            {
                if (isDelayed != value)
                {
                    isDelayed = value;
                    SetDelayed();
                }
            }
        }

        public object GetValueState;
        public Func<object, ConfigValue<T>> SetUsedValue;
        public Func<object, ConfigValue<T>> SetUnusedValue;

        void UpdateView()
        {
            var configValue = value;
            labelElement.style.unityFontStyleAndWeight = StyleKeyword.Null;

            if (value.Keyword != ConfigValueKeyword.Null)
            {
                labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            if (inputField is INotifyValueChanged<T>)
            {
                var notify = inputField as INotifyValueChanged<T>;
                notify.SetValueWithoutNotify(configValue.Value);
            }
            else if (inputField is INotifyValueChanged<Enum>)
            {
                var notify = inputField as INotifyValueChanged<Enum>;
                notify.SetValueWithoutNotify((Enum)(object)configValue.Value);
            }
        }

        void SetDelayed()
        {
            var isDelayedProperty = inputField.GetType().GetProperty("isDelayed");
            if (isDelayedProperty != null && isDelayedProperty.CanWrite)
            {
                isDelayedProperty.SetValue(inputField, isDelayed);
            }
        }




        public override void SetValueWithoutNotify(ConfigValue<T> newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateView();
        }

        public static ConfigValueField<T> Wrap(string label, VisualElement visualInput)
        {
            int index = visualInput.parent.IndexOf(visualInput);
            var parent = visualInput.parent;
            ConfigValueField<T> field = new ConfigValueField<T>(label, visualInput);
            visualInput.style.marginLeft = 0;
            visualInput.style.marginRight = 0;
            parent.Insert(index, field);
            return field;
        }

        public static ConfigValueField<T> Wrap(BaseField<string> visualInput)
        {
            ConfigValueField<T> field = Wrap(visualInput.label, visualInput);
            visualInput.label = null;
            return field;
        }


    }
}
