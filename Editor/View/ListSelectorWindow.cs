using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.UIElements.Extension
{
    public class ListSelectorWindow : EditorWindow
    {
        VisualElement root;
        ListView listView;
        Func<IEnumerable<object>> load;
        Func<object, string> getName;
        private Func<object, bool> isSelect;
        private Action<object, bool> onSelectChange;
        ToolbarSearchField searchField;
         

        private void CreateGUI()
        {
            if(load==null)
            {
                DestroyImmediate(this);  
                return;
            }

            root = new VisualElement();
            root.style.flexGrow = 1;
            rootVisualElement.Add(root);

            Toolbar toolbar = new Toolbar();
            ToolbarSpacer spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1f;
            toolbar.Add(spacer);
            searchField = new ToolbarSearchField();
            searchField.style.width = 0;
            searchField.style.flexGrow = 0.8f;
            searchField.focusable = true;
            toolbar.Add(searchField);
            root.Add(toolbar);


            listView = new ListView();
            listView.style.flexGrow = 1f;
            listView.fixedItemHeight = 25;
            listView.selectionType = SelectionType.Multiple;

            root.Add(listView);

            listView.makeItem = () =>
            {
                VisualElement container = new VisualElement();
                container.AddToClassList("list-item");
                container.style.justifyContent = Justify.Center;

                {
                    VisualElement itemContainer = new VisualElement();
                    itemContainer.AddToClassList("list-item_item");
                    itemContainer.style.flexDirection = FlexDirection.Row;                    

                    Toggle check = new Toggle();
                    check.AddToClassList("list-item_check");
                    check.style.marginTop = 2;
                    itemContainer.Add(check);
                    check.RegisterValueChangedCallback(e =>
                    {
                        var item = container.userData;
                        IEnumerable<object> items = GetSelectedItems();
                        if (!items.Any() || !items.Contains(item))
                        {
                            items = new[] { item };
                        }

                        if (IsSelectedCheck(items))
                        {
                            if (!e.newValue)
                            {
                                foreach (var _item in items)
                                {
                                    onSelectChange(_item, false);
                                }
                            }
                        }
                        else
                        {
                            if (e.newValue)
                            {
                                foreach (var _item in items)
                                {
                                    onSelectChange(_item, true);
                                }
                            }
                        }

                        listView.RefreshItems();
                    });

                    Label nameLabel = new Label();
                    nameLabel.AddToClassList("list-item_name");
                    itemContainer.Add(nameLabel);
                    container.Add(itemContainer);
                }

                {
                    VisualElement categoryContainer = new VisualElement();
                    categoryContainer.AddToClassList("list-item_category");
                    Label nameLabel = new Label();
                    nameLabel.AddToClassList("list-item_category_name");
                    categoryContainer.Add(nameLabel);
                    container.Add(categoryContainer);
                }

                return container;
            };

            listView.bindItem = (view, index) =>
            {
                var item = listView.itemsSource[index];
                view.userData = item;
                var propertyContainer = view.Q(className: "list-item_item");
                var categoryContainer = view.Q(className: "list-item_category");
                propertyContainer.style.display = DisplayStyle.None;
                categoryContainer.style.display = DisplayStyle.None;


                {
                    propertyContainer.style.display = DisplayStyle.Flex;

                    Toggle check = propertyContainer.Q<Toggle>(className: "list-item_check");
                    check.SetValueWithoutNotify(isSelect(item));

                    Label nameLabel = propertyContainer.Q<Label>(className: "list-item_name");
                    nameLabel.text =getName(item);
                }
          

            };

            LoadList();
        }

        void LoadList()
        {

            IEnumerable<object> items = load();
            string searchText = searchField.value;
            if (!string.IsNullOrEmpty(searchText))
            {
                items = items.Where(
                    o =>
                    {
                        string name = getName(o);
                        if (name.Contains(searchText, StringComparison.InvariantCulture))
                            return true;
                        return false;
                    });
            }

            listView.itemsSource = items.ToList();
            listView.RefreshItems();
        }

        IEnumerable<object> GetSelectedItems()
        {
            return listView.selectedItems.Where(o => o != null);
        }

        bool IsSelectedCheck(IEnumerable<object> items)
        {
            bool allCheck = items
                .All(o => isSelect(o));
            return allCheck;
        }

        public static void Show(Func<IEnumerable<object>> load, Func<object, string> getName, Func<object, bool> isSelect, Action<object, bool> onSelectChange)
        {
            var win = CreateInstance<ListSelectorWindow>();
            win.load = load;
            win.getName = getName;
            win.isSelect = isSelect;
            win.onSelectChange = onSelectChange;
            win.position = new Rect(100, 0, 400, 400);
            win.minSize = new Vector2(400, 400);
            win.ShowAuxWindow();
            //win.Show();
            win.CenterOnMainWin();
        }
    }
}