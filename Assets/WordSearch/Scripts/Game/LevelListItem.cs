using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class LevelListItem : RecyclableListItem<int>
	{
		#region Inspector Variables

		[SerializeField] private Text	levelText		= null;
		[SerializeField] private Image	categoryIcon	= null;
		[SerializeField] private Image	completedIcon	= null;
		[SerializeField] private Image	lockedIcon		= null;
		[SerializeField] private Image	playIcon		= null;

		#endregion

		#region Public Methods

		public override void Initialize(int levelIndex)
		{
		}

		public override void Setup(int levelIndex)
		{
			HideAllIcons();

			levelText.text = "LEVEL " + (levelIndex + 1).ToString();

			CategoryInfo activeCategory = GameManager.Instance.ActiveCategoryInfo;

			// Make sure the active category is not null
			if (activeCategory != null)
			{
				categoryIcon.sprite	= activeCategory.icon;

				if (GameManager.Instance.IsLevelCompleted(activeCategory, levelIndex))
				{
					SetCompleted();
				}
			    else if (GameManager.Instance.IsLevelLocked(activeCategory, levelIndex))
				{
					SetLocked();
				}
				else
				{
					SetPlayable();
				}
			}
		}

		public override void Removed()
		{
		}

		#endregion

		#region Private Methods

		private void SetCompleted()
		{
			completedIcon.enabled = true;
		}

		private void SetLocked()
		{
			lockedIcon.enabled = true;
		}

		private void SetPlayable()
		{
			playIcon.enabled = true;
		}

		private void HideAllIcons()
		{
			completedIcon.enabled	= false;
			lockedIcon.enabled		= false;
			playIcon.enabled		= false;
		}

		#endregion
	}
}
