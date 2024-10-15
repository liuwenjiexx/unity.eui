using System;
using System.ComponentModel;
using UnityEngine.UIElements;
using Unity;

namespace Unity.UI.Editor
{

    public class ColumnInfo : INotifyPropertyChanged
    {
        public ColumnInfo() { }

        public ColumnInfo(string header, int width)
        {
            this.header = header;
            this.width = width;
        }

        private string name;
        public string Name { get => name; set => PropertyChanged.Invoke(this, nameof(Name), ref name, value); }

        private string header;
        public string Header { get => header; set => PropertyChanged.Invoke(this, nameof(Header), ref header, value); }

        private string headerTooltip;
        public string HeaderTooltip { get => headerTooltip; set => PropertyChanged.Invoke(this, nameof(HeaderTooltip), ref headerTooltip, value); }

        private string headerUssClassName;
        public string HeaderUssClassName { get => headerUssClassName; set => PropertyChanged.Invoke(this, nameof(HeaderUssClassName), ref headerUssClassName, value); }

        public Func<ColumnInfo, VisualElement> CreateHeader;

        private int width;
        public int Width { get => width; set => PropertyChanged.Invoke(this, nameof(Width), ref width, value); }

        private string ussClassName;
        public string UssClassName { get => ussClassName; set => PropertyChanged.Invoke(this, nameof(UssClassName), ref ussClassName, value); }

        private bool visiable = true;
        public bool Visiable { get => visiable; set => PropertyChanged.Invoke(this, nameof(Visiable), ref visiable, value); }

        private bool editable;
        public bool Editable { get => editable; set => PropertyChanged.Invoke(this, nameof(Editable), ref editable, value); }

        private Type columnType;
        public Type ColumnType { get => columnType; set => PropertyChanged.Invoke(this, nameof(ColumnType), ref columnType, value); }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
