using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UIElements.Extension
{
    public abstract class ChartWidget
    {
        public FrameDataChart chart;
        public ChartDataSource dataSource;
        public bool visiable = true;

        public int layer;

        public Rect rect;

        public IChartPosition position;

        public virtual void Update()
        {

        }


        public virtual void BeginDraw()
        {

        }

        public abstract void Draw();

        public virtual void EndDraw()
        {
        }
    }


}