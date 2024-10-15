using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{

    public class DragManipulator : MouseManipulator
    {
        private bool isReady;
        private bool isDraging;
        private Func<bool> onStartDrag;
        bool captureMouse = false;

        public DragManipulator(Func<bool> onStartDrag)
        {
            this.onStartDrag = onStartDrag;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);
            //target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent, TrickleDown.TrickleDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            target.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);

        }
        void OnMouseDownEvent(MouseDownEvent e)
        {
            if (!CanStartManipulation(e))
                return;

            if (e.currentTarget == target && e.button == 0)
            {
                isReady = true;
                isDraging = false;

                //if (isReady)
                //{
                //    isReady = false;

                //    if (onStartDrag())
                //    {
                //        isDraging = true;
                //    }
                //}
                //  e.StopPropagation();
            }

        }


        void OnMouseMoveEvent(MouseMoveEvent e)
        {
            if (isReady)
            {
                isReady = false;

                if (onStartDrag())
                {
                    isDraging = true;
                }
            }
            //   e.StopImmediatePropagation();
            //e.StopPropagation();
        }
        void OnMouseUpEvent(MouseUpEvent e)
        {
            isReady = false;
            isDraging = false;
        }
            void OnDragUpdatedEvent(DragUpdatedEvent e)
        {
            if (isDraging)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
        }

    }
}
