using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UI.Editor
{
    public class MultiLineAttribute : Attribute
    {
        public MultiLineAttribute(int lines)
        {
            Lines = lines;
        }

        public int Lines { get; set; }
    }
}