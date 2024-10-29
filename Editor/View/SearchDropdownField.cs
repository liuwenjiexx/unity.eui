using System;
using System.Collections;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Extension
{
    public class SearchDropdownField : BaseField<object>, INotifyValueChanged<object>
    {
        //private object _value;
        private SearchPopupContent popup;
        private Label textElement;
        VisualElement inputContainer;
        public SearchDropdownField()
            : this(null)
        {
        }
        public SearchDropdownField(string label)
            : base(label, null)
        {

            inputContainer = this.Q(className: "unity-base-field__input");
            inputContainer.AddToClassList("unity-base-popup-field__input");
            inputContainer.AddToClassList("unity-popup-field__input");
            inputContainer.RegisterCallback<MouseDownEvent>(e => ShowPopup());

            textElement = new Label();
            textElement.AddToClassList("unity-base-popup-field__text");
            textElement.style.marginLeft = 0;
            textElement.style.marginRight = 0;
            inputContainer.Add(textElement);

            VisualElement arrow = new VisualElement();
            arrow.AddToClassList("unity-base-popup-field__arrow");
            inputContainer.Add(arrow);
            Add(inputContainer);

            this.RegisterValueChangedCallback(e =>
            {
                UpdateValue();
            });

            popup = new SearchPopupContent();
            popup.OnSelected += (item) =>
            {
                value = item;
            };
            Text = null;
        }

        public Func<object, string> FormatSelectedValueCallback;
        public Func<object, string> FormatListItemCallback { get => popup.formatListItemCallback; set => popup.formatListItemCallback = value; }
        public Action<IList> LoadItems { get => popup.loadItems; set => popup.loadItems = value; }

        public Func<object, string, bool> Filer { get => popup.filer; set => popup.filer = value; }

        public SearchPopupContent Popup => popup;



        //public new object value
        //{
        //    get => base.value;
        //    set
        //    {
        //        var oldValue = this.value;
        //        if (oldValue != value)
        //        {
        //            base.value = value;
        //            UpdateValue();
        //            using (var e = ChangeEvent<object>.GetPooled(oldValue, _value))
        //            {
        //                SendEvent(e);
        //            }
        //        }
        //    }
        //}

        public string Text
        {
            get => textElement.text;
            set => textElement.text = value;
        }

        public Label TextElement { get => textElement; }

        public override void SetValueWithoutNotify(object newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateValue();
        }

        void UpdateValue()
        {
            TextElement.text = ItemToString(value);
        }

        private string ItemToString(object item)
        {
            string text = null;
            if (item != null)
            {
                if (FormatSelectedValueCallback != null)
                    text = FormatSelectedValueCallback(item);
                if (FormatListItemCallback != null)
                    text = FormatListItemCallback(item);
                else
                    text = item.ToString();
            }
            return text;
        }

        private void ShowPopup()
        {
            popup.filer = Filer;

            popup.Show(inputContainer);
        }


        public new class UxmlFactory : UxmlFactory<SearchDropdownField, UxmlTraits> { }

    }
}