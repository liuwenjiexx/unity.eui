using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{
    /// <summary>
    /// 帧数据图表
    /// </summary>
    public class FrameDataChart : VisualElement
    {
        //int minFrameWidth = 10;
        private FrameDataChartCanvas canvas;
        public List<ChartDataSource> dataList = new();

        [NonSerialized]
        double nextUpdateTime;
        [NonSerialized]
        public float interval = 0.5f;

        public VisualElement titleContainer;
        public Label titleLabel;


        public FrameDataChart()
        {
            AddToClassList("chart");
            style.overflow = Overflow.Hidden;
            this.Add(new IMGUIContainer(Update));

            titleContainer = new VisualElement();
            titleContainer.AddToClassList("chart-title-container");
            Add(titleContainer);

            titleLabel = new Label();
            titleLabel.AddToClassList("chart-title__text");
            titleContainer.Add(titleLabel);

            canvas = new FrameDataChartCanvas(this);
            Add(canvas);

        }

        public string Title { get => titleLabel.text; set => titleLabel.text = value; }

        public int frameLimit = 100;

        public List<ChartWidget> widgets = new();

        public bool isPause;

        public static float NowTime
        {
            get
            {
                return Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// 最小每帧宽度
        /// </summary>
        //public int MinFrameWidth { get => minFrameWidth; set => minFrameWidth = value; }

        /// <summary>
        /// 每帧宽度
        /// </summary>
        public float FrameWidth { get => canvas.frameWidth; set => canvas.frameWidth = value; }

        /// <summary>
        /// 帧数
        /// </summary>
        public int MaxFrameCount { get => canvas.maxFrameCount; }

        public MeshGenerationContext context => canvas.context;

        public Action UpdateBefore;
        public Action UpdateAfter;


        public void Initialize()
        {
            foreach (var dataSource in dataList)
            {
                dataSource.Initialize(0f);

            }
        }

        public void AddWidget(ChartWidget widget)
        {
            widget.chart = this;
            widgets.Add(widget);
        }


        public static Color AlphaWeightColor(Color color)
        {
            Color newColor = color;
            if (newColor.a < 0.5f)
                newColor.a = Mathf.Clamp01(color.a * 2f);
            else
                newColor.a = Mathf.Clamp01(color.a * 0.5f);
            return newColor;
        }

        private void Update()
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (!isPause)
                {
                    if (Application.isPlaying)
                    {
                        if (Time.timeAsDouble > nextUpdateTime)
                        {
                            nextUpdateTime = Time.timeAsDouble + interval;
                            UpdateFrame();
                        }
                    }
                }
                MarkDirtyRepaint();
            }
        }

        public void UpdateFrame()
        {
            UpdateBefore?.Invoke();
            foreach (var queue in dataList)
            {
                if (queue.queue.Count >= frameLimit)
                {
                    queue.queue.Dequeue();
                }
                queue.UpdateFrame(MaxFrameCount);
            }
            UpdateAfter?.Invoke();
            canvas.MarkDirtyRepaint();
        }

        public ChartDataSource CreateDataSource(string name)
        {
            ChartDataSource dataSource = new ChartDataSource(this, name);

            dataList.Add(dataSource);
            return dataSource;
        }




        void DrawFrame(MeshGenerationContext ctx, ChartDataSource frameQueue)
        {
            int count = frameQueue.dataFrameCount;
            if (count == 0)
            {
                return;
            }

            foreach (var widget in frameQueue.widgets)
            {
                if (!widget.visiable)
                    continue;
                widget.Draw();
            }

        }


        internal Rect GetLeftLabelRect(Vector2 point, string text, int fontSize = 12)
        {
            if (string.IsNullOrEmpty(text)) return new Rect(point.x, point.y, 0, 0);
            float width = text.Length * fontSize * 0.5f + 10;
            float height = fontSize;
            Vector2 labelOffset = new Vector2(0f, 2);

            Vector2 pos = point + labelOffset;
            RectOffset padding = new RectOffset(0, 5, 2, 5);
            return padding.Add(new Rect(pos.x, pos.y, width, height));
        }

        internal Rect GetRightLabelRect(Vector2 point, string text, int fontSize = 12)
        {
            if (string.IsNullOrEmpty(text)) return new Rect(point.x, point.y, 0, 0);
            float width = text.Length * fontSize * 0.5f + 10;
            float height = fontSize;
            Vector2 labelOffset = new Vector2(-(width), 2);
            Vector2 pos = point + labelOffset;
            RectOffset padding = new RectOffset(5, 0, 2, 5);
            return padding.Add(new Rect(pos.x, pos.y, width, height));
        }




        internal void FillViewRect(MeshGenerationContext ctx, Rect rect, Color fillColor)
        {
            var painter = ctx.painter2D;
            var oldFillColor = painter.fillColor;
            painter.fillColor = fillColor;
            painter.BeginPath();
            painter.MoveTo(TransformViewPoint(new Vector2(rect.xMin, rect.yMin)));
            painter.LineTo(TransformViewPoint(new Vector2(rect.xMax, rect.yMin)));
            painter.LineTo(TransformViewPoint(new Vector2(rect.xMax, rect.yMax)));
            painter.LineTo(TransformViewPoint(new Vector2(rect.xMin, rect.yMax)));
            painter.Fill();
            painter.fillColor = oldFillColor;
        }
        internal void FillRect(MeshGenerationContext ctx, Rect rect, Color fillColor)
        {
            var painter = ctx.painter2D;
            var oldFillColor = painter.fillColor;
            painter.fillColor = fillColor;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.Fill();
            painter.fillColor = oldFillColor;
        }

        void DrawLabel(string text, float v, Color fontColor)
        {

        }

        public Rect CanvasRect => canvas.contentRect;

        internal Vector2 TransformPoint(Vector2 pos)
        {
            pos.y = CanvasRect.height - pos.y;
            return pos;
        }

        internal Vector2 TransformViewPoint(Vector2 pos)
        {
            pos.x = CanvasRect.width * pos.x;
            pos.y = CanvasRect.height * (1f - pos.y);
            return pos;
        }

        public float InvertY(float y)
        {
            return UIElementsUtility.InvertY(CanvasRect.height, y);
        }

        public Vector2 InvertY(Vector2 pos)
        {
            return UIElementsUtility.InvertY(CanvasRect.height, pos);
        }

        public Rect InvertY(Rect rect)
        {
            return UIElementsUtility.InvertY(CanvasRect.height, rect);
        }

        public float Width
        {
            get => CanvasRect.width;
        }

        public float Height
        {
            get => CanvasRect.height;
        }

        class FrameDataChartCanvas : VisualElement
        {
            private FrameDataChart owner;
            public float frameWidth = 30;
            public int maxFrameCount;
            public MeshGenerationContext context;
            private float lastCanvasWidth;

            public FrameDataChartCanvas(FrameDataChart owner)
            {
                this.owner = owner;
                AddToClassList("chart-canvas");
                generateVisualContent += DrawCanvas;
                this.Add(new IMGUIContainer(Update));
            }

            private void Update()
            {
                if (Event.current.type == EventType.Repaint)
                {
                    //MarkDirtyRepaint();
                }
            }

            void DrawCanvas(MeshGenerationContext ctx)
            {
                context = ctx;

                int canvasWidth = (int)contentRect.width;

                maxFrameCount = Mathf.CeilToInt(canvasWidth / frameWidth);

                if (maxFrameCount < 1)
                {
                    maxFrameCount = 1;
                    frameWidth = canvasWidth;
                }
                maxFrameCount += 2;

                if (lastCanvasWidth != contentRect.width)
                {
                    lastCanvasWidth = contentRect.width;
                    OnCanvasWidthChanged();
                }

                IEnumerable<ChartWidget> widgets = from o in owner.widgets.Concat((from o in owner.dataList
                                                                                   where o.Visiable
                                                                                   select o).SelectMany(o => o.widgets))
                                                   where o.visiable
                                                   orderby o.layer
                                                   select o;

                foreach (var widget in widgets)
                {
                    widget.BeginDraw();
                }

                foreach (var widget in widgets)
                {
                    widget.Draw();
                }

                foreach (var widget in widgets)
                {
                    widget.EndDraw();
                }


            }

            void OnCanvasWidthChanged()
            {
                int width = (int)contentRect.width;
                lastCanvasWidth = width;
            }
        }
    }

    public class ChartDataItem
    {
        public int frameCount;
        public float time;
        public object data;
        public float value;
        public string displayText;
    }

    public class ChartDataFrame
    {
        public int index;
        public float value;
        public float smoothValue;
        public float time;
        public float deltaTime;
        public List<ChartDataItem> list = new();
        public ChartDataFrame referenceParentFrame;
        public ChartDataFrame previous;
        //public ChartDataFrame referenceMaxFrame;
        //public ChartDataFrame referenceValueFrame;

        public float displayPercentage;
        public bool displayPercentageHandled;
        public Vector2 position;


        public Dictionary<string, object> values = new();
    }



}
