using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ho
{
    public class ProfileButton : MonoBehaviour
    {
        [SerializeField] Button profileButton;

        [SerializeField] TextMeshProUGUI profileText;
        [SerializeField] Animator animator;

        Savegame.Profile currentProfile;

        public Savegame.Profile CurrentProfile
        {
            get { return currentProfile; }
            set
            {
                currentProfile = value;
                profileText.text = currentProfile.playerName;
            }
        }

        private void Awake()
        {
            profileButton.onClick.RemoveAllListeners();
            profileButton.onClick.AddListener(() => OnSelectProfile());
        }
     
        public void OnSelectProfile()
        {
            GameController.save.currentProfile = currentProfile;
            Savegame.SetDirty();
            Popup.GetPopup<ProfilePopup>().OnSelectProfile(this);
            UIController.instance.mainMenuUI.UpdateProfileName();
            Audio.instance.PlaySound(UIController.instance.defaultClickAudio);
        }

        public void SetSelected(bool selected = true)
        {
            animator.SetBool("selected", selected);
        }
    }
}

