using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{
    public class GridView : VisualElement
    {
        Toolbar headerContainer;
        private ListView listView;
        object target;
        List<GridRow> list = new();
        private bool builded;

        public GridView()
        {
            this.AddStyle("GridView");

            headerContainer = new Toolbar();
            headerContainer.AddToClassList("grid-view-header");
            Add(headerContainer);

            listView = new ListView();
            listView.AddToClassList("grid-view-list");
            listView.style.flexGrow = 1f;
            var scrollView = listView.Q<ScrollView>();


            scrollView.Add(new IMGUIContainer(() =>
            {
                if (Event.current.type == EventType.Repaint)
                {
                    if (Columns.Count > 0)
                    {
                        //出现垂直滚动条时，最后一列对齐
                        int scrollWidth = 0;
                        if (scrollView.verticalScroller.style.display == DisplayStyle.Flex)
                        {
                            scrollWidth = (int)scrollView.verticalScroller.layout.width;
                        }

                        var last = Columns.LastOrDefault();
                        if (last.Width > 0)
                        {
                            int lastColumnWidth;
                            lastColumnWidth = last.Width + scrollWidth;
                            var header = headerContainer.Q(className: last.HeaderUssClassName);
                            if (header.style.width != lastColumnWidth)
                                header.style.width = lastColumnWidth;
                        }
                        else
                        {
                            headerContainer.style.paddingRight = scrollWidth;
                        }
                    }
                }
            }));

            Add(listView);
            listView.makeItem = () =>
            {
                VisualElement container = new VisualElement();
                container.AddToClassList("grid-view-row");
                container.style.flexDirection = FlexDirection.Row;
                container.style.alignItems = Align.Center;
                container.style.paddingLeft = 0f;

                int index = 0;
                foreach (var column in AvaliableColumns)
                {
                    VisualElement cellView = makeItem(container, index);
                    if (cellView == null)
                    {
                        cellView = new VisualElement();
                    }
                    cellView.AddToClassList(GetCellUssClassName(index));
                    if (!string.IsNullOrEmpty(column.UssClassName))
                        cellView.AddToClassList(column.UssClassName);
                    container.Add(cellView);
                    index++;
                }

                return container;
            };

            listView.bindItem = (view, index) =>
            {
                if (bindItem == null) return;

                int columnIndex = 0;
                foreach (var column in AvaliableColumns)
                {
                    VisualElement cellView = view.Q(className: GetCellUssClassName(columnIndex));
                    bindItem(view, cellView, index, columnIndex);
                    if (column.Width == 0)
                        cellView.style.flexGrow = 1f;
                    else
                        cellView.style.width = column.Width;
                    columnIndex++;
                }
            };
            listView.unbindItem = (view, index) =>
             {
                 if (unbindItem == null) return;

                 int columnIndex = 0;
                 foreach (var column in AvaliableColumns)
                 {
                     VisualElement cellView = view.Q(className: GetCellUssClassName(columnIndex));
                     unbindItem(view, cellView, index, columnIndex);
                     columnIndex++;
                 }
             };

            listView.destroyItem = (view) =>
            {
                if (destroyItem == null) return;

                int columnIndex = 0;
                foreach (var column in AvaliableColumns)
                {
                    VisualElement cellView = view.Q(className: GetCellUssClassName(columnIndex));
                    destroyItem(view, cellView, columnIndex);
                    columnIndex++;
                }
            };

            schedule.Execute(() =>
            {
                if (!builded)
                    Rebuild();
            });
        }

        public List<ColumnInfo> Columns { get; private set; } = new List<ColumnInfo>();

        public Toolbar HeaderContainer => headerContainer;

        public IList itemsSource { get => listView.itemsSource; set => listView.itemsSource = value; }


        public Func<VisualElement, int, VisualElement> makeItem;
        public Action<VisualElement, VisualElement, int, int> bindItem;
        public Action<VisualElement, VisualElement, int, int> unbindItem;
        public Action<VisualElement, VisualElement, int> destroyItem;

        IEnumerable<ColumnInfo> AvaliableColumns => Columns.Where(o => o.Visiable);

        string GetCellUssClassName(int column)
        {
            return $"grid-view-cell-{column}";
        }

        public void RefreshItems()
        {
            if (!builded)
            {
                Rebuild();
            }
            else
            {
                //listView.itemsSource = list.Where(o => o.isShow).ToList();
                listView.RefreshItems();
            }
        }

        public void Rebuild()
        {
            builded = true;
            headerContainer.Clear();
            bool first = true;
            foreach (var column in AvaliableColumns)
            {
                VisualElement header = null;
                if (column.CreateHeader != null)
                    header = column.CreateHeader(column);
                if (header == null)
                    header = new ToolbarButton();

                if (column.HeaderUssClassName != null)
                    header.AddToClassList(column.HeaderUssClassName);
                if (header is ToolbarMenu)
                {
                    var menuHeader = (ToolbarMenu)header;
                    menuHeader.text = column.Header;
                }
                else if (header is ToolbarButton)
                {
                    var buttonHeader = (ToolbarButton)header;
                    buttonHeader.text = column.Header;
                }
                header.style.unityTextAlign = TextAnchor.MiddleLeft;
                if (column.Width == 0)
                    header.style.flexGrow = 1f;
                else
                    header.style.width = column.Width;
                if (first)
                {
                    first = false;
                    header.style.borderLeftWidth = 1f;
                }
                headerContainer.Add(header);

            }
            listView.Rebuild();
        }

        public class GridRow
        {
            public GridRow(ColumnInfo columnInfo, VisualElement container)
            {
                this.columnInfo = columnInfo;
                this.container = container;
            }

            private ColumnInfo columnInfo;
            public ColumnInfo ColumnInfo => columnInfo;

            private VisualElement container;
            public VisualElement Container => container;

            public GridCell[] cells;

        }

        public class GridCell
        {
            public GridCell(GridView grid, int rowIndex, int columnIndex)
            {
                this.grid = grid;
                this.rowIndex = rowIndex;
                this.columnIndex = columnIndex;
            }

            private GridView grid;
            public VisualElement view;
            public int rowIndex;
            public int columnIndex;
            public GridRow row;
            public int RowIndex => rowIndex;
            public int ColumnIndex => columnIndex;

            public ColumnInfo ColumnInfo => grid.Columns[columnIndex];
        }

    }
}