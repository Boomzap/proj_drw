using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Linq;

namespace ho
{
    public class OtherGamesPopup : Popup
    {
        public Sprite[] otherGameSprites;
        public GameObject otherGamesEntryPrefab;
        [SerializeField] Transform scrollContent;
        public Scrollbar verticalScroll;

        public Button closeButton;

        [SerializeField]
        TextAsset otherGamesData;


        protected override void OnBeginShow(bool instant)
        {
            base.OnBeginShow(instant);

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Hide());
            scrollContent.gameObject.SetActive(false);
            verticalScroll.value = 1;
        }
        protected override void OnFinishShow()
        {
            base.OnFinishShow();
            scrollContent.gameObject.SetActive(true);
            verticalScroll.value = 1;
        }

#if UNITY_EDITOR
        [System.Serializable]
        class OtherGamesList
        {
            public string[] otherGames = new string[0];
        }

        [Button] void GenerateOtherGamesEntry()
        {
            var objects = scrollContent.GetComponentsInChildren<Transform>();
            foreach(var obj in objects)
            {
                if (obj.name.Contains("Content")) continue;
                DestroyImmediate(obj.gameObject);
            }

            OtherGamesList otherGames = JsonUtility.FromJson<OtherGamesList>(@"{""otherGames"":" + otherGamesData.text + @"}");

            foreach (var sprite in otherGameSprites)
            {
                GameObject entryObject = Instantiate(otherGamesEntryPrefab, scrollContent);
                entryObject.name = sprite.name.Replace("_", "").Replace("'", "");
                OtherGamesEntry entry = entryObject.GetComponent<OtherGamesEntry>();
                
                //Match link
                foreach(var link in otherGames.otherGames)
                {
                    if(link.Contains(entryObject.name))
                    {
                        entry.winLink = link;
                        entry.osxLink = link + "?mac";
                        break;
                    }
                }

                entry.entryImage.sprite = sprite;
            }

        }


#endif
    }

}
