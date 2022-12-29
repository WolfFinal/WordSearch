using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BBG.WordSearch
{
	public class WordListItem : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private Text		wordText		= null;
		[SerializeField] private GameObject	foundIndicator	= null;

		#endregion

		#region Public Methods

		public void Setup(string word)
		{
			wordText.text = word;
			foundIndicator.SetActive(false);
		}

		public void SetWordFound()
		{
			foundIndicator.SetActive(true);
		}

		#endregion
	}
}
