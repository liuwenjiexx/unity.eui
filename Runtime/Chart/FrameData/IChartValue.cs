using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UIElements.Extension
{

    public interface IChartValue
    {
        bool HasValue(ChartDataSource dataSource, ChartDataFrame frame);
        float GetValue(ChartDataSource dataSource, ChartDataFrame frame);
        //float GetDisplayPercentage(ChartDataSource dataSource, ChartDataFrame frame);

    }

    public class ChartValue
    {
        public static readonly IChartValue Value = new ChartCurrentValue();
        public static readonly IChartValue SmoothValue = new ChartAvgValue();
        public static readonly IChartValue MinValue = new ChartMinValue();
        public static readonly IChartValue MaxValue = new ChartMaxValue();
        public static readonly IChartValue MinSmoothValue = new ChartMinSmoothValue();
        public static readonly IChartValue MaxSmoothValue = new ChartMaxSmoothValue();

        public static IChartValue ConstValue(float value)
        {
            return new ChartConstValue(value);
        }

        public static IChartValue Scale(IChartValue chartValue, float scale)
        {
            return new ScaleValue()
            {
                value = chartValue,
                scale = scale
            };
        }


        private class ChartConstValue : IChartValue
        {
            private float value;

            public ChartConstValue(float value)
            {
                this.value = value;
            }

            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return value;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return true;
            }
        }

        private class ChartCurrentValue : IChartValue
        {
            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                if (frame == null)
                    return 0f;
                return frame.value;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return frame != null;
            }
        }
        private class ChartAvgValue : IChartValue
        {
            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                if (frame == null)
                    return 0f;
                return frame.smoothValue;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return frame != null;
            }
        }


        private class ChartMinValue : IChartValue
        {
            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return dataSource.minValue;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return true;
            }
        }
        private class ChartMaxValue : IChartValue
        {
            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return dataSource.maxValue;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return true;
            }
        }

        private class ChartMinSmoothValue : IChartValue
        {
            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return dataSource.smoothMinValue;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return true;
            }
        }
        private class ChartMaxSmoothValue : IChartValue
        {
            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return dataSource.smoothMaxValue;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return true;
            }
        }

        private class ScaleValue : IChartValue
        {
            public IChartValue value;
            public float scale;

            public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return value.GetValue(dataSource, frame) * scale;
            }

            public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
            {
                return value.HasValue(dataSource, frame);
            }
        }
    }

    public class ChartFullAverageValue : IChartValue
    {
        public bool ignoreEmptyData;
        public ChartFullAverageValue() { }

        public ChartFullAverageValue(bool ignoreEmptyData)
        {
            this.ignoreEmptyData = ignoreEmptyData;
        }

        public float GetDisplayPercentage(ChartDataSource dataSource)
        {
            float total = 0f;
            int count = 0;
            foreach (var frame in dataSource.dataFrames)
            {
                if (ignoreEmptyData && frame.list.Count == 0)
                    continue;
                total += frame.displayPercentage;
                count++;
            }

            if (count == 0)
                return 0f;

            return total / count;
        }

        public float GetValue(ChartDataSource dataSource, ChartDataFrame frame)
        {
            float total = 0f;
            int count = 0;
            foreach (var frame2 in dataSource.dataFrames)
            {
                if (ignoreEmptyData && frame2.list.Count == 0)
                    continue;
                total += frame2.value;
                count++;
            }

            if (count == 0)
                return 0f;

            return total / count;
        }

        public bool HasValue(ChartDataSource dataSource, ChartDataFrame frame)
        {
            foreach (var frame2 in dataSource.dataFrames)
            {
                if (ignoreEmptyData && frame2.list.Count == 0)
                    continue;
                return true;
            }
            return false;
        }
        public void UpdateFrame(ChartDataFrame frame)
        {
        }
    }




}
