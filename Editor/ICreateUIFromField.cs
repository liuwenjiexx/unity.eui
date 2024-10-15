using System.Reflection;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{
    public interface ICreateUIFromField
    {
        VisualElement CreateUIFromField(object target, FieldInfo fieldInfo);
    }
}