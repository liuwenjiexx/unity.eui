using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{

    class InputPlaceholder
    {
        private BaseField<string> inputField;
        private Label placeholderLabel;


        public InputPlaceholder(BaseField<string> inputField, string placeholderText = null)
        {
            this.inputField = inputField;

            placeholderLabel = new Label();
            placeholderLabel.AddToClassList("unity-text-field__placeholder");
            placeholderLabel.text = placeholderText;

            var input = inputField.Q(className: "unity-text-field__input");
            if (input != null)
            {
                input.Add(placeholderLabel);
            }

            Initalize();
        }


        public Label PlaceholderLabel
        {
            get => placeholderLabel;
        }

        public string PlaceholderText
        {
            get => placeholderLabel.text;
            set => placeholderLabel.text = value;
        }


        void Initalize()
        {

            inputField.RegisterCallback<FocusInEvent>(e =>
            {
                HidePlaceholder();
            });
            inputField.RegisterCallback<FocusOutEvent>(e =>
            {
                inputField.schedule.Execute(() =>
                {
                    if (string.IsNullOrEmpty(inputField.value))
                    {
                        ShowPlaceholder();
                    }
                    else
                    {
                        HidePlaceholder();
                    }
                });

            });

            CheckPlaceholder();
        }

        void CheckPlaceholder()
        {
            if (inputField.panel?.focusController?.focusedElement == inputField)
            {
                HidePlaceholder();
                return;
            }

            if (string.IsNullOrEmpty(inputField.value))
            {
                ShowPlaceholder();
            }
            else
            {
                HidePlaceholder();
            }
        }

        void ShowPlaceholder()
        {
            placeholderLabel.style.display = DisplayStyle.Flex;
        }

        void HidePlaceholder()
        {
            placeholderLabel.style.display = DisplayStyle.None;
        }


    }

}
