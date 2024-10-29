using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UIElements.Extension
{

    public class ChartLabel : ChartWidget
    {
        public Rect rect;
        public string text;
        public int fontSize = 12;
        public Color? fontColor;
        public Color? backgroundColor;
        public bool aliginRight;

        public Func<float, string> formatDisplayText;
        public IChartValue value;

        public ChartLabel()
        {
            layer = 10;
        }

        public override void Update()
        {
            if (value != null)
            {
                if (value.HasValue(dataSource, dataSource.currentFrame))
                {
                    float _value = value.GetValue(dataSource, dataSource.currentFrame);

                    if (formatDisplayText != null)
                    {
                        text = formatDisplayText(_value);
                    }
                    else
                    {
                        text = $"{value:0.#}";
                    }
                }
                else
                {
                    if (formatDisplayText != null)
                    {
                        text = formatDisplayText(0f);
                    }
                    else
                    {
                        text = $"";
                    }
                }
            }
        }

        public override void Draw()
        {
            if (string.IsNullOrEmpty(text)) return;

            var ctx = chart.context;

            Color fontColor;

            if (this.fontColor.HasValue)
            {
                fontColor = this.fontColor.Value;
            }
            else
            {
                fontColor = dataSource.Color;
            }

            Rect rect = this.rect;
            rect = chart.InvertY(rect);




            RectOffset padding = new RectOffset(5, 0, 2, 5);

            if (backgroundColor.HasValue)
            {
                //RectOffset padding = new RectOffset(5, 0, 2, 5);

                chart.FillRect(ctx, rect, backgroundColor.Value);
            }
            var textRect = padding.Remove(rect);

            ctx.DrawText(text, textRect.position, fontSize, fontColor);
        }
    }
    public class ChartSortLabel : ChartLabel
    {
        public bool isBeginDraw;

        public override void BeginDraw()
        {
            if (isBeginDraw)
                return;
            isBeginDraw = true;

            Rect canvasRect = chart.contentRect;

            IEnumerable<ChartDataSource> dataItems = from o in chart.dataList
                                                     where o.Visiable
                                                     select o;

            List<(ChartLabel, Rect)> sortLabels = new();
            foreach (var dir in new bool[] { true, false })
            {
                sortLabels.Clear();
                float space = 0;

                foreach (var frameQueue in dataItems)
                {
                    foreach (var label in frameQueue.widgets.OfType<ChartSortLabel>())
                    {
                        if (label.aliginRight != dir)
                            continue;
                        if (!label.visiable)
                            continue;
                        label.UpdatePosition();
                        var rect = label.rect;
                        //if (rect.width <= 0f)
                        //{
                        //    continue;
                        //}

                        //rect = UIElementsUtility.InvertYAxis(rect, canvasRect.height);
                        rect.height += space;
                        sortLabels.Add((label, rect));
                    }
                }

                //标签高度排序 
                UIElementsUtility.SortRectY(sortLabels, canvasRect);


                foreach (var item in sortLabels)
                {
                    var label = item.Item1;
                    var rect = item.Item2;

                    //rect = UIElementsUtility.InvertYAxis(rect, canvasRect.height);
                    rect.height -= space;
                    label.rect = rect;
                }

            }
        }


        public override void EndDraw()
        {
            if (!isBeginDraw)
                return;
            isBeginDraw = false;

        }

        public void UpdatePosition()
        {
            if (position != null)
            {
                rect.position = position.GetPosition(dataSource);
                if (aliginRight)
                {
                    rect = chart.GetRightLabelRect(rect.position, text);
                }
                else
                {
                    rect = chart.GetLeftLabelRect(rect.position, text);
                }
            }
            else
            {
                float displayPercentage = 0f;

                if (dataSource.currentFrame != null)
                {
                    displayPercentage = dataSource.currentFrame.displayPercentage;
                }

                if (aliginRight)
                {
                    rect.position = chart.TransformViewPoint(new Vector2(1f, displayPercentage));
                    rect.position = chart.InvertY(rect.position);
                    rect = chart.GetRightLabelRect(rect.position, text);
                }
                else
                {
                    rect.position = chart.TransformViewPoint(new Vector2(0f, displayPercentage));
                    rect = chart.GetLeftLabelRect(rect.position, text);
                }
            }

        }
    }



}
