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
    [Serializable]
    public class CharacterState
    {
        public string Name;
        [SpineSkin]
        public string skin;
        [SpineAnimation]
        public string animation;
        [SpineAnimation]
        public string thenLoop;
    }

    [RequireComponent(typeof(SkeletonAnimation))]
    public class CharacterSpine : MonoBehaviour
    {
        [SpineSkin]
        public string defaultState;
        public CharacterState[] states;

        CharacterState currentState;

        public SkeletonAnimation skeletonAnimation;

        Spine.AnimationState spineAnimationState;
        public Skeleton skeleton;
        BoundingBoxAttachment bbox;
        Bounds bounds;

        [SerializeField]
        public RectTransform   faceBounds;

        public CharacterEmotion emotion => GetComponentInChildren<CharacterEmotion>();

        SkeletonUtility utility;

        public SkeletonUtility Utility
        {
            get
            {
                if (utility == null) utility = GetComponent<SkeletonUtility>();

                return utility;
            }
        }

        SpriteRenderer[] sprites;
        string[] stateNames;

        public string[] StateNames
        {
            get
            {
                if (stateNames == null || stateNames.Length != states.Length)
                {
                    stateNames = states.Select(x => x.Name).ToArray();
                }

                return stateNames;
            }
        }

        public string DefaultState { get { return defaultState; } }
        public string DefaultAnimation { get { return skeletonAnimation.AnimationName; }} 

        private void Start()
        {
            UpdateComp();
            sprites = GetComponentsInChildren<SpriteRenderer>(false);
        }

        [Button]
        void AutoPopulateStates()
        {
            if (skeletonAnimation == null) skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation)
            {
                ExposedList<Skin> skins = skeletonAnimation.skeleton.Data.Skins;
                skins.RemoveAll(x => StrReplace.Equals(x.Name, "default"));
                states = skins.Select(x => new CharacterState{ animation = DefaultAnimation, skin = x.Name, Name = x.Name }).ToArray();
            }
        }

        [Button]
        void UpdateNames()
        {
            foreach (var t in states)
            {
                if (string.IsNullOrWhiteSpace(t.Name)) t.Name = t.skin;
            }
        }

        public bool HasState(string name) => GetState(name) != null;
        public bool IsValidState(string name) => GetState(name) != null;
        public void Flip(bool flip)
        {
            if (skeleton == null) return;
            skeleton.ScaleX = flip ? -1f : 1f;
        }
        public void ForceState(string name)
        {
            CharacterState toState = GetState(name);
            if (toState == null) return;
            currentState = toState;

            SetSkin(currentState.skin);
            if (spineAnimationState != null && toState != null && toState.animation != null)
            {
                spineAnimationState.ClearTracks();
                spineAnimationState.SetAnimation(0, toState.animation, true);
            }
        }

        public void SetState(string name)
        {
            CharacterState toState = GetState(name);
            if (toState == null) return;
            if (toState == currentState) return;
            currentState = toState;

            SetSkin(currentState.skin);
            SetAnimation(string.IsNullOrWhiteSpace(currentState.animation) ? DefaultAnimation : currentState.animation, currentState.thenLoop);
        }

        void SetAnimation(string anim, string thenLoop)
        {
            Validate();

            if (skeleton == null || spineAnimationState == null) return;
            if (string.IsNullOrEmpty(anim)) return;
            TrackEntry entry = spineAnimationState.SetAnimation(0, anim, string.IsNullOrEmpty(thenLoop));
            if (!string.IsNullOrEmpty(thenLoop))
            {
                spineAnimationState.AddAnimation(0, thenLoop, true, -1f);
            }
            entry.MixDuration = 0.01f;
        }

        void SetSkin(string name)
        {
            if (skeleton == null) UpdateComp();
            if (skeleton == null) return;
            if (skeleton.Data.FindSkin(name) != null)
            {
                skeleton.SetSkin(name);
            } else
            {
                Debug.Log($"Invalid skin {name} in character {gameObject.name}/{transform.parent.name}");
            }
            skeleton.SetSlotsToSetupPose();
            if (spineAnimationState != null) spineAnimationState.Apply(skeleton);

            UpdateAABB();
        }

        public CharacterState GetState(string name)
        {
            return states.First(x => StrReplace.Equals(name, x.Name));
        }

        void Validate()
        {
            if (skeletonAnimation == null)
                skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation != null)
            {
                spineAnimationState = skeletonAnimation.AnimationState;
                skeleton = skeletonAnimation.Skeleton;
            }
            foreach (var t in states)
            {
                if (string.IsNullOrEmpty(t.Name))
                    t.Name = t.skin;
            }
        }

        void UpdateBBoxAttachment()
        {
            Slot slot = skeleton.FindSlot("bounds");
            if (slot == null) slot = skeleton.FindSlot("Bounds");
            if (slot != null)
            {
                bbox = slot.Attachment as BoundingBoxAttachment;
            }
        }

        void UpdateComp()
        {
            Validate();
            UpdateBBoxAttachment();
        }

        public void SetColor(Color c)
        {
            skeleton?.SetColor(c);
            if (sprites != null)
            {
                foreach(var s in sprites)
                    s.color = c;
            }
        }

        public void UpdateAABB()
        {
            UpdateComp();

            if (bbox != null)
            {
                Vector3 size = new Vector3();
                var verts = bbox.Vertices;
                if (verts.Length > 1)
                {
                    Vector3 min = new Vector3(verts[0], verts[1]);
                    Vector3 max = min;
                    for (int i = 2; i < verts.Length; i+=2)
                    {
					    min.x = Mathf.Min(min.x, verts[i]);
					    max.x = Mathf.Max(max.x, verts[i]);

					    min.y = Mathf.Min(min.y, verts[i+1]);
					    max.y = Mathf.Max(max.y, verts[i+1]);
                    }

                    size = new Vector3(transform.lossyScale.x * (max.x-min.x), transform.lossyScale.y * (max.y-min.y), 0f);
                }
                bounds = new Bounds(transform.position + new Vector3(0f, size.y * 0.5f, 0f), size);
            }
        }

        public bool TestIntersection(Ray ray)
        {
            UpdateAABB();

            return bounds.IntersectRay(ray);
        }

        public bool IsValid => skeleton.Skin != null;
    }
}
