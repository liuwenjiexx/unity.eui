using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{
    public class DragBarManipulator : MouseManipulator
    {
        public bool verticalDirection;
        private bool isDraging;
        Vector2 downMousePos;
        Vector2 pos;
        private Vector2 startSize;
        public VisualElement container;
        public VisualElement controlTarget;
        private VisualElement targetContainer;
        public Vector2 anchor = new Vector2(0.5f, 0.5f);
        public Length minValue;
        public Length maxValue = new Length(100f, LengthUnit.Percent);
        public OnNotifyDelegate onNotify;
        public Action onStart;
        public Func<Vector2, Vector2> validate;
        public Vector2 multiFactor = Vector2.one;

        public delegate void OnNotifyDelegate(Vector2 startSize, Vector2 offset);


        public DragBarManipulator(VisualElement controlTarget, VisualElement container, Func<Vector2, Vector2> validate = null, OnNotifyDelegate onNotify = null)
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            this.container = container;
            this.controlTarget = controlTarget;
            this.validate = validate;
            this.onNotify = onNotify;

        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
            if (targetContainer != null)
            {
                targetContainer.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
                targetContainer.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
            }
        }

        void OnMouseDownEvent(MouseDownEvent e)
        {
            if (e.button == 0)
            {
                if (!isDraging)
                {
                    targetContainer = GetContainer();
                    downMousePos = target.ChangeCoordinatesTo(targetContainer, e.localMousePosition);
                    isDraging = true;
                    target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
                    target.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
                    MouseCaptureController.CaptureMouse(target);
                    startSize = controlTarget.layout.size;
                    onStart?.Invoke();
                }
            }
        }


        void OnMouseMoveEvent(MouseMoveEvent e)
        {
            if (isDraging)
            {

                Vector2 mousePos = target.ChangeCoordinatesTo(targetContainer, e.localMousePosition);
                Vector2 offset = mousePos - downMousePos;
                var newSize = startSize + Vector2.Scale(offset, multiFactor);
                newSize = ClipPos(newSize);

                if (controlTarget != null)
                {
                    if (verticalDirection)
                    {
                        controlTarget.style.height = newSize.y;
                    }
                    else
                    {
                        controlTarget.style.width = newSize.x;
                    }
                    onNotify?.Invoke(startSize, offset);
                }
            }
        }

        void OnMouseUpEvent(MouseUpEvent e)
        {

            if (isDraging)
            {
                isDraging = false;
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
                MouseCaptureController.ReleaseMouse(target);
            }
        }


        VisualElement GetContainer()
        {
            if (container == null)
                return target.parent;
            return container;
        }

        Vector2 ClipPos(Vector2 pos)
        {
            if (validate != null)
                return validate(pos);

            Rect containerRect = GetContainer().layout;
            var size = target.layout.size;
            if (verticalDirection)
            {
                pos.y = Mathf.Clamp(pos.y, GetSize(containerRect.height, minValue) - size.y * anchor.y, GetSize(containerRect.height, maxValue) - size.y * anchor.y);
            }
            else
            {
                pos.x = Mathf.Clamp(pos.x, GetSize(containerRect.width, minValue) - size.x * anchor.x, GetSize(containerRect.width, maxValue) - size.x * anchor.x);
            }
            return pos;
        }

        float GetSize(float size, Length length)
        {
            if (length.unit == LengthUnit.Percent)
                return size * (length.value / 100f);
            return length.value;
        }

    }
}
