using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ho
{
	public class ClickToSwapPiece : ClickToPiece
	{
		protected TimedVec3BounceLerp bounce = null;
		protected Vector3 originalPos;

		public string swapID;
		public Vector3 CorrectLocation => originalPos;

		ClickToRotatePiece ctrPiece;


        int baseSortOrder;
		public virtual void Swap(ClickToSwapPiece withPiece)
		{

			isActive = false;
			SetSelected(false);
			bounce = new TimedVec3BounceLerp(transform.position, withPiece.transform.position, new Vector3(0, 10, -1), 0.5f);
			this.ExecuteAfterDelay(0.5f, () => { SetHighlighted(false); });
		}
		public override bool IsAnimating() { return bounce != null; }

		public override bool IsCorrect()
		{
			return Vector3.Distance(originalPos, transform.position) < 2.0f;
		}


		public override void ForceComplete()
		{
			isComplete = true;
			transform.position = originalPos;
			bounce = null;
		}

		public void AnimateMove(Vector3 movePosition)
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", movePosition, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
			//transform.position = originalPos;
			bounce = null;
		}

		public void AnimateComplete()
		{
			isComplete = true;
			iTween.MoveTo(gameObject, iTween.Hash("position", originalPos, "time", 0.25f, "easetype", iTween.EaseType.easeOutQuart));
			//transform.position = originalPos;
			bounce = null;
		}

		public override void SetupPiece()
		{
			base.SetupPiece();
			originalPos = transform.position;
			baseSortOrder = sprite.sortingOrder;

			if (sdfRenderer)
				sdfRenderer.material = MinigameController.instance.jigsawPieceBorderSDFMaterial;
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
					if (owner.LockOnComplete && IsCorrect())
					{
						if(owner.mgType == ClickToMG.MGType.SwapRotate)
                        {
							if(ctrPiece == null)
								ctrPiece = GetComponent<ClickToRotatePiece>();

							if (ctrPiece.IsCorrect())
							{
								//CTS & CTR Piece are both correct Lock on complete
								isComplete = true;
								ctrPiece.ForceComplete();
							}
						}
						else
                        {
							isComplete = true;
						}
					}
				}
			}
			if (owner.disableInput)
				SetHighlighted(false);
		}

		public void SetHighlighted(bool b)
		{
			if (b)
				sprite.sortingOrder = baseSortOrder + 1000;
			else
				sprite.sortingOrder = baseSortOrder;

			sdfRenderer.gameObject.SetActive(b);
			sdfRenderer.color = Color.white;

			sdfRenderer.sortingOrder = sprite.sortingOrder;
		}

		public void Skip()
		{
			isActive = false;
			SetSelected(false);

			bounce = new TimedVec3BounceLerp(transform.position, originalPos, new Vector3(0, 10, -1), 0.5f);
			this.ExecuteAfterDelay(0.5f, () => { SetHighlighted(false); });
		}

		public override void OnClick()
		{
			if (IsComplete) return;
			if (bounce != null) return; // in motion

			if (owner.SelectedPiece == this)
			{
				owner.SelectedPiece = null;
				isActive = false;
				SetHighlighted(false);

				return;
			}

			Audio.instance.PlaySound(MinigameController.instance.onPieceSelected.GetClip(null));

			if (owner.SelectedPiece == null)
			{
				owner.SelectedPiece = this;
				isActive = true;
				SetHighlighted(true);
			}
			else
			{
				// let's make sure somehow someone hasn't manually added the wrong pieces
				ClickToSwapPiece other = owner.SelectedPiece as ClickToSwapPiece;
				// swap the two of them
				if (other && other.swapID.Equals(this.swapID))
				{
					SetHighlighted(true);
					other.Swap(this);
					Swap(other);
					owner.SelectedPiece = null;

					Audio.instance.PlaySound(MinigameController.instance.onPieceSwap.GetClip(null));
				}
				else
				{
					//Deselect Current Selected Piece
					ClickToSwapPiece currentPiece = owner.SelectedPiece as ClickToSwapPiece;
					currentPiece.SetHighlighted(false);
					currentPiece.isActive = false;
					owner.SelectedPiece = null;
				}
			}
		}
	}

}