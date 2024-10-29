using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{

    public class Toast
    {
        internal string style;
        internal string content;
        internal float? duration;
        internal ToastDuration durationType = ToastDuration.Normal;

        public Toast Content(string content)
        {
            this.content = content;
            return this;
        }


        public Toast Style(string style)
        {
            this.style = style;
            return this;
        }

        public Toast Duration(ToastDuration duration)
        {
            durationType = duration;
            this.duration = null;
            return this;
        }

        public Toast Duration(float duration)
        {
            this.duration = duration;
            durationType = ToastDuration.Normal;
            return this;
        }

        public void Show()
        { 
            UIElementsUtility.ToastLayer.ShowToast(this);
        }
    }

    public enum ToastDuration
    {
        Normal,
        Long,
    }

}