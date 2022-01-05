using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class MinigamePiece : MonoBehaviour
    {
        public SpriteRenderer sdfRenderer;
        public SpriteRenderer sprite;

        private void Reset()
        {
            sprite = GetComponent<SpriteRenderer>();
        }
    }
}