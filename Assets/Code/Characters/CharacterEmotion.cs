using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spine;
using Spine.Unity;

using UnityEngine;

namespace Boomzap.Character
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public class CharacterEmotion : MonoBehaviour
    {
        [SpineSkin]
        public string emotion;
        [SpineAnimation]
        public string eyes;

        SkeletonAnimation _skeletonAnimation;
        [SpineSlot]
        Slot movingMouth = null;

        public bool isTalking = false;

        public SkeletonAnimation skeletonAnimation
        {
            get 
            {
                if (_skeletonAnimation == null) 
                    _skeletonAnimation = GetComponent<SkeletonAnimation>();

                return _skeletonAnimation;
            }
        }

        public string[] allEmotions
        {
            get
            {
                SkeletonAnimation anim = skeletonAnimation;
                if (anim == null || anim.Skeleton == null || anim.Skeleton.Data.Skins.Count == 0) return null;

                string[] skins = new string[anim.Skeleton.Data.Skins.Count-1];
                for (int i = 0; i < skins.Length; i++)
                {
                    skins[i] = anim.Skeleton.Data.Skins.Items[i+1].Name;
                }

                return skins;
            }
        }

        public string[] allEyes
        {
            get
            {
                SkeletonAnimation anim = skeletonAnimation;
                if (anim == null || anim.Skeleton == null || anim.Skeleton.Data.Animations.Count == 0) return null;

                string[] anims = new string[anim.Skeleton.Data.Animations.Count];
                for (int i = 0; i < anims.Length; i++)
                {
                    anims[i] = anim.Skeleton.Data.Animations.Items[i].Name;
                }

                return anims;                
            }
        }

        public int SortOrder
        {
            set
            {
                skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = value;
            }
        }

        public string State
        {
            get
            {
                return $"{emotion}_{eyes}";
            }
            set
            {
                string[] values = StrReplace.Tokenize(value, '_');
                if (values == null || values.Length == 0) return;
                if (values.Length == 1)
                {
                    SetEmotion(values[0], "");
                } else if (values.Length >= 2)
                {
                    SetEmotion(values[0], values[1]);
                }
            }
        }

        public void SetColor(Color c)
        {
            skeletonAnimation?.skeleton.SetColor(c);
        }

        public void Flip(bool flip)
        {
            if (skeletonAnimation == null) return;

            Vector3 s = transform.localScale;
            s.x = flip ? Math.Abs(s.x) : -Math.Abs(s.x);
            transform.localScale = s;
        }

        private void Update()
        {
            if (movingMouth == null)
            {
                movingMouth = skeletonAnimation?.skeleton.FindSlot("mouth_talking") ?? null;
            }

            movingMouth?.SetColor(isTalking ? Color.clear : Color.clear);
        }
        
        public void SetEmotion(string _emotion, string _eyes)
        {
            if (!string.IsNullOrEmpty(_emotion)) emotion = _emotion.Trim();
            if (!string.IsNullOrEmpty(_eyes)) eyes = _eyes.Trim();

            SkeletonAnimation anim = skeletonAnimation;
            var setSkin = anim.Skeleton.Data.FindSkin(emotion);
            if (setSkin != null)
            {
                if (anim.Skeleton.Skin != setSkin)
                {
                    anim.Skeleton.SetSkin(setSkin);
                }
            } else
            {
                Debug.Log($"Invalid skin {name} in character {gameObject.name}/{transform.parent.name}");
            }

            anim.Skeleton.SetSlotsToSetupPose();

            if (Application.isPlaying)
            {
                anim.AnimationState.AddAnimation(0, eyes, true, 0.01f);
            } else
            {
                // if in the editor, just jump immediately
                anim.AnimationState.ClearTracks();
                anim.AnimationState.SetAnimation(0, eyes, true);
            }

            anim.AnimationState.Apply(anim.Skeleton);

            if (movingMouth == null)
            {
                movingMouth = skeletonAnimation?.skeleton.FindSlot("mouth_talking") ?? null;
            }
        }
    }
}
