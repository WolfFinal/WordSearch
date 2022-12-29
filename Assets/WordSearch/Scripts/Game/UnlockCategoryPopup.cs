using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class UnlockCategoryPopup : Popup
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private Text		categoryNameText		= null;
		[SerializeField] private Image		categoryIconImage		= null;
		[SerializeField] private GameObject	coinsUnlockContainer	= null;
		[SerializeField] private GameObject	keysUnlockContainer		= null;
		[SerializeField] private Text		unlockCoinAmountText	= null;
		[SerializeField] private Text		unlockKeyAmountText		= null;

		#endregion

		#region Public Methods

		public override void OnShowing(object[] inData)
		{
			base.OnShowing(inData);

			CategoryInfo categoryInfo = inData[0] as CategoryInfo;

			categoryNameText.text		= categoryInfo.displayName;
			categoryIconImage.sprite	= categoryInfo.icon;

			coinsUnlockContainer.SetActive(categoryInfo.lockType == CategoryInfo.LockType.Coins);
			keysUnlockContainer.SetActive(categoryInfo.lockType == CategoryInfo.LockType.Keys);

			unlockCoinAmountText.text	= categoryInfo.unlockAmount.ToString();
			unlockKeyAmountText.text	= categoryInfo.unlockAmount.ToString();
		}

		#endregion
	}
}
