using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class TopBar : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private CanvasGroup	backButton				= null;
		[SerializeField] private CanvasGroup	mainScreenContainer		= null;
		[SerializeField] private CanvasGroup	categoryContainer		= null;
		[SerializeField] private Text			categoryNameText		= null;
		[SerializeField] private Text			levelNumberText			= null;
		[SerializeField] private Text			coinAmountText			= null;
		[SerializeField] private Text			keyAmountText			= null;

		#endregion

		#region Member Variables

		#endregion

		#region Properties

		#endregion

		#region Unity Methods

		private void Update()
		{
			coinAmountText.text		= "x " + GameManager.Instance.Coins.ToString();
			keyAmountText.text		= "x " + GameManager.Instance.Keys.ToString();
			levelNumberText.text	= "LEVEL " + (GameManager.Instance.ActiveLevelIndex + 1);

			ScreenManager.Instance.OnSwitchingScreens = OnSwitchingScreens;
		}

		#endregion

		#region Public Methods

		private void Start()
		{
			backButton.alpha = 0f;
		}

		#endregion

		#region Protected Methods

		#endregion

		#region Private Methods

		private void OnSwitchingScreens(string fromScreenId, string toScreenId)
		{
			if (fromScreenId == "main")
			{
				FadeIn(backButton);
				FadeOut(mainScreenContainer);
				FadeIn(categoryContainer);

				if (GameManager.Instance.ActiveCategoryInfo != null)
				{
					categoryNameText.text = GameManager.Instance.ActiveCategoryInfo.displayName;
				}
			}
			else if (toScreenId == "main")
			{
				FadeOut(backButton);
				FadeIn(mainScreenContainer);
				FadeOut(categoryContainer);
			}

			if (toScreenId == "game")
			{
				levelNumberText.gameObject.SetActive(GameManager.Instance.ActiveGameMode == GameManager.GameMode.Progress);
			}
			else if (fromScreenId == "game")
			{
				levelNumberText.gameObject.SetActive(false);
			}
		}

		private void FadeIn(CanvasGroup canvasGroup)
		{
			UIAnimation anim = UIAnimation.Alpha(canvasGroup, 0f, 1f, 0.5f);
			anim.style = UIAnimation.Style.EaseOut;
			anim.Play();
		}

		private void FadeOut(CanvasGroup canvasGroup)
		{
			UIAnimation anim = UIAnimation.Alpha(canvasGroup, 1f, 0f, 0.5f);
			anim.style = UIAnimation.Style.EaseOut;
			anim.Play();
		}

		#endregion
	}
}
