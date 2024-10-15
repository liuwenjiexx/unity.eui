using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UI.Editor
{

    [AttributeUsage(AttributeTargets.Class)]
    public class UXMLAttribute : Attribute
    {
        //public UXMLAttribute(string uxmlAndUss)
        //{
        //    UXML = uxmlAndUss;
        //    USS = uxmlAndUss;
        //}

        public UXMLAttribute(string uxml, string uss = null)
        {
            UXML = uxml;
            USS = uss;
        }

        public string UXML { get; set; }
        public string USS { get; set; }
    }
}
