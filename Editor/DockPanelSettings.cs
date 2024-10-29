using System;
using System.ComponentModel;
using UnityEngine;
using Unity;

namespace UnityEditor.UIElements.Extension
{
    [Serializable]
    public class DockPanelSettings : INotifyPropertyChanged
    {
        [SerializeField]
        private float height;
        public float Height { get => height; set => PropertyChanged.Invoke(this, nameof(Height), ref height, value); }

        
        [SerializeField]
        private bool collapsed;
        public bool Collapsed { get => collapsed; set => PropertyChanged.Invoke(this, nameof(Collapsed), ref collapsed, value); }

        [SerializeField]
        private float collapsedSize;
        public float CollapsedSize { get => collapsedSize; set => PropertyChanged.Invoke(this, nameof(CollapsedSize), ref collapsedSize, value); }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
