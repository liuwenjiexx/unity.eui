using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace UnityEngine.UIElements.Extension
{
    [RequireComponent(typeof(UIDocument))]
    public class MaskLayer : MonoBehaviour
    {
        internal const string UxmlName = "MaskLayer";

        public const int Layer = 100;
        private Stack<Mask> stack = new();


        public static MaskLayer instance;

        private UIDocument document;
        private VisualElement contentContainer;
        private Mask current;

        private void Awake()
        {
            name = nameof(MaskLayer);
            document = GetComponent<UIDocument>();
            document.sortingOrder = Layer;
            document.visualTreeAsset = UIElementsUtility.GetUxml(UxmlName);
            document.enabled = enabled;
            var root = document.rootVisualElement;
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;
            if (!transform.parent)
                GameObject.DontDestroyOnLoad(gameObject);
            instance = this;
        }

        private void OnEnable()
        {
            document.enabled = true;
            contentContainer = document.rootVisualElement.Q("content-container");
            Hide();
        }

        private void OnDisable()
        {
            document.enabled = false;
            contentContainer = null;
        }

        private void Show()
        {
            document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        internal void Show(Mask progressBar)
        {
            Show();
            var view = CreateView(progressBar);
            stack.Push(progressBar);
            contentContainer.Add(view);
            progressBar.Update();
            progressBar.View.style.display = DisplayStyle.Flex;
            if (current != null)
            {
                current.View.style.display = DisplayStyle.None;
            }
            current = progressBar;
        }

        VisualElement CreateView(Mask progressBar)
        {
            string style = progressBar.Style;
 
            var view = UIElementsUtility.InstantiateUxml(style);
            view.style.flexGrow = 1;
            view.pickingMode = PickingMode.Ignore;
             
            UIElementsUtility.AddStyleSheet(view, style);
            progressBar.View = view;
            progressBar.TitleLabel = view.Q<Label>("title");
            progressBar.MessageLabel = view.Q<Label>("content");


            return view;
        }

        private void Update()
        {
            while (current != null)
            {
                if (current.Disposed)
                {
                    var progressBar = current;
                    contentContainer.Remove(progressBar.View);
                    current = null;
                    stack.Pop();
                    if (stack.Count > 0)
                    {
                        current = stack.Peek();
                    }
                    else
                    {
                        Hide();
                    }
                }
                else
                {
                    break;
                }
            }

            if (current != null)
            {
                current.Update();
            }
        }


    }


    public class Mask : IDisposable
    {
        public Mask(string style, string message)
            : this(style, null, message)
        {
        }

        public Mask(string style, string title, string message)
        {
            Title = title;
            Message = message;
            Style = style;
            if (MaskLayer.instance != null)
            {
                MaskLayer.instance.Show(this);
            }
        }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Style { get; set; }

        internal VisualElement View { get; set; }

        internal Label TitleLabel { get; set; }

        internal Label MessageLabel { get; set; }

        internal bool Disposed { get; private set; }



        public virtual void Update()
        {
            if (TitleLabel != null)
            {
                TitleLabel.text = Title;
            }
            if (MessageLabel != null)
            {
                MessageLabel.text = Message;
            }
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }

    public class ProgressMask : Mask
    {
        private float time;
        public ProgressMask(string message)
              : this(null, message)
        {

        }

        public ProgressMask(string title, string message)
            : base("ProgressBar", title, message)
        {
        }

        public override void Update()
        {
            time += Time.unscaledDeltaTime;

            if (TitleLabel != null)
            {
                TitleLabel.text = Title;
            }
            if (MessageLabel != null)
            {
                string dot = new string('.', (int)((time % 3) + 1));
                MessageLabel.text = $"{Message} {dot.PadRight(3)}";
            }
        }
    }
    public class ProgressSecondsMask : Mask
    {
        private float time;

        public ProgressSecondsMask(string message)
            : this(null, message)
        {

        }

        public ProgressSecondsMask(string title, string message)
            : base("ProgressBar", title, message)
        {
        }

        public override void Update()
        {
            time += Time.unscaledDeltaTime;

            if (TitleLabel != null)
            {
                TitleLabel.text = Title;
            }
            if (MessageLabel != null)
            {
                MessageLabel.text = $"{Message} ({time:0}s)";
            }
        }
    }
}