using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
	public class HighlightLetterPopup : Popup
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private HighlightLetterButton	letterButtonPrefab		= null;
		[SerializeField] private Transform				letterButtonContainer	= null;
		[SerializeField] private GameObject				noLettersToShow			= null;

		#endregion

		#region Member Variables

		private ObjectPool highlightLetterButtonPool;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			base.Initialize();

			highlightLetterButtonPool = new ObjectPool(letterButtonPrefab.gameObject, 1, letterButtonContainer);
		}

		public override void OnShowing(object[] inData)
		{
			base.OnShowing(inData);

			// Return all HighlightLetterButtons back to the pool
			highlightLetterButtonPool.ReturnAllObjectsToPool();

			Board board = inData[0] as Board;

			List<char>		letters			= new List<char>();
			HashSet<char>	lettersInList	= new HashSet<char>();

			// Get all the letters that appear on the board and have not already been shown using a highlight letter hint
			for (int row = 0; row < board.rows; row++)
			{
				for (int col = 0; col < board.cols; col++)
				{
					char letter = board.boardCharacters[row][col];

					if (!lettersInList.Contains(letter) && !board.letterHintsUsed.Contains(letter))
					{
						letters.Add(letter);
						lettersInList.Add(letter);
					}
				}
			}

			// Check if there are any letters we can actually show
			if (letters.Count == 0)
			{
				noLettersToShow.SetActive(true);
			}
			else
			{
				noLettersToShow.SetActive(false);

				// Sort the letters for 'A' to 'Z'
				letters.Sort();

				// Add a HighlightLetterButton for each letter in the list
				for (int i = 0; i < letters.Count; i++)
				{
					char letter = letters[i];

					HighlightLetterButton highlightLetterButton = highlightLetterButtonPool.GetObject<HighlightLetterButton>();

					highlightLetterButton.Setup(letter);

					highlightLetterButton.Data				= letter;
					highlightLetterButton.OnListItemClicked	= OnLetterButtonSelected;
				}
			}
		}

		#endregion

		#region Private Methods

		private void OnLetterButtonSelected(int index, object letter)
		{
			Hide(false, new object[] { letter });
		}

		#endregion
	}
}
