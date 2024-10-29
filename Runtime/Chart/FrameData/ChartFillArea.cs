using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace UnityEngine.UIElements.Extension
{
    public class ChartFillArea : ChartWidget
    {

        public ChartFillArea(ChartDataSource dataSource)
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

            if (count == 0)
                return;

            var oldFillColor = painter.fillColor;
            Color fillColor = dataSource.Color;
            fillColor.a *= 0.3f;
            painter.fillColor = fillColor;

            painter.BeginPath();

            int index = 0;
            Vector2 offset = new Vector2(0f, 0f); 
            foreach (var frame in frames)
            {
                if (index == 0)
                {
                    painter.MoveTo(chart.InvertY(new Vector2(frame.position.x, 0) + offset));
                }
                painter.LineTo(chart.InvertY(frame.position + offset));
                index++;
            }

            if (dataSource.currentFrame != null)
            {
                painter.LineTo(chart.InvertY(new Vector2(dataSource.currentFrame.position.x, 0) + offset));
            }

            painter.Fill();

            painter.fillColor = oldFillColor;

            //index = 0;
            //foreach (var frame in frames)
            //{
            //    chart.FillRect(chart.context, chart.InvertY(new Rect(frame.position.x - 1f * chart.FrameWidth + offset.x, offset.y, chart.FrameWidth, chart.Height)), index % 2 == 0 ? new Color(1, 0, 0, 0.2f) : new Color(0, 0, 1f, 0.2f));
            //    index++;
            //}
        }
    }

}
