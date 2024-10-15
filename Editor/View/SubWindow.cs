using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{

    public class SubWindow
    {
        private VisualElement root;
        SubWindowItem activeWindow;
        List<SubWindowItem> windowList;
        VisualElement tabContainer;
        VisualElement contentContainer;

        const string SubWindowUSS = "sub-window";
        const string SubWindowTabContainerUSS = "sub-window-tab-container";
        const string SubWindowContentContainerUSS = "sub-window-content-container";
        const string SubWindowTabSelectedUSS = "sub-window-tab-selected";
        const string SubWindowTabUSSPrefix = "sub-window-tab-";
        const string SubWindowContentUSSPrefix = "sub-window-content-";
        const string SubWindowTabUSS = "sub-window-tab";
        const string SubWindowTabTextUSS = "sub-window-tab-text";
        const string SubWindowTabCloseButtonUSS = "sub-window-tab-close-button";
        const string SubWindowContentUSS = "sub-window-content";

        public SubWindow()
        {
            windowList = new();
        }

        public void SetWindowTitle(string identity, string title)
        {
            var window = FindWindowItem(identity);
            if (window == null)
                return;
            window.title = title;
            window.tabItem.Q<Label>().text = title;
        }

        public void ShowWindow(SubWindowOptions options)
        {
            SubWindowItem window;

            if (!string.IsNullOrEmpty(options.Identity))
            {
                window = FindWindowItem(options.Identity);
                if (window == null)
                {
                    window = _CreateWindow(options);
                }
            }
            else
            {
                window = _CreateWindow(options);
            }

            ShowWindow(window.identity);
        }

        public void ShowWindow(string identity)
        {
            if (activeWindow != null && activeWindow.identity == identity)
                return;

            if (activeWindow != null)
            {
                if (activeWindow.window != null)
                {
                    //activeTab.window.OnInactive();
                }
            }

            foreach (var tabItem in windowList)
            {
                if (tabItem.identity == identity)
                {
                    tabItem.tabItem.AddToClassList(SubWindowTabSelectedUSS);
                    tabItem.contentContainer.style.display = DisplayStyle.Flex;
                    activeWindow = tabItem;
                }
                else
                {
                    tabItem.tabItem.RemoveFromClassList(SubWindowTabSelectedUSS);
                    tabItem.contentContainer.style.display = DisplayStyle.None;
                }
            }

            if (activeWindow == null || activeWindow.identity != identity)
            {
                return;
            }


            if (activeWindow.window == null)
            {
                var win = Activator.CreateInstance(activeWindow.windowType) as IView;
                win.Target = activeWindow.userData;
                win.OnEnable();
                activeWindow.viewRoot = win.CreateUI();

                if (activeWindow.viewRoot != null)
                {
                    activeWindow.contentContainer.Add(activeWindow.viewRoot);
                }
                activeWindow.window = win;
            }
            activeWindow.window.OnActive();
        }

        public bool HasWindow(string identity)
        {
            return FindWindowItem(identity) != null;
        }

        public IView FindWindow(string identity)
        {
            return FindWindowItem(identity)?.window;
        }

        private SubWindowItem FindWindowItem(string identity)
        {
            return windowList.FirstOrDefault(o => o.identity == identity);
        }

        public IView CreateWindow(SubWindowOptions options)
        {
            return _CreateWindow(options)?.window;
        }

        private SubWindowItem _CreateWindow(SubWindowOptions options)
        {
            SubWindowItem windowItem = new SubWindowItem();

            VisualElement tabItemParent = new VisualElement();
            tabItemParent.AddToClassList(SubWindowTabUSS);
            Label tabItemTitle = new Label();
            tabItemTitle.AddToClassList(SubWindowTabTextUSS);
            tabItemTitle.text = options.Title;
            tabItemParent.Add(tabItemTitle);

            Label tabItemCloseButton = new Label();
            tabItemCloseButton.AddToClassList(SubWindowTabCloseButtonUSS);
            tabItemCloseButton.text = "X";
            tabItemParent.Add(tabItemCloseButton);

            tabItemCloseButton.RegisterCallback<ClickEvent>(e =>
            {
                e.StopImmediatePropagation();
                CloseWindow(windowItem);
            });

            if ((options.Flags & SubWindowFlags.NonClosable) == SubWindowFlags.NonClosable)
            {
                tabItemCloseButton.style.display = DisplayStyle.None;
            }

            tabContainer.Add(tabItemParent);

            if (string.IsNullOrEmpty(options.Identity))
            {
                windowItem.identity = $"gen-{Guid.NewGuid().ToString("N")}";
            }
            else
            {
                windowItem.identity = options.Identity;
            }

            windowItem.title = options.Title;
            windowItem.windowType = options.WindowType;
            windowItem.userData = options.UserData;
            windowItem.tabItem = tabItemParent;

            var contentContainer = new VisualElement();
            contentContainer.AddToClassList(SubWindowContentUSS);
            contentContainer.style.display = DisplayStyle.None;
            this.contentContainer.Add(contentContainer);
            windowItem.contentContainer = contentContainer;

            windowList.Add(windowItem);


            tabItemParent.RegisterCallback<ClickEvent>(e =>
            {
                ShowWindow(windowItem.identity);
            });

            return windowItem;
        }
        public void CloseWindow(string identity)
        {
            var subWindow = FindWindowItem(identity);
            if (subWindow == null)
                return;
            CloseWindow(subWindow);
        }
        private void CloseWindow(SubWindowItem subWindow)
        {
            if (subWindow == null)
                return;

            int index = windowList.IndexOf(subWindow);
            if (index == -1)
                return;


            if (subWindow.window != null)
            {
                subWindow.window.OnDisable();
                subWindow.window = null;
            }

            if (subWindow.tabItem != null)
            {
                subWindow.tabItem.parent.Remove(subWindow.tabItem);
                subWindow.tabItem = null;
            }

            if (subWindow.contentContainer != null)
            {
                subWindow.contentContainer.parent.Remove(subWindow.contentContainer);
                subWindow.contentContainer = null;
            }

            windowList.RemoveAt(index);

            SubWindowItem next = null;
            if (index < windowList.Count)
            {
                next = windowList[index];
            }
            else if (index > 0)
            {
                next = windowList[index - 1];
            }
            if (next != null)
            {
                ShowWindow(next.identity);
            }
        }

        public void CloseAllWindow()
        {
            foreach (var windowItem in windowList.ToArray())
            {
                CloseWindow(windowItem);
            }
        }


        public static SubWindow Attach(VisualElement root)
        {
            
            SubWindow subWindow = new SubWindow();            
            subWindow.root = root;
            EditorUIUtility.AddStyle(root, nameof(SubWindow));
            root.AddToClassList(SubWindowUSS);

            subWindow.tabContainer = root.Q(className: SubWindowTabContainerUSS);
            subWindow.contentContainer = root.Q(className: SubWindowContentContainerUSS);
            subWindow.tabContainer.Clear();
            subWindow.contentContainer.Clear();

            return subWindow;
        }



        class SubWindowItem
        {
            public string identity;
            public string title;
            public Type windowType;
            public object userData;
            public VisualElement tabItem;
            public VisualElement contentContainer;
            public IView window;
            public VisualElement viewRoot;
            public SubWindowFlags flags;
        }
    }

    [Flags]
    public enum SubWindowFlags
    {
        None,
        NonClosable = 1 << 0,
    }

    public class SubWindowOptions
    {
        public string Identity { get; set; }
        public string Title { get; set; }
        public Type WindowType { get; set; }
        public object UserData { get; set; }

        public SubWindowFlags Flags { get; set; }
    }

}
