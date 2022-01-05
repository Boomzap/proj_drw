using System.Collections;
using UnityEngine;

using Sirenix.OdinInspector;

namespace Boomzap.Character
{
    [RequireComponent(typeof(Character))]
    public class CharacterPuppeteer : MonoBehaviour
    {
        [ValueDropdown("GetStates")]
        public string state;
        [ValueDropdown("GetEmotions")]
        public string emotion;
        [ValueDropdown("GetEyes")]
        public string eyes;
        public bool isLookingBack = false;
        public bool isFlipped = false;

        [SerializeField] Character character;

        private void Awake()
        {

        }

        private void Reset()
        {
            character = GetComponent<Character>();
            state = character.defaultState;
            emotion = character.defaultEmotion;
            eyes = character.defaultEyes;
        }

        void Start()
        {
            Visualize();
        }

        [Button]
        void Visualize()
        {
            if (character)
            {
                character.SetState(state);
                character.SetEmotion(emotion, eyes);
                character.Flip(isFlipped);
                character.SetLookBack(isLookingBack);
                character.ForceAlpha(1f);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        string[] GetStates()
        {
            if (character) return character.GetStates();
            return null;
        }

        string[] GetEyes()
        {
            if (character) return character.GetEmotion(state).allEyes;
            return null;
        }

        string[] GetEmotions()
        {
            if (character) return character.GetEmotion(state).allEmotions;
            return null;
        }
    }
}