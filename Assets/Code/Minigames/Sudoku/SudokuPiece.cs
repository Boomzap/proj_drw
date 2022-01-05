using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace ho
{
    public class SudokuPiece : ClickToSwapPiece
    {
        public SudokuMG sudokuBoard;
        public int boardIndex;
        public string groupId;

        public int row => sudokuBoard? (boardIndex - 1) / sudokuBoard.gridSize : 0;
        public int col => sudokuBoard? (boardIndex - 1) % sudokuBoard.gridSize : 0;

        public bool isInCorrectRow = false;

        public bool isInCorrectColumn = false;

        [ReadOnly]
        public Vector3 origLocalPosition;

        public void SetupPiece(int index)
        {
            sprite = GetComponent<SpriteRenderer>();
            sortValue = sprite.sortingOrder;
            polyCollider = GetComponent<PolygonCollider2D>();
            defaultMaterial = sprite.sharedMaterial ? sprite.sharedMaterial : sprite.material;
            sudokuBoard = GetComponentInParent<SudokuMG>();

            boardIndex = index;

            groupId = gameObject.name.Split('_').Last();
            origLocalPosition = transform.localPosition;

            if (sdfRenderer)
                sdfRenderer.material = MinigameController.instance.jigsawPieceBorderSDFMaterial;
        }

        public void Swap(SudokuPiece withPiece)
        {
            isActive = false;
            SetSelected(false);
            bounce = new TimedVec3BounceLerp(transform.position, withPiece.transform.position, new Vector3(0, 10, -1), 0.5f);
            this.ExecuteAfterDelay(0.5f, () => 
            {
                sudokuBoard.CheckMGComplete();
                SetHighlighted(false); 
            });
        }

        public override void OnClick()
        {
			if (bounce != null) return; // in motion

			if (sudokuBoard.selectedPiece == this)
			{
				sudokuBoard.selectedPiece = null;
				isActive = false;
				SetHighlighted(false);

				return;
			}

			Audio.instance.PlaySound(MinigameController.instance.onPieceSelected.GetClip(null));

			if (sudokuBoard.selectedPiece == null)
			{
				sudokuBoard.selectedPiece = this;
				isActive = true;
				SetHighlighted(true);
			}
			else
			{
				// let's make sure somehow someone hasn't manually added the wrong pieces
				SudokuPiece other = sudokuBoard.selectedPiece as SudokuPiece;
				// swap the two of them
				if (other && other.swapID.Equals(this.swapID))
				{
					SetHighlighted(true);

                    //Swap Board Index for checking groups
                    int swapIndex = other.boardIndex;
                    other.boardIndex = boardIndex;
                    boardIndex = swapIndex;

                    other.Swap(this);
					Swap(other);


					sudokuBoard.selectedPiece = null;

					Audio.instance.PlaySound(MinigameController.instance.onPieceSwap.GetClip(null));
				}
				else
				{
					//Deselect Current Selected Piece
					SudokuPiece currentPiece = sudokuBoard.selectedPiece as SudokuPiece;
					currentPiece.SetHighlighted(false);
					currentPiece.isActive = false;
					owner.SelectedPiece = null;
				}
			}
		}
        void Update()
        {
            if (finalAnim) return;
            if (sprite == null) return;
            if (isActive || IsAnimating())
            {
                sprite.sortingOrder = 1000 + sortValue;
            }
            else
            {
                sprite.sortingOrder = sortValue;
            }

            //if (isActive)
            //{
            //    float z = transform.position.z;
            //    if (z > ActiveHeight)
            //    {
            //        z += ActiveHeight * Time.deltaTime * sudokuBoard.selectedHeightSpeed;
            //        if (z < ActiveHeight) z = ActiveHeight;
            //        transform.position = new Vector3(transform.position.x, transform.position.y, z);
            //    }
            //}
            //else
            //{
            //    float z = transform.position.z;
            //    if (z < 0)
            //    {
            //        z -= Time.deltaTime * sudokuBoard.selectedHeightSpeed;
            //        if (z < 0) z = 0;
            //        transform.position = new Vector3(transform.position.x, transform.position.y, z);
            //    }
            //}

            UpdatePiece();

            //if (isFixedPiece == false)
            //    sprite.color = IsComplete ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
        }

        public override bool IsCorrect()
        {
            return isInCorrectColumn && isInCorrectRow;
        }

        protected override void UpdatePiece()
		{
			if (bounce != null)
			{
				bounce.Update();
				transform.position = bounce.Value;
				if (bounce.IsDone)
				{
					bounce = null;
				}
			}
			if (sudokuBoard.disableInput)
				SetHighlighted(false);
		}
	}

}
