using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UIElements.Extension
{

    /// <summary>
    /// 帧值计算器
    /// </summary>
    public interface IChartFrameCalculator
    {
        void CalculateNewFrame(ChartDataSource source, ChartDataFrame newFrame);

        void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames);
    }


    //public class SumFrameValueCalculator : IChartFrameCalculator
    //{

    //    public void CalculateNewFrame(ChartDataSource source, ChartDataFrame frame)
    //    {
    //        float value = 0f;
    //        foreach (var item in frame.list)
    //        {
    //            value += item.value;
    //        }
    //        frame.value = value;
    //    }


    //    public void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames)
    //    {
    //    }
    //}

    public class ReferenceFrameValueCalculator : IChartFrameCalculator
    {
        private ChartDataSource valueSource;
        public ReferenceFrameValueCalculator(ChartDataSource valueSource = null)
        {
            this.valueSource = valueSource;
        }

        public void CalculateNewFrame(ChartDataSource source, ChartDataFrame frame)
        {
            float value = 0f;
            if (valueSource != null)
            {
                if (valueSource.currentFrame != null)
                {
                    value = valueSource.currentFrame.value;
                }
            }
            frame.value = value;
        }

        public void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames)
        {
        }

    }


    /// <summary>
    /// 百分比帧值
    /// </summary>
    public class PercentageFrameValueCalculator : IChartFrameCalculator
    {
        private ChartDataSource valueSource;
        private ChartDataSource maxSource;

        public PercentageFrameValueCalculator(ChartDataSource maxSource, ChartDataSource valueSource = null)
        {
            this.maxSource = maxSource;
            this.valueSource = valueSource;
        }


        public void CalculateNewFrame(ChartDataSource source, ChartDataFrame frame)
        {
            float value = 0f;
            float current = 0f;
            float max = 0f;

            if (valueSource != null)
            {
                if (valueSource.currentFrame != null)
                {
                    current = valueSource.currentFrame.value;
                }
            }
            else
            {
                current = frame.value;
            }
            max = maxSource.currentFrame.value;

            if (Mathf.Abs(max) > 0.0001f)
            {
                value = current / max;
            }

            frame.value = value;
        }

        public void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames)
        {
        }
    }

    /// <summary>
    /// Value 转为百分比
    /// </summary>
    public class ValueToDisplayPercentageFrameCalculator : IChartFrameCalculator
    {
        public void CalculateNewFrame(ChartDataSource source, ChartDataFrame frame)
        {
        }

        public void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames)
        {
            foreach (var frame in frames)
            {
                frame.displayPercentage = frame.value;
                frame.displayPercentageHandled = true;
            }
        }
    }
     
    public class ParentDisplayPercentageFrameCalculator : IChartFrameCalculator
    {

        ChartDataSource parentSource;

        public ParentDisplayPercentageFrameCalculator(ChartDataSource parentSource)
        {
            this.parentSource = parentSource;
        }

        public void CalculateNewFrame(ChartDataSource source, ChartDataFrame frame)
        {

        }


        public void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames)
        {

            foreach (var frame in frames)
            {
                float percentage = 0f;
                var parentFrame = frame.referenceParentFrame;
                if (parentFrame != null)
                {
                    if (Mathf.Abs(parentFrame.value) > 0.001f)
                    {
                        percentage = frame.value / parentFrame.value;
                    }
                    percentage = parentFrame.displayPercentage * percentage;
                }
                frame.displayPercentage = percentage;
                frame.displayPercentageHandled = true;
            }
        }
    }


    public class ChartSpeedCalculator : IChartFrameCalculator
    {
        public ChartSpeedCalculator(bool smooth)
        {
            this.smooth = smooth;
            Speed = new ChartSpeedValue(this);
            MaxSpeed = new ChartMaxSpeedValue(this);
        }

        private bool smooth = true;
        private float maxSpeed;

        public readonly IChartValue Speed;
        public readonly IChartValue MaxSpeed;

        const string SPEED_KEY = "Speed";
        const string SMOOTH_SPEED_KEY = "SmoothSpeed";


        public void CalculateNewFrame(ChartDataSource source, ChartDataFrame newFrame)
        {

            float speed = 0f;
            if (Mathf.Abs(newFrame.deltaTime) > 0.0001f)
            {
                speed = newFrame.value / newFrame.deltaTime;
            }

            float smoothSpeed = 0f;
            if (newFrame.previous != null && newFrame.previous.values.TryGetValue(SMOOTH_SPEED_KEY, out var o))
            {
                float f = (float)o;
                smoothSpeed = (speed + f) * 0.5f;
            }
            else
            {
                smoothSpeed = speed;
            }


            newFrame.values[SPEED_KEY] = speed;
            newFrame.values[SMOOTH_SPEED_KEY] = smoothSpeed;

        }

        public void CalculateAllFrames(ChartDataSource source, List<ChartDataFrame> frames)
        {
            maxSpeed = 0f;

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                float speed = 0f;
                if (smooth)
                {
                    if (frame.values.TryGetValue(SMOOTH_SPEED_KEY, out var v))
                    {
                        speed = (float)v;
                    }
                }
                else
                {
                    if (frame.values.TryGetValue(SPEED_KEY, out var v))
                    {
                        speed = (float)v;
                    }
                }
                if (i == 0)
                {
                    maxSpeed = speed;
                }
                else
                {
                    if (speed > maxSpeed)
                    {
                        maxSpeed = speed;
                    }
                }
            }

        }


        class ChartSpeedValue : IChartValue
        {
            public ChartSpeedValue(ChartSpeedCalculator speedCalculator)
            {
                this.speedCalculator = speedCalculator;
            }

            public ChartSpeedCalculator speedCalculator;

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return frame != null;
            }

            public float GetValue(ChartDataSource dataSource)
            {
                var current = dataSource.currentFrame;
                if (current == null)
                    return 0f;
                return GetValue(dataSource, current);
            }

            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                float speed = 0f;
                if (speedCalculator.smooth)
                {
                    if (frame.values.TryGetValue(SMOOTH_SPEED_KEY, out var o))
                    {
                        speed = (float)o;
                    }
                }
                else
                {
                    if (frame.values.TryGetValue(SPEED_KEY, out var o))
                    {
                        speed = (float)o;
                    }
                }
                return speed;
            }
        }

        class ChartMaxSpeedValue : IChartValue
        {
            public ChartMaxSpeedValue(ChartSpeedCalculator speedCalculator)
            {
                this.speedCalculator = speedCalculator;
            }

            public ChartSpeedCalculator speedCalculator;

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return frame != null;
            }

            public float GetValue(ChartDataSource dataSource)
            {
                var current = dataSource.currentFrame;
                if (current == null)
                    return 0f;
                return GetValue(dataSource, current);
            }

            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return speedCalculator.maxSpeed;
            }
        }

        //public Vector2 GetPosition(ChartDataSource source)
        //{
        //    float speed = GetValue(source);
        //    float per = 0f;
        //    if (Mathf.Abs(maxSpeed) > 0.0001f)
        //    {
        //        per = speed / maxSpeed;
        //    }
        //    return per;
        //}
    }

}
