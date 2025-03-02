using System;

namespace UnityEditor.UIElements.Extension
{
    public class MemberDrawerAttribute : Attribute
    {
        public MemberDrawerAttribute(Type valueType, bool editorForChildClasses = false)
        {
            ValueType = valueType;
            EditorForChildClasses = editorForChildClasses;
        }

        public Type ValueType { get; set; }
        public bool EditorForChildClasses { get; set; }
    }

}
