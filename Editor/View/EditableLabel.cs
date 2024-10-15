using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{

    public class EditableLabel : TextField
    {
        private bool editMode;
        TextInputBase inputElement;
        private Label textElement;

        public EditableLabel()
        {
            inputElement = this.Q<TextInputBase>();
            isDelayed = true;
            //empty not create labelElement
            
            //labelElement.style.width = new Length(100, LengthUnit.Percent);
            //labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            //inputElement.style.width = new Length(100, LengthUnit.Percent);
            textElement = new Label();
            Add(textElement);
            
            textElement.style.flexGrow = 1f;
            inputElement.style.flexGrow = 1f;
            textElement.RegisterValueChangedCallback(e =>
            {
                e.StopImmediatePropagation();
            });
            bool editing = false;
            textElement.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    if (!DoubleClickToEdit || e.clickCount == 2)
                    {
                        if (!DoubleClickToEdit)
                            IsEditMode = true;
                        else
                            editing = true;
                        e.StopImmediatePropagation();
                    }
                }
            });
            textElement.RegisterCallback<MouseUpEvent>(e =>
            {
                if (e.button == 0)
                {
                    if (editing)
                    {
                        editing = false;
                        IsEditMode = true;
                        e.StopImmediatePropagation();
                    }
                }
            });

            inputElement.RegisterCallback<FocusOutEvent>(e =>
            {
                IsEditMode = false;
            });

            UpdateMode();

        }

        public bool DoubleClickToEdit { get; set; }

        public bool IsEditMode
        {
            get => editMode;
            set
            {
                if (editMode != value)
                {
                    editMode = value;
                    UpdateMode();
                    inputElement.Focus();
                }
            }
        }
        public override string value
        {
            get => base.value;
            set
            {
                if (base.value != value)
                {
                    base.value = value;
                    UpdateLabel();
                }
            }
        }
        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateLabel();
        }

        private string emptyLabel;
        public string EmptyLabel
        {
            get => emptyLabel;
            set
            {
                if (emptyLabel != value)
                {
                    emptyLabel = value;
                    UpdateLabel();
                }
            }
        }

        void UpdateMode()
        {
            if (editMode)
            {
                textElement.style.display = DisplayStyle.None;
                inputElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                textElement.style.display = DisplayStyle.Flex;
                inputElement.style.display = DisplayStyle.None;
            }
        }
        void UpdateLabel()
        {
            if (textElement == null) return;
            string labelText = value;
            if (string.IsNullOrEmpty(labelText))
                labelText = EmptyLabel;
            textElement.text = labelText;
        }
        public new class UxmlFactory : UxmlFactory<EditableLabel, UxmlTraits> { }

    }
}
