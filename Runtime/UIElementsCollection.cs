using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{
    [CreateAssetMenu(fileName = "UIElementsCollection", menuName = "UI Toolkit/UIElements Collection")]
    public class UIElementsCollection : ScriptableObject
    {
        //public UIElementsCollection parent;

        public UIElementsCollection[] baseCollections;

        [InspectorName("Uxml")]
        [SerializeField]
        public VisualTreeAsset[] uxmls;

        [SerializeField]
        public StyleSheet[] styleSheets;


        public VisualTreeAsset FindUxml(string name)
        {
            return FindUxml(name, true);
        }

        private VisualTreeAsset FindUxml(string name, bool includeChildren)
        {
            VisualTreeAsset asset = null;
            if (uxmls != null)
            {
                foreach (var item in uxmls)
                {
                    if (!item)
                        continue;
                    if (item.name == name)
                    {
                        asset = item;
                        break;
                    }
                }
            }

            //if (!asset && includeChildren && children != null)
            //{
            //    for (int i = children.Length - 1; i >= 0; i--)
            //    {
            //        var item = children[i];
            //        if (!item)
            //            continue;
            //        asset = item.FindUxml(name);
            //        if (asset)
            //            break;
            //    }
            //}

            //if (!asset && parent)
            //{
            //    asset = parent.FindUxml(name, false);
            //}

            return asset;
        }

        public StyleSheet FindStyleSheet(string name)
        {
            return FindStyleSheet(name, true);
        }

        private StyleSheet FindStyleSheet(string name, bool includeChildren)
        {
            StyleSheet asset = null;
            if (styleSheets != null)
            {
                foreach (var item in styleSheets)
                {
                    if (!item)
                        continue;
                    if (item.name == name)
                    {
                        asset = item;
                        break;
                    }
                }
            }
            /*
            if (!asset && includeChildren && children != null)
            {
                for (int i = children.Length - 1; i >= 0; i--)
                {
                    var item = children[i];
                    if (!item)
                        continue;
                    asset = item.FindStyleSheet(name);
                    if (asset)
                        break;
                }
            }
            */

            //if (!asset && parent)
            //{
            //    asset = parent.FindStyleSheet(name, false);
            //}

            return asset;
        }
    }
}
