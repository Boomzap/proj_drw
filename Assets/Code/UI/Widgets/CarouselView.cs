using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace ho
{
    public class CarouselView : MonoBehaviour
    {
        [BoxGroup("Animation")]
        public AnimationClip moveLeft;
        [BoxGroup("Animation")]
        public AnimationClip moveLeftCenter;
        [BoxGroup("Animation")]
        public AnimationClip moveLeftExit;

        [BoxGroup("Animation")]
        public AnimationClip moveRight;
        [BoxGroup("Animation")]
        public AnimationClip moveRightCenter;
        [BoxGroup("Animation")]
        public AnimationClip moveRightExit;

        [BoxGroup("Animation")]
        public AnimationClip moveExitRight;
        [BoxGroup("Animation")]
        public AnimationClip moveExitLeft;

        public Graphic[] graphicComponents;

        public CarouselAnimState currentAnimState;
        public enum CarouselAnimState
        {
            Center,
            Left,
            Right,
            Exit
        }

        [Button]
        public void CycleCarousel(bool cycleLeft)
        {
            switch(currentAnimState)
            {
                case CarouselAnimState.Center:
                    {
                        if(cycleLeft)
                        {
                            currentAnimState = CarouselAnimState.Left;
                            //Center to Left -> Anim Move Left
                            AnimUtil.PlayAnimation(gameObject, this, moveLeft.name);
                        }
                        else
                        {
                            currentAnimState = CarouselAnimState.Right;
                            //Center to Right -> Anim Move Right
                            AnimUtil.PlayAnimation(gameObject, this, moveRight.name);
                        }

                        break;
                    }
                case CarouselAnimState.Left:
                    {
                        if (cycleLeft)
                        {
                            currentAnimState = CarouselAnimState.Exit;
                            transform.SetAsFirstSibling();
                            //Left to Exit -> Anim Move Right Exit
                            AnimUtil.PlayAnimation(gameObject, this, moveRightExit.name);
                        }
                        else
                        {
                            currentAnimState = CarouselAnimState.Center;
                            transform.SetAsLastSibling();
                            //Left to Center -> Anim Move Right Center
                            AnimUtil.PlayAnimation(gameObject, this, moveRightCenter.name);
                        }

                        break;
                    }
                case CarouselAnimState.Exit:
                    {
                        if (cycleLeft)
                        {
                            currentAnimState = CarouselAnimState.Right;
                            // Exit to Right -> Anim Move Right
                            AnimUtil.PlayAnimation(gameObject, this, moveExitRight.name);
                        }
                        else
                        {
                            currentAnimState = CarouselAnimState.Left;
                            //Left to Center -> Anim Move Right Center
                            AnimUtil.PlayAnimation(gameObject, this, moveExitLeft.name);
                        }

                        break;
                    }
                case CarouselAnimState.Right:
                    {
                        if (cycleLeft)
                        {
                            currentAnimState = CarouselAnimState.Center;
                            transform.SetAsLastSibling();
                            // Exit to Right -> Anim Left Center
                            AnimUtil.PlayAnimation(gameObject, this, moveLeftCenter.name);
                        }
                        else
                        {
                            currentAnimState = CarouselAnimState.Exit;
                            transform.SetAsFirstSibling();
                            //Left to Center -> Anim Move Exit Right
                            AnimUtil.PlayAnimation(gameObject, this, moveLeftExit.name);
                        }

                        break;
                    }
            }
        }

        public void SetGraphicsColor()
        {
            graphicComponents = GetComponentsInChildren<Graphic>(true);

            foreach (var graphic in graphicComponents)
            {
                if (graphic.name.Contains("TXT")) continue;

                Color newColor = currentAnimState == CarouselView.CarouselAnimState.Center ? Color.white : Color.white * .8f;
                newColor.a = 1f;
                graphic.color = newColor;

                //if(currentChapter)
                //graphic.material = GameController.save.IsChapterAvailable(currentChapter) ? null : UIController.instance.grayscale;
            }
        }
    }
}
