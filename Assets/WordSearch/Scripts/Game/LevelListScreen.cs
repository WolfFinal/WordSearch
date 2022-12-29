using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class LevelListScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private LevelListItem	levelListItemPrefab	= null;
		[SerializeField] private RectTransform	levelListContainer	= null;
		[SerializeField] private ScrollRect		levelListScrollRect	= null;

		#endregion

		#region Member Variables

		private RecyclableListHandler<int> levelListHandler;

		#endregion

		#region Public Methods

		public override void Show(bool back, bool immediate)
		{
			base.Show(back, immediate);

			if (back)
			{
				levelListHandler.Refresh();
			}
			else
			{
				List<int> levelIndicies = new List<int>();

				for (int i = 0; i < GameManager.Instance.ActiveCategoryInfo.levelFiles.Count; i++)
				{
					levelIndicies.Add(i);
				}

				if (levelListHandler == null)
				{
					levelListHandler					= new RecyclableListHandler<int>(levelIndicies, levelListItemPrefab, levelListContainer, levelListScrollRect);
					levelListHandler.OnListItemClicked	= OnLevelListItemClicked;
					levelListHandler.Setup();
				}
				else
				{
					levelListHandler.UpdateDataObjects(levelIndicies);
				}
			}
		}

		#endregion

		#region Private Methods

		private void OnLevelListItemClicked(int levelIndex)
		{
			if (!GameManager.Instance.IsLevelLocked(GameManager.Instance.ActiveCategoryInfo, levelIndex))
			{
				GameManager.Instance.StartLevel(GameManager.Instance.ActiveCategoryInfo, levelIndex);
			}
		}

		#endregion
	}
}
