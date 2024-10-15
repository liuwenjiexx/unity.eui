using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.UI.Editor
{
    public class ItemDrawerAttribute : Attribute
    {
        public ItemDrawerAttribute(Type drawerType)
        {
            DrawerType = drawerType;
        }
        public ItemDrawerAttribute()
        {
        }

        public Type DrawerType { get; set; }

        public Dictionary<string, object> Parameter { get; set; }

        public string InitalizeMethod { get; set; }

    }
}