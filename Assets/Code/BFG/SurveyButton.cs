using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ho
{
    public class SurveyButton : MonoBehaviour
    {
        SpriteRenderer sr;

        MaterialPropertyBlock block;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            
            block = new MaterialPropertyBlock();


            block.SetFloat("_Intensity", 0f);
            block.SetTexture("_MainTex", sr.sprite.texture);
            sr.SetPropertyBlock(block);
        }

        private void OnMouseUpAsButton()
        {
            Audio.instance.PlaySound(UIController.instance.defaultClickAudio);
            Application.Quit();
        }

        private void OnMouseEnter()
        {
            block.SetFloat("_Intensity", 0.2f);
            sr.SetPropertyBlock(block);
        }

        private void OnMouseExit()
        {
            block.SetFloat("_Intensity", 0.0f);
            sr.SetPropertyBlock(block);
        }
    }
}
