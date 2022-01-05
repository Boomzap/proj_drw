#if SURVEY_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System;
using UnityEngine.Events;
using System.Linq;

namespace ho
{
    public partial class GameController : SimpleSingleton<GameController>
    {
        public void FadeToSurveyEnd(UnityEngine.Events.UnityAction onFadeOutComplete = null)
        {
            if (InWorldState<SurveyEndWorld>()) return;

            UIController.instance.FadeOut(() =>
            {
                WorldStateCleanup();

                StateCache.instance.PreloadSurveyEnd(() =>
                {
                    onFadeOutComplete?.Invoke();

                    currentWorldStateObject = Instantiate(StateCache.instance.SurveyEnd, menuScenePos);
                    currentWorldState = currentWorldStateObject.GetComponent<SurveyEndWorld>();
                    currentCamera.transform.position = menuScenePos.position + cameraOffset;

                    UIController.instance.HideAll(true);

                    UIController.instance.FadeIn(() =>
                    {
                        //UIController.instance.mainMenuUI.Show();
                    });
                });
            });
        }

    }
}
#endif