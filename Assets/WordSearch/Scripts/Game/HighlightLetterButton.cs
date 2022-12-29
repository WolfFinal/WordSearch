using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class HighlightLetterButton : ClickableListItem
	{
		#region Inspector Variables

		[SerializeField] private Text letterText = null;

		#endregion

		#region Public Methods

		public void Setup(char letter)
		{
			letterText.text = letter.ToString();
		}

		#endregion
	}
}
