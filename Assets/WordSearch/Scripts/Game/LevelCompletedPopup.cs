using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class LevelCompletedPopup : Popup
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private GameObject playAgainButton			= null;
		[SerializeField] private GameObject nextLevelButton			= null;

		[Space]

		[SerializeField] private GameObject rewardsContainer		= null;
		[SerializeField] private GameObject coinRewardContainer		= null;
		[SerializeField] private GameObject keyRewardContainer		= null;
		[SerializeField] private GameObject ADSReward				= null;
		[SerializeField] private Text		coinRewardAmountText	= null;
		[SerializeField] private Text		keyRewardAmountText		= null;

		#endregion

		#region Public Methods

		public override void OnShowing(object[] inData)
		{
			base.OnShowing(inData);

			bool	progressLevelCompleted	= (bool)inData[0];
			int		coinsAwarded			= (int)inData[1];
			int		keyAwarded				= (int)inData[2];
			bool	lastLevel				= (bool)inData[3];

			playAgainButton.SetActive(!progressLevelCompleted);
			nextLevelButton.SetActive(progressLevelCompleted && !lastLevel);
			ADSReward.SetActive(true);

			bool awardCoins	= coinsAwarded > 0;
			bool awardKeys	= keyAwarded > 0;

			rewardsContainer.SetActive(awardCoins || awardKeys);
			coinRewardContainer.SetActive(awardCoins);
			keyRewardContainer.SetActive(awardKeys);

			coinRewardAmountText.text	= "x " + coinsAwarded;
			keyRewardAmountText.text	= "x " + keyAwarded;
		}

		#endregion
	}
}
