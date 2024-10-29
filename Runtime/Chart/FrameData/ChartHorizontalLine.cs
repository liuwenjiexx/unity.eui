using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UIElements.Extension
{


    public class ChartHorizontalLine : ChartWidget
    {

        public ChartHorizontalLine(IChartValue value, IChartPosition position = null)
        {
            this.Value = value;
            this.position = position;
            layer = 5;
        }

        public IChartValue Value { get; set; }

        public override void Draw()
        {
            var ctx = chart.context;
             
            if (position != null)
            {
                rect.position = position.GetPosition(dataSource); 
            }
        

            var painter = ctx.painter2D;
            var oldStrokeColor = painter.strokeColor;
            painter.strokeColor = dataSource.Color;
            painter.BeginPath();
            Vector2 pos = chart.InvertY(rect.position);
            painter.MoveTo(new Vector2(0f, pos.y));
            painter.LineTo(new Vector2(chart.Width, pos.y));

            painter.Stroke();
            painter.strokeColor = oldStrokeColor;
            //if (dataSource.displayValuePercentage == 0.5f)
            //{
            //    Debug.Log("content: " + contentRect.height + ", " + TransformViewPoint(new Vector2(0f, dataSource.displayValuePercentage)).y);
            //}
        }
    }
}
