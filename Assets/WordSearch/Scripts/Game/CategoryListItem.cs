using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if BBG_MT_IAP
using BBG.MobileTools;
#endif

namespace BBG.WordSearch
{
	public class CategoryListItem : RecyclableListItem<CategoryInfo>
	{
		#region Inspector Variables

		[SerializeField] private Text			nameText				= null;
		[SerializeField] private Image			iconImage				= null;
		[SerializeField] private Image			backgroundImage			= null;
		[SerializeField] private ProgressBar	levelProgressBar		= null;
		[SerializeField] private Text			levelProgressText		= null;

		[Space]

		[SerializeField] private GameObject		progressBarContainer	= null;
		[SerializeField] private GameObject		lockedContainer			= null;
		[SerializeField] private GameObject		coinsUnlockContainer	= null;
		[SerializeField] private GameObject		keysUnlockContainer		= null;
		[SerializeField] private GameObject		iapUnlockContainer		= null;

		[Space]

		[SerializeField] private Text			coinsUnlockAmountText	= null;
		[SerializeField] private Text			keysUnlockAmountText	= null;
		[SerializeField] private Text			iapUnlockPriceText		= null;

		#endregion

		#region Member Variables

		private const float animDuration	= 400f;
		private const float animStartY		= -150f;
		private const float animStartScale	= 0.8f;

		#if BBG_MT_IAP
		private string	lockedProductId;
		#endif

		#endregion

		#region Public Methods

		public override void Initialize(CategoryInfo categoryInfo)
		{

		}

		public override void Setup(CategoryInfo categoryInfo)
		{
			nameText.text			= categoryInfo.displayName;
			iconImage.sprite		= categoryInfo.icon;
			backgroundImage.color	= categoryInfo.categoryColor;

			SetProgress(categoryInfo);

			SetLocked(categoryInfo);
		}

		public override void Removed()
		{
			#if BBG_MT_IAP
			IAPManager.Instance.OnIAPInitialized -= OnIAPInitialized;
			#endif
		}

		#endregion

		#region Private Methods

		private void SetProgress(CategoryInfo categoryInfo)
		{
			int totalLevels			= categoryInfo.levelFiles.Count;
			int numLevelsCompleted	= GameManager.Instance.LastCompletedLevels.ContainsKey(categoryInfo.saveId) ? GameManager.Instance.LastCompletedLevels[categoryInfo.saveId] + 1 : 0;

			levelProgressBar.SetProgress((float)numLevelsCompleted / (float)totalLevels);

			levelProgressText.text = string.Format("{0} / {1}", numLevelsCompleted, totalLevels);
		}

		private void SetLocked(CategoryInfo categoryInfo)
		{
			bool isCategoryLocked = GameManager.Instance.IsCategoryLocked(categoryInfo);

			progressBarContainer.SetActive(!isCategoryLocked);
			lockedContainer.SetActive(isCategoryLocked);
			coinsUnlockContainer.SetActive(isCategoryLocked && categoryInfo.lockType == CategoryInfo.LockType.Coins);
			keysUnlockContainer.SetActive(isCategoryLocked && categoryInfo.lockType == CategoryInfo.LockType.Keys);
			iapUnlockContainer.SetActive(isCategoryLocked && categoryInfo.lockType == CategoryInfo.LockType.IAP);

			#if BBG_MT_IAP
			IAPManager.Instance.OnIAPInitialized -= OnIAPInitialized;
			#endif

			switch (categoryInfo.lockType)
			{
				case CategoryInfo.LockType.Coins:
					coinsUnlockAmountText.text = "x " + categoryInfo.unlockAmount;
					break;
				case CategoryInfo.LockType.Keys:
					keysUnlockAmountText.text = "x " + categoryInfo.unlockAmount;
					break;
				case CategoryInfo.LockType.IAP:
					SetIAPPrice(categoryInfo.iapProductId);
					break;
			}
		}

		private void SetIAPPrice(string productId)
		{
			#if BBG_MT_IAP
			if (IAPManager.Instance.IsInitialized)
			{
				UnityEngine.Purchasing.Product product = IAPManager.Instance.GetProductInformation(productId);

				if (product == null)
				{
					iapUnlockPriceText.text = "Does Not Exist";
				}
				else
				{
					iapUnlockPriceText.text = product.metadata.localizedPriceString;
				}
			}
			else
			{
				lockedProductId							= productId;
				IAPManager.Instance.OnIAPInitialized	+= OnIAPInitialized;
			}
			#else
			iapUnlockPriceText.text = "IAP Not Enabled";
			#endif
		}

		private void OnIAPInitialized(bool success)
		{
			#if BBG_MT_IAP
			IAPManager.Instance.OnIAPInitialized -= OnIAPInitialized;

			if (success)
			{
				SetIAPPrice(lockedProductId);
			}
			else
			{
				iapUnlockPriceText.text = "Failed To Initialize";
			}
			#endif
		}

		#endregion
	}
}
