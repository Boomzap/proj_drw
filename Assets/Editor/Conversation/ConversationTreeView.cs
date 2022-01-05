using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.Collections.Generic;

namespace Boomzap.Conversation
{
    public class ConversationTreeNode : TreeViewItem
    {
        static string DescribeNode(Node node)
        {
            if (node is RootNode) return "[root]";
            ConversationNode cnode = node as ConversationNode;

            string convoText = cnode.text;

            if (string.IsNullOrEmpty(convoText)) return "[continue]";
            
            return convoText.Replace("\n", " / ");
        }


        public Node     node;
        public bool     isLink = false;

        public bool drawItalic
        {
            get 
            {
                if (node is RootNode) return false;
                ConversationNode cnode = node as ConversationNode;

                return cnode.nodeStyle == ConversationNode.NodeStyle.Narration;
            }
        }

        public override string displayName 
        { 
            get => DescribeNode(node);
        }

        internal ConversationTreeNode(int id, RootNode node)
            : base(id, 0, DescribeNode(node))
        {
            this.node = node;
            RefreshIcon();
        }

        internal ConversationTreeNode(int id, ConversationNode node)
            : base(id, 0, DescribeNode(node))
        {
            this.node = node;
            RefreshIcon();
        }

        internal ConversationTreeNode(int id)
            : base(id, -1, "real root")
        {
            this.node = null;
        }

        public void RefreshIcon()
        {
            if (node == null) return;

            if (node.isOption)
            {
                icon = EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo").image as Texture2D;
            } else
            {
                icon = EditorGUIUtility.IconContent("sv_icon_dot14_pix16_gizmo").image as Texture2D;
            }
        }

        public void RemoveChild(ConversationTreeNode child)
        {
            if (hasChildren && children.Contains(child))
            {
                children.Remove(child);
                child.parent = null;
            }
        }

        public void InsertChild(ConversationTreeNode child, int index)
        {
            if (!hasChildren)
            {
                AddChild(child);
            }
            else
            {
                children.Insert(index, child);
                child.parent = this;
            }
        }

        public ConversationTreeNode GetSiblingAfter()
        {
            int myIndex = IndexInParent();
            if (myIndex == parent.children.Count - 1) return null;
            return parent.children[myIndex+1] as ConversationTreeNode;
        }

        public int IndexInParent()
        {
            return parent.children.IndexOf(this);
        }

        public bool CanMoveUp
        {
            get 
            {
                ConversationTreeNode parentNode = parent as ConversationTreeNode;
                if (parentNode == null) return false;

                return parentNode.children.IndexOf(this) > 0;
            }
        }

        public bool CanMoveDown
        {
            get 
            {
                ConversationTreeNode parentNode = parent as ConversationTreeNode;
                if (parentNode == null) return false;

                return parentNode.children.IndexOf(this) < (parentNode.children.Count - 1);
            }
        }

        public bool ContainsChild(ConversationTreeNode child)
        {
            if (!hasChildren) return false;
            return children.Contains(child);
        }
    }

    public class ConversationTreeView : TreeView
    {
        const string EDITOR_PREFIX = "Boomzap.Conversation.";
        Conversation editingConversation;
        public Conversation conversation { get => editingConversation; }
        int nextId = 0;
        int nodeIdOffset = 100000;   // for 'hacking' the annoying auto-select of "dragging" rows even after we remove them

        string textSearchString = "";

        public Color dialogColor
        {
            get { return GetEditorPrefsColor("dialogColor", new Color32(214, 157, 133, 255)); }
            set { SetEditorPrefsColor("dialogColor", value); }
        }

        public Color narrationColor
        {
            get { return GetEditorPrefsColor("narrationColor", new Color32(216, 160, 223, 255)); }
            set { SetEditorPrefsColor("narrationColor", value); }
        }

        public Color optionColor
        {
            get { return GetEditorPrefsColor("optionColor", new Color32(156, 220, 254, 255)); }
            set { SetEditorPrefsColor("optionColor", value); }
        }

        public Color linkColor
        {
            get { return GetEditorPrefsColor("linkColor", new Color32(220, 220, 170, 255)); }
            set { SetEditorPrefsColor("linkColor", value); }
        }

        public bool showDebugNodeIDs
        {
            get { return EditorPrefs.GetBool(EDITOR_PREFIX + "showNodeIDs", false); }
            set { EditorPrefs.SetBool(EDITOR_PREFIX + "showNodeIDs", value); }
        }

        public List<ConversationTreeNode> GetClipboard() => owner.GetClipboard();
        public void SetClipboard(IEnumerable<ConversationTreeNode> data) => owner.SetClipboard(data);

        public float iconWidth = 16f;
        public float spaceBetweenIconAndText = 2f;
        public float iconLeftPadding = 2f;
        public float iconRightPadding = 2f;
        public float iconTotalPadding => iconLeftPadding + iconRightPadding;

        List<SerializableGUID> expandedNodeGUIDs = new List<SerializableGUID>();
        ConversationTreeNode[] dragAndDropNodes = null;
        ConversationTreeNode[] dragAndDropInsertBefore = null;
        ConversationTreeNode dragAndDropParent = null;
        bool altDown = false;

        Color GetEditorPrefsColor(string name, Color defaultColor)
        {
            string completeName = EDITOR_PREFIX + name;

            float r = EditorPrefs.GetFloat(completeName + "_R", defaultColor.r);
            float g = EditorPrefs.GetFloat(completeName + "_G", defaultColor.g);
            float b = EditorPrefs.GetFloat(completeName + "_B", defaultColor.b);
            float a = EditorPrefs.GetFloat(completeName + "_A", defaultColor.a);

            return new Color(r, g, b, a);            
        }

        void SetEditorPrefsColor(string name, Color toColor)
        {
            string completeName = EDITOR_PREFIX + name;

            EditorPrefs.SetFloat(completeName + "_R", toColor.r);
            EditorPrefs.SetFloat(completeName + "_G", toColor.g);
            EditorPrefs.SetFloat(completeName + "_B", toColor.b);
            EditorPrefs.SetFloat(completeName + "_A", toColor.a);            
        }

        public ConversationTreeNode[] selectedNodes
        {
            get
            {
                var selectedIds = GetSelection();
                return selectedIds.Select(x => FindItem(x, rootItem) as ConversationTreeNode).Where(x => x != null).ToArray();
            }
        }

        internal static class Styles
        {
            public static GUIStyle lineStyle = null;
            public static GUIStyle lineBoldStyle = null;
            public static GUIStyle lineItalicStyle = null;
            public static GUIStyle selectedBackgroundStyle = null;
            public static Texture2D[] selectedBackgroundStyleDefaultTextures = null;
        }

        ConversationTreeViewMenu menu;
        ConversationEditor owner;
        

        public ConversationTreeView(TreeViewState state, Conversation convo, ConversationEditor owner)
            : base(state)
        {
            editingConversation = convo;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.owner = owner;


            menu = new ConversationTreeViewMenu(this);
            Reload();
        }

        void InitializeStyles()
        {
                    
            Styles.lineStyle = new GUIStyle("TV Line");
            Styles.lineBoldStyle = new GUIStyle("TV LineBold");
            Styles.lineItalicStyle = new GUIStyle("TV LineBold");

            Styles.lineItalicStyle.fontStyle = FontStyle.Italic;
        }
        public void SetSearchString(string s)
        {
            textSearchString = s;

            SetSelection(FindItemsByText(s), TreeViewSelectionOptions.RevealAndFrame);
        }

        int[] FindItemsByText(string s)
        {
            List<int> matching = new List<int>();

            if (string.IsNullOrEmpty(s))
                return new int[0];

            Stack<ConversationTreeNode> nodesToVisit = new Stack<ConversationTreeNode>();
            nodesToVisit.Push(rootItem.children[0] as ConversationTreeNode);

            while (nodesToVisit.Count > 0)
            {
                ConversationTreeNode current = nodesToVisit.Pop();

                if (!current.isLink)
                {
                    if (current.hasChildren)
                    {
                        foreach (var c in current.children)
                            nodesToVisit.Push(c as ConversationTreeNode);
                    }

                    ConversationNode node = current.node as ConversationNode;
                    if (node != null && node.text.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matching.Add(current.id);
                    }
                }
            }

            return matching.ToArray();
        }
        
        ConversationTreeNode CreateChildNode(ConversationNode node, bool isLink)
        {
            ConversationTreeNode treeNode = new ConversationTreeNode(nextId++, node);
            treeNode.isLink = isLink;


            if (!isLink)
            {
                for (int i = 0; i < node.NumChildren(); i++)
                {
                    bool isChildLink = node.IsLink(i);
                    ConversationTreeNode childNode = CreateChildNode(node.GetChild(i) as ConversationNode, isChildLink);
                    treeNode.AddChild(childNode);
                }
            }
            


            return treeNode;
        }
        protected override void ExpandedStateChanged()
        {
            base.ExpandedStateChanged();

            if (rootItem == null) return;

            
            expandedNodeGUIDs = GetExpanded().Select(x => (FindItem(x, rootItem) as ConversationTreeNode)).Where(x => x != null && x.node != null).Select(x => x.node.guid).ToList();
        }

        public override void OnGUI(Rect rect)
        {
            if (Styles.lineBoldStyle == null)
                InitializeStyles();


            altDown = (Event.current.modifiers & EventModifiers.Alt) != 0;
            
            base.OnGUI(rect);

        }

        protected override TreeViewItem BuildRoot()
        {
            nextId = 0;

            if (dragAndDropParent != null)
                nextId = nodeIdOffset;

            // this just serves as a docking root, it doesn't contain any data
            ConversationTreeNode rootNode = new ConversationTreeNode(nextId++);

            // and this is the actual conversation root node
            ConversationTreeNode visibleRootNode = new ConversationTreeNode(nextId++, editingConversation.root);
            rootNode.AddChild(visibleRootNode);

            for (int i = 0; i < editingConversation.root.NumChildren(); i++)
            {
                ConversationNode convNode = editingConversation.root.GetChild(i) as ConversationNode;
                bool isLink = editingConversation.root.IsLink(i);

                ConversationTreeNode childNode = CreateChildNode(convNode, isLink);
                visibleRootNode.AddChild(childNode);
            }

            SetupDepthsFromParentsAndChildren(rootNode);
            
            if (expandedNodeGUIDs != null)
            {
                var previouslyExpanded = expandedNodeGUIDs.Select(x => FindItem(x, rootNode, false)).Where(x => x != null).Select(x => x.id);
                SetExpanded(previouslyExpanded.ToList());
            }
            SetExpanded(visibleRootNode.id, true);

//             if (selectedChildren != null)
//             {   
//                 var previouslySelected = selectedChildren.Select(x => FindItem(x, rootNode)).Where(x => x != null).Select(x => x.id);
//                 SetSelection(previouslySelected.ToList());
//             }

            return rootNode;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            bool isRoot = false;
            ConversationTreeNode node = item as ConversationTreeNode;
            isRoot = node.node == null || node.node is RootNode;
            return base.CanMultiSelect(item) && !isRoot;
        }

        protected override void DoubleClickedItem(int id)
        {
            TreeViewItem tvi = FindItem(id, rootItem);
            ConversationTreeNode node = tvi as ConversationTreeNode;

            if (node.isLink)
            {
                ConversationTreeNode ownerNode = FindItem(node.node);
                if (ownerNode != null)
                {
                    SetSelection(new int[] { ownerNode.id }, TreeViewSelectionOptions.RevealAndFrame);
                }
            }

            base.DoubleClickedItem(id);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (Event.current.rawType != EventType.Repaint) return;

            Rect rowRect = args.rowRect;
            ConversationTreeNode treeNode = args.item as ConversationTreeNode;
            ConversationNode node = treeNode.node as ConversationNode;

            rowRect.xMin += extraSpaceBeforeIconAndLabel + GetContentIndent(treeNode);

            GUIStyle lineStyle = node == null ? Styles.lineBoldStyle : (node.nodeStyle == ConversationNode.NodeStyle.Monologue ? Styles.lineItalicStyle : Styles.lineStyle);

            Rect iconRect = rowRect;
            iconRect.width = iconWidth;
            iconRect.x += iconLeftPadding;

            string displayText = treeNode.displayName;
            if (node && !node.repeatable) displayText = "*" + displayText;
            bool isFindMatch = false;

            if (!string.IsNullOrEmpty(textSearchString))
            {
                int findIndex = 0;
                while (true)
                {
                    findIndex = displayText.IndexOf(textSearchString, findIndex, System.StringComparison.OrdinalIgnoreCase);
                    if (findIndex >= 0)
                    {
                        displayText = displayText.Insert(findIndex, "<b><size=13>");
                        displayText = displayText.Insert(findIndex + 12 + textSearchString.Length, "</size></b>");
                        findIndex += 19 + textSearchString.Length;
                        isFindMatch = true;
                    }
                    else break;
                }
            }


            Texture icon = treeNode.icon;
            if (icon != null)
            {
                
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                rowRect.xMin += iconWidth + iconTotalPadding + spaceBetweenIconAndText;
            }

            if (treeNode.isLink)
            {
                iconRect.xMin += iconWidth + iconTotalPadding;
                iconRect.xMax += iconWidth + iconTotalPadding;
                GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("d_Linked").image, ScaleMode.ScaleToFit);
                rowRect.xMin += iconWidth + iconTotalPadding + (icon == null ? spaceBetweenIconAndText : 0f);
            }

            if (isFindMatch)
            {
                iconRect.xMin += iconWidth + iconTotalPadding;
                iconRect.xMax += iconWidth + iconTotalPadding;
                GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("d_Search Icon").image, ScaleMode.ScaleToFit);
                rowRect.xMin += iconWidth + iconTotalPadding + (icon == null ? spaceBetweenIconAndText : 0f);
            }

            Color textColor = dialogColor;

            if (treeNode.isLink)
            {
                textColor = linkColor;
            } else if (node != null)
            {
                if (node.isOption)
                {
                    textColor = optionColor;
                } else
                {
                    switch (node.nodeStyle)
                    {
                        case ConversationNode.NodeStyle.Dialog: textColor = dialogColor; break;
                        case ConversationNode.NodeStyle.Monologue: textColor = dialogColor; break;
                        case ConversationNode.NodeStyle.Narration: textColor = narrationColor; break;
                    }
                }
            }

            lineStyle.normal.textColor = lineStyle.focused.textColor = textColor;
            lineStyle.richText = true;

            if (showDebugNodeIDs)
            {
                rowRect.xMax -= 350f;
                lineStyle.Draw(rowRect, displayText, false, false, args.selected && dragAndDropNodes == null, args.focused && dragAndDropNodes == null);

                rowRect.xMin = rowRect.xMax;
                rowRect.xMax += 350f;

                lineStyle.Draw(rowRect, treeNode.node.guid.ToStringHex(), false, false, args.selected && dragAndDropNodes == null, args.focused && dragAndDropNodes == null);
            }
            else if (node)
            {
                rowRect.xMax -= 400f;
                lineStyle.Draw(rowRect, displayText, false, false, args.selected && dragAndDropNodes == null, args.focused && dragAndDropNodes == null);

                rowRect.xMin = rowRect.xMax;
                rowRect.xMax += 200f;

                lineStyle.Draw(rowRect, string.Join(",", node.setFlags), false, false, args.selected && dragAndDropNodes == null, args.focused && dragAndDropNodes == null);

                rowRect.xMin = rowRect.xMax;
                rowRect.xMax += 200f;

                lineStyle.Draw(rowRect, node.flagCondition, false, false, args.selected && dragAndDropNodes == null, args.focused && dragAndDropNodes == null);
            }
            else
            {
                lineStyle.Draw(rowRect, displayText, false, false, args.selected && dragAndDropNodes == null, args.focused && dragAndDropNodes == null);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            ConversationTreeNode treeNode = (ConversationTreeNode)FindItem(id, rootItem);
            
            menu.Show(treeNode);
        }

        public ConversationTreeNode AddChildNode(ConversationTreeNode parent, ConversationTreeNode addBefore = null)
        {
            ConversationNode childNode = ConversationNode.Create(editingConversation);

            if (parent.node && !(parent.node is RootNode))
            {
                ConversationNode parentNode = parent.node as ConversationNode;

                childNode.overrideSpeakingCharacter = parentNode.overrideSpeakingCharacter;
                childNode.speakingCharacter = parentNode.speakingCharacter;
                childNode.nodeStyle = parentNode.nodeStyle;
            }

            ConversationTreeNode treeNode = CreateChildNode(childNode, false);

            if (addBefore != null && addBefore.parent == parent)
            {
                parent.node.AddChildBefore(childNode, addBefore.node);
                int childIndex = parent.children.IndexOf(addBefore);
                parent.children.Insert(childIndex, treeNode);
                treeNode.parent = parent;
            } else
            {
                parent.node.AddChild(childNode);
                parent.AddChild(treeNode);
            }

            childNode.text = "---";
            SetupDepthsFromParentsAndChildren(parent);

            SetExpanded(parent.id, false);
            SetExpanded(parent.id, true);
            SetSelection(new int[] { treeNode.id });

            return treeNode;
        }

        public List<ConversationTreeNode> FindNodesUsedAsLink(ConversationTreeNode n)
        {
            List<ConversationTreeNode> list = new List<ConversationTreeNode>();

            if (n.isLink) return list;

            Stack<ConversationTreeNode> nodesToVisit = new Stack<ConversationTreeNode>();
            nodesToVisit.Push(n);

            while (nodesToVisit.Count > 0)
            {
                ConversationTreeNode current = nodesToVisit.Pop();

                if (!current.isLink)
                {
                    if (current.hasChildren)
                    {
                        foreach (var c in current.children)
                            nodesToVisit.Push(c as ConversationTreeNode);
                    }

                    ConversationNode node = current.node as ConversationNode;
                    if (node != null && node.IsUsedAsLink)
                    {
                        list.Add(current);
                    }
                }
            }

            return list;
        }

        public bool ContainsAnyNode(IEnumerable<ConversationTreeNode> nodes, IEnumerable<ConversationTreeNode> checkNodes, bool recurse, bool includingLinks = true)
        {
            var targets = checkNodes.Select(x => x.node as ConversationNode);

            foreach (var node in nodes)
            {
                var search = node.node as ConversationNode;
                
                if (checkNodes.Contains(node)) return true;    // it me
                if (includingLinks && targets.Contains(search)) return true;  // for links

                if (recurse && !node.isLink && node.hasChildren)
                {
                    if (ContainsAnyNode(node.children.Select(x => x as ConversationTreeNode), checkNodes, true, includingLinks))
                        return true;
                }
            }

            return false;
        }

        public bool ContainsNode(IEnumerable<ConversationTreeNode> nodes, ConversationTreeNode toNode, bool recurse, bool includingLinks = true)
        {
            var target = toNode.node as ConversationNode;

            foreach (var node in nodes)
            {
                var search = node.node as ConversationNode;
                
                if (node == toNode) return true;    // it me
                if (includingLinks && search == target) return true;  // for links

                if (recurse && !node.isLink && node.hasChildren)
                {
                    if (ContainsNode(node.children.Select(x => x as ConversationTreeNode), toNode, true, includingLinks))
                        return true;
                }
            }

            return false;
        }

        public void RemoveNode(ConversationTreeNode n, bool reparentChildren, bool bypassConfirmation = false, bool needValidationOnClipboard = true)
        {
            List<ConversationTreeNode> linkNodes = FindNodesUsedAsLink(n);
            int nextSelectNodeId = n.id;

            if (linkNodes.Count == 0 || 
                bypassConfirmation ||
                EditorUtility.DisplayDialog("Confirmation", "Removing this element will destroy all links associated with it. Continue?", "Yes", "No"))
            {
                ConversationTreeNode parent = n.parent as ConversationTreeNode;
                Node nodeParent = parent.node;

                foreach (var treeNode in linkNodes)
                {
                    ConversationNode node = treeNode.node as ConversationNode;

                    List<ConversationTreeNode> parents = node.LinkParents.Select(x => FindItem(x))
                                                                         .Where(x => x != null).ToList();

                    foreach (var linkParent in parents)
                    {
                        (linkParent.node as ConversationNode).RemoveLink(treeNode.node);
                        linkParent.RemoveChild(treeNode);
                    }
                }

                int previousTreeIndex = parent.children.IndexOf(n);

                if (n.hasChildren && reparentChildren)
                {
                    nextSelectNodeId = n.children[0].id;
                }
                else if (parent.children.Count == 1)
                {
                    nextSelectNodeId = parent.id;
                }
                else if (previousTreeIndex < (parent.children.Count - 1))
                {
                    nextSelectNodeId = parent.children[previousTreeIndex+1].id;
                }
                else
                {
                    nextSelectNodeId = parent.children[previousTreeIndex-1].id;
                }
                    

                parent.RemoveChild(n);
                

                if (n.isLink)
                {
                    (nodeParent as ConversationNode).RemoveLink(n.node as ConversationNode);
                } else
                {
                    // copy children to the parent unless they're a link
                    // can make sure they get added in the same position in the heirarchy as the deleted node was

                    int previousNodeIndex = nodeParent.IndexOfChild(n.node);
                    nodeParent.RemoveChild(n.node);

                    var clipboard = GetClipboard();
                    if (clipboard != null && needValidationOnClipboard && ContainsNode(clipboard, n, false))
                    {
                        clipboard = null;
                        SetClipboard(null);
                        Debug.Log("An item in the clipboard was deleted from the tree, the clipboard has been cleared.");
                    }

                    if (reparentChildren)
                    {
                        if (n.hasChildren)
                        {
                            foreach (var t in n.children)
                            {
                                if (nodeParent is RootNode && ((t as ConversationTreeNode).isLink)) continue;
                                parent.InsertChild(t as ConversationTreeNode, previousTreeIndex);
                            }


                            n.children.Clear();
                        }

                        for (int i = 0; i < n.node.NumChildren(); )
                        {
                            if (nodeParent is RootNode && n.node.IsLink(i))
                            {
                                (n.node as ConversationNode).RemoveLink(n.node.GetChild(i));
                                continue;
                            }

                            nodeParent.AddChildBefore(n.node.GetChild(i), previousNodeIndex);
                            i++;
                        }

                        n.node.ClearChildren();
                    } else
                    {
                        if (clipboard != null && needValidationOnClipboard && ContainsNode(clipboard, n, true))
                        {
                            clipboard = null;
                            SetClipboard(null);
                            Debug.Log("An item in the clipboard was deleted from the tree, the clipboard has been cleared.");
                        }
                    }


                }

                SetSelection(new int[] { nextSelectNodeId });
                SetupDepthsFromParentsAndChildren(parent);
            }
        }

        public ConversationTreeNode FindItem(Node node, ConversationTreeNode startingFrom = null, bool isLink = false)
        {
            if (startingFrom == null) startingFrom = rootItem as ConversationTreeNode;
            if (startingFrom.node == node && startingFrom.isLink == isLink) return startingFrom;

            if (startingFrom.hasChildren)
            {
                foreach (var c in startingFrom.children)
                {
                    var f = FindItem(node, c as ConversationTreeNode, isLink);
                    if (f != null) return f;
                }
            }

            return null;
        }

        public ConversationTreeNode FindItem(SerializableGUID guid, ConversationTreeNode startingFrom = null, bool isLink = false)
        {
            if (startingFrom == null) startingFrom = rootItem as ConversationTreeNode;
            if (startingFrom.node && startingFrom.node.guid == guid && startingFrom.isLink == isLink) return startingFrom;

            if (startingFrom.hasChildren)
            {
                foreach (var c in startingFrom.children)
                {
                    var f = FindItem(guid, c as ConversationTreeNode, isLink);
                    if (f != null) return f;
                }
            }

            return null;
        }

        public ConversationTreeNode AddLinkNode(ConversationTreeNode parent, ConversationTreeNode target)
        {
            ConversationTreeNode treeNode = CreateChildNode(target.node as ConversationNode, true);

            ConversationNode parentNode = parent.node as ConversationNode;
            ConversationNode targetNode = target.node as ConversationNode;

            parent.AddChild(treeNode);
            parentNode.AddLink(targetNode);

            SetupDepthsFromParentsAndChildren(parent);

            SetExpanded(parent.id, false);
            SetExpanded(parent.id, true);
            SetSelection(new int[] { treeNode.id });

            return treeNode;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            var items = FindRows(args.draggedItemIDs);
            foreach (var item in items)
            {
                if ((item as ConversationTreeNode).node is RootNode) return false;
            }
            

            return altDown || items.All(x => x.depth == items[0].depth && x.parent == items[0].parent);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {   
            DragAndDrop.PrepareStartDrag();

            dragAndDropNodes = FindRows(args.draggedItemIDs).Select(x => x as ConversationTreeNode).OrderByDescending(x => x.IndexInParent()).ToArray();
            dragAndDropInsertBefore = dragAndDropNodes.Select(x => x.GetSiblingAfter()).ToArray();
            dragAndDropParent = dragAndDropNodes[0].parent as ConversationTreeNode;

            foreach (var node in dragAndDropNodes)
            {
                // hold shift = move single
                RemoveNode(node, altDown, true, false);

            }

            

            DragAndDrop.objectReferences = new Object[0];

            DragAndDrop.StartDrag(dragAndDropNodes.Length == 1 ? dragAndDropNodes[0].displayName : "< Multiple >");

            Reload();

            SetSelection(new int[0]);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            bool isValid = true;
        
            ConversationTreeNode parentItem = args.parentItem as ConversationTreeNode;

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                {
                    if (parentItem.isLink) { isValid = false; break; }
                    if (dragAndDropNodes.Any(x => x.isLink) && parentItem.node is RootNode) { isValid = false; break; }
                    //if (parentItem == dragAndDropNodes[0].parent) { isValid = false; break; }
                    
                    //if (dragAndDropNodes.Contains(parentItem)) { isValid = false; break; }
                    //if (ContainsAnyNode(new ConversationTreeNode[] { parentItem }, dragAndDropNodes, true, false)) { isValid = false; break; }

                    break;
                }

                case DragAndDropPosition.OutsideItems:
                {
                    isValid = false; 
                    break;
                }

                case DragAndDropPosition.BetweenItems:
                {
                    if (parentItem.node == null) { isValid  = false; break; }
                    if (dragAndDropNodes.Any(x => x.isLink) && parentItem.node is RootNode) { isValid = false; break; }

                    break;
                }
                
            }

            if (args.performDrop)
            {
                if (isValid)
                {
                    // drop in order at end of node
                    if (args.dragAndDropPosition == DragAndDropPosition.UponItem)
                    {
                        // nodes are stored in descending index order
                        for (int i = dragAndDropNodes.Length - 1; i >= 0; i--)
                        {
                            parentItem.AddChild(dragAndDropNodes[i]);

                            if (dragAndDropNodes[i].isLink)
                                (parentItem.node as ConversationNode).AddLink(dragAndDropNodes[i].node);
                            else
                                parentItem.node.AddChild(dragAndDropNodes[i].node);
                        }
                    // drop in order at specified index
                    } else
                    {
                        for (int i = 0; i < dragAndDropNodes.Length; i++)
                        {
                            parentItem.InsertChild(dragAndDropNodes[i], args.insertAtIndex);

                            if (dragAndDropNodes[i].isLink)
                                (parentItem.node as ConversationNode).AddLinkAt(dragAndDropNodes[i].node, args.insertAtIndex);
                            else
                                parentItem.node.AddChildAt(dragAndDropNodes[i].node, args.insertAtIndex);
                        }
                    }                 

                    EditorUtility.SetDirty(editingConversation);
                } else
                {
                    for (int i = 0; i < dragAndDropNodes.Length; i++)
                    {
                        if (dragAndDropInsertBefore[i] == null)
                        {
                            dragAndDropParent.AddChild(dragAndDropNodes[i]);

                            if (dragAndDropNodes[i].isLink)
                                (dragAndDropParent.node as ConversationNode).AddLink(dragAndDropNodes[i].node);
                            else
                                dragAndDropParent.node.AddChild(dragAndDropNodes[i].node);
                        }
                        else
                        {
                            int idx = dragAndDropInsertBefore[i].IndexInParent();
                            dragAndDropParent.InsertChild(dragAndDropNodes[i], idx);

                            if (dragAndDropNodes[i].isLink)
                                (dragAndDropParent.node as ConversationNode).AddLinkAt(dragAndDropNodes[i].node, idx);
                            else
                                dragAndDropParent.node.AddChildBefore(dragAndDropNodes[i].node, dragAndDropInsertBefore[i].node);
                        }
                    }
                }

                dragAndDropParent = null;
                dragAndDropInsertBefore = null;

                Reload();

                SetSelection(dragAndDropNodes.Select(x => FindItem(x.node, null, x.isLink).id).ToList(), TreeViewSelectionOptions.RevealAndFrame);

                dragAndDropNodes = null;
            }

            return isValid ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Rejected;
        }
    }
}
