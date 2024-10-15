using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Unity.UI.Editor
{

    /*
    public class IconField : BaseField<Texture2D>
    {
        int pickIconSize = 16;
        static GUIContent pickIcon;
        Image image;
        Button pickButton;

        public IconField()
           : this(null)
        {

        }

        public IconField(string label)
           : base(label, null)
        {
            if (pickIcon == null)
            {
                pickIcon = EditorGUIUtility.IconContent("d_pick_uielements@2x");
                //pickIcon = EditorGUIUtility.IconContent("d_pick@2x");

            }
            style.height = EditorGUIUtility.singleLineHeight;
            var input = Children().First(o => o != labelElement);
            input.style.display = DisplayStyle.None;
            image = new Image(); 
            Add(image);
            VisualElement padding = new VisualElement();
            padding.style.flexGrow = 1f;
            Add(padding);
            pickButton = new Button();
            pickButton.style.backgroundImage = pickIcon.image as Texture2D;
            pickButton.style.width = 18;
            pickButton.style.height = 16;
            pickButton.style.paddingLeft = 0;
            pickButton.style.paddingRight = 0;
            pickButton.style.paddingTop = 0;
            pickButton.style.paddingBottom = 0;
            pickButton.style.backgroundColor = Color.clear;
            pickButton.style.borderLeftWidth = 0;
            pickButton.style.borderRightWidth = 0;
            pickButton.style.borderTopWidth = 0;
            pickButton.style.borderBottomWidth = 0;

            pickButton.clicked += () =>
            {
                IconSelector.Show(AllowBuiltin, null, newIcon =>
                {
                    value = newIcon;
                });
            };
            Add(pickButton);

            RegisterCallback<GeometryChangedEvent>(e =>
            {
                image.style.width = localBound.height;
                image.style.height = localBound.height;
            });
            
        }
         
        public override Texture2D value
        {
            get => base.value;
            set
            {
                base.value = value; 
            }
        }

        public bool AllowBuiltin { get; set; }

        public override void SetValueWithoutNotify(Texture2D newValue)
        {
            base.SetValueWithoutNotify(newValue);
            image.image = newValue;
            if (image.image)
            {
                image.tooltip = image.image.name;
            }
            else
            {
                image.tooltip = null;
            }
        }
         
    }*/
}