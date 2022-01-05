using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOTriviaObject : HOInteractiveObject
    {
        public override void InitializeDefaults(string roomName)
        {
            RegenerateCollision();
            displayKey = LocalizationUtil.FindLocalizationEntry(roomName +"/" + gameObject.name , string.Empty, true, TableCategory.Trivia);
        }
        public override bool OnClick()
        {
            HOGameController.instance.OnTriviaObjectClick(this);
            return true;
        }

#if UNITY_EDITOR
        [Button]
        void RegenerateSDF()
        {
            GetComponentInParent<HORoom>().GenerateSDFs(new HOInteractiveObject[] { this });
        }
#endif
    }
}

