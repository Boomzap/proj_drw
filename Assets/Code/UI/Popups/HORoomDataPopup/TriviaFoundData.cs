using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ho
{
    public class TriviaFoundData : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI[] triviaTexts;

        void ClearTriviaKeys()
        {
            foreach(var text in triviaTexts)
            {
                text.text = "???";
            }
        }

        public void Setup(string[] triviaKeys)
        {
            ClearTriviaKeys();

            foreach(string key in triviaKeys)
            {
                int triviaIndex = -1;

                //Get Last char for trivia key
                //NOTE* Trivia Keys must end in a number for this to work. e.g. t_trivia1 or t_trivia_1

                var lastChar = key.Length > 1? key[key.Length - 1].ToString() : "-1";
                int.TryParse(lastChar, out triviaIndex);

                if(triviaIndex > 0)
                {
                    triviaIndex--;
                    if (triviaIndex >= 0 && triviaIndex < triviaTexts.Length)
                        triviaTexts[triviaIndex].text = LocalizationUtil.FindLocalizationEntry(key, string.Empty, false, TableCategory.Trivia);
                }
            }
        }

    }
}
