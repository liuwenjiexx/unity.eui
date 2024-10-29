using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.Editor;

namespace UnityEditor.UIElements.Extension
{
    [MemberDrawer(typeof(string))]
    class StringDrawer : ValueDrawer
    {
        private TextField inputField;

        public override VisualElement CreateUI()
        {
            inputField = new TextField();
            inputField.isDelayed = true;
            inputField.label = DisplayName;
            Bindings.Add(BindingSet.Bind(inputField, PropertyPath));
            return inputField;
        }
    }

    [MemberDrawer(typeof(int))]
    class Int32Drawer : ValueDrawer
    {
        private IntegerField inputField;

        public override VisualElement CreateUI()
        {
            inputField = new IntegerField();
            inputField.isDelayed = true;
            inputField.label = DisplayName;
            inputField.RegisterValueChangedCallback(e =>
            {
                Value = e.newValue;
            });
            Bindings.Add(BindingSet.Bind(inputField, PropertyPath));
            return inputField;
        }
 
    }


    [MemberDrawer(typeof(long))]
    class Int64Drawer : ValueDrawer
    {
        LongField inputField;
        public override VisualElement CreateUI()
        {
            inputField = new LongField();
            inputField.isDelayed = true;
            inputField.label = DisplayName;
            inputField.RegisterValueChangedCallback(e =>
            {
                Value = e.newValue;
            });

            Bindings.Add(BindingSet.Bind(inputField, PropertyPath));
            return inputField;
        }
 
    }

    [MemberDrawer(typeof(float))]
    class Float32Drawer : ValueDrawer
    {
        private FloatField inputField;
        public override VisualElement CreateUI()
        {
            inputField = new FloatField();
            inputField.isDelayed = true;
            inputField.label = DisplayName;
            inputField.RegisterValueChangedCallback(e =>
            {
                Value = e.newValue;
            });
            Bindings.Add(BindingSet.Bind(inputField, PropertyPath));
            return inputField;
        }
 
    }
    [MemberDrawer(typeof(double))]
    class Float64Drawer : ValueDrawer
    {
        DoubleField inputField;
        public override VisualElement CreateUI()
        {
            inputField = new DoubleField();
            inputField.isDelayed = true;
            inputField.label = DisplayName;
            inputField.RegisterValueChangedCallback(e =>
            {
                Value = e.newValue;
            });
            Bindings.Add(BindingSet.Bind(inputField, PropertyPath));
            return inputField;
        }
 
    }

    [MemberDrawer(typeof(Enum), true)]
    class EnumDrawer : ValueDrawer
    {
        private BaseField<Enum> inputField;

        public override VisualElement CreateUI()
        {
            if (ValueType.IsDefined(typeof(FlagsAttribute), false))
            {
                inputField = new EnumFlagsField((Enum)ValueType.DefaultValue());
            }
            else
            {
                inputField = new EnumField((Enum)ValueType.DefaultValue());
            }
            inputField.label = DisplayName;
            Bindings.Add(BindingSet.Bind(inputField, PropertyPath));

            return inputField;
        }
 

    }

}
