using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace BBG.WordSearch
{
	public class CharacterGrid : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		#region Enums

		private enum HighlighPosition
		{
			AboveLetters,
			BelowLetters
		}

		#endregion

		#region Inspector Variables

		[SerializeField] private float				maxCellSize				= 0;
		[SerializeField] private SelectedWord		selectedWord			= null;

		[Header("Letter Settings")]
		[SerializeField] private Font 				letterFont				= null;
		[SerializeField] private int 				letterFontSize			= 0;
		[SerializeField] private Color				letterColor				= Color.white;
		[SerializeField] private Color				letterHighlightedColor	= Color.white;
		[SerializeField] private Vector2			letterOffsetInCell		= Vector2.zero;

		[Header("Highlight Settings")]
		[SerializeField] private HighlighPosition	highlightPosition		= HighlighPosition.AboveLetters;
		[SerializeField] private Sprite				highlightSprite			= null;
		[SerializeField] private float				highlightExtraSize		= 0f;
		[SerializeField] private List<Color>		highlightColors 		= null;

		[Header("Highlight Letter Settings")]
		[SerializeField] private Sprite				highlightLetterSprite	= null;
		[SerializeField] private float				highlightLetterSize		= 0f;
		[SerializeField] private Color				highlightLetterColor	= Color.white;


		#endregion

		#region Member Variables

		private Board currentBoard;

		private RectTransform					gridContainer;
		private RectTransform					gridOverlayContainer;
		private RectTransform					gridUnderlayContainer;
		private RectTransform					highlighLetterContainer;
		private ObjectPool						characterPool;
		private ObjectPool						highlightLetterPool;
		private List<List<CharacterGridItem>>	characterItems;
		private List<Image>						highlights;

		private float currentScale;
		private float currentCellSize;

		// Used when the player is selecting a word
		private Image				selectingHighlight;
		private bool				isSelecting;
		private int					selectingPointerId;
		private CharacterGridItem	startCharacter;
		private CharacterGridItem	lastEndCharacter;

		#endregion

		#region Properties

		private float	ScaledHighlighExtraSize 	{ get { return highlightExtraSize * currentScale; } }
		private Vector2	ScaledLetterOffsetInCell	{ get { return letterOffsetInCell * currentScale; } }
		private float	ScaledHightlightLetterSize	{ get { return highlightLetterSize * currentScale; } }
		private float	CellFullWidth				{ get { return currentCellSize; } }
		private float	CellFullHeight				{ get { return currentCellSize; } }

		#endregion

		#region Unity Methods

		public void OnPointerDown(PointerEventData eventData)
		{
			if (selectingPointerId != -1)
			{
				// There is already a mouse/pointer highlighting words 
				return;
			}

			if (GameManager.Instance.ActiveGameState == GameManager.GameState.BoardActive)
			{
				// Get the closest word to select
				CharacterGridItem characterItem = GetCharacterItemAtPosition(eventData.position);

				if (characterItem != null)
				{
					// Start selecting
					isSelecting			= true;
					selectingPointerId	= eventData.pointerId;
					startCharacter		= characterItem;
					lastEndCharacter	= characterItem;

					AssignHighlighColor(selectingHighlight);
					selectingHighlight.gameObject.SetActive(true);

					UpdateSelectingHighlight(eventData.position);
					UpdateSelectedWord();

					SoundManager.Instance.Play("highlight");
				}
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.pointerId != selectingPointerId)
			{
				return;
			}

			if (GameManager.Instance.ActiveGameState == GameManager.GameState.BoardActive)
			{
				UpdateSelectingHighlight(eventData.position);
				UpdateSelectedWord();
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId != selectingPointerId)
			{
				return;
			}

			if (startCharacter != null && lastEndCharacter != null && GameManager.Instance.ActiveGameState == GameManager.GameState.BoardActive)
			{
				// Set the text color back to the normal color, if the selected word is actually a word then the HighlightWord will set the color back to the highlighted color
				SetTextColor(startCharacter, lastEndCharacter, letterColor, false);

				// Get the start and end row/col position for the word
				Cell wordStartPosition	= new Cell(startCharacter.Row, startCharacter.Col);
				Cell wordEndPosition	= new Cell(lastEndCharacter.Row, lastEndCharacter.Col);

				string highlightedWord = GetWord(wordStartPosition, wordEndPosition);

				// Call OnWordSelected to notify the WordSearchController that a word has been selected
				string foundWord = GameManager.Instance.OnWordSelected(highlightedWord);

				// If the word was a word that was suppose to be found then highligh the word and create the floating text
				if (!string.IsNullOrEmpty(foundWord))
				{
					ShowWord(wordStartPosition, wordEndPosition, foundWord, true);

					SoundManager.Instance.Play("word-found");
				}
			}

			// End selecting and hide the select highlight
			isSelecting			= false;
			selectingPointerId	= -1;
			startCharacter		= null;
			lastEndCharacter	= null;
			selectingHighlight.gameObject.SetActive(false);
			selectedWord.Clear();
		}

		#endregion

		#region Public Methods

		public void Initialize()
		{
			// In order for the IPointer/IDrag handlers to work properly we need to put a graphic component this gameobject
			if (gameObject.GetComponent<Graphic>() == null)
			{
				Image image = gameObject.AddComponent<Image>();
				image.color = Color.clear;
			}

			// Create a GameObject to hold all the letters, set it as a child of CharacterGrid and set its anchors to expand to fill
			gridContainer = CreateContainer("grid_container", typeof(RectTransform), typeof(GridLayoutGroup), typeof(CanvasGroup));

			// Create a GameObject that will be be used to place things overtop of the letter grid
			gridOverlayContainer = CreateContainer("grid_overlay_container", typeof(RectTransform));

			// Only need an underlay container if the higlighs position is set to under the letters
			if (highlightPosition == HighlighPosition.BelowLetters)
			{
				// Create a GameObject that will be be used to place things under the letter grid
				gridUnderlayContainer = CreateContainer("grid_underlay_container", typeof(RectTransform));

				gridUnderlayContainer.SetAsFirstSibling();
			}

			// Create a container that will hold the letter images whena highlight letter hint is used
			highlighLetterContainer = CreateContainer("highligh_letter_container", typeof(RectTransform));

			// Create a CharacterGridItem that will be used as a template by the ObjectPool to create more instance
			CharacterGridItem templateCharacterGridItem = CreateCharacterGridItem();
			templateCharacterGridItem.name = "template_character_grid_item";
			templateCharacterGridItem.gameObject.SetActive(false);
			templateCharacterGridItem.transform.SetParent(transform, false);

			GameObject characterPoolContainer = new GameObject("character_pool");
			characterPoolContainer.transform.SetParent(transform);
			characterPoolContainer.SetActive(false);

			// Create a highlight letter image that will be used as a template by the ObjectPool to create more instance
			Image templateHighlightLetterImage = CreateHighlightLetterImage();
			templateCharacterGridItem.name = "template_highlight_letter_image";
			templateCharacterGridItem.gameObject.SetActive(false);
			templateCharacterGridItem.transform.SetParent(transform, false);

			characterPool		= new ObjectPool(templateCharacterGridItem.gameObject, 25, characterPoolContainer.transform);
			highlightLetterPool	= new ObjectPool(templateHighlightLetterImage.gameObject, 1, highlighLetterContainer);
			characterItems		= new List<List<CharacterGridItem>>();
			highlights			= new List<Image>();

			// Instantiate an instance of the highlight to use when the player is selecting a word
			selectingHighlight = CreateNewHighlight();
			selectingHighlight.gameObject.SetActive(false);

			selectingPointerId	= -1;
		}

		public void Setup(Board board)
		{
			Clear();

			// We want to scale the CharacterItem so that the UI Text changes size
			currentCellSize	= SetupGridContainer(board.rows, board.cols);
			currentScale	= currentCellSize / maxCellSize;

			for (int i = 0; i < board.boardCharacters.Count; i++)
			{
				characterItems.Add(new List<CharacterGridItem>());

				for (int j = 0; j < board.boardCharacters[i].Count; j++)
				{
					// Get a new character from the object pool
					CharacterGridItem characterItem = characterPool.GetObject().GetComponent<CharacterGridItem>();

					characterItem.Row			= i;
					characterItem.Col			= j;
					characterItem.IsHighlighted	= false;

					characterItem.gameObject.SetActive(true);
					characterItem.transform.SetParent(gridContainer, false);

					characterItem.characterText.text					= board.boardCharacters[i][j].ToString();
					characterItem.characterText.color					= letterColor;
					characterItem.characterText.transform.localScale	= new Vector3(currentScale, currentScale, 1f);

					(characterItem.characterText.transform as RectTransform).anchoredPosition = ScaledLetterOffsetInCell;

					characterItems[i].Add(characterItem);
				}
			}

			currentBoard = board;

			UIAnimation anim = UIAnimation.Alpha(gridContainer.GetComponent<CanvasGroup>(), 0f, 1f, .5f);
			anim.style = UIAnimation.Style.EaseOut;
			anim.Play();
		}

		public Image HighlightWord(Cell start, Cell end, bool useSelectedColour)
		{
			Image highlight = CreateNewHighlight();

			highlights.Add(highlight);

			CharacterGridItem startCharacterItem	= characterItems[start.row][start.col];
			CharacterGridItem endCharacterItem		= characterItems[end.row][end.col];

			// Position the highlight over the letters
			PositionHighlight(highlight, startCharacterItem, endCharacterItem);

			// Set the text color of the letters to the highlighted color
			SetTextColor(startCharacterItem, endCharacterItem, letterHighlightedColor, true);

			if (useSelectedColour && selectingHighlight != null)
			{
				highlight.color = selectingHighlight.color;
			}

			return highlight;
		}

		public void SetWordFound(string word)
		{
			if (currentBoard == null)
			{
				return;
			}

			for (int i = 0; i < currentBoard.wordPlacements.Count; i++)
			{
				Board.WordPlacement wordPlacement = currentBoard.wordPlacements[i];

				if (word == wordPlacement.word)
				{
					Cell startPosition	= wordPlacement.startingPosition;
					Cell endPosition	= new Cell(startPosition.row + wordPlacement.verticalDirection * (word.Length - 1), startPosition.col + wordPlacement.horizontalDirection * (word.Length - 1));

					HighlightWord(startPosition, endPosition, false);

					break;
				}
			}
		}

		public void Clear()
		{
			characterPool.ReturnAllObjectsToPool();
			highlightLetterPool.ReturnAllObjectsToPool();
			characterItems.Clear();

			for (int i = 0; i < highlights.Count; i++)
			{
				Destroy(highlights[i].gameObject);
			}

			highlights.Clear();

			gridContainer.GetComponent<CanvasGroup>().alpha = 0f;
		}

		public void ShowWordHint(string word)
		{
			if (currentBoard == null)
			{
				return;
			}

			for (int i = 0; i < currentBoard.wordPlacements.Count; i++)
			{
				Board.WordPlacement wordPlacement = currentBoard.wordPlacements[i];

				if (word == wordPlacement.word)
				{
					Cell startPosition	= wordPlacement.startingPosition;
					Cell endPosition	= new Cell(startPosition.row + wordPlacement.verticalDirection * (word.Length - 1), startPosition.col + wordPlacement.horizontalDirection * (word.Length - 1));

					ShowWord(startPosition, endPosition, word, false);

					break;
				}
			}
		}

		public void ShowLetterHint(char letterToShow)
		{
			for (int row = 0; row < currentBoard.rows; row++)
			{
				for (int col = 0; col < currentBoard.cols; col++)
				{
					char letter = currentBoard.boardCharacters[row][col];

					if (letter == letterToShow)
					{
						CharacterGridItem characterGridItem = characterItems[row][col];

						Vector2 position = (characterGridItem.transform as RectTransform).anchoredPosition;

						RectTransform highlightLetter = highlightLetterPool.GetObject<RectTransform>();

						highlightLetter.sizeDelta = new Vector2(ScaledHightlightLetterSize, ScaledHightlightLetterSize);

						highlightLetter.anchoredPosition = position;
					}
				}
			}
		}

		#endregion

		#region Private Methods

		private RectTransform CreateContainer(string name, params System.Type[] types)
		{
			GameObject		containerObj	= new GameObject(name, types);
			RectTransform	container		= containerObj.GetComponent<RectTransform>();

			container.SetParent(transform, false);
			container.anchoredPosition	= Vector2.zero;
			container.anchorMin			= Vector2.zero;
			container.anchorMax			= Vector2.one;
			container.offsetMin			= Vector2.zero;
			container.offsetMax			= Vector2.zero;

			return container;
		}

		private void ShowWord(Cell wordStartPosition, Cell wordEndPosition, string word, bool useSelectedColor)
		{
			CharacterGridItem startCharacter	= characterItems[wordStartPosition.row][wordStartPosition.col];
			CharacterGridItem endCharacter		= characterItems[wordEndPosition.row][wordEndPosition.col];

			Image highlight = HighlightWord(wordStartPosition, wordEndPosition, useSelectedColor);

			// Create the floating text in the middle of the highlighted word
			Vector2 startPosition	= (startCharacter.transform as RectTransform).anchoredPosition;
			Vector2 endPosition		= (endCharacter.transform as RectTransform).anchoredPosition;
			Vector2 center			= endPosition + (startPosition - endPosition) / 2f;

			Text floatingText = CreateFloatingText(word, highlight.color, center);

			Color toColor = new Color(floatingText.color.r, floatingText.color.g, floatingText.color.b, 0f);

			UIAnimation anim;

			anim = UIAnimation.PositionY(floatingText.rectTransform, center.y, center.y + 75f, 1f);
			anim.Play();

			anim = UIAnimation.Color(floatingText, toColor, 1f);
			anim.OnAnimationFinished = (GameObject obj) => { GameObject.Destroy(obj); };
			anim.Play();
		}

		private string GetWord(Cell start, Cell end)
		{
			int rowInc		= (start.row == end.row) ? 0 : (start.row < end.row ? 1 : -1);
			int colInc		= (start.col == end.col) ? 0 : (start.col < end.col ? 1 : -1);
			int incAmount	= Mathf.Max(Mathf.Abs(start.row - end.row), Mathf.Abs(start.col - end.col));

			string word	= "";

			for (int i = 0; i <= incAmount; i++)
			{
				word = word + currentBoard.boardCharacters[start.row + i * rowInc][start.col + i * colInc];
			}

			return word;
		}

		private void UpdateSelectedWord()
		{
			if (startCharacter != null && lastEndCharacter != null)
			{
				Cell wordStartPosition	= new Cell(startCharacter.Row, startCharacter.Col);
				Cell wordEndPosition	= new Cell(lastEndCharacter.Row, lastEndCharacter.Col);

				selectedWord.SetSelectedWord(GetWord(wordStartPosition, wordEndPosition), selectingHighlight.color);
			}
			else
			{
				selectedWord.Clear();
			}
		}

		private void UpdateSelectingHighlight(Vector2 screenPosition)
		{
			if (isSelecting)
			{
				CharacterGridItem endCharacter = GetCharacterItemAtPosition(screenPosition);

				// If endCharacter is null then the mouse position must be off the grid container
				if (endCharacter != null)
				{
					int startRow = startCharacter.Row;
					int startCol = startCharacter.Col;

					int endRow = endCharacter.Row;
					int endCol = endCharacter.Col;

					int rowDiff = endRow - startRow;
					int colDiff	= endCol - startCol;

					// Check to see if the line from the start to the end is not vertical/horizontal/diagonal
					if (rowDiff != colDiff && rowDiff != 0 && colDiff != 0)
					{
						// Now we will find the best new end character position. All code below makes the highlight snap to a proper vertical/horizontal/diagonal line
						if (Mathf.Abs(colDiff) > Mathf.Abs(rowDiff))
						{
							if (Mathf.Abs(colDiff) - Mathf.Abs(rowDiff) > Mathf.Abs(rowDiff))
							{
								rowDiff = 0;
							}
							else
							{
								colDiff = AssignKeepSign(colDiff, rowDiff);
							}
						}
						else
						{
							if (Mathf.Abs(rowDiff) - Mathf.Abs(colDiff) > Mathf.Abs(colDiff))
							{
								colDiff = 0;
							}
							else
							{
								colDiff = AssignKeepSign(colDiff, rowDiff);
							}
						}

						if (startCol + colDiff < 0)
						{
							colDiff = colDiff - (startCol + colDiff);
							rowDiff = AssignKeepSign(rowDiff, Mathf.Abs(colDiff));
						}
						else if (startCol + colDiff >= currentBoard.cols)
						{
							colDiff = colDiff - (startCol + colDiff - currentBoard.cols + 1);
							rowDiff = AssignKeepSign(rowDiff, Mathf.Abs(colDiff));
						}

						endCharacter = characterItems[startRow + rowDiff][startCol + colDiff];
					}
				}
				else
				{
					// Use the last selected end character
					endCharacter = lastEndCharacter;
				}

				if (lastEndCharacter != null)
				{
					SetTextColor(startCharacter, lastEndCharacter, letterColor, false);
				}

				// Position the select highlight in the proper position
				PositionHighlight(selectingHighlight, startCharacter, endCharacter);

				// Set the text color of the letters to the highlighted color
				SetTextColor(startCharacter, endCharacter, letterHighlightedColor, false);

				// If the new end character is different then the last play a sound
				if (lastEndCharacter != endCharacter)
				{
					SoundManager.Instance.Play("highlight");
				}

				// Set the last end character so if the player drags outside the grid container then we have somewhere to drag to
				lastEndCharacter = endCharacter;
			}
		}

		private void PositionHighlight(Image highlight, CharacterGridItem start, CharacterGridItem end)
		{
			RectTransform	highlightRectT	= highlight.transform as RectTransform;
			Vector2			startPosition	= (start.transform as RectTransform).anchoredPosition;
			Vector2			endPosition		= (end.transform as RectTransform).anchoredPosition;

			float distance			= Vector2.Distance(startPosition, endPosition);
			float highlightWidth	= currentCellSize + distance + ScaledHighlighExtraSize;
			float highlightHeight	= currentCellSize + ScaledHighlighExtraSize;
			float scale				= highlightHeight / highlight.sprite.rect.height;

			// Set position and size
			highlightRectT.anchoredPosition	= startPosition + (endPosition - startPosition) / 2f;

			// Now Set the size of the highlight
			highlightRectT.localScale	= new Vector3(scale, scale);
			highlightRectT.sizeDelta	= new Vector2(highlightWidth / scale, highlight.sprite.rect.height);

			// Set angle
			float angle = Vector2.Angle(new Vector2(1f, 0f), endPosition - startPosition);

			if (startPosition.y > endPosition.y)
			{
				angle = -angle;
			}

			highlightRectT.eulerAngles = new Vector3(0f, 0f, angle);
		}

		/// <summary>
		/// Sets all character text colors from start to end 
		/// </summary>
		private void SetTextColor(CharacterGridItem start, CharacterGridItem end, Color color, bool isHighlighted)
		{
			int rowInc		= (start.Row == end.Row) ? 0 : (start.Row < end.Row ? 1 : -1);
			int colInc		= (start.Col == end.Col) ? 0 : (start.Col < end.Col ? 1 : -1);
			int incAmount	= Mathf.Max(Mathf.Abs(start.Row - end.Row), Mathf.Abs(start.Col - end.Col));

			for (int i = 0; i <= incAmount; i++)
			{
				CharacterGridItem characterGridItem = characterItems[start.Row + i * rowInc][start.Col + i * colInc];

				// If the character grid item is part of a word that is highlighed then it's color will always be set to the letterHighlightedColor
				if (characterGridItem.IsHighlighted)
				{
					characterGridItem.characterText.color = letterHighlightedColor;
				}
				else
				{
					// If the word is being highlighted then set the flag
					if (isHighlighted)
					{
						characterGridItem.IsHighlighted = isHighlighted;
					}

					// Set the text color to the color that was given
					characterGridItem.characterText.color = color;
				}
			}
		}

		private CharacterGridItem GetCharacterItemAtPosition(Vector2 screenPoint)
		{
			for (int i = 0; i < characterItems.Count; i++)
			{
				for (int j = 0; j < characterItems[i].Count; j++)
				{
					Vector2 localPoint;

					RectTransformUtility.ScreenPointToLocalPointInRectangle(characterItems[i][j].transform as RectTransform, screenPoint, null, out localPoint);

					// Check if the localPoint is inside the cell in the grid
					localPoint.x += CellFullWidth / 2f;
					localPoint.y += CellFullHeight / 2f;

					if (localPoint.x >= 0 && localPoint.y >= 0 && localPoint.x < CellFullWidth && localPoint.y < CellFullHeight)
					{
						return characterItems[i][j];
					}
				}
			}
			return null;
		}

		private float SetupGridContainer(int rows, int columns)
		{
			// Add a GridLayoutGroup so make positioning letters much easier
			GridLayoutGroup	gridLayoutGroup = gridContainer.GetComponent<GridLayoutGroup>();

			// Get the width and height of a cell
			float cellWidth		= gridContainer.rect.width / (float)columns;
			float cellHeight	= gridContainer.rect.height / (float)rows;
			float cellSize		= Mathf.Min(cellWidth, cellHeight, maxCellSize);

			gridLayoutGroup.cellSize		= new Vector2(cellSize, cellSize);
			gridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
			gridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
			gridLayoutGroup.constraintCount	= columns;

			return cellSize;
		}

		private CharacterGridItem CreateCharacterGridItem()
		{
			GameObject characterGridItemObject	= new GameObject("character_grid_item", typeof(RectTransform));
			GameObject textObject				= new GameObject("character_text", typeof(RectTransform));

			// Set the text object as a child of the CharacterGridItem object and set its position as the offset
			textObject.transform.SetParent(characterGridItemObject.transform);
			(textObject.transform as RectTransform).anchoredPosition = letterOffsetInCell;

			// Add the Text component for the item and set the font/fontSize
			Text characterText 		= textObject.AddComponent<Text>();
			characterText.font 		= letterFont;
			characterText.fontSize	= letterFontSize;
			characterText.color		= letterColor;

			// Create a ContentSizeFitter for the text object so the size will always fit the letter in it
			ContentSizeFitter textCSF	= textObject.AddComponent<ContentSizeFitter>();
			textCSF.horizontalFit		= ContentSizeFitter.FitMode.PreferredSize;
			textCSF.verticalFit			= ContentSizeFitter.FitMode.PreferredSize;

			// Add the CharacterGridItem component
			CharacterGridItem characterGridItem	= characterGridItemObject.AddComponent<CharacterGridItem>();
			characterGridItem.characterText		= characterText;

			return characterGridItem;
		}

		private Image CreateHighlightLetterImage()
		{
			GameObject	highlightImageObj		= new GameObject("highligh_image_obj", typeof(RectTransform));
			Image		highlightLetterImage	= highlightImageObj.AddComponent<Image>();

			highlightLetterImage.sprite	= highlightLetterSprite;
			highlightLetterImage.color	= highlightLetterColor;

			highlightLetterImage.rectTransform.sizeDelta = new Vector2(highlightLetterSize, highlightLetterSize);
			highlightLetterImage.rectTransform.anchorMin = new Vector2(0f, 1f);
			highlightLetterImage.rectTransform.anchorMax = new Vector2(0f, 1f);

			return highlightLetterImage;
		}

		private Image CreateNewHighlight()
		{
			GameObject		highlightObject	= new GameObject("highlight");
			RectTransform	highlightRectT	= highlightObject.AddComponent<RectTransform>();
			Image			highlightImage	= highlightObject.AddComponent<Image>();

			highlightRectT.anchorMin = new Vector2(0f, 1f);
			highlightRectT.anchorMax = new Vector2(0f, 1f);
			highlightRectT.SetParent(highlightPosition == HighlighPosition.AboveLetters ? gridOverlayContainer : gridUnderlayContainer, false);

			highlightImage.type			= Image.Type.Sliced;
			highlightImage.fillCenter	= true;
			highlightImage.sprite		= highlightSprite;

			AssignHighlighColor(highlightImage);

			if (selectingHighlight != null)
			{
				// Set the selected highlight as the last sibling so that it will always be drawn ontop of all other highlights
				selectingHighlight.transform.SetAsLastSibling();
			}

			return highlightImage;
		}

		private void AssignHighlighColor(Image highlight)
		{
			Color color = Color.white;

			if (highlightColors.Count > 0)
			{
				color = highlightColors[Random.Range(0, highlightColors.Count)];
			}
			else
			{
				Debug.LogError("[CharacterGrid] Highlight Colors is empty.");
			}

			highlight.color	= color;
		}

		private Text CreateFloatingText(string text, Color color, Vector2 position)
		{
			GameObject		floatingTextObject	= new GameObject("found_word_floating_text", typeof(Shadow));
			RectTransform	floatingTextRectT	= floatingTextObject.AddComponent<RectTransform>();
			Text			floatingText		= floatingTextObject.AddComponent<Text>();

			floatingText.text		= text;
			floatingText.font 		= letterFont;
			floatingText.fontSize	= letterFontSize;
			floatingText.color		= color;

			floatingTextRectT.anchoredPosition	= position;
			floatingTextRectT.localScale		= new Vector3(currentScale, currentScale, 1f);
			floatingTextRectT.anchorMin 		= new Vector2(0f, 1f);
			floatingTextRectT.anchorMax 		= new Vector2(0f, 1f);
			floatingTextRectT.SetParent(gridOverlayContainer, false);

			ContentSizeFitter csf	= floatingTextObject.AddComponent<ContentSizeFitter>();
			csf.verticalFit			= ContentSizeFitter.FitMode.PreferredSize;
			csf.horizontalFit		= ContentSizeFitter.FitMode.PreferredSize;

			return floatingText;

		}

		private int AssignKeepSign(int a, int b)
		{
			return (a / Mathf.Abs(a)) * Mathf.Abs(b);
		}

		#endregion
	}
}
