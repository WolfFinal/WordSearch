using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
	public class BoardCreatorWorker : Worker
	{
		#region Classes

		private class WorkingBoard
		{
			public BoardCreator.BoardConfig	config;
			public List<List<Cell>>			cells;
			public List<object[]>			placedWords;
			public List<List<Undo>>			undos;

			public WorkingBoard(BoardCreator.BoardConfig config)
			{
				this.config = config;
			}

			public void SetLetter(int row, int col, char letter)
			{
				Cell cell = cells[row][col];

				if (cell.letter != letter)
				{
					Undo undo = new Undo();

					undo.row	= row;
					undo.col	= col;
					undo.type	= BoardCreatorWorker.Undo.Type.Letter;
					undo.obj	= cell.letter;

					undos[undos.Count - 1].Add(undo);

					cell.letter = letter;
				}
			}

			public void SetCanPlaceWord(int row, int col, Board.WordDirection direction, bool canPlace)
			{
				Cell cell = cells[row][col];

				if (cell.canPlaceWord[direction] != canPlace)
				{
					Undo undo = new Undo();

					undo.row		= row;
					undo.col		= col;
					undo.direction	= direction;
					undo.type		= BoardCreatorWorker.Undo.Type.CanPlaceWord;
					undo.obj		= cell.canPlaceWord[direction];

					undos[undos.Count - 1].Add(undo);

					cell.canPlaceWord[direction] = canPlace;
				}
			}

			public void SetMaxWordLength(int row, int col, Board.WordDirection direction, int maxLen)
			{
				Cell cell = cells[row][col];

				if (cell.maxWordLength[direction] != maxLen)
				{
					Undo undo = new Undo();

					undo.row		= row;
					undo.col		= col;
					undo.direction	= direction;
					undo.type		= BoardCreatorWorker.Undo.Type.MaxWordLength;
					undo.obj		= cell.maxWordLength[direction];

					undos[undos.Count - 1].Add(undo);

					cell.maxWordLength[direction] = maxLen;
				}
			}

			public void AddLetterReq(int row, int col, Board.WordDirection direction, object[] letterReq)
			{
				Undo undo = new Undo();

				undo.row		= row;
				undo.col		= col;
				undo.direction	= direction;
				undo.type		= BoardCreatorWorker.Undo.Type.LetterReq;

				undos[undos.Count - 1].Add(undo);

				cells[row][col].letterReqs[direction].Add(letterReq);
			}

			public void BeginUndo()
			{
				undos.Add(new List<Undo>());
			}

			public void Undo()
			{
				List<Undo> undosToUndo = undos[undos.Count - 1];

				undos.RemoveAt(undos.Count - 1);

				for (int i = 0; i < undosToUndo.Count; i++)
				{
					Undo undo = undosToUndo[i];

					switch (undo.type)
					{
						case BoardCreatorWorker.Undo.Type.Letter:
							cells[undo.row][undo.col].letter = (char)undo.obj;
							break;
						case BoardCreatorWorker.Undo.Type.CanPlaceWord:
							cells[undo.row][undo.col].canPlaceWord[undo.direction] = (bool)undo.obj;
							break;
						case BoardCreatorWorker.Undo.Type.MaxWordLength:
							cells[undo.row][undo.col].maxWordLength[undo.direction] = (int)undo.obj;
							break;
						case BoardCreatorWorker.Undo.Type.LetterReq:
							List<object[]> letterReqs = cells[undo.row][undo.col].letterReqs[undo.direction];
							letterReqs.RemoveAt(letterReqs.Count - 1);
							break;
					}
				}
			}
		}

		private class Cell
		{
			public WordSearch.Cell	pos;
			public char		letter;

			public Dictionary<Board.WordDirection, bool>			canPlaceWord;
			public Dictionary<Board.WordDirection, int>				maxWordLength;
			public Dictionary<Board.WordDirection, List<object[]>>	letterReqs;
		}

		private class Undo
		{
			public enum Type
			{
				Letter,
				CanPlaceWord,
				MaxWordLength,
				LetterReq
			}

			public int					row;
			public int					col;
			public Board.WordDirection	direction;
			public Type					type;
			public object				obj;
		}

		#endregion

		#region Member Variables

		private System.Diagnostics.Stopwatch timer;

		#endregion

		#region Properties

		// In properties
		public BoardCreator.BoardConfig Config { get; set; }

		// Out properties
		public string	Error			{ get; private set; }
		public Board	CompletedBoard	{ get; private set; }

		#endregion

		#region Protected Methods

		protected override void Begin()
		{
			timer = new System.Diagnostics.Stopwatch();

			// Sort the words by length, this will create boards with more words on them on average
			Config.words.Sort((string w1, string w2) => { return w1.Length - w2.Length; });

			// Remove any spaces from the words
			for (int i = 0; i < Config.words.Count; i++)
			{
				Config.words[i] = Config.words[i].Trim();
			}
		}

		protected override void DoWork()
		{
			timer.Start();

			try
			{
				WorkingBoard workingBoard = new WorkingBoard(Config);

				// If the algo has a timeout then we will create multiple boards and pick the one that has the most words on it
				if (workingBoard.config.algoTimeoutInMilliseconds > 0 || workingBoard.config.numSamplesToGenerate > 0)
				{
					int		boardsGenerated	= 0;
					float	curBestScore	= 0;
					Board	curBestBoard	= null;

					while (timer.ElapsedMilliseconds < workingBoard.config.algoTimeoutInMilliseconds || boardsGenerated < workingBoard.config.numSamplesToGenerate)
					{
						// Initialize a new working board
						InitWorkingBoard(workingBoard);

						// Start placing the words on the board until we can no longer place any more words
						PlaceNextWord(workingBoard, 0);

						float boardScore = ScoreBoard(workingBoard);

						// Check if the board generated contains more words on it then the previously generate board
						if (boardScore > curBestScore)
						{
							curBestScore = boardScore;
							curBestBoard = CreateBoard(workingBoard);

							// Check if we have placed all words on the board
							if (curBestBoard.wordPlacements.Count == Config.words.Count)
							{
								break;
							}
						}

						boardsGenerated++;
					}

					// Fill all the blank spaces on the board with random characters
					FillBlankSpaces(curBestBoard, Config.randomCharacters);

					// Set the completed board to the current best board (Board with most words on it)
					CompletedBoard = curBestBoard;
				}
				// Else we will just run the algo once and it will run until it has placed all words on the board or it tries all possible combinations
				else
				{
					// Initialize the board
					InitWorkingBoard(workingBoard);

					// Start placing the words on the board until we can no longer place any more words
					bool result = PlaceNextWord(workingBoard, 0);

					if (result)
					{
						Board board = CreateBoard(workingBoard);

						// Fill all the blank spaces on the board with random characters
						FillBlankSpaces(board, Config.randomCharacters);

						CompletedBoard = board;
					}
					else
					{
						Error = "Could not place all the words on the board";
					}
				}
			}
			catch (System.Exception ex)
			{
				Error = ex.Message + "\n" + ex.StackTrace;
			}

			timer.Stop();

			Stop();
		}

		#endregion

		#region Private Methods

		private void InitWorkingBoard(WorkingBoard workingBoard)
		{
			workingBoard.cells			= new List<List<Cell>>();
			workingBoard.placedWords	= new List<object[]>();
			workingBoard.undos			= new List<List<Undo>>();

			// Init the working board cells
			for (int row = 0; row < workingBoard.config.rows; row++)
			{
				workingBoard.cells.Add(new List<Cell>());

				for (int col = 0; col < workingBoard.config.cols; col++)
				{
					Cell cell = new Cell();

					cell.pos			= new WordSearch.Cell(row, col);
					cell.letter			= Board.BlankChar;
					cell.canPlaceWord	= new Dictionary<Board.WordDirection, bool>();
					cell.maxWordLength	= new Dictionary<Board.WordDirection, int>();
					cell.letterReqs		= new Dictionary<Board.WordDirection, List<object[]>>();

					bool[] canPlaceWord = {
						row > 0,																	// Up
						row > 0 && col < workingBoard.config.cols - 1,								// UpRight
						col < workingBoard.config.cols - 1,											// Right
						row < workingBoard.config.rows - 1 && col < workingBoard.config.cols - 1,	// DownRight
						row < workingBoard.config.rows - 1,											// Down
						row < workingBoard.config.rows - 1 && col > 0,								// DownLeft
						col > 0,																	// Left
						row > 0 && col > 0															// UpLeft
					};

					int[] maxWordLength = {
						row + 1,																	// Up
						Mathf.Min(row + 1, workingBoard.config.cols - col),							// UpRight
						workingBoard.config.cols - col,												// Right
						Mathf.Min(workingBoard.config.rows - row, workingBoard.config.cols - col),	// DownRight
						workingBoard.config.rows - row,												// Down
						Mathf.Min(workingBoard.config.rows - row, col + 1),							// DownLeft
						col + 1, 																	// Left
						Mathf.Min(row + 1, col + 1), 												// UpLeft
					};

					for (int i = 0; i < (int)Board.WordDirection.COUNT; i++)
					{
						Board.WordDirection wordDirection = (Board.WordDirection)i;

						cell.canPlaceWord.Add(wordDirection, canPlaceWord[i]);
						cell.maxWordLength.Add(wordDirection, maxWordLength[i]);
						cell.letterReqs.Add(wordDirection, new List<object[]>());
					}

					workingBoard.cells[row].Add(cell);
				}
			}
		}

		private void FillBlankSpaces(Board board, string randomCharacters)
		{
			// Randomly place characters on the board to fill the blank spaces
			for (int row = 0; row < board.rows; row++)
			{
				for (int col = 0; col < board.cols; col++)
				{
					if (board.boardCharacters[row][col] == Board.BlankChar)
					{
						board.boardCharacters[row][col] = randomCharacters[Random.Range(0, randomCharacters.Length)];
					}
				}
			}
		}

		private bool PlaceNextWord(WorkingBoard workingBoard, int wordIndex)
		{
			if (Stopping)
			{
				return false;
			}

			if (wordIndex >= workingBoard.config.words.Count)
			{
				return true;
			}

			// Check if we have run out of time, if so return true and the curBestBoard will be used
			if (workingBoard.config.algoTimeoutInMilliseconds > 0 && timer.ElapsedMilliseconds >= workingBoard.config.algoTimeoutInMilliseconds)
			{
				return true;
			}

			// Get the word we are about to place on the board
			string word = workingBoard.config.words[wordIndex].Replace(" ", "");

			// Get all the possible starting cells/directions on the board where we can place the word
			List<object[]> possibleWordStarts = GetAllPossibleWordStarts(workingBoard, word);

			// Try and place the word on all possible staring positions
			for (int i = 0; i < possibleWordStarts.Count; i++)
			{
				if (Stopping)
				{
					return false;
				}

				// Pick a random starting position
				int			randIndex		= workingBoard.config.random.Next(i, possibleWordStarts.Count);
				object[]	wordStartToTry	= possibleWordStarts[randIndex];

				// Swap them so if we fail to place the word, the next iteration og the for loop will no pick this word start again
				possibleWordStarts[randIndex]	= possibleWordStarts[i];
				possibleWordStarts[i]			= wordStartToTry;

				Cell				startingCell	= (Cell)wordStartToTry[0];
				Board.WordDirection	direction		= (Board.WordDirection)wordStartToTry[1];

				workingBoard.BeginUndo();

				// Place the words characters on the board
				PlaceWord(workingBoard, word, startingCell, direction);

				// Try and place the next word
				if (PlaceNextWord(workingBoard, wordIndex + 1))
				{
					workingBoard.placedWords.Add(new object[] { word, startingCell.pos.row, startingCell.pos.col, direction });

					return true;
				}

				// Undo all changes to the working board
				workingBoard.Undo();
			}

			if (workingBoard.config.algoTimeoutInMilliseconds == 0 && workingBoard.config.numSamplesToGenerate == 0)
			{
				// If the algo has no timout then we will continue until all the words have been placed on the board or we tried all possible combinations
				return false;
			}
			else
			{
				// The algo timer is not 0 and we failed to place the next work because there were no valid spots to place it. So we return true now
				// so we can restart the algo with a blank board. This strategy yields boards with the most words on them.
				return true;
			}
		}

		private List<object[]> GetAllPossibleWordStarts(WorkingBoard workingBoard, string word)
		{
			List<object[]> possibleWordStarts = new List<object[]>();

			for (int row = 0; row < workingBoard.config.rows; row++)
			{
				for (int col = 0; col < workingBoard.config.cols; col++)
				{
					Cell cell = workingBoard.cells[row][col];

					if (cell.letter != Board.BlankChar && cell.letter != word[0])
					{
						continue;
					}

					for (int i = 0; i < (int)Board.WordDirection.COUNT; i++)
					{
						if (Stopping)
						{
							return possibleWordStarts;
						}

						Board.WordDirection wordDirection = (Board.WordDirection)i;

						// Check if any word can start on this cell in the current direction
						if (!cell.canPlaceWord[wordDirection])
						{
							continue;
						}

						// Check if the word length is less than the max word length
						if (word.Length > cell.maxWordLength[wordDirection])
						{
							continue;
						}

						// Check that the word has the proper letters for any words it will cross
						List<object[]>	letterReqs		= cell.letterReqs[wordDirection];
						bool			meetsAllReqs	= true;

						for (int j = 0; j < letterReqs.Count; j++)
						{
							object[]	letterReq	= letterReqs[j];
							char		letter		= (char)letterReq[0];
							int			dist		= (int)letterReq[1];

							if (dist < word.Length && letter != word[dist])
							{
								meetsAllReqs = false;
								break;
							}
						}

						if (!meetsAllReqs)
						{
							continue;
						}

						// The word can be placed at the current cell in the current direction
						object[] wordStart = { cell, wordDirection };

						possibleWordStarts.Add(wordStart);
					}
				}
			}

			return possibleWordStarts;
		}

		private void PlaceWord(WorkingBoard workingBoard, string word, Cell startingCell, Board.WordDirection direction)
		{
			int rowInc, colInc;

			WordDirectionToRowColInc(direction, out rowInc, out colInc);

			Board.WordDirection oppositeDirection = (Board.WordDirection)(((int)direction + 4) % (int)Board.WordDirection.COUNT);

			for (int i = 0; i < word.Length; i++)
			{
				int		row		= startingCell.pos.row + i * rowInc;
				int		col		= startingCell.pos.col + i * colInc;
				char	letter	= word[i];

				workingBoard.SetLetter(row, col, letter);

				workingBoard.SetCanPlaceWord(row, col, direction, false);
				workingBoard.SetCanPlaceWord(row, col, oppositeDirection, false);

				for (int j = 0; j < (int)Board.WordDirection.COUNT; j++)
				{
					Board.WordDirection reqDir = (Board.WordDirection)j;

					if (reqDir == direction || reqDir == oppositeDirection)
					{
						continue;
					}

					SetLetterReqs(workingBoard, row, col, reqDir, letter);
				}
			}

			Cell lastCell = workingBoard.cells[startingCell.pos.row + (word.Length - 1) * rowInc][startingCell.pos.col + (word.Length - 1) * colInc];

			SetNewMaxLengths(workingBoard, startingCell, oppositeDirection, direction);
			SetNewMaxLengths(workingBoard, lastCell, direction, oppositeDirection);
		}

		private void SetLetterReqs(WorkingBoard workingBoard, int startRow, int startCol, Board.WordDirection direction, char letter)
		{
			int rowInc, colInc;

			WordDirectionToRowColInc(direction, out rowInc, out colInc);

			Board.WordDirection oppositeDirection = (Board.WordDirection)(((int)direction + 4) % (int)Board.WordDirection.COUNT);

			for (int count = 1, row = startRow + rowInc, col = startCol + colInc;
			     row >= 0 && col >= 0 && row < workingBoard.config.rows && col < workingBoard.config.cols;
			     count++, row += rowInc, col += colInc)
			{
				object[] letterReq = { letter, count };

				workingBoard.AddLetterReq(row, col, oppositeDirection, letterReq);
			}
		}

		private void SetNewMaxLengths(WorkingBoard workingBoard, Cell startingCell, Board.WordDirection moveDirection, Board.WordDirection setDirection)
		{
			int rowInc, colInc;

			WordDirectionToRowColInc(moveDirection, out rowInc, out colInc);

			for (int count = 1, row = startingCell.pos.row + rowInc, col = startingCell.pos.col + colInc;
			     row >= 0 && col >= 0 && row < workingBoard.config.rows && col < workingBoard.config.cols;
			     count++, row += rowInc, col += colInc)
			{
				if (workingBoard.cells[row][col].maxWordLength[setDirection] < count)
				{
					break;
				}

				workingBoard.SetMaxWordLength(row, col, setDirection, count);
			}
		}

		private void WordDirectionToRowColInc(Board.WordDirection direction, out int rowInc, out int colInc)
		{
			rowInc = 0;
			colInc = 0;

			switch (direction)
			{
				case Board.WordDirection.Up:
					rowInc = -1;
					break;
				case Board.WordDirection.UpRight:
					rowInc = -1;
					colInc = 1;
					break;
				case Board.WordDirection.Right:
					colInc = 1;
					break;
				case Board.WordDirection.DownRight:
					rowInc = 1;
					colInc = 1;
					break;
				case Board.WordDirection.Down:
					rowInc = 1;
					break;
				case Board.WordDirection.DownLeft:
					rowInc = 1;
					colInc = -1;
					break;
				case Board.WordDirection.Left:
					colInc = -1;
					break;
				case Board.WordDirection.UpLeft:
					rowInc = -1;
					colInc = -1;
					break;
			}
		}

		private Board CreateBoard(WorkingBoard workingBoard)
		{
			Board board = new Board();

			board.rows				= workingBoard.config.rows;
			board.cols				= workingBoard.config.cols;
			board.words				= new List<string>();
			board.boardCharacters	= new List<List<char>>();
			board.wordPlacements	= new List<Board.WordPlacement>();

			for (int i = 0; i < workingBoard.placedWords.Count; i++)
			{
				object[] placedWord = workingBoard.placedWords[i];

				string				word		= (string)placedWord[0];
				int					row			= (int)placedWord[1];
				int					col			= (int)placedWord[2];
				Board.WordDirection	direction	= (Board.WordDirection)placedWord[3];

				board.words.Add(word);

				Board.WordPlacement wordPlacement	= new Board.WordPlacement();
				wordPlacement.word					= word;
				wordPlacement.startingPosition		= new WordSearch.Cell(row, col);

				WordDirectionToRowColInc(direction, out wordPlacement.verticalDirection, out wordPlacement.horizontalDirection);

				board.wordPlacements.Add(wordPlacement);
			}

			// Create the Board characters matrix, any blank characters will be replaced with a random character
			for (int row = 0; row < board.rows; row++)
			{
				board.boardCharacters.Add(new List<char>());

				for (int col = 0; col < board.cols; col++)
				{
					char letter = workingBoard.cells[row][col].letter;

					if (letter == Board.BlankChar)
					{
						string randomCharacters = workingBoard.config.randomCharacters;

						letter = randomCharacters[workingBoard.config.random.Next(0, randomCharacters.Length)];
					}

					board.boardCharacters[board.boardCharacters.Count - 1].Add(letter);
				}
			}

			return board;
		}

		private float ScoreBoard(WorkingBoard workingBoard)
		{
			// Point for each placed word
			float score = workingBoard.placedWords.Count;

			// Points based on the distribution of horizontal, vertical, and diagonal words
			List<int> set = new List<int>(){ 0, 0, 0 };
			for (int i = 0; i < workingBoard.placedWords.Count; i++)
			{
				object[]			placedWord	= workingBoard.placedWords[i];
				Board.WordDirection	wordDir		= (Board.WordDirection)placedWord[3];

				switch (wordDir)
				{
					case Board.WordDirection.Left:
					case Board.WordDirection.Right:
						set[0]++;
						break;
					case Board.WordDirection.Up:
					case Board.WordDirection.Down:
						set[1]++;
						break;
					default:
						set[2]++;
						break;
				}
			}

			set.Sort();

			// Smaller the range between min and max directions the more points (Max points achieved here is num placed words / 2)
			score += workingBoard.placedWords.Count - (set[2] - set[0]);

			return score;
		}

		private string PrintCells(WorkingBoard workingBoard)
		{
			string str = "";

			for (int row = 0; row < workingBoard.config.rows; row++)
			{
				for (int cols = 0; cols < workingBoard.config.cols; cols++)
				{
					Cell cell = workingBoard.cells[row][cols];

					str += cell.letter == Board.BlankChar ? "_" : cell.letter.ToString();
				}

				str += "\n";
			}

			Debug.Log(str);

			return str;
		}

		private string PrintLetterReqs(WorkingBoard workingBoard)
		{
			string str = "";

			for (int row = 0; row < workingBoard.config.rows; row++)
			{
				for (int cols = 0; cols < workingBoard.config.cols; cols++)
				{
					Cell cell = workingBoard.cells[row][cols];

					for (int i = 0; i < (int)Board.WordDirection.COUNT; i++)
					{
						Board.WordDirection direction = (Board.WordDirection)i;

						List<object[]> letterReqs = cell.letterReqs[direction];

						for (int j = 0; j < letterReqs.Count; j++)
						{
							str += string.Format("{0},{1} {2} {3} {4}\n", row, cols, direction, letterReqs[j][0], letterReqs[j][1]);
						}
					}
				}
			}

			Debug.Log(str);

			return str;
		}

		#endregion
	}
}
