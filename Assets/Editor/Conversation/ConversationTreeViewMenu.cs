using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Boomzap.Conversation
{
    internal static class ListTool
    {
        public static void Swap<T>(this IList<T> list, int a, int b)
        {
            T tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
        }
    }

    public class ConversationTreeViewMenu
    {
        ConversationTreeView owner;

        const int POPUP_ADD			= 1 << 0;
        const int POPUP_ADD_OPTION	= 1 << 1;
        const int POPUP_INSERT		= 1 << 2;
        const int POPUP_REMOVE		= 1 << 3;
        const int POPUP_COPY		= 1 << 4;
        const int POPUP_CUT			= 1 << 5;
        const int POPUP_PASTE		= 1 << 6;
        const int POPUP_PASTE_LINK	= 1 << 7;
        const int POPUP_MOVE_UP		= 1 << 8;
        const int POPUP_MOVE_DOWN	= 1 << 9;
        const int POPUP_TOGGLE_REPLAY= 1 << 10;
        const int POPUP_COPY_DATA	= 1 << 11;
        const int POPUP_FILL	    = 1 << 12;

        public ConversationTreeViewMenu(ConversationTreeView owner)
        {
            this.owner = owner;
        }

        void AddMenuItem(GenericMenu menu, string text, int bitfield, int bitmask, GenericMenu.MenuFunction2 func, object param)
        {
            menu.AddItem(new GUIContent(text), false, (bitmask != -1 && (bitfield & bitmask) == 0) ? null : func, param);
        }

        public void Show(ConversationTreeNode context)
        {
            GenericMenu menu = new GenericMenu();

            int popupElements = int.MaxValue;
            var selected = owner.selectedNodes;
		    
           // ConversationTreeViewNode clipboard = clipboardEntries.GetValueOrDefault(selectedConversation);
            List<ConversationTreeNode> clipboard = owner.GetClipboard();

            if (context == null)
            {
                // default to right click on root node?
                popupElements = 0;
            }
            else
            {
                if (selected.Length > 1)
                {
                    popupElements &= ~(POPUP_ADD | POPUP_ADD_OPTION | POPUP_PASTE | POPUP_PASTE_LINK | POPUP_INSERT | POPUP_COPY_DATA | POPUP_FILL | POPUP_MOVE_DOWN | POPUP_MOVE_UP);
                }

                if (context.node is RootNode)                               
                {
                    popupElements &= ~(POPUP_COPY | POPUP_CUT | POPUP_PASTE_LINK | POPUP_INSERT | POPUP_REMOVE | POPUP_FILL | POPUP_COPY_DATA | POPUP_MOVE_DOWN | POPUP_MOVE_UP);
                }

                if (clipboard != null)
                {
                    foreach (ConversationTreeNode clip in clipboard)
                    {
                        // no pasting a cut entry as a link
                        if (clip.parent == null) popupElements &= ~POPUP_PASTE_LINK;
                        // no copy-paste, only cut-paste.
                        if (clip.parent != null) popupElements &= ~POPUP_PASTE;
                    }
                }
                                            

                if (clipboard == null)      popupElements &= ~(POPUP_PASTE | POPUP_PASTE_LINK);
                if (context.isLink)         popupElements = POPUP_REMOVE | POPUP_COPY | POPUP_CUT | POPUP_MOVE_UP | POPUP_MOVE_DOWN;
                if (!context.CanMoveUp)     popupElements &= ~POPUP_MOVE_UP;
                if (!context.CanMoveDown)   popupElements &= ~POPUP_MOVE_DOWN;

            }	

            AddMenuItem(menu, "Add",            popupElements, POPUP_ADD,           (object o) => AddNode(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Add Option",     popupElements, POPUP_ADD_OPTION,    (object o) => AddOptionNode(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Insert",         popupElements, POPUP_INSERT,        (object o) => InsertNode(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Remove",         popupElements, POPUP_REMOVE,        (object o) => RemoveSelectedNodes(), context);
            menu.AddSeparator("");

            AddMenuItem(menu, "From Clipboard", popupElements, POPUP_ADD,               (object o) => AddFromClipboard(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Overwrite Text From Clipboard", popupElements, POPUP_REMOVE, (object o) => OverwriteTextFromClipboard(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Option from Clipboard", popupElements, POPUP_ADD_OPTION, (object o) => AddOptionFromClipboard(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Characters from Clipboard", popupElements, POPUP_FILL,   (object o) => OverrideCharactersFromClipboard(o as ConversationTreeNode), context);
            menu.AddSeparator("");

            AddMenuItem(menu, "Cut",            popupElements, POPUP_CUT,           (object o) => CopySelected(true), context);
            AddMenuItem(menu, "Paste",          popupElements, POPUP_PASTE,         (object o) => PasteClipboard(o as ConversationTreeNode, false), context);

            AddMenuItem(menu, "Copy Link",      popupElements, POPUP_COPY,          (object o) => CopySelected(false), context);
            AddMenuItem(menu, "Paste Link",     popupElements, POPUP_PASTE_LINK,    (object o) => PasteClipboard(o as ConversationTreeNode, true), context);
            
            AddMenuItem(menu, "Copy Puppeteering", popupElements, POPUP_COPY_DATA,  (object o) => CopyPuppeteering(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Paste Puppeteering", popupElements, POPUP_COPY_DATA, (object o) => PastePuppeteering(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Fill Puppeteering", popupElements, POPUP_COPY_DATA,  (object o) => FillPuppeteering(o as ConversationTreeNode), context);

            menu.AddSeparator("");

            AddMenuItem(menu, "Move Up",        popupElements, POPUP_MOVE_UP,       (object o) => MoveUp(o as ConversationTreeNode), context);
            AddMenuItem(menu, "Move Down",      popupElements, POPUP_MOVE_DOWN,     (object o) => MoveDown(o as ConversationTreeNode), context);

//             menu.AddItem(new GUIContent("Toggle Replay"), false, (popupElements & POPUP_TOGGLE_REPLAY) != 0 ? (GenericMenu.MenuFunction2) OnToggleReplay : null, active);
//             menu.AddItem(new GUIContent("Copy Emotions"),	  false, (popupElements & POPUP_COPY_DATA) != 0 ? (GenericMenu.MenuFunction2) OnCopyEmotions : null, active);
//             menu.AddItem(new GUIContent("Paste Emotions"),	  false, (popupElements & POPUP_COPY_DATA) != 0 ? (GenericMenu.MenuFunction2) OnPasteEmotions : null, active);
//             menu.AddItem(new GUIContent("Fill Emotions"),	  false, (popupElements & POPUP_COPY_DATA) != 0 ? (GenericMenu.MenuFunction2) OnFillEmotions : null, active);
//             menu.AddItem(new GUIContent("Fill Data"),	  false, (popupElements & POPUP_COPY_DATA) != 0 ? (GenericMenu.MenuFunction2) OnFillData : null, active);
// 		
// 	
//             menu.AddSeparator("");
// 
//             menu.AddItem(new GUIContent("Move Up"),   false, (popupElements & POPUP_MOVE_UP)   != 0 ? (GenericMenu.MenuFunction2) OnMoveUp   : null, active);
//             menu.AddItem(new GUIContent("Move Down"), false, (popupElements & POPUP_MOVE_DOWN) != 0 ? (GenericMenu.MenuFunction2) OnMoveDown : null, active);

                menu.ShowAsContext();            
        }

        void CopyPuppeteering(ConversationTreeNode contextNode)
        {

        }

        void PastePuppeteering(ConversationTreeNode contextNode)
        {
            
        }

        void FillPuppeteering(ConversationTreeNode contextNode)
        {
            
        }

        void MoveUp(ConversationTreeNode contextNode)
        {
            int curIndex = contextNode.parent.children.IndexOf(contextNode);
            contextNode.parent.children.Swap(curIndex, curIndex-1);

            curIndex = contextNode.node.parent.IndexOfChild(contextNode.node);
            contextNode.node.parent.SwapChildren(curIndex, curIndex-1);

            EditorUtility.SetDirty(owner.conversation);

            owner.Reload();            
        }

        void MoveDown(ConversationTreeNode contextNode)
        {
            int curIndex = contextNode.parent.children.IndexOf(contextNode);
            contextNode.parent.children.Swap(curIndex, curIndex+1);

            curIndex = contextNode.node.parent.IndexOfChild(contextNode.node);
            contextNode.node.parent.SwapChildren(curIndex, curIndex+1);

            EditorUtility.SetDirty(owner.conversation);

            owner.Reload();
        }

        ConversationTreeNode AddNode(ConversationTreeNode contextNode)
        {
            var newNode = owner.AddChildNode(contextNode);
            EditorUtility.SetDirty(owner.conversation);

            // this is hacky and i know it is hacky. remove from project if not using i2loc.
            //I2.Loc.LocalizationEditor.ParseTerms(true, false, false);

            return newNode;
        }

        ConversationTreeNode AddOptionNode(ConversationTreeNode contextNode)
        {
            ConversationTreeNode newNode = owner.AddChildNode(contextNode);

            newNode.node.isOption = true;
            newNode.RefreshIcon();      
            
            EditorUtility.SetDirty(owner.conversation);

            // this is hacky and i know it is hacky. remove from project if not using i2loc.
            //I2.Loc.LocalizationEditor.ParseTerms(true, false, false);
            return newNode;
        }

        ConversationTreeNode InsertNode(ConversationTreeNode contextNode)
        {
            ConversationTreeNode prevNode = contextNode as ConversationTreeNode;

            var newNode = owner.AddChildNode(prevNode.parent as ConversationTreeNode, prevNode);
            EditorUtility.SetDirty(owner.conversation);

            // this is hacky and i know it is hacky. remove from project if not using i2loc.
           // I2.Loc.LocalizationEditor.ParseTerms(true, false, false);
            return newNode;
        }

        bool DoesAscendantExistInList(TreeViewItem node, IEnumerable<ConversationTreeNode> inList)
        {
            if (node.parent == null) return false;
            if (inList.Contains(node.parent)) return true;
            return DoesAscendantExistInList(node.parent, inList);
        }

        void CopySelected(bool cut)
        {
            var selectedNodes = owner.selectedNodes;
            var toCopy = selectedNodes.Where(x => !DoesAscendantExistInList(x, selectedNodes));
            owner.SetClipboard(toCopy);

            if (cut)
            {
                foreach (var s in toCopy)
                {
                    List<ConversationTreeNode> linkNodes = owner.FindNodesUsedAsLink(s);
                    if (linkNodes != null && linkNodes.Count > 0)
                    {
                        if (EditorUtility.DisplayDialog("Confirmation", "Removing this element will destroy all links associated with it. Continue?", "Yes", "No") == false)
                        {
                            return;
                        } else
                        {
                            break;
                        }
                    }
                }



                foreach (var s in toCopy)
                    owner.RemoveNode(s, false, true, false);

                EditorUtility.SetDirty(owner.conversation);
                owner.Reload();
            }
        }

        void PasteClipboard(ConversationTreeNode contextNode, bool asLink)
        {
            bool hadHardLinks = false;
            var clipboard = owner.GetClipboard();
            if (clipboard == null) return;

            // all methods to copy to clipboard assure that only the 'most adult' nodes of the selection have been copied
            foreach (var srcNode in clipboard)
            {
                if (srcNode.node is RootNode) throw new System.Exception("Root node must not be copied.");

                ConversationNode preExistingNode = owner.conversation.FindNode(srcNode.node.guid);
                if (!srcNode.isLink && !asLink && preExistingNode != null)
                {
                    EditorUtility.DisplayDialog("Error", "There is currently no benefit to copy-and-pasting wholesale conversation rows.\nIf you want to duplicate a tree, paste it as a link.\nIf you really think you have a valid use for copy-and-paste, tell JD.", "OK");
                    return;
                }

                if (srcNode.isLink || asLink)
                {
                    owner.AddLinkNode(contextNode, srcNode);
                    continue;
                }

                hadHardLinks = true;
                // we don't deep copy here because of the above restriction.
                // deep copying causes huge amounts of duplication in the assetdatabase because the old copies (for cuts) are never cleaned up in the previous system,
                // plus with our loc integration it would cause needless duplication.
                // furthermore no deepcopy is needed when all the data is intact in our 'clipboard' still, and it is unique.
                
                contextNode.AddChild(srcNode);
                contextNode.node.AddChild(srcNode.node);
            }

            if (hadHardLinks)
                owner.SetClipboard(null);

            EditorUtility.SetDirty(owner.conversation);
            owner.Reload();
        }

        void OverrideCharactersFromClipboard(ConversationTreeNode contextNode)
        {
            List<ClipboardParser.ClipboardData> lines = ClipboardParser.FromClipboard();

            foreach (var t in lines)		
		    {
			    ConversationNode node = contextNode.node as ConversationNode;

			    //node.Emotion = t.emotion;
			    node.speakingCharacter = Character.CharacterManager.instance.GetCharacterByName(t.narrator);
			    if (node.speakingCharacter == null)
			    {
				    node.speakingCharacterName = "";
				    node.overrideSpeakingCharacter = t.narrator;
			    } else
			    {
				    node.speakingCharacterName = t.narrator;
                    node.overrideSpeakingCharacter = string.Empty;
                }

                if (!contextNode.hasChildren) break;
                contextNode = contextNode.children[0] as ConversationTreeNode;
		    }

            EditorUtility.SetDirty(owner.conversation);
        }

        void OverwriteTextFromClipboard(ConversationTreeNode contextNode)
        {
            List<ClipboardParser.ClipboardData> lines = ClipboardParser.FromClipboard();

            ConversationTreeNode fromNode = contextNode;

            foreach (var t in lines)
            {
                ConversationNode node = fromNode.node as ConversationNode;
                if (node == null) break;

                node.text = t.text;

                if (fromNode.children == null || fromNode.children.Count == 0) break;

                var firstChild = fromNode.children.FirstOrDefault();
                if (firstChild == null) break;

                fromNode = firstChild as ConversationTreeNode;
                if (fromNode == null) break;
            }

            EditorUtility.SetDirty(owner.conversation);
        }

        void AddFromClipboard(ConversationTreeNode contextNode)
        {
            List<ClipboardParser.ClipboardData> lines = ClipboardParser.FromClipboard();

            foreach (var t in lines)
            {
                ConversationTreeNode treeNode = AddNode(contextNode);
                ConversationNode node = treeNode.node as ConversationNode;

                node.text = t.text;
                if (!string.IsNullOrEmpty(t.emotion))
                {
                    ///
                }

                node.speakingCharacter = Character.CharacterManager.instance.GetCharacterByName(t.narrator);
                if (node.speakingCharacter == null)
                {
                    node.speakingCharacterName = "";
                    node.overrideSpeakingCharacter = t.narrator;
                } else
                {
                    node.speakingCharacterName = t.narrator;
                    node.overrideSpeakingCharacter = string.Empty;
                }

                contextNode = treeNode;
            }



            EditorUtility.SetDirty(owner.conversation);
            owner.Reload();
        }

        void AddOptionFromClipboard(ConversationTreeNode contextNode)
        {
            ConversationTreeNode optionNode = AddOptionNode(contextNode);
            AddFromClipboard(optionNode);
            ConversationNode node = optionNode.node as ConversationNode;
            if (node.NumChildren() > 0)
            {
                node.text = (node.GetChild(0) as ConversationNode).text;

                owner.RemoveNode(optionNode.children[0] as ConversationTreeNode, true);
            }

            EditorUtility.SetDirty(owner.conversation);
            owner.Reload();
        }


        void RemoveSelectedNodes()
        {
            ConversationTreeNode[] toDelete = owner.selectedNodes;

            // remove them in descending order of depth so no parents get deleted before children
            foreach (var n in toDelete.OrderByDescending(x => x.depth))
            {
                owner.RemoveNode(n, true);
            }

            EditorUtility.SetDirty(owner.conversation);
            owner.Reload();
        }


    }
}
