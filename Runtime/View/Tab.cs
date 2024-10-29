using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{
    public class Tab
    {
        private string defaultTab;
        private VisualElement tabItemContainer;
        private VisualElement tabContentContainer;

        private string activeTab;

        public const string TabItemPrefix = "tab-";
        public const string TabContentPrefix = "tab-content-";

        public Tab(VisualElement root)
        {

            tabItemContainer = root.Q(className: "tab-item-container");
            tabContentContainer = root.Q(className: "tab-content-container");

            string firstTabName = null;
            foreach (var tabItem in tabItemContainer.Children())
            {
                if (!tabItem.name.StartsWith(TabItemPrefix))
                    continue;
                string tabName = tabItem.name.Substring(TabItemPrefix.Length);
                if (firstTabName == null)
                    firstTabName = tabName;
                tabItem.RegisterCallback<MouseDownEvent>((e) =>
                {
                    ShowTab(tabName);
                });
            }



            var defaultTabItem = tabItemContainer.Q(className: "tab-item-default");
            if (defaultTabItem != null && defaultTabItem.name.StartsWith(TabItemPrefix))
            {
                defaultTab = defaultTabItem.name.Substring(TabItemPrefix.Length);
            }

            if (string.IsNullOrEmpty(defaultTab))
            {
                defaultTab = firstTabName;
            }

            ShowTab(defaultTab);

        }

        public string DefaultTab
        {
            get => defaultTab;
            set => defaultTab = value;
        }

        public string ActiveTab => activeTab;

        public event TabChangedDelegate TabChanged;

        public delegate void TabChangedDelegate(string newTab, string oldTab);


        public void ShowTab(string tabName)
        {

            if (activeTab == tabName)
                return;

            foreach (var tabItem in tabItemContainer.Children())
            {
                if (tabItem.name == TabItemPrefix + tabName)
                {
                    tabItem.AddToClassList("tab-item-active");
                }
                else
                {
                    tabItem.RemoveFromClassList("tab-item-active");
                }
            };

            foreach (var tabItem in tabContentContainer.Children())
            {
                if (tabItem.name == TabContentPrefix + tabName)
                {
                    tabItem.style.display = DisplayStyle.Flex;
                }
                else
                {
                    tabItem.style.display = DisplayStyle.None;
                }
            }

            string oldTab = tabName;
            activeTab = tabName;
            TabChanged?.Invoke(activeTab, oldTab);
        }

        public VisualElement GetTabItem(string tabName)
        {
            string name = TabItemPrefix + tabName;
            return tabItemContainer.Children().FirstOrDefault(o => o.name == name);
        }

        public VisualElement GetTabContent(string tabName)
        {
            string name = TabContentPrefix + tabName;
            return tabContentContainer.Children().FirstOrDefault(o => o.name == name);
        }

    }
}
