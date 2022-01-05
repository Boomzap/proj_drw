using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace ho
{
    public class RepairGroup : MonoBehaviour
    {
        public RepairType repairType;

        public RepairGroup nextGroup;

        [ReadOnly]
        public List<RepairablePiece> brokenPieces = new List<RepairablePiece>();


        [Button]
        public void Setup()
        {
            brokenPieces = GetComponentsInChildren<RepairablePiece>().ToList();

            foreach (var brokenPiece in brokenPieces)
            {
                brokenPiece.repairGroup = this;
            }
        }

        [Button]
        public void RefreshRepairType()
        {
            gameObject.name = "[GROUP] " + repairType.ToString();

            foreach(var brokenPiece in brokenPieces)
            {
                brokenPiece.repairType = repairType;
            }
        }

        [Button]
        public void SetupNextGroup()
        {
            if (nextGroup)
            {
                //Disable colliders of next group
                nextGroup.EnablePieceColliders(false);
            }
            else
                Debug.Log("Setup failed, Fill in Next Group");
        }

        public void OnRepairedPiece(RepairablePiece brokenPiece)
        {
            if(brokenPieces.Contains(brokenPiece))
            {
                brokenPieces.Remove(brokenPiece);

                //If there is a next group
                if(nextGroup != null && brokenPieces.Count == 0)
                {
                    nextGroup.EnablePieceColliders(true);
                }
            }
        }

        public void EnablePieceColliders(bool enable = true)
        {
            foreach (var brokenPiece in brokenPieces)
            {
                brokenPiece.pieceCollider.enabled = enable;
            }
        }
    }
}