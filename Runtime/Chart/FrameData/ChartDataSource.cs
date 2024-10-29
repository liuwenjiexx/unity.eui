using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{
    public class ChartDataSource
    {
        public FrameDataChart chart;

        public Queue<ChartDataFrame> queue = new();

        public ChartDataFrame newFrame;

        private Color color = new Color(1, 1, 0, 0.5f);

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                UpdateColor();
            }
        }

        void UpdateColor()
        {
            if (Visiable)
            {
                colorImage.style.backgroundColor = color;
            }
            else
            {
                colorImage.style.backgroundColor = Color.black;
            }
        }

        public ChartDataSource referenceParentSource;
        private bool isPercentageValue;


        private bool visiable = true;

        public ChartDataFrame currentFrame;
        public float? fixedMaxValue;
        public float displayMaxValue;


        public List<IChartFrameCalculator> beforeFrameCalculators = new();

        public List<IChartFrameCalculator> afterFrameCalculators = new();

        public List<ChartWidget> widgets = new();

        private ChartFillArea fillArea;
        private ChartAreaLine strokeArea;


        public float minValue;
        public float maxValue;
        public float smoothMinValue;
        public float smoothMaxValue;
        public float sumValue;
        public float avgValue;
        VisualElement titleContainer;
        public Label titleLabel;
        public VisualElement colorImage;

        public ChartDataSource(FrameDataChart chart, string name)
        {
            this.chart = chart;

            titleContainer = new VisualElement();
            titleContainer.AddToClassList("chart-data");
            titleContainer.name = name;
            chart.titleContainer.Add(titleContainer);

            colorImage = new VisualElement();
            colorImage.style.backgroundColor = new Color(1, 1, 0, 0.5f);
            colorImage.AddToClassList("chart-data__color");
            titleContainer.Add(colorImage);

            titleLabel = new Label();
            titleLabel.AddToClassList("chart-data__text");
            titleContainer.Add(titleLabel);


            titleLabel.RegisterCallback<MouseDownEvent>(e =>
            {
                e.StopPropagation();
                Visiable = !Visiable;
            });

            fillArea = new ChartFillArea(this);
            fillArea.visiable = false;
            AddWidget(fillArea);

            strokeArea = new ChartAreaLine(this);
            strokeArea.visiable = false;
            AddWidget(strokeArea);

            UpdateColor();
        }

        public string Title { get => titleLabel.text; set => titleLabel.text = value; }

        public ChartFillArea Fill
        {
            get => fillArea;
        }

        public ChartAreaLine Line
        {
            get => strokeArea;
        }
        public bool IsPercentageValue { get => isPercentageValue; set => isPercentageValue = value; }
        public bool Visiable
        {
            get => visiable; set
            {
                visiable = value;
                UpdateColor();
            }
        }


        public void AddWidget(ChartWidget widget)
        {
            widget.dataSource = this;
            widgets.Add(widget);
        }

        internal void Initialize(float deltaTime)
        {
            newFrame = null;

            //foreach (var item in frameQueue.Select(o => o.list).ToList())
            //{
            //    if (currentFrame == null)
            //    {
            //        currentFrame = new ChartDataFrame();
            //        currentFrame.time=
            //    }
            //}
            newFrame = new ChartDataFrame();
            newFrame.time = Time.realtimeSinceStartup;


            foreach (var w in widgets)
            {
                w.chart = chart;
            }
        }

        public void Add(ChartDataItem item)
        {
            newFrame.list.Add(item);
        }
        public int dataFrameCount;
        public List<ChartDataFrame> dataFrames = new();




        public void UpdateFrame(int currentColumnCount)
        {

            //foreach (var item in currentFrame.list)
            //{ 
            //} 

            if (referenceParentSource != null)
            {
                newFrame.referenceParentFrame = referenceParentSource.currentFrame;
            }


            newFrame.deltaTime = Time.realtimeSinceStartup - newFrame.time;

            if (newFrame.list != null)
            {
                float sum = 0f;
                foreach (var item in newFrame.list)
                {
                    sum += item.value;
                }
                newFrame.value = sum;
            }

            foreach (var item in beforeFrameCalculators)
            {
                item.CalculateNewFrame(this, newFrame);
            }

            if (newFrame.previous != null)
            {
                newFrame.smoothValue = (newFrame.previous.smoothValue + newFrame.value) * 0.5f;
            }
            else
            {
                newFrame.smoothValue = newFrame.value;
            }




            foreach (var item in afterFrameCalculators)
            {
                item.CalculateNewFrame(this, newFrame);
            }

            queue.Enqueue(newFrame);


            newFrame = new ChartDataFrame()
            {
                time = Time.realtimeSinceStartup,
                previous = newFrame
            };

            dataFrameCount = 0;
            displayMaxValue = 0f;
            sumValue = 0f;
            avgValue = 0f;
            float dynamicMaxValue = 0f;
            ChartDataFrame maxFrame = null;
            ChartDataFrame minFrame = null;
            float? minSmoothValue = null, maxSmoothValue = null;

            currentFrame = null;
            this.minValue = 0f;
            this.maxValue = 0f;
            this.smoothMinValue = 0f;
            this.smoothMaxValue = 0f;

            dataFrames.Clear();

            int skipCount;

            if (queue.Count > currentColumnCount)
            {
                dataFrameCount = currentColumnCount;
                skipCount = queue.Count - currentColumnCount;
            }
            else
            {
                dataFrameCount = queue.Count;
                skipCount = 0;
            }

            dataFrames.AddRange(queue.Skip(skipCount));


            int index = 0;
            foreach (var frame in dataFrames)
            {
                frame.index = index;
                float value = frame.value;

                if (minFrame == null)
                {
                    minFrame = frame;
                    maxFrame = frame;
                }
                else if (value < minFrame.value)
                {
                    minFrame = frame;
                }
                else if (value > maxFrame.value)
                {
                    maxFrame = frame;
                }

                if (!minSmoothValue.HasValue)
                {
                    minSmoothValue = frame.smoothValue;
                    maxSmoothValue = frame.smoothValue;
                }
                else if (frame.smoothValue < minSmoothValue.Value)
                {
                    minSmoothValue = frame.smoothValue;
                }
                else if (frame.smoothValue > maxSmoothValue.Value)
                {
                    maxSmoothValue = frame.smoothValue;
                }

                sumValue += frame.value;
                currentFrame = frame;
                index++;
            }

            if (index > 0)
            {
                avgValue = sumValue / index;
            }

            foreach (var frame in dataFrames)
            {
                frame.displayPercentageHandled = false;
            }

            foreach (var item in beforeFrameCalculators)
            {
                item.CalculateAllFrames(this, dataFrames);
            }

            /*
            if (resolveMaxValue != null)
            {
                maxValue = resolveMaxValue(this, current);
            }else */
            if (fixedMaxValue.HasValue)
            {
                displayMaxValue = fixedMaxValue.Value;
            }
            else
            {
                float maxScale = 1.2f;
                if (maxFrame != null)
                {
                    dynamicMaxValue = maxFrame.value;
                    dynamicMaxValue *= maxScale;
                }
                displayMaxValue = dynamicMaxValue;
            }

            index = 0;

            float frameWidth = chart.FrameWidth;
            float height = chart.Height;
            Vector2 offset = new Vector2(chart.Width - (dataFrameCount - 1) * frameWidth, 0f);

            if (minSmoothValue.HasValue)
            {
                this.smoothMinValue = minSmoothValue.Value;
                this.smoothMaxValue = maxSmoothValue.Value;
            }

            if (minFrame != null)
            {
                this.minValue = minFrame.value;
                this.maxValue = maxFrame.value;
            }

            foreach (var frame in dataFrames)
            {
                if (!frame.displayPercentageHandled)
                {
                    float percentage;
                    percentage = GetDisplayPercentage(frame, displayMaxValue);
                    frame.displayPercentage = percentage;
                }

                Vector2 pos = new Vector2();

                pos.x = (index) * frameWidth;
                pos.y = height * frame.displayPercentage;
                pos += offset;
                frame.position = pos;
                index++;
            }

            foreach (var item in afterFrameCalculators)
            {
                item.CalculateAllFrames(this, dataFrames);
            }

            foreach (var label in widgets)
            {
                if (!label.visiable)
                    continue;
                label.Update();
            }

        }

        public void UpdatePercentage(float maxValue)
        {

        }


        public virtual float GetDisplayPercentage(ChartDataFrame frame, float displayMaxValue)
        {
            float percentage = 0f;
            float value = frame.value;


            if (displayMaxValue != 0f)
            {
                percentage = value / displayMaxValue;
            }
            return percentage;
        }
    }




}
