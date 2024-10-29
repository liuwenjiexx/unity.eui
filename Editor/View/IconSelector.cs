using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace UnityEditor.UIElements.Extension
{
    public class IconSelector : EditorWindow
    {
        static List<IconInfo> allIconNames;
        VisualElement root;
        ListView listView;
        int columnCount;
        static Vector2 cellSize = new Vector2(100, 120);
        private int lastColumnCount;
        [System.NonSerialized]
        private bool listLoaded;
        bool isBuiltin = false;
        bool isProject = false;
        int filterMinSize;
        int filterMaxSize;
        string searchText;
        DateTime? nextLoadListTime;
        ToolbarToggle tglAll, tglBuiltin, tglProject;
        ToolbarToggle tglSize16, tglSize32, tglSize64, tglSize128, tglSizeGreater;
        bool allowBuiltin = true;
        bool allowProject = true;
        IconInfo selected;
        VisualElement selectedView;
        Action<Texture2D> onClosed;
        Action<Texture2D> onUpdated;
        const string ItemSelectedUssClass = "icon-container-selected";

        class IconInfo
        {
            public string name;
            public string assetPath;
            public string guid;
            public bool isBuiltin;
            public bool isProject;
            public bool isIconContent;
            public Texture2D image;
            public int maxSize;

            public static IconInfo None = new IconInfo() { name = "None" };
        }

        private void OnEnable()
        {


        }

        void LoadIcons()
        {
            allIconNames = new();

            foreach (var tex in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (string.IsNullOrEmpty(tex.name))
                    continue;
                var content = EditorGUIUtility.IconContent(tex.name);
                string assetPath = AssetDatabase.GetAssetPath(tex);

                if (content != null && !content.image)
                    content = null;
                if (content == null)
                {
                    if (!EditorUtility.IsPersistent(tex) || string.IsNullOrEmpty(assetPath))
                    {
                        continue;
                    }
                }

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tex, out var guid, out long id);
                IconInfo iconInfo = new IconInfo()
                {
                    name = tex.name,
                    guid = guid,
                    assetPath = assetPath,
                    image = tex,
                    maxSize = Mathf.Max(tex.width, tex.height)
                };

                if (content != null)
                {
                    iconInfo.isIconContent = true;
                    iconInfo.isBuiltin = true;
                }
                else if (assetPath.StartsWith("Assets/"))
                {
                    iconInfo.isProject = true;
                }

                allIconNames.Add(iconInfo);
            }
        }




        private void CreateGUI()
        {

            root = typeof(IconSelector).LoadUXML(rootVisualElement);
            root.style.flexGrow = 1;
            rootVisualElement.AddStyle("EditorUI");
            rootVisualElement.AddStyle(nameof(IconSelector));

            var toolbar = root.Q<Toolbar>();
            tglAll = toolbar.Q<ToolbarToggle>("all");
            tglBuiltin = toolbar.Q<ToolbarToggle>("builtin");
            tglProject = toolbar.Q<ToolbarToggle>("project");

            //tglAll.RegisterValueChangedCallback(e =>
            //{
            //    if (e.newValue)
            //    {
            //        isBuiltin = false;
            //        isProject = false;
            //        Refresh();
            //        LoadList();
            //    }
            //    else
            //    {
            //        tglAll.SetValueWithoutNotify(true);
            //    }
            //});
            tglBuiltin.RegisterValueChangedCallback(e =>
            {
                //if (e.newValue)
                //{
                //isBuiltin = true;
                //isProject = false;
                isBuiltin = e.newValue;
                if (isBuiltin)
                    isProject = false;
                Refresh();
                LoadList();
                //}
                //else
                //{
                //    tglBuiltin.SetValueWithoutNotify(true);
                //}
            });


            tglProject.RegisterValueChangedCallback(e =>
            {
                //if (e.newValue)
                //{
                //    isBuiltin = false;
                //    isProject = true;
                isProject = e.newValue;
                if (isProject)
                    isBuiltin = false;
                Refresh();
                LoadList();
                //}
                //else
                //{
                //    tglProject.SetValueWithoutNotify(true);
                //}
            });
            tglSize16 = toolbar.Q<ToolbarToggle>("size-16");
            tglSize16.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    filterMinSize = 0;
                    filterMaxSize = 16;
                }
                else
                {
                    filterMinSize = 0;
                    filterMaxSize = 0;
                }
                Refresh();
                LoadList();
            });
            tglSize32 = toolbar.Q<ToolbarToggle>("size-32");
            tglSize32.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    filterMinSize = 16;
                    filterMaxSize = 32;
                }
                else
                {
                    filterMinSize = 0;
                    filterMaxSize = 0;
                }
                Refresh();
                LoadList();
            });
            tglSize64 = toolbar.Q<ToolbarToggle>("size-64");
            tglSize64.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    filterMinSize = 32;
                    filterMaxSize = 64;
                }
                else
                {
                    filterMinSize = 0;
                    filterMaxSize = 0;
                }
                Refresh();
                LoadList();
            });
            tglSize128 = toolbar.Q<ToolbarToggle>("size-128");
            tglSize128.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    filterMinSize = 64;
                    filterMaxSize = 128;
                }
                else
                {
                    filterMinSize = 0;
                    filterMaxSize = 0;
                }
                Refresh();
                LoadList();
            });
            tglSizeGreater = toolbar.Q<ToolbarToggle>("size-greater");
            tglSizeGreater.RegisterValueChangedCallback(e =>
            {
                if (e.newValue)
                {
                    filterMinSize = 128;
                    filterMaxSize = 0;
                }
                else
                {
                    filterMinSize = 0;
                    filterMaxSize = 0;
                }
                Refresh();
                LoadList();
            });


            ToolbarSearchField searchField;
            searchField = toolbar.Q<ToolbarSearchField>();
            searchField.SetValueWithoutNotify(searchText);
            searchField.RegisterValueChangedCallback(e =>
            {
                searchText = searchField.value;
                nextLoadListTime = DateTime.Now.AddSeconds(0.3f);
            });

            var gridContainer = root.Q("grid-container");
            listView = new ListView();
            listView.selectionType = SelectionType.None;
            listView.fixedItemHeight = cellSize.y;
            listView.RegisterCallback<GeometryChangedEvent>(e =>
            {
                LoadList();
            });

            listView.makeItem = () =>
            {
                VisualElement container = new VisualElement();
                container.AddToClassList("icon-row");
                container.style.height = cellSize.y;
                for (int i = 0; i < columnCount; i++)
                {
                    VisualElement cellContainer = new VisualElement();
                    cellContainer.AddToClassList("icon-container");
                    cellContainer.style.width = cellSize.x;
                    cellContainer.style.height = cellSize.y;
                    Image img = new Image();
                    img.AddToClassList("icon-img");
                    cellContainer.Add(img);
                    Label label = new Label();
                    label.AddToClassList("icon-label");
                    cellContainer.Add(label);
                    Label sizeLabel = new Label();
                    sizeLabel.AddToClassList("icon-size");
                    cellContainer.Add(sizeLabel);

                    cellContainer.AddManipulator(new MenuManipulator(e =>
                    {
                        var iconInfo = cellContainer.userData as IconInfo;
                        e.menu.AppendAction("Ping", act =>
                        {
                            EditorGUIUtility.PingObject(iconInfo.image);
                        });
                        e.menu.AppendAction("Copy Name", act =>
                        {
                            EditorGUIUtility.systemCopyBuffer = iconInfo.name;
                        });
                        e.menu.AppendAction("Copy AssetPath", act =>
                        {
                            EditorGUIUtility.systemCopyBuffer = iconInfo.assetPath;
                        });
                        e.menu.AppendAction("Copy Code", act =>
                        {
                            EditorGUIUtility.systemCopyBuffer = $"EditorGUIUtility.IconContent(\"{iconInfo.name}\")";
                        });
                    }));
                    cellContainer.RegisterCallback<MouseDownEvent>(e =>
                    {
                        var iconInfo = cellContainer.userData as IconInfo;
                        var image = iconInfo.image;
                        if (selected != iconInfo)
                        {
                            int index;
                            if (selected != null)
                            {
                                index = FindItemIndex(selected);
                                listView.RefreshItem(index);
                            }
                            selected = iconInfo;
                            index = FindItemIndex(selected);
                            listView.RefreshItem(index);
                        }


                        if (e.clickCount > 1)
                        {
                            root.schedule.Execute(() =>
                            {
                                Close();
                            });
                        }
                        else
                        {
                            onUpdated?.Invoke(image);
                        }
                    });

                    container.Add(cellContainer);
                }
                return container;
            };
            listView.bindItem = (view, index) =>
            {
                var items = listView.itemsSource[index] as IconInfo[];
                for (int i = 0; i < columnCount; i++)
                {
                    var cellView = view.Children().ElementAt(i);
                    var iconInfo = items[i];
                    if (iconInfo == null)
                    {
                        cellView.style.display = DisplayStyle.None;
                        continue;
                    }
                    if (selected == iconInfo)
                    {
                        selectedView = cellView;
                        if (!cellView.ClassListContains(ItemSelectedUssClass))
                        {
                            cellView.AddToClassList(ItemSelectedUssClass);
                        }
                    }
                    else
                    {
                        cellView.RemoveFromClassList(ItemSelectedUssClass);
                    }

                    cellView.userData = iconInfo;
                    cellView.style.display = DisplayStyle.Flex;
                    var tex = iconInfo.image;
                    var img = cellView.Q<Image>(className: "icon-img");
                    var label = cellView.Q<Label>(className: "icon-label");
                    var sizeLabel = cellView.Q<Label>(className: "icon-size");
                    if (tex)
                    {
                        img.style.display = DisplayStyle.Flex;
                        img.image = tex;
                        label.text = $"{iconInfo.name}";
                        sizeLabel.text = $"({tex.width}x{tex.height})";
                        cellView.tooltip = $"{iconInfo.name}\nAssetPath: {iconInfo.assetPath}";
                    }
                    else
                    {
                        img.image = null;
                        label.text = iconInfo.name;
                        sizeLabel.text = null;
                        img.style.display = DisplayStyle.None;
                        cellView.tooltip = "None";
                    }


                }
            };
            gridContainer.Add(listView);
            searchField.Focus();
            Refresh();
            LoadList();
        }

        int FindItemIndex(IconInfo item)
        {
            if (item == null) return -1;
            for (int i = 0; i < listView.itemsSource.Count; i++)
            {
                var items = listView.itemsSource[i] as IconInfo[];
                if (items.Contains(item))
                    return i;
            }
            return -1;
        }

        void Refresh()
        {
            if (allowBuiltin)
            {
                tglBuiltin.style.display = DisplayStyle.Flex;
                tglBuiltin.SetValueWithoutNotify(isBuiltin);
            }
            else
            {
                tglBuiltin.style.display = DisplayStyle.None;
            }
            tglAll.SetValueWithoutNotify(!(isBuiltin || isProject));
            tglProject.SetValueWithoutNotify(isProject);

            tglSize16.SetValueWithoutNotify(false);
            tglSize32.SetValueWithoutNotify(false);
            tglSize64.SetValueWithoutNotify(false);
            tglSize128.SetValueWithoutNotify(false);
            tglSizeGreater.SetValueWithoutNotify(false);

            switch (filterMaxSize)
            {
                case 16:
                    tglSize16.SetValueWithoutNotify(true);
                    break;
                case 32:
                    tglSize32.SetValueWithoutNotify(true);
                    break;
                case 64:
                    tglSize64.SetValueWithoutNotify(true);
                    break;
                case 128:
                    tglSize128.SetValueWithoutNotify(true);
                    break;
                case 0:
                    if (filterMinSize == 128)
                        tglSizeGreater.SetValueWithoutNotify(true);
                    break;
            }

        }

        private void OnDestroy()
        {
            if (selected != null && selected.image)
            {
                onClosed?.Invoke(selected.image);
            }
        }

        void LoadList()
        {
            if (listView == null) return;

            LoadIcons();

            columnCount = Mathf.FloorToInt((listView.localBound.width - 10) / cellSize.x);
            if (columnCount < 1) return;

            IEnumerable<IconInfo> sourceItems = allIconNames;

            if (!allowBuiltin)
            {
                sourceItems = sourceItems.Where(o => !o.isBuiltin);
            }

            if (isBuiltin)
            {
                sourceItems = sourceItems.Where(o => o.isBuiltin);
            }
            if (isProject)
            {
                sourceItems = sourceItems.Where(o => o.isProject);
            }

            if (filterMaxSize > 0)
            {
                sourceItems = sourceItems.Where(o => filterMinSize < o.maxSize && o.maxSize <= filterMaxSize);
            }
            else if (filterMinSize > 0)
            {
                sourceItems = sourceItems.Where(o => filterMinSize < o.maxSize);
            }


            if (!string.IsNullOrEmpty(searchText))
            {
                sourceItems = sourceItems.Where(o => o.name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase));
            }

            sourceItems = sourceItems.OrderBy(o => o.name);
            sourceItems = new IconInfo[] { IconInfo.None }.Concat(sourceItems);

            var sourceList = sourceItems.ToArray();
            List<IconInfo[]> list = new List<IconInfo[]>();
            for (int i = 0; i < sourceList.Length; i += columnCount)
            {
                IconInfo[] items = new IconInfo[columnCount];
                for (int j = 0; j < columnCount; j++)
                {
                    int index = i + j;
                    if (index >= sourceList.Length)
                        break;
                    items[j] = sourceList[index];
                }
                list.Add(items);
            }

            listView.itemsSource = list;


            if (lastColumnCount != columnCount || !listLoaded)
            {
                lastColumnCount = columnCount;
                listLoaded = true;

                listView.Rebuild();
            }
            else
            {
                listView.RefreshItems();
            }
        }

        private void Update()
        {
            if (nextLoadListTime.HasValue && DateTime.Now > nextLoadListTime.Value)
            {
                nextLoadListTime = null;
                LoadList();
            }
        }

        public static void Show(bool allowBuiltin, Action<Texture2D> onClosed, Action<Texture2D> onUpdated = null)
        {
            var win = CreateInstance<IconSelector>();
            win.allowBuiltin = allowBuiltin;
            win.onClosed = onClosed;
            win.onUpdated = onUpdated;
            win.ShowAuxWindow();
            win.position = new Rect(100, 0, 800, 400);
            win.minSize = new Vector2(800, 400);
            win.CenterOnMainWin();
        }

    }
}