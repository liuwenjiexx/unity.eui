using System.ComponentModel;
using UnityEngine.UIElements;

namespace Unity.UI.Editor
{
    public interface IView : INotifyPropertyChanged
    {
        /// <summary>
        /// tab name
        /// </summary>
        string DisplayName { get; set; }

        object Owner { get; set; }

        void OnEnable();

        void OnDisable();

        VisualElement CreateUI();

        object Target { get; set; }

        void OnActive();


    }


}
