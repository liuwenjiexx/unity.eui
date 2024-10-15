using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{
    public class InspectorView<T, TInspectorObject> : IView
           where TInspectorObject : ScriptableObject
        //where TInspectorObject : InspectorView<T, TInspectorObject>.InspectorObject
    {
        [NonSerialized]
        TInspectorObject inspectorObject;
        [NonSerialized]
        UnityEditor.Editor inspectorEditor;


        public string DisplayName { get; set; }
        public object Owner { get; set; }
        public object Target { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnEnable()
        {
            inspectorObject = ScriptableObject.CreateInstance<TInspectorObject>();
            //inspectorObject = ScriptableObject.CreateInstance<InspectorObject>();

            inspectorObject.hideFlags = HideFlags.DontSave;
            typeof(TInspectorObject).GetField("target").SetValue(inspectorObject, Target);
            //inspectorObject.target = (T)Target;
            inspectorEditor = UnityEditor.Editor.CreateEditor(inspectorObject);
            inspectorEditor.hideFlags = HideFlags.DontSave;
        }

        public virtual void OnDisable()
        {
            if (inspectorEditor)
            {
                UnityEngine.Object.DestroyImmediate(inspectorEditor);
                inspectorEditor = null;
            }

            if (inspectorObject)
            {
                UnityEngine.Object.DestroyImmediate(inspectorObject);
                inspectorObject = null;
            }
        }

        public virtual VisualElement CreateUI()
        {
            return inspectorEditor.CreateInspectorGUI();
        }

        public virtual void OnActive()
        {

        }
        //嵌套类 CreateInspectorGUI is null
        [Serializable]
        public class InspectorObject : ScriptableObject
        {
            [SerializeField]
            public T target;

        }

    }

}
