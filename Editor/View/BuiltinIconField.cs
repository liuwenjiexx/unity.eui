using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Extension
{

    public class BuiltinIconField : BaseField<BuiltinIcon>
    {
        int pickIconSize = 16;
        static GUIContent pickIcon;
        Image image;
        Label nameLabel;
        Button pickButton;

        public BuiltinIconField()
           : this(null)
        {

        }

        public BuiltinIconField(string label)
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

            labelElement.AddManipulator(new MenuManipulator(e =>
            {
                e.menu.AppendAction("Copy Icon Name", act =>
                {
                    if (value.Image)
                    {
                        EditorGUIUtility.systemCopyBuffer = value.Image.name;
                    }
                });
            }));

            image = new Image();
            Add(image);

            nameLabel = new Label();
            nameLabel.style.marginTop = 2;
            nameLabel.style.flexGrow = 1f;
            nameLabel.RegisterCallback<MouseDownEvent>(e =>
            {
                if (value.Image)
                {
                    EditorGUIUtility.systemCopyBuffer = value.Image.name;
                }
            });
            Add(nameLabel);

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
                    value = new BuiltinIcon(newIcon);
                });
            };
            Add(pickButton);

            RegisterCallback<GeometryChangedEvent>(e =>
            {
                image.style.width = localBound.height;
                image.style.height = localBound.height;
            });

        }

        public override BuiltinIcon value
        {
            get => base.value;
            set
            {
                if (!object.Equals(base.value, value))
                {
                    base.value = value;
                }
            }
        }

        public bool AllowBuiltin { get; set; } = true;

        public override void SetValueWithoutNotify(BuiltinIcon newValue)
        {
            base.SetValueWithoutNotify(newValue);
            image.image = newValue.Image;
            if (image.image)
            {
                image.tooltip = image.image.name;
                nameLabel.text = $"({image.image.name})";
            }
            else
            {
                image.tooltip = null;
                nameLabel.text = "(None)";
            }



        }



    }
}
