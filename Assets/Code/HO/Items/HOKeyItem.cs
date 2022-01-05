using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ho
{
    public class HOKeyItem : HOFindableObject
    {
        [BoxGroup("Object Settings"), ReadOnly]
        public string promptKey;

        bool isDragging = false;

        public bool IsDragging { get { return isDragging; } set { isDragging = value; } }


        //Reset Variables
        Vector3 defaultPos = Vector3.zero;

        SpriteRenderer spriteRenderer;

        int defaultSortingOrder;

        public override void InitializeDefaults(string roomName)
        {
            RegenerateCollision();

            logicValidity = new HOFindableLogicValidity();

            AddValidLogicTypes(logicValidity);
            displayKey = HOUtil.GetRoomObjectLocalizedName(roomName, gameObject.name, true);
            promptKey = HOUtil.GetRoomObjectLocalizedName(roomName, gameObject.name + "_prompt", true);
        }

        public override bool OnClick()
        {
            UIController.instance.hoMainUI.itemPrompt.ShowPrompt(LocalizationUtil.FindLocalizationEntry(promptKey));
            return true;
        }

        public void ResetToDefaultState()
        {
            isDragging = false;
            transform.position = defaultPos;
            defaultSortingOrder = spriteRenderer.sortingOrder;
        }

        private void Awake()
        {
            defaultPos = transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultSortingOrder = spriteRenderer.sortingOrder;
        }

        private void Update()
        {
            if(isDragging)
            {
                var worldPos = GameController.instance.currentCamera.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                transform.position = worldPos;
                spriteRenderer.sortingOrder = 500;
            }
        }
    }
}
