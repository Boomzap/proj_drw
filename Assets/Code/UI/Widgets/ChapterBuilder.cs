using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class ChapterBuilder : MonoBehaviour
    {
        public List<SceneEntry> sceneEntries = new List<SceneEntry>();

        public GameObject sceneEntryPrefab;

        public Transform scrollViewContent;

        [SerializeField] TextMeshProUGUI chapterDisplayNameText;

        [SerializeField] CarouselView carouselView;

        [SerializeField] ScrollRect m_ScrollRect;

        Chapter currentChapter = null;

        private void OnEnable()
        {
            Canvas.ForceUpdateCanvases();
            m_ScrollRect.verticalScrollbar.value = 1f;
        }

        public void SetupChapter(Chapter chapter)
        {
            currentChapter = chapter;

            sceneEntries.ForEach(x => Destroy(x.gameObject));
            sceneEntries.Clear();

            chapterDisplayNameText.text = chapter.chapterDisplayName;

            for(int i = 0; i < chapter.sceneEntries.Length; i++)
            {
                Chapter.Entry chapterEntry = chapter.sceneEntries[i];

                SceneEntry sceneEntry = Instantiate(sceneEntryPrefab, scrollViewContent).GetComponent<SceneEntry>();
                
                string description = LocalizationUtil.FindLocalizationEntry($"UI/Chapter/{chapter.name}/scene_{i}_desc", string.Empty, false, TableCategory.UI);

                if (i == 0)
                {
                    sceneEntry.Setup(chapterEntry, description, true);
                }
                else
                {
                    sceneEntry.Setup(chapterEntry, description, chapterEntry.isEntryUnlocked);
                }

                sceneEntries.Add(sceneEntry);
            }
        }

        public SceneEntry GetSceneEntryByIndex(int index)
        {
            return sceneEntries[index];
        }

        public CarouselView.CarouselAnimState GetAnimState()
        {
            return carouselView.currentAnimState;
        }
    }
}
