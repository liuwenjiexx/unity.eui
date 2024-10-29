using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UIElements.Extension
{
    public interface IChartPosition
    {
        Vector2 GetPosition(ChartDataSource source);
    }

    public interface IChartPercentage
    {
        float GetPercentage(ChartDataSource source, ChartDataFrame frame);
    }

    public class ChartPosition
    {
        public static readonly IChartPosition Left = new ChartCurrentPosition(false);
        public static readonly IChartPosition Right = new ChartCurrentPosition(true);

        public static readonly IChartPosition LeftTop = new ChartPosition2() { x = 0f, y = 1f };
        public static readonly IChartPosition RightTop = new ChartPosition2() { x = 1f, y = 1f };

        private class ChartCurrentPosition : IChartPosition
        {
            private bool? aliginRight;

            public ChartCurrentPosition(bool aliginRight)
            {
                this.aliginRight = aliginRight;
            }



            public Vector2 GetPosition(ChartDataSource dataSource)
            {
                var chart = dataSource.chart;
                var current = dataSource.currentFrame;
                if (aliginRight.HasValue)
                {
                    float y = 0f;
                    if (current != null)
                        y = current.position.y;
                    if (aliginRight.Value)
                    {
                        return new Vector2(chart.Width, y);
                    }
                    return new Vector2(0f, y);
                }
                else
                {
                    if (current == null)
                        return new Vector2(chart.Width, 0f);
                    return current.position;
                }
            }
        }

        private class ChartPosition2 : IChartPosition
        {
            public float x;
            public float y;
            public Vector2 GetPosition(ChartDataSource source)
            {
                var chart = source.chart;
                return new Vector2(chart.Width * x, chart.Height * y);
            }
        }

    }

    public class ChartPreviousAveragePosition : IChartPosition
    {
        private bool? aliginRight;
        public ChartPreviousAveragePosition()
        {
        }

        public ChartPreviousAveragePosition(bool aliginRight)
        {
            this.aliginRight = aliginRight;
        }

        public static readonly ChartPreviousAveragePosition Current = new ChartPreviousAveragePosition();
        public static readonly ChartPreviousAveragePosition Left = new ChartPreviousAveragePosition(false);
        public static readonly ChartPreviousAveragePosition Right = new ChartPreviousAveragePosition(true);

        public Vector2 GetPosition(ChartDataSource dataSource)
        {
            var current = dataSource.currentFrame;
            if (current == null)
            {
                if (aliginRight.HasValue && !aliginRight.Value)
                {
                    return new Vector2(0f, 0f);
                }
                return new Vector2(dataSource.chart.Width, 0f);
            }

            Vector2 pos;
            pos = current.position;

            if (current.previous != null)
            {
                pos.y = (current.previous.position.y + pos.y) * 0.5f;
            }

            if (aliginRight.HasValue)
            {
                if (aliginRight.Value)
                {
                    pos.x = dataSource.chart.Width;
                }
                else
                {
                    pos.x = 0f;
                }
            }

            return pos;
        }
    }

    public class ChartPreviousAveragePercentagePosition : IChartPosition
    {
        public float maxValue = 1f;
        private bool? aliginRight;

        public ChartPreviousAveragePercentagePosition(float maxValue)
        {
            this.maxValue = maxValue;
        }

        public ChartPreviousAveragePercentagePosition(float maxValue, bool aliginRight)
        {
            this.maxValue = maxValue;
            this.aliginRight = aliginRight;
        }

        public static readonly ChartPreviousAveragePercentagePosition Current = new ChartPreviousAveragePercentagePosition(1f);
        public static readonly ChartPreviousAveragePercentagePosition Left = new ChartPreviousAveragePercentagePosition(1f, false);
        public static readonly ChartPreviousAveragePercentagePosition Right = new ChartPreviousAveragePercentagePosition(1f, true);

        public Vector2 GetPosition(ChartDataSource dataSource)
        {
            var current = dataSource.currentFrame;
            if (current == null)
            {
                if (aliginRight.HasValue && !aliginRight.Value)
                {
                    return new Vector2(0f, 0f);
                }
                return new Vector2(dataSource.chart.Width, 0f);
            }

            Vector2 pos;
            pos = current.position;

            float percentage;

            percentage = current.value;


            if (current.previous != null)
            {
                percentage = (current.previous.value + percentage) * 0.5f;
            }

            percentage /= maxValue;

            pos.y = dataSource.chart.Height * percentage;

            if (aliginRight.HasValue)
            {
                if (aliginRight.Value)
                {
                    pos.x = dataSource.chart.Width;
                }
                else
                {
                    pos.x = 0f;
                }
            }

            return pos;
        }
    }

    public class PercentageFramePosition : IChartPosition
    {
        public IChartValue value;
        public IChartValue maxValue;
        public bool? aliginRight;

        public static readonly PercentageFramePosition Current = new PercentageFramePosition();
        public static readonly PercentageFramePosition Left = new PercentageFramePosition() { aliginRight = false };
        public static readonly PercentageFramePosition Right = new PercentageFramePosition() { aliginRight = true };

        public PercentageFramePosition()
        {
        }



        public Vector2 GetPosition(ChartDataSource dataSource)
        {
            var current = dataSource.currentFrame;
            if (current == null)
            {
                if (aliginRight.HasValue && !aliginRight.Value)
                {
                    return new Vector2(0f, 0f);
                }
                return new Vector2(dataSource.chart.Width, 0f);
            }

            float percentage;
            if (value != null)
            {
                percentage = value.GetValue(dataSource, current);
            }
            else
            {
                percentage = current.value;
            }

            if (maxValue != null)
            {
                float maxF = maxValue.GetValue(dataSource, current);
                if (maxF != 0f)
                {
                    percentage = percentage / maxF;
                }
                else
                {
                    percentage = 0f;
                }
            }

            Vector2 pos = new();

            pos.y = dataSource.chart.Height * percentage;

            if (aliginRight.HasValue)
            {
                if (aliginRight.Value)
                {
                    pos.x = dataSource.chart.Width;
                }
            }

            return pos;
        }
    }

}
