using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.UI.Editor
{
    [Serializable]
    public struct BuiltinIcon : IEquatable<BuiltinIcon>
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private string assetGuid;

        private Texture2D image;

        public BuiltinIcon(Texture2D image)
        {
            name = null;
            assetGuid = null;
            this.image = null;
            Image = image;
        }

        public Texture2D Image
        {
            get
            {
                if (!image)
                {
                    image = null;
                    if (!string.IsNullOrEmpty(name))
                    {
                        var content = EditorGUIUtility.IconContent(name);
                        if (content != null)
                        {
                            image = content.image as Texture2D;
                        }
                    }
                    else if (!string.IsNullOrEmpty(assetGuid))
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            image = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                        }
                    }
                }
                return image;
            }
            set
            {
                if (image != value)
                {
                    image = value;
                    name = null;
                    assetGuid = null;

                    if (image)
                    {
                        if (EditorUtility.IsPersistent(image))
                        {
                            string assetPath = AssetDatabase.GetAssetPath(image);
                            if (!string.IsNullOrEmpty(assetPath) && (assetPath.StartsWith("Assets", StringComparison.InvariantCultureIgnoreCase)
                                || assetPath.StartsWith("Packages/", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(image, out assetGuid, out long id);
                            }
                        }

                        if (assetGuid == null)
                        {
                            var content = EditorGUIUtility.IconContent(image.name);
                            if (content != null && content.image)
                            {
                                name = image.name;
                            }
                        }
                    }
                }

            }
        }

        public bool Equals(BuiltinIcon other)
        {
            return name == other.name && assetGuid == other.assetGuid;
        }
    }
}
