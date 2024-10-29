using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace UnityEngine.UIElements.Extension
{
    public static class UIElementsUtility
    {

        internal const string ProgressBarUxmlName = "_ProgressBar";

        public const string DefaultUSS = "_Default";


        public static UIElementsCollection UiCollection;

        public static ToastLayer ToastLayer { get; private set; }

        public static MaskLayer MaskLayer { get; private set; }

        public static Func<UIDocument> createUiDoc;

        static Dictionary<string, VisualTreeAsset> uxmlDic;
        static Dictionary<string, List<StyleSheet>> styleSheetDic;


        public static void Initialize(UIElementsCollection uiCollection, Func<UIDocument> createUiDoc)
        {
            UiCollection = uiCollection;

            uxmlDic = new Dictionary<string, VisualTreeAsset>();
            styleSheetDic = new Dictionary<string, List<StyleSheet>>();

            if (uiCollection != null)
            {
                InitializeUICollection(uiCollection);
            }


            UIElementsUtility.createUiDoc = createUiDoc;
            var doc = createUiDoc();
            MaskLayer = doc.gameObject.AddComponent<MaskLayer>();
            doc = createUiDoc();
            ToastLayer = doc.gameObject.AddComponent<ToastLayer>();

        }

        public static void InitializeUICollection(UIElementsCollection uiCollection)
        {
            if (uiCollection == null)
                return;

            if (uiCollection.baseCollections != null)
            {
                foreach (var item in uiCollection.baseCollections)
                {
                    if (!item) continue;
                    InitializeUICollection(item);
                }
            }

            if (uiCollection.uxmls != null)
            {
                foreach (var item in uiCollection.uxmls)
                {
                    if (!item) continue;
                    uxmlDic[item.name] = item;
                }
            }

            if (uiCollection.styleSheets != null)
            {
                foreach (var item in uiCollection.styleSheets)
                {
                    if (!item) continue;
                    if (!styleSheetDic.TryGetValue(item.name, out var list))
                    {
                        list = new List<StyleSheet>();
                        styleSheetDic[item.name] = list;
                    }
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }

        }

        public static void Initialize(VisualElement root, UIElementsCollection asset)
        {
            var uss = asset.FindStyleSheet(DefaultUSS);
            if (uss)
            {
                root.styleSheets.Add(uss);
            }
        }

        public static VisualTreeAsset FindUxml(string name)
        {
            if (uxmlDic.TryGetValue(name, out var item))
            {
                return item;
            }
            return null;
        }

        public static VisualTreeAsset GetUxml(string uxmlName)
        {
            VisualTreeAsset tpl = FindUxml(uxmlName);

            if (tpl == null)
            {
                Debug.LogError($"uxml '{uxmlName}' missing");
            }
            return tpl;
        }

        public static TemplateContainer InstantiateUxml(string uxmlName)
        {
            var tpl = GetUxml(uxmlName);
            return tpl.CloneTree();
        }

        public static IReadOnlyCollection<StyleSheet> FindStyleSheet(string name)
        {
            if (styleSheetDic.TryGetValue(name, out var item))
            {
                return item;
            }
            return Array.Empty<StyleSheet>();
        }

        public static void AddStyleSheet(VisualElement elem, string ussName)
        {
            foreach (var uss in FindStyleSheet(ussName))
            {
                if (!uss) continue;
                elem.styleSheets.Add(uss);
            }
        }


        public static float InvertY(float height, float y)
        {
            return height - y;
        }

        public static Vector2 InvertY(float height, Vector2 pos)
        {
            return new Vector2(pos.x, height - pos.y);
        }

        public static Rect InvertY(float height, Rect rect)
        {
            var yMin = InvertY(height, rect.yMin);
            var yMax = InvertY(height, rect.yMax);
            rect.yMin = yMax;
            rect.yMax = yMin;
            return rect;
        }


        /* public class LayoutRect
         {
             public object State { get; private set; }
             public Rect Rect { get; set; }

             public LayoutRect(object state, Rect rect)
             {
                 State = state;
                 Rect = rect;
             }
         }*/

        public static void SortRectY<T>(List<(T state, Rect rect)> rects, Rect canvasRect)
        {
            List<(T state, Rect rect)> tmp = new();

            foreach (var frameQueue in rects)
            {
                var rect = frameQueue.rect;
                if (rect.width > 0)
                {
                    if (rect.y < canvasRect.y)
                    {
                        rect.y = canvasRect.y;
                    }
                    else if (rect.yMax > canvasRect.yMax)
                    {
                        rect.y -= rect.yMax - canvasRect.yMax;
                    }
                    int index = -1;
                    //查找小于插入位置
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        if (rect.y < tmp[i].rect.y)
                        {
                            //开始位置是否在上一个范围内
                            if (i > 0)
                            {
                                if (rect.y < tmp[i - 1].rect.yMax)
                                {
                                    rect.y = tmp[i - 1].rect.yMax;
                                }
                            }

                            //结束位置是否在下一个范围内，如果在范围内，从前往后找可移动空间
                            if (rect.yMax > tmp[i].rect.y)
                            {
                                float remainingSpace = rect.yMax - tmp[i].rect.y;
                                int moveStart = i;
                                int moveEnd = -1;
                                float lastMoveSpace = 0f;
                                for (int j = moveStart; j < tmp.Count; j++)
                                {
                                    //结束空隙
                                    float afterSpace;

                                    //最后一个直接移动剩余的全部空间，可以溢出
                                    if (j == tmp.Count - 1)
                                    {
                                        afterSpace = remainingSpace;
                                    }
                                    else
                                    {
                                        //结束空隙
                                        afterSpace = tmp[j + 1].rect.y - tmp[j].rect.yMax;
                                    }

                                    if (afterSpace > 0f)
                                    {
                                        if (afterSpace >= remainingSpace)
                                        {
                                            lastMoveSpace = remainingSpace;
                                        }
                                        else
                                        {
                                            lastMoveSpace = afterSpace;
                                        }
                                    }
                                    else
                                    {
                                        lastMoveSpace = 0f;
                                    }
                                    remainingSpace -= lastMoveSpace;
                                    moveEnd = j;
                                    if (remainingSpace <= 0f)
                                        break;
                                }

                                if (moveEnd >= 0)
                                {
                                    for (int j = moveEnd; j >= moveStart; j--)
                                    {
                                        var newRect = tmp[j].rect;
                                        if (j == moveEnd)
                                        {
                                            newRect.y += lastMoveSpace;
                                        }
                                        else
                                        {
                                            newRect.y = tmp[j + 1].rect.y - newRect.height;
                                        }
                                        tmp[j] = (tmp[j].state, newRect);
                                    }
                                }
                            }

                            tmp.Insert(i, (frameQueue.state, rect));
                            index = i;

                            break;
                        }
                    }

                    //末尾插入
                    if (index == -1)
                    {
                        if (tmp.Count > 0)
                        {
                            var last = tmp[tmp.Count - 1];
                            //开始位置在末尾范围内
                            if (rect.y < last.rect.yMax)
                            {
                                rect.y = last.rect.yMax;
                            }
                        }

                        index = tmp.Count;
                        tmp.Add((frameQueue.state, rect));
                    }
                }
            }

            if (tmp.Count > 0)
            {
                //检查末尾范围溢出
                float lastOverflowSpace = tmp[tmp.Count - 1].rect.yMax - canvasRect.yMax;
                if (lastOverflowSpace > 0f)
                {
                    float remainingSpace = lastOverflowSpace;
                    int moveStart = tmp.Count - 1;
                    int moveEnd = -1;
                    float lastMoveSpace = 0f;
                    //从后往向前找可移动空间
                    for (int j = moveStart; j >= 0; j--)
                    {
                        if (tmp[j].rect.y < canvasRect.y)
                            break;
                        float beforeSpace;
                        if (j == 0)
                        {
                            beforeSpace = tmp[j].rect.y - canvasRect.y;
                        }
                        else
                        {
                            beforeSpace = tmp[j].rect.y - Mathf.Max(tmp[j - 1].rect.yMax, canvasRect.y);
                        }

                        if (beforeSpace > 0f)
                        {
                            if (beforeSpace >= remainingSpace)
                            {
                                lastMoveSpace = remainingSpace;
                            }
                            else
                            {
                                lastMoveSpace = beforeSpace;
                            }
                        }
                        else
                        {
                            lastMoveSpace = 0f;
                        }
                        moveEnd = j;
                        remainingSpace -= lastMoveSpace;
                        if (remainingSpace <= 0f)
                            break;
                    }

                    if (moveEnd >= 0)
                    {
                        for (int j = moveEnd; j <= moveStart; j++)
                        {
                            Rect newRect = tmp[j].rect;
                            if (j == moveEnd)
                            {
                                newRect.y -= lastMoveSpace;
                            }
                            else
                            {
                                newRect.y += tmp[j - 1].rect.yMax + newRect.height;
                            }
                            tmp[j] = (tmp[j].state, newRect);
                        }
                    }
                }
            }


            for (int i = 0; i < rects.Count; i++)
            {
                var state = rects[i].state;

                foreach (var item in tmp)
                {
                    if (object.Equals(item.state, state))
                    {
                        rects[i] = (state, item.rect);
                    }
                }
            }
        }

    }


}