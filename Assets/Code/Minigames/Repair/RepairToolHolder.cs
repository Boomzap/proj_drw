using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class RepairToolHolder : MonoBehaviour
    {
        [SerializeField] RepairToolList repairToolList;
        [SerializeField] Image toolCopy;
        [SerializeField] List<RepairToolList.RepairTool> currentRepairTools = new List<RepairToolList.RepairTool>();


        [ReadOnly, BoxGroup("Tool Components")] public List<Button> toolButtons;
        [ReadOnly, BoxGroup("Tool Components")] public Image[] toolImages;

        [ReadOnly, ShowInInspector] List<RepairToolList.RepairTool> repairTools = new List<RepairToolList.RepairTool>();

        bool isSkipped = false;

        int selectedIndex = -1;

        public bool SkipMinigame { set { isSkipped = value; } }

        public GameObject ToolCopy { get { return toolCopy.gameObject;  } }

        [Button]
        public void Setup()
        {
            isSkipped = false;
            selectedIndex = -1;
            toolCopy.gameObject.SetActive(false);

            if(toolButtons == null)
            {
                toolButtons = GetComponentsInChildren<Button>(true).ToList();
            }
            
            if(toolImages == null)
            {
                toolButtons = GetComponentsInChildren<Button>(true).ToList();
            }
        }

        public void UpdateTools()
        {
            repairTools.Clear();

            currentRepairTools = MinigameController.instance.ActiveMinigameAsType<RepairMG>().GetMinigameTools();

            Setup();

            for (int i = 0; i < toolButtons.Count; i++)
            {
                if (i < currentRepairTools.Count)
                {
                    //Updates Tool UI Images
                    toolImages[i].sprite = currentRepairTools[i].toolSprite_default;
                    repairTools.Add(currentRepairTools[i]);
                }
                toolButtons[i].gameObject.SetActive(i < currentRepairTools.Count);
            }
        }



        public void SelectTool(string selectedToolName)
        {
            if(currentRepairTools.Any(x => x.toolName == selectedToolName))
            {
                var selectedTool = currentRepairTools.FirstOrDefault(x => x.toolName == selectedToolName);
                var index = currentRepairTools.IndexOf(selectedTool);

                //If currently selected is different, select that tool
                if(index != selectedIndex)
                    OnClickTool(index);
            }
            else
            {
                Debug.Log($"Current tool is not found for this minigame! Please update {MinigameController.instance.ActiveMinigame.name}'s tool list.");
            }
        }

        public void OnClickTool(int index)
        {
            if (MinigameController.instance.isHintPlaying) return;

            if (selectedIndex == -1)
            {
                //Select Tool
                selectedIndex = index;
                SelectTool();
                
            }
            else if (selectedIndex >= 0 && selectedIndex == index)
            { 
                DeselectTool();
                selectedIndex = -1;
            }
            else if (selectedIndex >= 0 && selectedIndex != index)
            {
                DeselectTool();
                selectedIndex = index; //Updates new Selected
                SelectTool();
            }
        }

        void SelectTool()
        {
            MinigameController.instance.ActiveMinigameAsType<RepairMG>().UpdateRepairType(currentRepairTools[selectedIndex]);
            toolImages[selectedIndex].sprite = repairTools[selectedIndex].toolSprite_on;
            iTween.ScaleTo(toolImages[selectedIndex].gameObject, Vector3.one * 1.2f, 0.3f);

            toolCopy.sprite = repairTools[selectedIndex].toolSprite_on;
            toolCopy.gameObject.SetActive(true);
        }
        
        public void DeselectTool()
        {
            MinigameController.instance.ActiveMinigameAsType<RepairMG>().UpdateRepairType(null);
            if (repairTools.Count > 0 && selectedIndex >= 0 )
            {
                toolImages[selectedIndex].sprite = repairTools[selectedIndex].toolSprite_default;
                iTween.ScaleTo(toolImages[selectedIndex].gameObject, Vector3.one, 0.3f);
            }
                
            toolCopy.gameObject.SetActive(false);
        }

        public void MoveToolCopy(Vector3 position)
        {
            iTween.MoveTo(toolCopy.gameObject, iTween.Hash("position", position, "time", 0.25f, "easetype", "easeOutQuart"));
        }

        private void Update()
        {
            if (isSkipped) return;

            if(toolCopy.gameObject.activeInHierarchy)
            {
                toolCopy.transform.position = Input.mousePosition;
            }
        }
    }
}

