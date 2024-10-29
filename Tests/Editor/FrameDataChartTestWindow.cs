using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yanmonet.SettingsManagement.Editor;
using Yanmonet.SettingsManagement;
using UnityEngine.UIElements;
using System;
using Random = UnityEngine.Random;

namespace Yanmonet.UI.UIElements.Tests
{
    public class FrameDataChartTestWindow : EditorWindow
    {
        private VisualElement root;
        private FrameDataChart chart;
        [NonSerialized]
        ChartDataSource originDataSource;
        [NonSerialized]
        ChartDataSource compressionDataSource;
        [NonSerialized]
        ChartDataSource compressionRatioDataSource;
        [NonSerialized]
        double nextRefreshChartTime;


        static string PackageName => SettingsUtility.GetPackageName(typeof(FrameDataChart));

        [MenuItem("Test/FrameDataChart Test")]
        static void ShowWindow()
        {
            var win = GetWindow<FrameDataChartTestWindow>();
            win.Show();
        }


        private void CreateGUI()
        {
            root = EditorSettingsUtility.LoadUXML(rootVisualElement, EditorSettingsUtility.GetTestsEditorUXMLPath(PackageName, nameof(FrameDataChartTestWindow)));
            root.style.flexGrow = 1f;

            chart = new FrameDataChart();
            root.Q("frame-chart").Add(chart);
            chart.Title = "Frame Chart";

            chart.style.flexGrow = 1f;
            chart.UpdateBefore += UpdateTestData;

            originDataSource = chart.CreateDataSource("origin-data");
            originDataSource.Title = "Origin";
            originDataSource.Color = new Color(0.8f, 0.5f, 0.16f);
            originDataSource.Fill.visiable = true;
            originDataSource.Line.visiable = true;
            originDataSource.AddWidget(new ChartSortLabel()
            {
                value = ChartValue.Value,
                position = ChartPosition.Right,
                formatDisplayText = o => $"{ToByteUnitString((int)o)}",
                aliginRight = true,
                backgroundColor = Color.black
            });

            compressionDataSource = chart.CreateDataSource("compression-data");
            compressionDataSource.Title = "Compression";
            compressionDataSource.Color = new Color(0.16f, 0.56f, 0.8f);
            compressionDataSource.Fill.visiable = true;
            compressionDataSource.Line.visiable = true;
            compressionDataSource.referenceParentSource = originDataSource;
            compressionDataSource.beforeFrameCalculators.Add(new ParentDisplayPercentageFrameCalculator(originDataSource));

            var speed = new ChartSpeedCalculator(true);
            compressionDataSource.afterFrameCalculators.Add(speed);

            compressionDataSource.AddWidget(new ChartSortLabel()
            {
                value = ChartValue.Value,
                position = ChartPosition.Right,
                formatDisplayText = o => $"{ToByteUnitString((int)o)}",
                aliginRight = true,
                backgroundColor = Color.black
            });

            compressionDataSource.AddWidget(new ChartSortLabel()
            {
                value = speed.Speed,
                formatDisplayText = o => $"{ToByteUnitString((int)o)}/s",
                aliginRight = false,
                backgroundColor = Color.black,
                position = new PercentageFramePosition()
                {
                    value = speed.Speed,
                    maxValue = speed.MaxSpeed
                }
            });
            compressionDataSource.AddWidget(new ChartHorizontalLine(speed.Speed)
            {
                position = new PercentageFramePosition()
                {
                    value = speed.Speed,
                    maxValue = speed.MaxSpeed
                }
            });

            compressionDataSource.AddWidget(new ChartSortLabel()
            {
                value = speed.MaxSpeed,
                formatDisplayText = o => $"max {ToByteUnitString((int)o)}/s",
                aliginRight = false,
                backgroundColor = Color.black,
                position = ChartPosition.LeftTop
            });
            compressionDataSource.AddWidget(new ChartHorizontalLine(speed.Speed)
            {
                position = ChartPosition.LeftTop,
            });

            compressionRatioDataSource = chart.CreateDataSource("compression-ratio-data");
            compressionRatioDataSource.Title = "Compression Ratio";
            compressionRatioDataSource.Color = new Color(0.85f, 0.77f, 0.66f);
            compressionRatioDataSource.IsPercentageValue = true;
            compressionRatioDataSource.beforeFrameCalculators.Add(new PercentageFrameValueCalculator(originDataSource, compressionDataSource));
            compressionRatioDataSource.beforeFrameCalculators.Add(new ValueToDisplayPercentageFrameCalculator());

            //compressionRatioDataSource.AddWidget(new ChartSortLabel()
            //{
            //    value = new ChartCurrentValue(),
            //    PositionProvider = CurrentFramePosition.Right,
            //    formatDisplayText = o => $"{o * 100f:0.#}%",
            //    aliginRight = true,
            //    backgroundColor = Color.black
            //});
            var avgValue = ChartValue.SmoothValue;

            compressionRatioDataSource.AddWidget(new ChartSortLabel()
            {
                value = avgValue,
                position = new PercentageFramePosition()
                {
                    aliginRight = false,
                    value = avgValue,
                },
                formatDisplayText = o => $"{o * 100f:0.#}%",
                aliginRight = false,
                backgroundColor = Color.black
            });
            compressionRatioDataSource.AddWidget(new ChartHorizontalLine(avgValue)
            {
                position = new PercentageFramePosition()
                {
                    aliginRight = false,
                    value = avgValue,
                }
            }); 

            chart.Initialize();

        }


        string ToByteUnitString(int bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes}B";
            }
            else if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024D:0.#}KB";
            }
            else if (bytes < 1024D * 1024 * 1024)
            {
                return $"{bytes / (1024D * 1024):0.#}MB";
            }
            return $"{bytes / (1024D * 1024 * 1024):0.#}GB";
        }

        private void Update()
        {
            if (Time.realtimeSinceStartupAsDouble > nextRefreshChartTime)
            {
                nextRefreshChartTime = Time.realtimeSinceStartupAsDouble + 0.2f;
            }
        }


        void UpdateTestData()
        {

            if (Application.isPlaying)
            {
                float v = Random.value * 1000;
                //v = 1000;
                float rate = Random.value * 0.8f;
                //rate = 0.8f;
                originDataSource.Add(new ChartDataItem()
                {
                    frameCount = Time.frameCount,
                    time = Time.realtimeSinceStartup,
                    //value = 0.3f*1000
                    value = v
                });

                compressionDataSource.Add(new ChartDataItem()
                {
                    frameCount = Time.frameCount,
                    time = Time.realtimeSinceStartup,
                    //value = 0.3f*1000
                    value = v * rate,
                });

            }

        }

    }

}