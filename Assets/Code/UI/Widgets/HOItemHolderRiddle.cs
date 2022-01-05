using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ho
{
    public class HOItemHolderRiddle : HOItemHolder
    {
        protected override void UpdateText(string newText, bool animate)
        {
            if (findables[0] == null || findables.Count == 0)
                newText = "";
            else
            {
                var riddles = findables[0].GetRiddleText();

                newText = LocalizationUtil.FindLocalizationEntry(riddles[Random.Range(0, riddles.Length)]);
            }

            base.UpdateText(newText, animate);
        }
    }
}