using System.Reflection;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Extension
{
    public interface ICreateUIFromField
    {
        VisualElement CreateUIFromField(object target, FieldInfo fieldInfo);
    }
}