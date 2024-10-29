using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Editor;

namespace UnityEditor.UIElements.Extension
{

    public class ObjectView : VisualElement
    {
        Toolbar headerContainer;
        private ListView listView;
        object target;
        MemberWrap root;
        bool expandAll;
        List<MemberWrap> list = new();
        private bool builded;

        public ObjectView()
        {
            headerContainer = new Toolbar();
            headerContainer.AddToClassList("msg-header");
            Add(headerContainer);
            NameColumn = new ColumnInfo("Name", 0)
            {
                Name = "member-name",
                HeaderUssClassName = "object-header-name",
                CreateHeader = (c) =>
                {
                    var headerName = new ToolbarMenu();
                    headerName.menu.AppendAction("Expand All", act =>
                    {
                        ExpandAll();
                    });
                    headerName.menu.AppendAction("Collapse All", act =>
                    {
                        CollapseAll();
                    });
                    return headerName;
                }
            };
            Columns.Add(NameColumn);

            ValueColumn = new ColumnInfo("Value", 350)
            {
                Name = "member-value",
                HeaderUssClassName = "object-header-value",
            };
            Columns.Add(ValueColumn);

            TypeColumn = new ColumnInfo("Type", 200)
            {
                Name = "member-type",
                HeaderUssClassName = "object-header-type",
            };
            Columns.Add(TypeColumn);

            listView = new ListView();
            listView.AddToClassList("object-list");
            listView.style.flexGrow = 1f;
            listView.fixedItemHeight = 20;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            var scrollView = listView.Q<ScrollView>();

            scrollView.Add(new IMGUIContainer(() =>
            {
                if (Event.current.type == EventType.Repaint)
                {
                    int scrollWidth = 0;
                    if (scrollView.verticalScroller.style.display == DisplayStyle.Flex)
                    {
                        scrollWidth = (int)scrollView.verticalScroller.layout.width;
                    }

                    var last = Columns.Last();
                    if (last.Width > 0)
                    {
                        int lastColumnWidth;
                        lastColumnWidth = last.Width + scrollWidth;
                        var header = headerContainer.Q(className: last.HeaderUssClassName);
                        if (header.style.width != lastColumnWidth)
                            header.style.width = lastColumnWidth;
                    }
                    else
                    {
                        headerContainer.style.paddingRight = scrollWidth;
                    }
                }
            }));
            Add(listView);
            listView.makeItem = () =>
            {
                VisualElement container = new VisualElement();
                container.AddToClassList("object-item");
                container.style.flexDirection = FlexDirection.Row;
                container.style.alignItems = Align.Center;
                container.style.paddingLeft = 0f;

                VisualElement depth = new VisualElement();
                depth.AddToClassList("object-item-depth");
                container.Add(depth);

                Foldout foldout = new Foldout();
                foldout.style.width = 14;
                foldout.style.marginLeft = 0;
                foldout.style.paddingLeft = 0;
                var mark = foldout.Q("unity-checkmark");
                if (mark != null)
                {
                    mark.style.marginLeft = 0;
                    mark.style.marginRight = 0;
                }
                foldout.RegisterValueChangedCallback(e =>
                {
                    var member = container.userData as MemberWrap;
                    if (e.newValue)
                    {
                        Expand(member);
                    }
                    else
                    {
                        Collapse(member, true);
                    }
                });
                container.Add(foldout);

                foreach (var column in Columns)
                {
                    if (!column.Visiable) continue;

                    Label cellLabel = new Label();
                    cellLabel.name = column.Name;
                    cellLabel.style.overflow = Overflow.Hidden;
                    if (column.Width > 0)
                        cellLabel.style.width = column.Width;
                    else
                        cellLabel.style.flexGrow = 1f;
                    container.Add(cellLabel);

                    if (column == NameColumn)
                    {
                        cellLabel.AddManipulator(new ContextualMenuManipulator((evt) =>
                        {
                            var member = container.userData as MemberWrap;
                            evt.menu.AppendAction("Copy", (e) =>
                            {
                                EditorGUIUtility.systemCopyBuffer = member.name;
                            }, (e) =>
                            {
                                return DropdownMenuAction.Status.Normal;
                            });
                        }));
                    }
                    else if (column == ValueColumn)
                    {
                        cellLabel.AddManipulator(new ContextualMenuManipulator((evt) =>
                        {
                            var member = container.userData as MemberWrap;
                            evt.menu.AppendAction("Copy", (e) =>
                            {
                                if (member.value != null)
                                {
                                    EditorGUIUtility.systemCopyBuffer = member.value.ToString();
                                }
                                else
                                {
                                    EditorGUIUtility.systemCopyBuffer = string.Empty;
                                }
                            }, (e) =>
                            {
                                if (!member.displayValue)
                                    return DropdownMenuAction.Status.Disabled;
                                return DropdownMenuAction.Status.Normal;
                            });
                        }));
                    }else if (column == TypeColumn)
                    {
                        cellLabel.AddManipulator(new ContextualMenuManipulator((evt) =>
                        {
                            var member = container.userData as MemberWrap;
                            evt.menu.AppendAction("Copy", (e) =>
                            {
                                EditorGUIUtility.systemCopyBuffer = member.valueType?.FullName;
                            }, (e) =>
                            {
                                return DropdownMenuAction.Status.Normal;
                            });
                        }));
                    }
                }

                return container;
            };

            listView.bindItem = (view, index) =>
            {
                var member = listView.itemsSource[index] as MemberWrap;
                view.userData = member;
                var foldout = view.Q<Foldout>();

                VisualElement depth = view.Q(className: "object-item-depth");
                if (CanExpand(member))
                {
                    depth.style.width = member.depth * 16;
                }
                else
                {
                    depth.style.width = member.depth * 16 + 14;
                }

                if (NameColumn.Visiable)
                {
                    var nameLabel = view.Q<Label>(NameColumn.Name);
                    nameLabel.text = member.name;
                    nameLabel.tooltip = $"{member.name} ({member.valueType.Name})";
                }

                if (ValueColumn.Visiable)
                {
                    var valueLabel = view.Q<Label>(ValueColumn.Name);
                    string valueText = null;
                    if (member.displayValue)
                    {
                        if (member.value == null)
                        {
                            valueText = NullDisplayText;
                        }
                        else
                        {
                            if (!CanExpand(member))
                            {
                                valueText = member.value.ToString();
                            }
                        }
                    }
                    valueLabel.text = valueText;
                    valueLabel.tooltip = valueText;

                }
                if (TypeColumn.Visiable)
                {
                    var typeLabel = view.Q<Label>(TypeColumn.Name);
                    typeLabel.text = GetDisplayTypeName(member.valueType);
                    typeLabel.tooltip = member.valueType.FullName;
                }

                //typeLabel.RemoveFromClassList("value-msg");
                if (CanExpand(member))
                {
                    foldout.style.display = DisplayStyle.Flex;
                    //typeLabel.AddToClassList("value-msg");
                }
                else
                {
                    foldout.style.display = DisplayStyle.None;
                }

                foldout.SetValueWithoutNotify(member.expand);

            };

            schedule.Execute(() =>
            {
                if (!builded)
                    Rebuild();
            });
        }



        public object Target
        {
            get => target;
            set
            {
                if (!object.Equals(target, value))
                {
                    root = null;
                    list.Clear();
                    target = value;

                    if (target != null)
                    {
                        root = new MemberWrap() { value = target, valueType = target.GetType(), depth = -1 };
                        root.isShow = false;
                        list.Add(root);
                        CreateMembers(root);
                        ExpandAll();
                    }
                }
            }
        }

        public List<ColumnInfo> Columns { get; private set; } = new List<ColumnInfo>();

        public ColumnInfo NameColumn { get; private set; }
        public ColumnInfo ValueColumn { get; private set; }
        public ColumnInfo TypeColumn { get; private set; }

        public Toolbar HeaderContainer => headerContainer;

        public static string NullDisplayText = "(null)";

        void CreateMembers(MemberWrap parent)
        {
            if (parent.value != null)
            {
                var targetType = parent.value.GetType();
                foreach (var mInfo in targetType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.GetField))
                {
                    if (!(mInfo.MemberType == MemberTypes.Property || mInfo.MemberType == MemberTypes.Field))
                        continue;
                    Type valueType;
                    object value;
                    var fInfo = mInfo as FieldInfo;
                    if (fInfo != null)
                    {
                        if (!fInfo.IsPublic) continue;

                        valueType = fInfo.FieldType;
                        value = fInfo.GetValue(parent.value);
                    }
                    else
                    {
                        var pInfo = mInfo as PropertyInfo;
                        if (!pInfo.CanRead) continue;
                        if (pInfo.GetMethod == null || !pInfo.GetMethod.IsPublic) continue;
                        if (pInfo.IsIndexer()) continue;
                        valueType = pInfo.PropertyType;
                        value = pInfo.GetValue(parent.value);
                    }
                    if (value != null)
                    {
                        valueType = value.GetType();
                    }

                    MemberWrap child = new MemberWrap()
                    {
                        name = mInfo.Name,
                        value = value,
                        valueType = valueType,
                        depth = parent.depth + 1,
                        expand = false,
                        isShow = parent.expand,
                        isLeaf = true,
                        member = mInfo,
                        parent = parent
                    };
                    if (parent.children == null) parent.children = new();
                    parent.children.Add(child);
                    list.Add(child);
                    var typeCode = Type.GetTypeCode(valueType);
                    if (typeCode == TypeCode.Object)
                    {
                        if (!valueType.IsEnum)
                        {
                            IList list = null;
                            Type itemType = null;
                            if (valueType.IsArray)
                            {
                                list = (Array)value;
                                itemType = valueType.GetElementType();
                            }
                            else
                            {
                                list = value as IList;
                                if (list != null)
                                {
                                    var listType = valueType.FindByGenericTypeDefinition(typeof(IList<>));
                                    if (listType != null)
                                    {
                                        itemType = listType.GetGenericArguments()[0];
                                    }
                                    else
                                    {
                                        itemType = typeof(object);
                                    }
                                }
                            }

                            if (list != null)
                            {
                                child.displayValue = false;

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var itemValue = list[i];
                                    var itemType2 = itemValue != null ? itemValue.GetType() : itemType;
                                    MemberWrap itemMember = new MemberWrap()
                                    {
                                        name = $"[{i}]",
                                        value = itemValue,
                                        valueType = itemType2,
                                        depth = child.depth + 1,
                                        expand = false,
                                        isShow = child.expand,
                                        isLeaf = true,
                                        member = null,
                                        parent = child,
                                        displayValue = false,
                                    };
                                    if (child.children == null) child.children = new();
                                    child.children.Add(itemMember);
                                    this.list.Add(itemMember);

                                    if (itemType == typeof(string) || itemType.IsPrimitive)
                                    {
                                        //if (itemValue == null)
                                        //    itemMember.name = NullDisplayText;
                                        //else
                                        //    itemMember.name = itemValue.ToString();
                                        itemMember.displayValue = true;
                                    }
                                    else
                                    {
                                        CreateMembers(itemMember);
                                    }
                                }
                            }
                            else if (value is IDictionary dic)
                            {
                                Type keyType = typeof(object);
                                itemType = typeof(object);

                                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                                {
                                    keyType = valueType.GetGenericArguments()[0];
                                    itemType = valueType.GetGenericArguments()[1];
                                }

                                foreach (var key in dic.Keys)
                                {
                                    var itemValue = dic[key];
                                    var keyType2 = key != null ? key.GetType() : keyType;
                                    var itemType2 = itemValue != null ? itemValue.GetType() : itemType;
                                    MemberWrap itemMember = new MemberWrap()
                                    {
                                        name = "[Key]",
                                        value = key,
                                        valueType = keyType2,
                                        depth = child.depth + 1,
                                        expand = false,
                                        isShow = child.expand,
                                        isLeaf = true,
                                        member = null,
                                        parent = child,
                                        displayValue = true,
                                    };
                                    if (child.children == null) child.children = new();
                                    child.children.Add(itemMember);
                                    this.list.Add(itemMember);

                                    MemberWrap valueMember = new MemberWrap()
                                    {
                                        name = "[Value]",
                                        value = itemValue,
                                        valueType = itemType2,
                                        depth = child.depth + 1,
                                        expand = false,
                                        isShow = child.expand,
                                        isLeaf = true,
                                        member = null,
                                        parent = child,
                                        displayValue = true,
                                    };

                                    //if (itemMember.children == null) itemMember.children = new();
                                    child.children.Add(valueMember);
                                    this.list.Add(valueMember);
                                    CreateMembers(valueMember);
                                }
                            }
                            else
                            {

                                CreateMembers(child);
                            }
                        }
                    }
                }
            }
            if (parent.children == null || parent.children.Count == 0)
            {
                parent.isLeaf = true;
            }
            else
            {
                parent.isLeaf = false;
            }
        }

        public void Refresh()
        {
            if (!builded)
                Rebuild();
            listView.itemsSource = list.Where(o => o.isShow).ToList();
            listView.RefreshItems();
        }

        public void Rebuild()
        {
            builded = true;
            headerContainer.Clear();

            foreach (var column in Columns)
            {
                if (!column.Visiable) continue;

                VisualElement header = null;
                if (column.CreateHeader != null)
                    header = column.CreateHeader(column);
                if (header == null)
                    header = new ToolbarButton();

                if (column.HeaderUssClassName != null)
                    header.AddToClassList(column.HeaderUssClassName);
                if (header is ToolbarMenu)
                {
                    var menuHeader = (ToolbarMenu)header;
                    menuHeader.text = column.Header;
                }
                else if (header is ToolbarButton)
                {
                    var buttonHeader = (ToolbarButton)header;
                    buttonHeader.text = column.Header;
                }
                header.style.unityTextAlign = TextAnchor.MiddleLeft;
                if (column.Width == 0)
                    header.style.flexGrow = 1f;
                else
                    header.style.width = column.Width;

                headerContainer.Add(header);

            }
            listView.Rebuild();
        }


        public bool ExpandAll()
        {
            bool changed = false;
            expandAll = true;
            foreach (var item in list.Skip(1))
            {
                if (CanExpand(item))
                {
                    item.expand = true;
                }
                if (!item.isShow)
                {
                    item.isShow = true;
                    changed = true;
                }
            }
            if (changed)
                Refresh();
            return changed;
        }

        bool CanExpand(MemberWrap item)
        {
            if (item.isLeaf)
                return false;
            return true;
        }

        bool Expand(MemberWrap item)
        {
            if (item.expand || !CanExpand(item))
                return false;
            bool changed = false;

            foreach (var child in item.children)
            {
                if (!child.isShow)
                {
                    child.isShow = true;
                    changed = true;
                }
            }
            item.expand = true;
            if (changed)
                Refresh();
            return changed;
        }



        public bool CollapseAll()
        {
            expandAll = false;
            var changed = false;
            if (root.children != null)
            {
                foreach (var child in root.children)
                {
                    changed |= Collapse(child, false);
                }
            }
            if (changed)
                Refresh();
            return changed;
        }
        bool Collapse(MemberWrap item, bool refresh)
        {
            if (!item.expand)
                return false;
            var changed = false;
            foreach (var child in item.AllChildren())
            {
                child.expand = false;
                if (child.isShow)
                {
                    child.isShow = false;
                    changed |= true;
                }
            }

            item.expand = false;
            if (changed && refresh)
                Refresh();
            return changed;
        }

        string GetDisplayTypeName(Type type)
        {
            var itemType = GetItemType(type);
            if (itemType != null)
            {
                return GetDisplayTypeName(itemType) + "[]";
            }
            return type.Name;
        }

        Type GetItemType(Type type)
        {
            Type itemType = null;
            if (type.IsArray)
            {
                itemType = type.GetElementType();
            }
            else
            {
                var listType = type.FindByGenericTypeDefinition(typeof(IList<>));
                if (listType != null)
                {
                    itemType = listType.GetGenericArguments()[0];
                }
            }

            return itemType;
        }
        class MemberWrap
        {
            public string name;
            public MemberInfo member;
            public object value;
            public int depth;
            public MemberWrap parent;
            public bool expand;
            public List<MemberWrap> children;
            public bool isShow;
            public Type valueType;
            public bool isLeaf;
            public bool displayValue = true;

            public object GetMemberValue(object owner)
            {

                object value = null;

                if (owner != null)
                {
                    var pInfo = owner.GetType().GetProperty(member.Name);
                    if (pInfo != null)
                    {
                        value = pInfo.GetValue(owner);
                    }
                    else
                    {
                        var fInfo = owner.GetType().GetField(member.Name);
                        if (fInfo != null)
                        {
                            value = fInfo.GetValue(owner);
                        }
                    }
                }
                return value;
            }

            public IEnumerable<MemberWrap> AllChildren()
            {
                if (children == null || children.Count == 0)
                    yield break;
                foreach (var child in children)
                {
                    yield return child;
                    foreach (var child2 in child.AllChildren())
                    {
                        yield return child2;
                    }
                }
            }

        }


    }
}
