using Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Editor;

namespace Unity.UI.Editor
{
    [MemberDrawer(typeof(Array))]
    internal class ArrayDrawer : ValueDrawer
    {
        List<ArrayItem> Children { get; set; } = new();

        VisualElement contentContainer;
        private Type itemType;

        class ArrayItem
        {
            public ValueDrawer drawer;
            public VisualElement view;
        }

        public override void CreateDrawer()
        {
            
            if (ValueType.IsArray)
            {
                Array array = Value as Array;
                itemType = ValueType.GetElementType();
                for (int i = 0; i < array.Length; i++)
                {
                    var item = array.GetValue(i);
                    if (item == null)
                    {
                        item = itemType.CreateInstance();
                        array.SetValue(item, i);
                    }

                    CreateItemDrawer(item, i);
                }
            }
            else
            {
                IList list = (IList)Value;
                Type listType = Value.GetType().FindByGenericTypeDefinition(typeof(IList<>));
                itemType = listType.GetGenericArguments()[0];

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null)
                    {
                        item = itemType.CreateInstance();
                        list[i] = item;
                    }
                    CreateItemDrawer(item, i);
                }
            }
        }

        ValueDrawer CreateItemDrawer(object value, int index)
        {
            var drawer = Inspector.CreateValueDrawer($"{index}", value, itemType, this, GetItemPropertyPath(index));
            Children.Add(new ArrayItem() { drawer = drawer });

            drawer.CreateDrawer();
            return drawer;
        }

        void CreateItemUI(ValueDrawer drawer)
        {
            VisualElement view = null;

            if (!drawer.enabled)
            {
                drawer.enabled = true;
                drawer.OnEnable();
            }

            view = drawer.CreateUI();
            if (view != null)
            {
                contentContainer.Add(view);
                Inspector.UpdateIndent(drawer, view);

                int index = Children.FindIndex(o => o.drawer == drawer);

                var label = view.Q(className: "unity-base-field__label");
                if (label == null)
                {
                    label = view;
                }
                view.AddManipulator(new MenuManipulator(e =>
                {
                    e.menu.AppendAction($"Delete [{index}]", act =>
                    {
                        if (index >= 0 && index < Children.Count)
                        {

                            if (ValueType.IsArray)
                            {
                                Array array = (Array)Value;
                                Array newArray = Array.CreateInstance(itemType, array.Length - 1);
                                for (int k = 0, n = 0; k < array.Length; k++)
                                {
                                    if (k != index)
                                    {
                                        newArray.SetValue(array.GetValue(k), n++);
                                    }
                                }
                            }
                            else
                            {
                                var list = (IList)Value;
                                list.RemoveAt(index);
                            }
                            if (view != null)
                            {
                                view.parent.Remove(view);
                            }
                            Children.RemoveAt(index);
                            Rebuild();
                            Inspector.Diried = true;
                        }
                    });
                }));

            }
            var item = Children.First(o => o.drawer == drawer);
            item.view = view;
        }


        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < Children.Count; i++)
            {
                var item = Children[i];
                var drawer = item.drawer;
                drawer.Clear();
                item.view = null;
            }
            Children.Clear();
            if (contentContainer != null)
            {
                contentContainer.Clear();
            }
        }

        protected  void Rebuild()
        {
            Clear();
            CreateDrawer();
             
            CreateChildrenUI();
            BindingSet.Bind();
        }

        string GetItemPropertyPath(int index)
        {
            return $"{PropertyPath}[{index}]";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            foreach (var child in Children)
            {
                child.drawer.OnEnable();
            }
        }

        public override void OnDisable()
        {
            foreach (var child in Children)
            {
                child.drawer.OnDisable();
            }

            base.OnDisable();
        }

        public override VisualElement CreateUI()
        {
            VisualElement container = new VisualElement();
            VisualElement headerContainer = new VisualElement();

            headerContainer.AddToClassList("unity-base-field__label");
            headerContainer.style.flexDirection = FlexDirection.Row;

            contentContainer = new VisualElement();

            Foldout foldout = new Foldout();
            //foldout.style.width = 18;
            foldout.text = DisplayName;
            foldout.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    contentContainer.style.display = DisplayStyle.Flex;
                }
                else
                {
                    contentContainer.style.display = DisplayStyle.None;
                }
            });
            foldout.SetValueWithoutNotify(true);
            foldout.style.flexGrow = 1f;
            headerContainer.Add(foldout);

            Label add = new Label();
            add.text = "+";
            add.RegisterCallback<MouseDownEvent>(e =>
            {
                object value = itemType.CreateInstance();
                int index;
                if (ValueType.IsArray)
                {
                    Array array = (Array)Value;
                    Array newArray = Array.CreateInstance(itemType, array.Length + 1);
                    index = newArray.Length - 1;
                    array.CopyTo(newArray, 0);
                    newArray.SetValue(value, index);
                }
                else
                {
                    IList list = (IList)Value;
                    list.Add(value);
                    index = list.Count - 1;
                }
                Inspector.Diried = true;
                var drawer = CreateItemDrawer(value, index);
                Rebuild();
            });
            headerContainer.Add(add);

            container.Add(headerContainer);


            container.Add(contentContainer);

            CreateChildrenUI();

            return container;
        }

        void CreateChildrenUI()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var item = Children[i];
                var drawer = item.drawer;
                if (!drawer.enabled)
                {
                    drawer.enabled= true;
                    drawer.OnEnable();
                }
                CreateItemUI(drawer);
            }
        }
 
    }
}
