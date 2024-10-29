using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements.Extension
{
    public static class Extensions  
    {
        static FieldInfo m_FormatSelectedValueCallback;
        static FieldInfo m_FormatListItemCallback;

        internal static void SetFormatSelectedValueCallback(this DropdownField field, Func<string, string> format)
        {
            if (m_FormatSelectedValueCallback == null)
                m_FormatSelectedValueCallback = typeof(DropdownField).GetField("m_FormatSelectedValueCallback", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            m_FormatSelectedValueCallback?.SetValue(field, format);
        }

        internal static void SetFormatListItemCallback(this DropdownField field, Func<string, string> format)
        {
            if (m_FormatListItemCallback == null)
                m_FormatListItemCallback = typeof(DropdownField).GetField("m_FormatListItemCallback", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            m_FormatListItemCallback?.SetValue(field, format);
        }

    }
}
