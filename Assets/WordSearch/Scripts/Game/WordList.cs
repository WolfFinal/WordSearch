using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBG.WordSearch
{
	public class WordList : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private RectTransform	wordListContainer	= null;
		[SerializeField] private WordListItem	wordListItemPrefab	= null;
		[SerializeField] private CanvasGroup	wordListCanvasGroup	= null;


		#endregion

		#region Member Variables

		private ObjectPool							wordListItemPool;
		private Dictionary<string, WordListItem>	wordListItems;

		#endregion

		#region Public Methods

		public void Initialize()
		{
			wordListItemPool	= new ObjectPool(wordListItemPrefab.gameObject, 10, wordListContainer);
			wordListItems		= new Dictionary<string, WordListItem>();
		}

		public void Setup(Board board)
		{
			Clear();

			// Add all the words to the word list container
			for (int i = 0; i < board.words.Count; i++)
			{
				CreateWordListItem(board.words[i], wordListItemPool);
			}

			UIAnimation anim = UIAnimation.Alpha(wordListCanvasGroup, 0f, 1f, 0.5f);
			anim.style = UIAnimation.Style.EaseOut;
			anim.Play();
		}

		public void SetWordFound(string word)
		{
			if (wordListItems.ContainsKey(word))
			{
				wordListItems[word].SetWordFound();
			}
			else
			{
				Debug.LogError("[WordList] Word does not exist in the word list: " + word);
			}
		}

		public void Clear()
		{
			wordListItemPool.ReturnAllObjectsToPool();
			wordListItems.Clear();

			wordListContainer.sizeDelta	= new Vector2(wordListContainer.sizeDelta.x, 0f);
			wordListCanvasGroup.alpha	= 0f;
		}

		#endregion

		#region Private Methods

		private WordListItem CreateWordListItem(string word, ObjectPool itemPool)
		{
			WordListItem wordListItem = null;

			if (!wordListItems.ContainsKey(word))
			{
				wordListItem = itemPool.GetObject<WordListItem>();

				wordListItem.Setup(word);

				wordListItems.Add(word, wordListItem);
			}
			else
			{
				Debug.LogWarning("[WordList] Board contains duplicate words. Word: " + word);
			}

			return wordListItem;
		}

		#endregion
	}
}
