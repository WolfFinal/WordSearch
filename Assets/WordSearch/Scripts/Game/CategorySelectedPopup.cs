using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class CategorySelectedPopup : Popup
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private Text			categoryNameText			= null;
		[SerializeField] private Image			categoryIconImage			= null;
		[SerializeField] private CanvasGroup	selectModeContainer			= null;
		[SerializeField] private CanvasGroup	selectDifficultyContainer	= null;

		[Space]

		[SerializeField] private Text			casualPlayButtonText		= null;
		[SerializeField] private CanvasGroup	casualContinueButton		= null;
		[SerializeField] private CanvasGroup	progressPlayButton			= null;
		[SerializeField] private Text			progressMessageText			= null;

		#endregion

		#region Public Methods

		public override void OnShowing(object[] inData)
		{
			base.OnShowing(inData);

			CategoryInfo	categoryInfo		= inData[0] as CategoryInfo;
			bool			casualHasProgress	= (bool)inData[1];
			bool			allLevelsCompleted	= (bool)inData[2];

			categoryNameText.text		= categoryInfo.displayName;
			categoryIconImage.sprite	= categoryInfo.icon;

			selectModeContainer.alpha			= 1f;
			selectModeContainer.blocksRaycasts	= true;
			selectModeContainer.interactable	= true;

			selectDifficultyContainer.alpha				= 0f;
			selectDifficultyContainer.blocksRaycasts	= false;
			selectDifficultyContainer.interactable		= false;

			casualPlayButtonText.text			= casualHasProgress ? "NEW GAME" : "PLAY";
			casualContinueButton.interactable	= casualHasProgress;
			casualContinueButton.alpha			= casualHasProgress ? 1f : 0.3f;

			progressPlayButton.interactable		= !allLevelsCompleted;
			progressPlayButton.alpha			= !allLevelsCompleted ? 1f : 0.3f;

			if (allLevelsCompleted)
			{
				progressMessageText.text = "All levels completed!";
			}
		}

		public void OnCasualSelected()
		{
			UIAnimation anim;

			selectModeContainer.interactable	= false;
			selectModeContainer.blocksRaycasts	= false;

			anim		= UIAnimation.Alpha(selectModeContainer, 1f, 0f, 0.35f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.Play();
			
			selectDifficultyContainer.interactable		= true;
			selectDifficultyContainer.blocksRaycasts	= true;

			anim		= UIAnimation.Alpha(selectDifficultyContainer, 0f, 1f, 0.35f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.Play();
		}

		public void OnDifficultySelected(int difficultyIndex)
		{
			Hide(false, new object[] { "casual_newgame", difficultyIndex });
		}

		#endregion
	}
}
