using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

namespace UnityEngine.UIElements.Extension
{
    public class GridView : VisualElement
    {

        ScrollView scrollView;

        public Func<VisualElement> makeItem;
        public Action<VisualElement, int> bindItem;

        public IList itemsSource;
        private List<Item> items = new List<Item>();
        private List<Item> unusedItems = new List<Item>();
        private VisualElement placeholder;
        private VisualElement itemsContainer;
        public Vector2Int cellSize;
        int rowCount;
        int columnCount;

        private float maxHeight;
        private Item lastHoverItem;
        private int selectedIndex;
        private int lastDownIndex;

        public GridView()
        {
            this.scrollView = new ScrollView();
            scrollView.style.flexGrow = 1f;
            scrollView.verticalScroller.valueChanged += VerticalScroller_valueChanged;
            Add(scrollView);
            itemsContainer = new VisualElement();
            scrollView.contentContainer.Add(itemsContainer);
            placeholder = new VisualElement();
            scrollView.contentContainer.Add(placeholder);

            RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (value != selectedIndex)
                {
                    selectedIndex = value;
                    SelectChanged(selectedIndex);
                }
            }
        }

        public event Action<int> SelectChanged;
        public event Action<int> ItemClick;

        bool IsAvaliableCell(Vector2Int cell)
        {
            if (!(0 <= cell.x && cell.x < columnCount && 0 <= cell.y && cell.y < rowCount))
                return false;
            int index = CellToIndex(cell);
            if (index < 0 || index >= itemsSource.Count)
                return false;
            return true;
        }

        Vector2Int MousePositionToCell(Vector2 mousePos)
        {
            var pos = this.ChangeCoordinatesTo(scrollView.contentContainer, mousePos);
            return new Vector2Int((int)(pos.x / cellSize.x), (int)(pos.y / cellSize.y));
        }

        public Vector2Int IndexToCell(int index)
        {
            return new Vector2Int(index % columnCount, index / columnCount);
        }
        public int CellToIndex(Vector2Int cell)
        {
            return cell.y * columnCount + cell.x;
        }


        private void VerticalScroller_valueChanged(float value)
        {
            RefreshItems();
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            RefreshItems();
        }

        void OnMouseDownEvent(MouseDownEvent e)
        {
            var cell = MousePositionToCell(e.localMousePosition);

            if (IsAvaliableCell(cell))
            {
                int index = CellToIndex(cell);
                lastDownIndex = index;
                SelectedIndex = index;
                e.StopPropagation();
            }
            else
            {
                lastDownIndex = -1;
            }
        }

        void OnMouseMoveEvent(MouseMoveEvent e)
        {
            var cell = MousePositionToCell(e.localMousePosition);
            Item item = null;
            if (IsAvaliableCell(cell))
            {
                int index = CellToIndex(cell);
                item = items.FirstOrDefault(o => o.dataIndex == index);
            }
            if (lastHoverItem != item)
            {
                if (lastHoverItem != null)
                {
                    OnItemUnhover(lastHoverItem);
                    lastHoverItem = null;
                }
                lastHoverItem = item;
                if (item != null)
                {
                    OnItemHover(item);
                }
            }
            e.StopPropagation();
        }

        void OnMouseUpEvent(MouseUpEvent e)
        {
            var cell = MousePositionToCell(e.localMousePosition);

            if (IsAvaliableCell(cell))
            {
                int index = CellToIndex(cell);
                if (lastDownIndex == index)
                {
                    ItemClick?.Invoke(index);
                }
                e.StopPropagation();
            }
        }

        void UnusedItem(Item item)
        {
            if (lastHoverItem == item)
            {
                OnItemUnhover(lastHoverItem);
                lastHoverItem = null;
            }

            item.dataIndex = -1;
            item.container.style.display = DisplayStyle.None;
            unusedItems.Add(item);
        }

        void OnItemHover(Item item)
        {
            item.container.AddToClassList("gridview-item-hover");
            item.container.style.backgroundColor = new Color(0f, 0f, 0f, 0.3f);
        }

        void OnItemUnhover(Item item)
        {
            item.container.RemoveFromClassList("gridview-item-hover");
            item.container.style.backgroundColor = new StyleColor(StyleKeyword.None);
        }

        public void RefreshItems()
        {
            if (itemsContainer.layout.width == float.NaN)
                return;
            var n = (int)(itemsContainer.layout.width / cellSize.x);
            if (n < 0)
                return;
            columnCount = n;

            if (columnCount == 0 || itemsSource.Count == 0)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    var item = items[i];
                    items.RemoveAt(i);
                    UnusedItem(item);
                }
                return;
            }

            if (itemsSource.Count > 0)
            {
                var maxCell = IndexToCell(itemsSource.Count - 1);
                maxHeight = cellSize.y * (maxCell.y + 1);
                rowCount = Mathf.CeilToInt(itemsSource.Count / (float)columnCount);
            }
            else
            {
                maxHeight = 0;
                rowCount = 0;
            }

            placeholder.style.height = maxHeight;

            int startRow = Mathf.FloorToInt(scrollView.scrollOffset.y / cellSize.y);
            int endRow = startRow + (Mathf.CeilToInt(scrollView.contentViewport.layout.height / cellSize.y));

            if (startRow < 0)
                startRow = 0;

            endRow = Mathf.Min(endRow, Mathf.CeilToInt(itemsSource.Count / (float)columnCount));

            int startIndex = CellToIndex(new Vector2Int(0, startRow));
            int endIndex = CellToIndex(new Vector2Int(columnCount - 1, endRow));
            if (endIndex >= itemsSource.Count)
            {
                endIndex = itemsSource.Count - 1;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!(startIndex <= item.dataIndex && item.dataIndex <= endIndex))
                {
                    items.RemoveAt(i);
                    UnusedItem(item);
                    i--;
                }
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                Item item;
                item = items.FirstOrDefault(o => o.dataIndex == i);
                IStyle style;
                VisualElement view;
                if (item == null)
                {
                    if (unusedItems.Count > 0)
                    {
                        item = unusedItems[unusedItems.Count - 1];
                        unusedItems.RemoveAt(unusedItems.Count - 1);
                    }
                    else
                    {
                        view = new VisualElement();
                        style = view.style;
                        style.position = Position.Absolute;
                        style.display = DisplayStyle.Flex;
                        style.width = cellSize.x;
                        style.height = cellSize.y;
                        var itemView = makeItem?.Invoke();
                        if (itemView != null)
                        {
                            view.Add(itemView);
                            itemView.style.width = cellSize.x;
                            itemView.style.height = cellSize.y;
                        }
                        item = new Item() { container = view, view = itemView };
                        itemsContainer.Add(view);
                    }

                    view = item.container;
                    style = view.style;
                    style.display = DisplayStyle.Flex;

                    items.Add(item);
                    item.dataIndex = i;
                    bindItem?.Invoke(item.view, item.dataIndex);
                }
                var cell = IndexToCell(i);
                view = item.container;
                style = view.style;
                style.left = cellSize.x * cell.x;
                style.top = cellSize.y * cell.y;
            }


        }

        public void ScrollToIndex(int index)
        {

        }


        class Item
        {
            public int dataIndex;
            public VisualElement container;
            public VisualElement view;
        }
    }
}