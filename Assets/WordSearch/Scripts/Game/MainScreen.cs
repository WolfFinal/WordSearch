using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BBG.WordSearch
{
	public class MainScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private CategoryListItem	categoryListItemPrefab	= null;
		[SerializeField] private RectTransform		categoryListContainer	= null;
		[SerializeField] private ScrollRect			categoryListScrollRect	= null;

		#endregion

		#region Member Variables

		private RecyclableListHandler<CategoryInfo>	categoryListHandler;
		private CategoryInfo						selectedCategory;

		#endregion

		#region Public Methods

		public override void Show(bool back, bool immediate)
		{
			base.Show(back, immediate);

			UpdateUI();

			#if BBG_MT_IAP
			BBG.MobileTools.IAPManager.Instance.OnProductPurchased += IAPProductPurchased;
			#endif
		}

		public override void Hide(bool back, bool immediate)
		{
			base.Hide(back, immediate);

			#if BBG_MT_IAP
			BBG.MobileTools.IAPManager.Instance.OnProductPurchased -= IAPProductPurchased;
			#endif
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the UI by clearing the screen and setting up all categories again
		/// </summary>
		private void UpdateUI()
		{
			if (categoryListHandler == null)
			{
				categoryListHandler						= new RecyclableListHandler<CategoryInfo>(GameManager.Instance.CategoryInfos, categoryListItemPrefab, categoryListContainer, categoryListScrollRect);
				categoryListHandler.OnListItemClicked	= OnCategoryListItemClicked;
				categoryListHandler.Setup();
			}
			else
			{
				categoryListHandler.Refresh();
			}
		}

		/// <summary>
		/// Invoked when a category list item is clicked
		/// </summary>
		private void OnCategoryListItemClicked(CategoryInfo categoryInfo)
		{
			selectedCategory = categoryInfo;

			// Check if the category is locked
			if (GameManager.Instance.IsCategoryLocked(selectedCategory))
			{
				switch (selectedCategory.lockType)
				{
					case CategoryInfo.LockType.Coins:
					case CategoryInfo.LockType.Keys:
						// Show the unlock popup
						ShowUnlockCategoryPopup(selectedCategory);
						break;
					case CategoryInfo.LockType.IAP:
						// Start the buy product flow
						#if BBG_MT_IAP
						BBG.MobileTools.IAPManager.Instance.BuyProduct(selectedCategory.iapProductId);
						#endif
						break;
				}
			}
			else
			{
				object[] popupData =
				{
					selectedCategory,
					GameManager.Instance.HasSavedCasualBoard(selectedCategory),
					GameManager.Instance.AllLevelsComplete(selectedCategory)
				};

				// Show the category selected popup so the player can choose casual vs level mode
				PopupManager.Instance.Show("category_selected", popupData, OnCategorySelectedPopupClosed);
			}
		}

		private void OnCategorySelectedPopupClosed(bool cancelled, object[] outData)
		{
			if (!cancelled)
			{
				string action = outData[0] as string;

				// Check what action the player selected
				switch (action)
				{
					case "casual_newgame":
						GameManager.Instance.StartCasual(selectedCategory, (int)outData[1]);
						break;
					case "casual_continue":
						GameManager.Instance.ContinueCasual(selectedCategory);
						break;
					case "progress_play":
						GameManager.Instance.StartNextLevel(selectedCategory);
						break;
					case "progress_levels":
						GameManager.Instance.SetActiveCategory(selectedCategory);
						ScreenManager.Instance.Show("levels");
						break;
				}
			}
		}

		private void ShowUnlockCategoryPopup(CategoryInfo categoryInfo)
		{
			PopupManager.Instance.Show("unlock_category", new object[] { categoryInfo }, OnUnlockCategoryPopupClosed);
		}

		private void OnUnlockCategoryPopupClosed(bool cancelled, object[] outData)
		{
			if (!cancelled)
			{
				if (GameManager.Instance.UnlockCategory(selectedCategory))
				{
					UpdateUI();
				}
			}
		}

		private void IAPProductPurchased(string productId)
		{
			UpdateUI();
		}

		#endregion
	}
}