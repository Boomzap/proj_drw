using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    public class ButtonMaterialController : MonoBehaviour
    {
        [ReadOnly, SerializeField]
        Material dimmerMaterial;
        
        Material matInstance = null;
        public Material MaterialInstance { get => matInstance; }

        public Image linkedImage;
        public float lightIntensity = 0f;
        public float desatIntensity = 0f;

        public bool disableVisualChanges = false;

        #if UNITY_EDITOR
        private void Reset()
        {
            dimmerMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GlowDesatMat.mat");

            Button btn = GetComponent<Button>();
            if (btn)
            {
                linkedImage = btn.image;
            }
        }
        #endif

        private void Update()
        {
            if (disableVisualChanges)
            {
                matInstance?.SetFloat("_LightIntensity", 0f);
                matInstance?.SetFloat("_DesatIntensity", 0f);
            } else
            {
                matInstance?.SetFloat("_LightIntensity", lightIntensity);
                matInstance?.SetFloat("_DesatIntensity", desatIntensity);
            }
        }

        private void Awake()
        {
            if (disableVisualChanges) return;

            if (matInstance == null)
                matInstance = Instantiate(dimmerMaterial);


            if (linkedImage)
                linkedImage.material = matInstance;

            matInstance?.SetFloat("_LightIntensity", 0f);
            matInstance?.SetFloat("_DesatIntensity", 0f);
        }
    }
}
