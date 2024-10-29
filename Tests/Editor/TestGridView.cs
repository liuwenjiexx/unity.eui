using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Yanmonet.UIElements.Extensions;

public class TestGridView : EditorWindow
{
    GridView gridView;
    public void CreateGUI()
    {
        ListView l;

        VisualElement h = new VisualElement();
        h.style.flexDirection = FlexDirection.Row;
        h.style.alignItems = Align.Stretch;
        for (int i = 0; i < 5; i++)
        {
            var toIndex = i * 3;
            Button btn = new Button();
            btn.text = toIndex.ToString();
            btn.clicked += () =>
            {
                gridView.ScrollToIndex(toIndex);
            };
            h.Add(btn);
        }
        rootVisualElement.Add(h);

        gridView = new GridView();
        gridView.style.flexGrow = 1f;
        rootVisualElement.Add(gridView);

        gridView.cellSize = new Vector2Int(100, 100);
        gridView.SelectChanged += GridView_SelectChanged;
        gridView.ItemClick += GridView_ItemClick; ;

        gridView.makeItem = () =>
        {
            Label label = new Label();
            label.style.borderTopColor = Color.black;
            label.style.borderLeftColor = Color.black;
            label.style.borderRightColor = Color.black;
            label.style.borderBottomColor = Color.black;

            label.style.borderLeftWidth = 1f;
            label.style.borderRightWidth = 1f;
            label.style.borderTopWidth = 1f;
            label.style.borderBottomWidth = 1f;

            return label;
        };

        gridView.bindItem = (view, index) =>
        {
            Label label = (Label)view;
            label.text = gridView.itemsSource[index] as string + " " + gridView.IndexToCell(index);
        };


        List<string> list = new List<string>();
        for (int i = 0; i < 27; i++)
        {
            list.Add(i.ToString());
        }

        gridView.itemsSource = list;
        gridView.RefreshItems();

    }

    private void GridView_ItemClick(int index)
    {
        Debug.Log("item click " + index);
    }
     
    private void GridView_SelectChanged(int index)
    {
        Debug.Log("select " + index);
    }

    [MenuItem("Test/GridViewWindow")]
    static void ShowWindow()
    {
        GetWindow<TestGridView>().Show();
    }

}
