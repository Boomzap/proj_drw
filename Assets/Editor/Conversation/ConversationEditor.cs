using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;

using Boomzap.Character;

namespace Boomzap.Conversation
{
    public partial class ConversationEditor : EditorWindow
    {
        [OnOpenAsset(1)]
        public static bool OpenEditor(int instanceID, int line)
        {
            UnityEngine.Object o = EditorUtility.InstanceIDToObject(instanceID);
            if (o == null) return false;

            if (o is Conversation)
            {
                GetWindow<ConversationEditor>().Initialize(o as Conversation);
                return true;
            }
            return false;
        }

        [SerializeField] TreeViewState  treeViewState;
        ConversationTreeView treeView;
        ConversationTextArea textArea = new ConversationTextArea();
        public bool focusTextArea = false;
        Dictionary<Conversation, List<ConversationTreeNode>> clipboardData = new Dictionary<Conversation, List<ConversationTreeNode>>();

        Vector2 nodeTextAreaScrollPosition;
        Vector2 spineSlotScrollPosition;
        
        [SerializeField] string currentSearchString = "";

        [SerializeField] Conversation selectedConversation = null;

        ConversationTreeNode lastSelectedNode = null;

        Rect headerRect;

        enum NodeEditMode
        { 
            Characters = 0,
            Flags = 1,
            SpineSlots = 2
        };

        NodeEditMode editMode = NodeEditMode.Characters;

        public void SetClipboard(IEnumerable<ConversationTreeNode> entries)
        {
            clipboardData[selectedConversation] = entries == null ? null : new List<ConversationTreeNode>(entries);
        }

        void BuildNodeMap(Node fromNode, List<SerializableGUID> listedChildren)
        {
            listedChildren.Add(fromNode.guid);
            for (int i = 0; i < fromNode.NumChildren(); i++)
            {
                if (fromNode.IsLink(i)) continue;
                BuildNodeMap(fromNode.GetChild(i), listedChildren);
            }
        }

        void CleanupUnusedAssetConnections()
        {
            if (selectedConversation == null)
                return;

            string baseAssetPath = AssetDatabase.GetAssetPath(selectedConversation);
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(baseAssetPath);

            List<SerializableGUID> hardLinkedNodes = new List<SerializableGUID>();
            BuildNodeMap(selectedConversation.root, hardLinkedNodes);

            foreach (Object o in allAssets)
            {
                if (o is Node)
                {
                    Node n = o as Node;
                    if (!hardLinkedNodes.Contains(n.guid))
                    {
                        AssetDatabase.RemoveObjectFromAsset(n);
                    }
                }
            }
        }

        public List<ConversationTreeNode> GetClipboard()
        {
            List<ConversationTreeNode> retValue;

            if (clipboardData.TryGetValue(selectedConversation, out retValue))
            {
                return retValue;
            }

            return null;
        }

        private void OnGUI()
        {
            if (treeView == null) return;
            if (selectedConversation == null) return;

            using (new EditorGUILayout.VerticalScope())
            {
                DrawHeader();

                if (Event.current.type == EventType.Repaint)
                    headerRect = GUILayoutUtility.GetLastRect();

                Rect treeRect = new Rect(5f, headerRect.yMax + 5f, position.width - 10f, position.height - 365f);
                treeView.OnGUI(treeRect);

                using (new GUILayout.AreaScope(new Rect(5f, treeRect.yMax + 5f, position.width - 10f, position.height - treeRect.yMax - 10f)))
                {
                    DrawFooter();
                }
            }
        }

        void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
            {
                GUILayout.Space(1);

                ConversationTreeNode[] selectedNodes = treeView.selectedNodes;
                ConversationTreeNode firstNode = selectedNodes.Length == 0 ? null : selectedNodes[0];

                ConversationNode node = firstNode == null ? null : firstNode.node as ConversationNode;

                using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 8, 8) }, GUILayout.ExpandHeight(true), GUILayout.MaxWidth(500), GUILayout.MinWidth(300)))
                {
                    GUI.SetNextControlName("Boomzap.Conversation.TextArea");
                    using (new EditorGUI.DisabledScope(selectedNodes.Length != 1 || node == null || firstNode.isLink))
                    {
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            string changedText = textArea.OnGUILayout(node == null ? "Invalid selection for modifying" : node.text, ref nodeTextAreaScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                            if (check.changed && node != null)
                            {
                                node.text = changedText;
                                EditorUtility.SetDirty(selectedConversation);
                            }

                            if (focusTextArea && Event.current.type == EventType.Repaint)
                            {
                                EditorGUI.FocusTextInControl("Boomzap.Conversation.TextArea");
                                focusTextArea = false;
                                Event.current.Use();
                            }
                        }

                        GUILayout.Space(4);
                        if (GUILayout.Button("GrammarBot"))
                        {
                            //
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 8, 8) }, GUILayout.ExpandHeight(true), GUILayout.ExpandHeight(true)))
                {
                    if (firstNode == null || selectedNodes.Any(x => x.node is RootNode))
                    {
                        DrawRootNodeSettings();
                    } else if (node != null)
                    {
                        using (new EditorGUI.DisabledScope(selectedNodes.Any(x => x.isLink) || selectedNodes.Length > 1))
                        {
                            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
                            {
                                editMode = (NodeEditMode)GUILayout.Toolbar((int)editMode, new string[] { "Characters", "Flags", "Spine Slots" }, GUILayout.ExpandWidth(true));

                                switch (editMode)
                                {
                                    case NodeEditMode.Characters:
                                        DrawEmotions(selectedNodes[0]);
                                        break;

                                    case NodeEditMode.Flags:
                                        DrawFlags(selectedNodes[0]);
                                        break;

                                    case NodeEditMode.SpineSlots:
                                        spineSlotScrollPosition = EditorGUILayout.BeginScrollView(spineSlotScrollPosition);
                                        DrawSpineSlots(selectedNodes[0]);
                                        EditorGUILayout.EndScrollView();
                                        break;
                                }
                            }
                        }
                    }
                }
            
                GUILayout.Space(1);
            }
        }

        void DrawRootNodeSettings()
        {
            treeView.dialogColor =      EditorGUILayout.ColorField("Dialog Color", treeView.dialogColor);
            treeView.narrationColor =   EditorGUILayout.ColorField("Narration Color", treeView.narrationColor);
            treeView.optionColor =      EditorGUILayout.ColorField("Option Color", treeView.optionColor);
            treeView.linkColor =        EditorGUILayout.ColorField("Link Color", treeView.linkColor);
            treeView.showDebugNodeIDs = EditorGUILayout.Toggle("Show Node IDs", treeView.showDebugNodeIDs);

            EditorGUI.BeginChangeCheck();
            selectedConversation.repeatable = EditorGUILayout.Toggle("Repeatable", selectedConversation.repeatable);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(selectedConversation);
        }

        void DrawFlags(ConversationTreeNode forNode)
        {
            ConversationNode node = forNode.node as ConversationNode;
            SerializedObject obj = new SerializedObject(node);
            
            EditorGUILayout.PropertyField(obj.FindProperty("setFlags"));


            EditorGUILayout.DelayedTextField(obj.FindProperty("flagCondition"));
            obj.ApplyModifiedProperties();

        }

        void DrawSpineSlots(ConversationTreeNode forNode)
        {
            EditorGUI.BeginChangeCheck();

            ConversationNode node = forNode.node as ConversationNode;

            EditorGUILayout.Space(5f);

            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                using (new EditorGUI.DisabledScope(node == null))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Copy from Parent"))
                        {
                            ConversationTreeNode parentNode = forNode.parent as ConversationTreeNode;
                            ConversationNode b = parentNode.node as ConversationNode;

                            if (b)
                            {
                                for (int i = 0; i < ConversationManager.MaxCharacterSlots; i++)
                                {
                                    CloneCharacterSettings(b.characters[i], node.characters[i]);
                                }
                            }
                        }

                        if (GUILayout.Button("Visualizer"))
                        {
                            ShowVisualizer();
                            UpdateVisualizer(node);
                        }
                    }
                }

                EditorGUILayout.Space(5f);

                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    for (int i = 0; i < ConversationManager.MaxCharacterSlots; i++)
                    {
                        DrawCharacterSpineSlots(node.characters[i], (forNode.parent as ConversationTreeNode).node as ConversationNode, i, forNode);
                    }
                }
            }

            bool forceRefresh = lastSelectedNode != forNode;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selectedConversation);

                forceRefresh = true;
            }

            if (forceRefresh && node != null)
            {
                lastSelectedNode = forNode;
                UpdateVisualizer(forNode.node as ConversationNode);
            }

        }

        void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                GUIStyle style = new GUIStyle("Button") { alignment = TextAnchor.MiddleCenter };

                if (GUILayout.Button(selectedConversation.name, style))
                {
                    Selection.activeObject = selectedConversation;
                    EditorGUIUtility.PingObject(selectedConversation);
                }

                EditorGUILayout.Space(10f);

                EditorGUIUtility.labelWidth = 100f;

                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    currentSearchString = EditorGUILayout.TextField(new GUIContent("Search: "), currentSearchString);

                    if (changeCheck.changed)
                        UpdateSearch();
                }

            }

        }

        void UpdateSearch()
        {
            treeView.SetSearchString(currentSearchString);
        }

        void DrawEmotions(ConversationTreeNode forNode)
        {
            EditorGUI.BeginChangeCheck();

            ConversationNode node = forNode.node as ConversationNode;

            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                using (new EditorGUI.DisabledScope(node == null))
                {
                    
                    if (node)
                    {
                        EditorGUI.BeginChangeCheck();
                        node.repeatable = EditorGUILayout.Toggle("Repeatable", node.repeatable);
                        if (EditorGUI.EndChangeCheck())
                            EditorUtility.SetDirty(selectedConversation);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Copy from Parent"))
                        {
                            ConversationTreeNode parentNode = forNode.parent as ConversationTreeNode;
                            ConversationNode b = parentNode.node as ConversationNode;

                            if (b)
                            {
                                for (int i = 0; i < ConversationManager.MaxCharacterSlots; i++)
                                {
                                    CloneCharacterSettings(b.characters[i], node.characters[i]);
                                }
                                node.background = b.background;
                                node.transitionStyle = b.transitionStyle;
                            }
                        }

                        if (GUILayout.Button("Visualizer"))
                        {
                            ShowVisualizer();
                            UpdateVisualizer(node);
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUIUtility.labelWidth = 50f;

                        node.nodeStyle = (ConversationNode.NodeStyle)EditorGUILayout.EnumPopup(new GUIContent("Style:"), node.nodeStyle);
                        node.speakingCharacter = DrawCharacterSelect(node.speakingCharacter, "Speaker:");

                        node.overrideSpeakingCharacter = EditorGUILayout.DelayedTextField(new GUIContent("Override speaker name:"), node.overrideSpeakingCharacter);
                    }
                }

                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
                {
                    //Background Transition
                    EditorGUIUtility.labelWidth = 100f;

                    var roomTracker = ho.HORoomAssetManager.instance.roomTracker;
                     
                    string background = DrawStringSelect(node.background, roomTracker.GetRoomNames() , "Background:");
                    if (background != node.background)
                    {
                        node.background = background;
                    }

                    TransitionStyle style = (TransitionStyle)EditorGUILayout.EnumPopup(new GUIContent("Entry Transition:"), node.transitionStyle);
                    if (style != node.transitionStyle)
                    {
                        node.transitionStyle = style;
                    }
                }
                EditorGUIUtility.labelWidth = 50f;
                EditorGUILayout.Space(5f);

                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    for (int i = 0; i < ConversationManager.MaxCharacterSlots; i++)
                    {
                        DrawCharacterSetting(node.characters[i], (forNode.parent as ConversationTreeNode).node as ConversationNode, i, forNode);
                    }
                }
            }

            bool forceRefresh = lastSelectedNode != forNode;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selectedConversation);

                forceRefresh = true;
            }

            if (forceRefresh && node != null)
            {
                lastSelectedNode = forNode;
                UpdateVisualizer(forNode.node as ConversationNode);
            }

        }

        void MoveCharacterSlot(int whichIdx, int dir)
        {
            ConversationTreeNode[] selectedNodes = treeView.selectedNodes;
            ConversationTreeNode firstNode = selectedNodes.Length == 0 ? null : selectedNodes[0];

            ConversationNode node = firstNode == null ? null : firstNode.node as ConversationNode;

            if (node == null) return;

            node.characters[whichIdx].noChangeFromParent = false;
            node.characters[whichIdx + dir].noChangeFromParent = false;

            node.characters.Swap(whichIdx, whichIdx + dir);
        }

        void DrawSlotSwapHeader(int idx)
        {
            using (new EditorGUILayout.HorizontalScope(new GUIStyle("Helpbox"), GUILayout.ExpandWidth(true)))
            {
                using (new EditorGUI.DisabledScope(idx <= 0))
                {
                    if (GUILayout.Button("<"))
                        MoveCharacterSlot(idx, -1);
                }

                GUILayout.Label($"Slot #{idx+1}");

                using (new EditorGUI.DisabledScope(idx >= (ConversationManager.MaxCharacterSlots-1)))
                {
                    if (GUILayout.Button(">"))
                        MoveCharacterSlot(idx, +1);
                }
            }

            EditorGUILayout.Space(1);
        }

        void CloneCharacterSettings(ConversationNode.CharacterStateDefinition from, ConversationNode.CharacterStateDefinition to)
        {
            to.character = from.character;

            to.state = from.state;
            to.eyes = from.eyes;
            to.emotion = from.emotion;
            to.faceLeft = from.faceLeft;
            to.lookBack = from.lookBack;
            to.spineSlots = from.spineSlots;
        }

        void DrawCharacterSetting(ConversationNode.CharacterStateDefinition characterState, ConversationNode parentNode, int slotIndex, ConversationTreeNode selfNode)
        {
            if (characterState.noChangeFromParent && parentNode != null && !selfNode.isLink)
                characterState.CloneFrom(parentNode.characters[slotIndex]);

            if (characterState.noChangeFromParent && parentNode == null)
                characterState.noChangeFromParent = false;

            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                DrawSlotSwapHeader(slotIndex);

                Character.CharacterInfo characterInfo = null;

                using (new EditorGUI.DisabledScope(characterState.noChangeFromParent))
                {
                    characterInfo = DrawCharacterSelect(characterState.character);

                    if (characterInfo != characterState.character)  // changed
                    {
                        characterState.character = characterInfo;

                        if (characterInfo != null && parentNode != null)
                        {
                            // if we have this char in a previous scene, copy it's state
                            ConversationNode.CharacterStateDefinition prevNodeCharacter = parentNode.characters.FirstOrDefault(x => x.character == characterInfo);

                            if (prevNodeCharacter != null)
                            {
                                CloneCharacterSettings(prevNodeCharacter, characterState);
                            } else
                            {
                                characterState.state = characterInfo.EditorCharacter.defaultState;
                                characterState.eyes = characterInfo.EditorCharacter.defaultEyes;
                                characterState.emotion = characterInfo.EditorCharacter.defaultEmotion;
                                characterState.spineSlots = characterInfo.EditorCharacter.defaultSlots;
                            }
                        } else if (characterInfo != null)
                        {
                            characterState.state = characterInfo.EditorCharacter.defaultState;
                            characterState.eyes = characterInfo.EditorCharacter.defaultEyes;
                            characterState.emotion = characterInfo.EditorCharacter.defaultEmotion;
                            characterState.spineSlots = characterInfo.EditorCharacter.defaultSlots;
                        }
                    }
                }

                Character.Character character = characterInfo?.EditorCharacter ?? null;

                using (new EditorGUI.DisabledScope(characterInfo == null || character == null || characterState.noChangeFromParent))
                {
                    if (character == null)
                    {
                        DrawStringSelect("", new string[0]);
                        DrawStringSelect("", new string[0]);
                        DrawStringSelect("", new string[0]);
                        DrawStringSelect("", new string[0]);
                        EditorGUILayout.ToggleLeft("Look Back", false);
                        EditorGUILayout.ToggleLeft("Face Left", false);

                    } else
                    {
                        string[] data = string.IsNullOrEmpty(characterState.state) ? new string[0] : characterState.state.Split('.');
                        string skin = data.Length > 0 ? data[0] : character.GetSkins()[0];
                        string state = data.Length > 1 ? data[1] : character.GetStatesInSkin(skin)[0];
                        string emotion = characterState.emotion;
                        string eyes = characterState.eyes;

                        string newSkin = DrawStringSelect(skin, character.GetSkins().Select(x => x.Split('.')[0]).Distinct().ToArray());
                        if (newSkin != skin)
                        {
                            skin = newSkin;
                            state = character.GetStatesInSkin(skin)[0];
                        }
                        string newState = DrawStringSelect(state, character.GetStatesInSkin(skin));
                        if (newState != state)
                        {
                            state = newState;
                            //emotion = character.GetEmotion(state).allEmotions[0];   // do these need to be reset?
                            //eyes = character.GetEmotion(state).allEyes[0];
                        }
                        string newEmotion = DrawStringSelect(emotion, character.GetEmotion(skin + "." + state).allEmotions);
                        if (newEmotion != emotion)
                        {
                            emotion = newEmotion;
                            //eyes = character.GetEmotion(state).allEyes[0];
                        }
                        string newEyes = DrawStringSelect(eyes, character.GetEmotion(skin + "." + state).allEyes);
                        if (newEyes != eyes)
                        {
                            eyes = newEyes;
                        }

                        characterState.lookBack = EditorGUILayout.ToggleLeft("Look Back", characterState.lookBack);
                        characterState.faceLeft = EditorGUILayout.ToggleLeft("Face Left", characterState.faceLeft);
                        
                        characterState.state = skin + "." + state;
                        characterState.emotion = emotion;
                        characterState.eyes = eyes;
                    }
                }

                using (new EditorGUI.DisabledScope(parentNode == null))
                {
                    bool s = EditorGUILayout.ToggleLeft("No change", characterState.noChangeFromParent);

                    if (s != characterState.noChangeFromParent)
                    {
                        characterState.noChangeFromParent = s;
                    }
                }
            }
        }

        void DrawCharacterSpineSlots(ConversationNode.CharacterStateDefinition characterState, ConversationNode parentNode, int slotIndex, ConversationTreeNode selfNode)
        {
            if (characterState.noChangeFromParent && parentNode != null && !selfNode.isLink)
                characterState.CloneFrom(parentNode.characters[slotIndex]);

            if (characterState.noChangeFromParent && parentNode == null)
                characterState.noChangeFromParent = false;

            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                //DrawSlotSwapHeader(slotIndex);

                Character.CharacterInfo characterInfo = null;

                using (new EditorGUI.DisabledScope(characterState.noChangeFromParent))
                {
                    characterInfo = DrawCharacterSelect(characterState.character);

                    if (characterInfo != characterState.character)  // changed
                    {
                        characterState.character = characterInfo;

                        if (characterInfo != null && parentNode != null)
                        {
                            // if we have this char in a previous scene, copy it's state
                            ConversationNode.CharacterStateDefinition prevNodeCharacter = parentNode.characters.FirstOrDefault(x => x.character == characterInfo);

                            if (prevNodeCharacter != null)
                            {
                                CloneCharacterSettings(prevNodeCharacter, characterState);
                            }
                            else
                            {
                                characterState.state = characterInfo.EditorCharacter.defaultState;
                                characterState.eyes = characterInfo.EditorCharacter.defaultEyes;
                                characterState.emotion = characterInfo.EditorCharacter.defaultEmotion;
                                characterState.spineSlots = characterInfo.EditorCharacter.defaultSlots;
                            }
                        }
                        else if (characterInfo != null)
                        {
                            characterState.state = characterInfo.EditorCharacter.defaultState;
                            characterState.eyes = characterInfo.EditorCharacter.defaultEyes;
                            characterState.emotion = characterInfo.EditorCharacter.defaultEmotion;
                            characterState.spineSlots = characterInfo.EditorCharacter.defaultSlots;
                        }
                    }
                }

                Character.Character character = characterInfo?.EditorCharacter ?? null;

                using (new EditorGUI.DisabledScope(characterInfo == null || character == null || characterState.noChangeFromParent))
                {
                    if (character != null)
                    {
                        if (characterState == null)
                        {
                            Debug.Log("Character state is null");
                            return;
                        }
                        List<CharacterSpineSlot.SlotToggle> slotToggles = new List<CharacterSpineSlot.SlotToggle>();

                        if (characterState.spineSlots != null && characterState.spineSlots.Count > 0)
                        {
                            slotToggles = characterState.spineSlots;
                        }
                        else
                        {
                            slotToggles = character.GetSlotToggles();
                        }

                        if (slotToggles != null && slotToggles.Count > 0)
                        {
                            EditorGUILayout.Space(5f);
                            for(int i = 0; i < slotToggles.Count; i++)
                            {
                                slotToggles[i].Enabled = EditorGUILayout.ToggleLeft(slotToggles[i].Label, slotToggles[i].Enabled);
                            }
                        }
                        else
                            EditorGUILayout.LabelField("Slot Toggles not Found");

                        characterState.spineSlots = slotToggles;
                    }
                }

                using (new EditorGUI.DisabledScope(parentNode == null))
                {
                    bool s = EditorGUILayout.ToggleLeft("No change", characterState.noChangeFromParent);

                    if (s != characterState.noChangeFromParent)
                    {
                        characterState.noChangeFromParent = s;
                    }
                }
            }
        }

        string DrawStringSelect(string current, string[] options, string label = "")
        {
            int curIndex = 0;

            if (options.Contains(current))
                curIndex = System.Array.IndexOf(options, current);

            int selection = EditorGUILayout.Popup(label, curIndex, options);

            if (selection < 0)
                return string.Empty;

            return options.Length > 0 ? options[selection] : string.Empty;
        }

        Character.CharacterInfo DrawCharacterSelect(Character.CharacterInfo current, string label = "")
        {
            string[] selections = new string[] { "(none)" }.Concat(CharacterManager.instance.characters.Select(x => x.name)).ToArray();
            int curIndex = 0;
            
            if (CharacterManager.instance.characters.Contains(current))
                curIndex = System.Array.IndexOf(CharacterManager.instance.characters, current) + 1;

            if (current == null)
                curIndex = 0;

            int selection = 0;
            
            if (string.IsNullOrEmpty(label))
                selection = EditorGUILayout.Popup(curIndex, selections);
            else
                selection = EditorGUILayout.Popup(new GUIContent(label), curIndex, selections);

            if (selection <= 0)
                return null;

            return CharacterManager.instance.characters[selection-1];
        }

        private void OnEnable()
        {
            if (selectedConversation != null)
            {
                Initialize(selectedConversation);
            }

        }

        private void OnDestroy()
        {
            if (selectedConversation != null)
            {
                Housekeeping();
                AssetDatabase.SaveAssets();
            }
        }

        void Initialize(Conversation conversation)
        {
            if (selectedConversation != null)
            {
                Housekeeping();
                AssetDatabase.SaveAssets();
            }

            ConversationManager.instance.InitTextProvider();
            
            selectedConversation = conversation;
            if (treeViewState == null)
                treeViewState = new TreeViewState();
            treeView = new ConversationTreeView(treeViewState, selectedConversation, this);
                
            titleContent = new GUIContent("Conversation Editor", EditorGUIUtility.IconContent("tree_icon_leaf").image);

            Show();

            Housekeeping();
        }

        void Housekeeping()
        {
            CleanupUnusedAssetConnections();

            if (selectedConversation != null)
            {
                ProcessConversationOrdering(selectedConversation);
                //ProcessConversationTidy(selectedConversation);
            }
        }

        // REMOVE BELOW IF NOT USING I2LOC
        //void ProcessConversationTidy(Conversation conversation)
        //{
        //    List<string> existingTerms = I2.Loc.LocalizationManager.GetTermsList(conversation.name);
        //    List<string> convoTerms = new List<string>();

        //    Stack<ConversationNode> toProcess = new Stack<ConversationNode>();

        //    for (int i = conversation.root.NumChildren() - 1; i >= 0; i--)
        //    {
        //        if (conversation.root.IsLink(i)) continue;
        //        toProcess.Push(conversation.root.GetChild(i) as ConversationNode);
        //    }

        //    while (toProcess.Count > 0)
        //    {
        //        ConversationNode node = toProcess.Pop();

        //        for (int i = node.NumChildren() - 1; i >= 0; i--)
        //        {
        //            if (node.IsLink(i)) continue;
        //            toProcess.Push(node.GetChild(i) as ConversationNode);
        //        }

        //        string locKey = conversation.name + "/" + node.guid.ToStringHex();
        //        convoTerms.Add(locKey);
        //    }

        //    int removeCount = 0;

        //    foreach (var badTerm in existingTerms.Where(x => !convoTerms.Contains(x)))
        //    {
        //        I2.Loc.LanguageSourceData sourceData = I2.Loc.LocalizationManager.GetSourceContaining(badTerm);
                        
        //        if (sourceData == null) continue;

        //        sourceData.RemoveTerm(badTerm);
        //        removeCount++;
        //    }

        //    if (removeCount > 0)
        //    {
        //        Debug.Log($"Cleaned up {removeCount} loc strings for removed nodes in {conversation.name}");

        //        foreach (var s in I2.Loc.LocalizationManager.Sources)
        //        {
        //            s.UpdateDictionary(true);
        //        }
        //        I2.Loc.LocalizationManager.UpdateSources();

        //        //LocalizationEditor.ScheduleUpdateTermsToShowInList();
        //        I2.Loc.LocalizationEditor.ParseTerms(true, false, false);
        //    }
        //}

        void ProcessConversationOrdering(Conversation conversation)
        {
            int currentOrder = 0;

            ConversationManager.instance.InitTextProvider();

            Stack<ConversationNode> toProcess = new Stack<ConversationNode>();

            for (int i = conversation.root.NumChildren() - 1; i >= 0; i--)
            {
                if (conversation.root.IsLink(i)) continue;
                toProcess.Push(conversation.root.GetChild(i) as ConversationNode);
            }

            while (toProcess.Count > 0)
            {
                ConversationNode node = toProcess.Pop();

                for (int i = node.NumChildren() - 1; i >= 0; i--)
                {
                    if (node.IsLink(i)) continue;
                    toProcess.Push(node.GetChild(i) as ConversationNode);
                }

                //string locKey = conversation.name + "/" + node.guid.ToStringHex();
                //I2.Loc.TermData termData = I2.Loc.LocalizationManager.GetTermData(locKey);
                //if (termData != null)
                //{
                //    termData.OrderHint = currentOrder++;
                //}
            }
        }
    }
}
