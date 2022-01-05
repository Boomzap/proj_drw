using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

namespace ho
{
    public class BFG : MonoBehaviour
    {
        public string surveyLinkFilePath = "./bfg_survey.txt";

#if SURVEY_BUILD
        private void OnApplicationQuit()
        {
            if (Application.isEditor) return;

            try
            {
                string surveyLink = File.ReadAllText(surveyLinkFilePath);
                surveyLink.TrimEnd(new char[] { '\r', '\n' });
                Application.OpenURL(surveyLink);
            } catch (FileNotFoundException)
            {
                Debug.Log($"Application ending: {surveyLinkFilePath} not found");
            }
        }
#endif
    }
}
