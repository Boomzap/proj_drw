using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;


namespace ho
{
    public class HOFindableObject : HOInteractiveObject
    {
        [BoxGroup("Object Settings", Order = 0)]
        public HODifficulty difficultyLevel = HODifficulty.Easy;
        [BoxGroup("Object Settings")]
        public HOFindableLogicValidity logicValidity = new HOFindableLogicValidity();
        [BoxGroup("Object Settings")]
        public bool isSpecialStoryItem = false;


        [SerializeField, ReadOnly]
        public string objectGroup = "";
        [SerializeField, ReadOnly]
        public string objectBaseName = "";

        [ReadOnly, EnableIf("hasRiddleMode")]
        public string[] riddleText;

        public string[] GetRiddleText() => riddleText;

        public bool hasRiddleMode { get { return logicValidity.validLogicTypes.Contains("HOLogicRiddle") || logicValidity.validLogicTypes.Contains("HOLogicSpecialRiddle"); } }

        public bool IsValidForLogic(HOLogic logic)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (logic is HOLogicDebug)
                return logicValidity.validLogicTypes.Contains("HOLogicStandard");
#endif
            if (logic is HOLogicScramble)
                return logicValidity.validLogicTypes.Contains("HOLogicStandard");

            return logicValidity.validLogicTypes.Contains(logic.GetType().Name);
        }

        public bool IsValidForLogic<T>() where T : HOLogic
        {
#if UNITY_EDITOR
            if (typeof(T) == typeof(HOLogicDebug))
                return logicValidity.validLogicTypes.Contains("HOLogicStandard");
#endif
            if (typeof(T) == typeof(HOLogicScramble))
                return logicValidity.validLogicTypes.Contains("HOLogicStandard");

            return logicValidity.validLogicTypes.Contains(typeof(T).Name);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

#if UNITY_EDITOR


        [BoxGroup("Generate Riddle Keys"), ShowIf("hasRiddleMode"), Button(ButtonSizes.Large)]
        public void GenerateRiddleKeys()
        {
            var room = GetComponentInParent<HORoom>();
            //Debug.Log("Riddle Mode!");
            if (room)
            {
                //Debug.Log("Riddle Texts Created");
                riddleText = HOUtil.GetRoomObjectRiddle(room.name, name);
            }
        }

        [Button]
        void RegenerateSDF()
        {
            GetComponentInParent<HORoom>().GenerateSDFs(new HOInteractiveObject[] { this });
        }
#endif

        protected void AddValidLogicTypes(HOFindableLogicValidity logicValidity)
        {
            logicValidity.validLogicTypes.Add("HOLogicDebug");
            logicValidity.validLogicTypes.Add("HOLogicStandard");
            logicValidity.validLogicTypes.Add("HOLogicNoVowel");
            logicValidity.validLogicTypes.Add("HOLogicSilhouette");
            logicValidity.validLogicTypes.Add("HOLogicPicture");
        }

        public override void InitializeDefaults(string roomName)
        {
            RegenerateCollision();

            logicValidity = new HOFindableLogicValidity();

            if (name.ToLower().StartsWith("o_det"))
            {
                logicValidity.validLogicTypes.Clear();
                logicValidity.validLogicTypes.Add("HOLogicDetail");

                // no localization added for detail objects
                return;
            }

            char lastChar = name[name.Length - 1];

            if (char.IsNumber(lastChar))
            {
                var match = Regex.Match(name, @"^(.*?)(\d+)");

                int groupIndex = 0;

                if (match.Success && match.Groups.Count == 3)
                {
                    int.TryParse(match.Groups[2].Value, out groupIndex);

                    if (groupIndex > 0)
                    {
                        objectGroup = match.Groups[1].Value;
                        objectBaseName = objectGroup;

                        if (name[0] == 'x')
                        {
                            // don't bother with pluralization here, as they're only valid for FindX
                            logicValidity.validLogicTypes.Add("HOLogicFindX");
                            HOUtil.GetRoomObjectFindXTerm(roomName, objectBaseName);
                        }
                        else if(name[0] == 'p')
                        {
                            logicValidity.validLogicTypes.Add("HOLogicPairs");
                            logicValidity.validLogicTypes.Add("HOLogicDebug");
                            displayKey = HOUtil.GetRoomObjectLocalizedName(roomName, objectGroup, true);
                        }
                        else
                        {
                            //displayKey = HOUtil.GetRoomObjectLocalizedName(roomName, gameObject.name, true);
                            logicValidity.validLogicTypes.Add("HOLogicStandard");
                            logicValidity.validLogicTypes.Add("HOLogicDebug");
                            //Add Key for multiples
                            HOUtil.GetRoomObjectPluralization(roomName, objectGroup, groupIndex);
                            displayKey = HOUtil.GetRoomObjectLocalizedName(roomName, objectGroup, true);
                            
                        }
                        return;
                    }
                }

            }

            AddValidLogicTypes(logicValidity);
            displayKey = HOUtil.GetRoomObjectLocalizedName(roomName, gameObject.name, true);
        }

        public override bool OnClick()
        {
            HOGameController.instance.OnFindableObjectClick(this);
            return true;
        }
    }
}
