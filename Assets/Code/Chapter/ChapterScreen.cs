using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

namespace ho
{
    public class ChapterScreen : MonoBehaviour, IWorldState
    {
        [SerializeField]
        ChapterVis source;
        [SerializeField]
        ChapterVis dest;
        [SerializeField]
        BookFX bookFX;

        void OnChapterAnimDone()
        {
            bookFX.Hide();

            source.chapterCompleteRibbon.gameObject.SetActive(dest.chapterCompleteRibbon.gameObject.activeSelf);
            source.chapterImage.sprite = dest.chapterImage.sprite;

            source.chapterTitle.text = dest.chapterTitle.text;
            source.chapterBlurb.text = dest.chapterBlurb.text;
        }

        public void SetChapter(Chapter chapter, bool animate, bool isChapterToRight)
        {
            bool chapterComplete = GameController.save.IsChapterComplete(chapter);

            if (animate)
            {
                string prevTitle = source.chapterTitle.text;
                string prevBlurb = source.chapterBlurb.text;
                Sprite prevSprite = source.chapterImage.sprite;
                bool prevActive = source.chapterCompleteRibbon.gameObject.activeInHierarchy;

                dest.chapterImage.sprite = chapter.chapterImage;
                dest.chapterCompleteRibbon.gameObject.SetActive(chapterComplete);

                dest.chapterTitle.text =  LocalizationUtil.FindLocalizationEntry(chapter.chapterDisplayName, string.Empty, false, TableCategory.UI);
                dest.chapterBlurb.text = LocalizationUtil.FindLocalizationEntry(chapter.chapterInfoText, string.Empty, false, TableCategory.UI);

                dest.chapterTitle.ForceMeshUpdate();
                dest.chapterBlurb.ForceMeshUpdate();

                if (!isChapterToRight)
                {
                    dest.chapterImage.sprite = prevSprite;//chapter.chapterImage;
                    dest.chapterCompleteRibbon.gameObject.SetActive(prevActive);
                    source.chapterTitle.text = chapter.chapterDisplayName;
                    source.chapterBlurb.text = chapter.chapterInfoText;
                } else
                {
//                     source.chapterImage.sprite = prevSprite;
//                     source.chapterCompleteRibbon.gameObject.SetActive(prevActive);
                }

                bookFX.Show();
                bookFX.Animate(isChapterToRight, OnChapterAnimDone);

                if (isChapterToRight)
                {
                    source.chapterTitle.text = chapter.chapterDisplayName;
                    source.chapterBlurb.text = chapter.chapterInfoText;
                } else
                {
                    source.chapterTitle.text = prevTitle;
                    source.chapterBlurb.text = prevBlurb;
                    source.chapterImage.sprite = chapter.chapterImage;
                    source.chapterCompleteRibbon.gameObject.SetActive(chapterComplete);
                    dest.chapterImage.sprite = chapter.chapterImage;
                    dest.chapterCompleteRibbon.gameObject.SetActive(chapterComplete);
                }

            } else
            {

                bookFX.Hide();

                source.chapterCompleteRibbon.gameObject.SetActive(chapterComplete);
                source.chapterImage.sprite = chapter.chapterImage;

                source.chapterTitle.text = chapter.chapterDisplayName;
                source.chapterBlurb.text = chapter.chapterInfoText;
            }

        }

        public void OnLeave()
        {
            StateCache.instance.UnloadChapterScreen();
        }

        public bool ShouldDestroyOnLeave()
        {
            return true;
        }
    }
}