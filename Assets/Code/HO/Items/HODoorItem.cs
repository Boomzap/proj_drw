using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ho
{
    public class HODoorItem : HOInteractiveObject
    {
        [BoxGroup("Object Settings")]
        public string   doorName;

        [BoxGroup("Object Settings"), ReadOnly]
        public string promptKey;

        public Material mouseoverMaterial;

        bool            isMouseOver = false;
        float           brightenIntensityTarget = 0f;
        float           brightenIntensity = 0f;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        private void Awake()
        {
            GetComponent<SpriteRenderer>().material = Instantiate(mouseoverMaterial);
        }

        // Update is called once per frame
        void Update()
        {
            float brightDelta = Mathf.Abs(brightenIntensity - brightenIntensityTarget);

            if (brightDelta > 0.001f)
            {
                brightenIntensity += Mathf.Min(Time.deltaTime*1.5f, brightDelta) * Mathf.Sign(brightenIntensityTarget - brightenIntensity);
            } else
            {
                brightenIntensity = brightenIntensityTarget;
            }

            GetComponent<SpriteRenderer>().material.SetFloat("_Intensity", brightenIntensity);
        }

        private void OnDisable()
        {
            
        }

        public void SetIsMouseover(bool b)
        {
            if (isMouseOver == b) return;
            isMouseOver = b;

            brightenIntensityTarget = isMouseOver ? 0.4f : 0f;
        }

        public override void InitializeDefaults(string roomName)
        {
            RegenerateCollision();

            string[] name = gameObject.name.Split('.');

            if(name.Length > 1 && name[1].Contains("close"))
            {
                displayKey = string.Empty;
                promptKey = HOUtil.GetRoomObjectLocalizedName(roomName, name[0] + "_prompt", true);
            }
        }

        public override bool OnClick()
        {
            HOGameController.instance.OnDoorObjectClick(this);
            return true;
        }

#if UNITY_EDITOR
        [Button]
        public void SetupOpenDoorGlow()
        {
            if (gameObject.name.ToLower().Contains("open"))
            {
                sdfRenderer.gameObject.SetActive(true);
                sdfRenderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DoorOpenGlow.mat");
            }
        }
#endif
    }
}
