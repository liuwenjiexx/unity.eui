using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace UnityEngine.UIElements.Extension
{

    public class ChartAreaLine : ChartWidget
    {
        public ChartAreaLine(ChartDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public override void Draw()
        {
            if (!dataSource.Visiable)
                return;

            var frames = dataSource.dataFrames;
            var count = dataSource.dataFrameCount;
            var painter = chart.context.painter2D;

            Vector2 pos;

            var currentColumnCount = chart.MaxFrameCount;
            float columnPerWidth = 1f / currentColumnCount;

            Vector2 offset = new Vector2(chart.FrameWidth * -1f, 0f);
            offset.x = 0f;
            var oldStrokeColor = painter.strokeColor;
            Color strokeColor = dataSource.Color;
            strokeColor = FrameDataChart.AlphaWeightColor(strokeColor);
            painter.strokeColor = strokeColor;

            painter.BeginPath();

            //if (frameQueue.fill)
            //{
            //    pos = TransformViewPoint(new Vector2(0.5f* columnPerWidth, 0f) + offset);
            //    painter.MoveTo(pos);
            //}

            int index = 0;
            foreach (var frame in frames)
            {
                if (index >= currentColumnCount)
                    break;

                pos.x = (index + 0.5f) * columnPerWidth;
                pos.y = frame.displayPercentage;

                if (index == 0)
                {
                    //if (frameQueue.fill)
                    //{
                    //    painter.LineTo(TransformViewPoint(pos + offset));
                    //}
                    //else
                    {
                        //painter.MoveTo(chart.TransformViewPoint(pos + offset));
                        painter.MoveTo(chart.InvertY( frame.position+ offset));
                    }
                }
                else
                {
                    //painter.LineTo(chart.TransformViewPoint(pos + offset));
                    painter.LineTo(chart.InvertY(frame.position + offset));
                }

                index++;
            }


            painter.Stroke();

            painter.strokeColor = oldStrokeColor;

        }
    }


}
