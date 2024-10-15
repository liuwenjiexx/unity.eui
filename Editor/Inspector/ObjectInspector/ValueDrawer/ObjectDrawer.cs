using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{
    public class ObjectDrawer : ValueDrawer
    {
        List<ArrayItem> Children { get; set; } = new List<ArrayItem>();
        VisualElement contentContainer;


        class ArrayItem
        {
            public ValueDrawer drawer;
            public VisualElement view;
        }

        public override void CreateDrawer()
        {
            foreach (var drawer in Inspector.CreateDrawer(Value, this, PropertyPath))
            {
                Children.Add(new ArrayItem() { drawer = drawer });
            }
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
            headerContainer.Add(foldout);
            container.Add(headerContainer);


            container.Add(contentContainer);

            CreateChildrenUI();

            return container;
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

        void CreateChildrenUI()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var item = Children[i];
                var drawer = item.drawer;
                if (!drawer.enabled)
                {
                    drawer.enabled = true;
                    drawer.OnEnable();
                }

                var content = drawer.CreateUI();
                if (content != null)
                {
                    contentContainer.Add(content);
                    Inspector.UpdateIndent(drawer, content);
                }
            }
        }
    }
}
