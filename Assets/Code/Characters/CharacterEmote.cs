using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using Spine;
using Sirenix.OdinInspector;

namespace Boomzap.Character
{
    public class CharacterEmote : MonoBehaviour
    {
        public SkeletonDataAsset skeletonDataAsset;

        [Serializable]
        public class State
        {
            [SpineSkin] public string skin;
            [SpineAnimation] public string animation;
        }

        public State[] states;
        public Spine.Unity.SkeletonAnimation skeletonAnimation;
        public Character toCharacter;

        [Button]
        void AutoPopulateEmotes()
        {
            if (skeletonAnimation == null) skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation)
            {
                ExposedList<Skin> skins = skeletonAnimation.skeleton.Data.Skins;
                skins.RemoveAll(x => StrReplace.Equals(x.Name, "default"));
                states = skins.Select(x => new State{skin = x.Name}).ToArray();
            }
        }

        private void Update()
        {
            transform.localScale = new Vector3(toCharacter.IsFlipped ? -1f : 1f, 1f, 1f);
        }

        public void TriggerEmote(string stateName)
        {
            gameObject.SetActive(true);

            State state = states.First(x => StrReplace.Equals(x.skin, stateName));
            if (state == null)
            {
                Debug.Log("Invalid emote " + stateName);
                return;
            }

            skeletonAnimation.initialSkinName = state.skin;
            skeletonAnimation.Initialize(true);
            skeletonAnimation.AnimationState.SetAnimation(0, state.animation, false);
            skeletonAnimation.AnimationState.End += delegate (TrackEntry trackEntry) { gameObject.SetActive(false); };
        }
    }
}
