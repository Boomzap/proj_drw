using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

using Sirenix.OdinInspector;
using System.Linq;

namespace Boomzap.Character
{
    [RequireComponent(typeof(SkeletonAnimation))]
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class CharacterSpineSlot : MonoBehaviour
    {

        //Has to exist in reference for conversation editor to work properly

        public SkeletonAnimation _skeletonAnimation;

        public SkeletonAnimation skeletonAnimation
        {
            get
            {
                if (_skeletonAnimation == null)
                    _skeletonAnimation = GetComponent<SkeletonAnimation>();

                return _skeletonAnimation;
            }
        }

        //Public access for editing prefab mode
        public List<SlotToggle> slots = new List<SlotToggle>();

        public List<SlotToggle> SpineSlots
        {
            get
            {
                FillSlotToggles();

                if (slots == null)
                {
                    Debug.Log("Slots empty");
                    return null;
                }


                return slots;
            }

            set { slots = value; }
        }

        [System.Serializable]
        public class SlotToggle
        {
            [ToggleGroup("Enabled", "$Label"), OnValueChanged("UpdateSlot")]
            public bool Enabled;

            [HideInInspector]
            public string slotName;

            [HideInInspector]
            public CharacterSpineSlot characterSlot;


            [HideInInspector]
            public string Label { get { return slotName; } }


            void UpdateSlot()
            {
                CharacterSpineSlot.UpdateSpineSlot(characterSlot, this);
            }

        }

        public static void UpdateSpineSlot(CharacterSpineSlot characterSlot, SlotToggle slotToggle)
        {
            if (characterSlot == null)
            {
                Debug.Log("Character Slot not found! Please Fill Slot Toggle again!");
                return;
            }
            //Debug.Log($"Enabled: {slotToggle.Enabled}");
            if (slotToggle.Enabled)
            {
                var slot = characterSlot.FindSlot(slotToggle.slotName);
                if(slot != null)
                {
                    slot.SetColor(new Color32(255, 255, 255, 255));
                }
                else
                {
                    Debug.Log($"Slot Toggle {slotToggle.slotName} has been removed from spine!");
                    characterSlot.slots.Remove(slotToggle);
                }
            }
            else
            {
                var slot = characterSlot.FindSlot(slotToggle.slotName);
                if(slot != null)
                {
                    slot.SetColor(new Color32(0, 0, 0, 0));
                }
                else
                {
                    Debug.Log($"Slot Toggle {slotToggle.slotName} has been removed from spine!");
                    characterSlot.slots.Remove(slotToggle);
                }
            }
        }

        [Button]
        public void FillSlotToggles()
        {
            if (skeletonAnimation == null)
            {
                Debug.Log("No Spine Detected!");
                return;
            }

            if (skeletonAnimation.Skeleton == null)
            {
                Debug.Log("No Spine Skeleton Detected! Spine Slots did not update properly!");
                return;
            }

            var s = skeletonAnimation.Skeleton.Slots.Items;
            if (s == null)
            {
                Debug.Log("No Spine Slots Detected");
                return;
            }

            slots.Clear();
            //Creates new Slot Toggles
            for (int i = 0; i < s.Length; i++)
            {
                SlotToggle toggle = new SlotToggle
                {
                    Enabled = true,
                    slotName = s[i].ToString(),
                    characterSlot = this
                };
                slots.Add(toggle);
            }
        }
        public Slot FindSlot(string slotName)
        {
            if (skeletonAnimation == null)
            {
                Debug.Log("Skeleton animation not found!");
                return null;
            }

            var slot = skeletonAnimation.Skeleton.FindSlot(slotName);

            if (slot == null)
            {
                Debug.Log($"Skeleton slot {slotName} not found!");
                return null;
            }

            return slot;
        }

        public void UpdateAllSlots(CharacterSpineSlot characterSlot)
        {
            for(int i = 0; i < slots.Count; i++)
            {
                UpdateSpineSlot(characterSlot, slots[i]);
            }
        }


#if UNITY_EDITOR

        //Updates Face prefab when entering edit mode
        private void Awake()
        {
            UpdateAllSlots(this);
        }

        public string[] allSlotNames
        {
            get
            {
                _skeletonAnimation = GetComponent<SkeletonAnimation>();
                var s = _skeletonAnimation.Skeleton.Slots.Items;
                //Convert Slots into string
                string[] slotNames = new string[s.Length];

                for (int i = 0; i < slotNames.Length; i++)
                {
                    slotNames[i] = s[i].ToString();
                }
                return slotNames.ToArray();
            }
        }

#endif
    }
}