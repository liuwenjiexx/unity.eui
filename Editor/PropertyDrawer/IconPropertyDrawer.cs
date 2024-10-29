using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Extension
{
    [CustomPropertyDrawer(typeof(IconEditorAttribute))]
    public class IconPropertyDrawer : PropertyDrawer
    {
        int pickIconSize = 16;
        static GUIContent pickIcon;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (pickIcon == null)
            {
                pickIcon = EditorGUIUtility.IconContent("d_pick_uielements@2x");
            }

            IconEditorAttribute attribute = this.attribute as IconEditorAttribute;
            float iconSize = position.height;
            var image = property.objectReferenceValue as Texture2D;

            GUI.Label(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);

            Rect pos = position;
            pos.xMin += EditorGUIUtility.labelWidth;

            if (image)
            {

                if (Event.current.type == EventType.Repaint)
                {
                    Rect rect = new Rect(pos.x, pos.y, iconSize, iconSize);
                    GUI.DrawTexture(rect, image);
                }
            }
            else
            {
                GUI.Label(pos, "(None)");
            }
            GUIStyle pickStyle = new GUIStyle("button");
            pickStyle.padding = new RectOffset();
            pickStyle.margin = new RectOffset();
            pickStyle.border = new RectOffset();
            pickStyle.normal.background = null;
            pickStyle.normal.scaledBackgrounds = null;
            pickStyle.onNormal.scaledBackgrounds = null;

            int ctrlId = GUIUtility.GetControlID(FocusType.Passive);
            var state = (SelectedState)GUIUtility.GetStateObject(typeof(SelectedState), ctrlId);

            if (state.hasNew)
            {
                state.hasNew = false;
                if (property.objectReferenceValue != state.image)
                {
                    property.objectReferenceValue = state.image;
                    GUI.changed = true; 
                }
                state.image = null;
            }
            
            if (GUI.Button(new Rect(position.xMax - pickIconSize, position.y, pickIconSize, pickIconSize), pickIcon, pickStyle))
            {
                IconSelector.Show(attribute.AllowBuiltin, null, newIcon =>
                {
                    state.image = newIcon;
                    state.hasNew = true;
                });
            }


        }
        class SelectedState
        {
            public Texture2D image;
            public bool hasNew;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();
            Image img = new Image();
            img.image = property.objectReferenceValue as Texture2D;
            container.Add(img);

            container.RegisterCallback<MouseUpEvent>(e =>
            {

            });
            return container;
        }
    }
}
