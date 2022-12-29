using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class SelectedWord : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private Text		selectedWordText		= null;
		[SerializeField] private GameObject	selectedWordContainer	= null;
		[SerializeField] private Image		selectedWordBkgImage	= null;

		#endregion

		#region Public Methods

		public void SetSelectedWord(string word, Color color)
		{
			selectedWordText.text = word;
			selectedWordContainer.SetActive(true);

			selectedWordBkgImage.color = color;
		}

		public void Clear()
		{
			selectedWordText.text = "";
			selectedWordContainer.SetActive(false);
		}

		#endregion
	}
}
