using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Boomzap.Character
{
    public class CharacterAttachment : MonoBehaviour
    {
        SpriteRenderer[] sprites;

        private void Start()
        {
            sprites = GetComponentsInChildren<SpriteRenderer>(true);
        }

        public void SetColor(Color c)
        {
            if (sprites == null) return;

            foreach (var s in sprites)
                s.color = c;
        }
    }
}
