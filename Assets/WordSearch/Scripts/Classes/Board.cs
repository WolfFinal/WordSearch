using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
	public class Board
	{
		#region Classes

		public class WordPlacement
		{
			public string	word;
			public Cell		startingPosition;
			public int		verticalDirection;
			public int		horizontalDirection;
		}

		#endregion

		#region Enums

		public enum WordDirection
		{
			Up,
			UpRight,
			Right,
			DownRight,
			Down,
			DownLeft,
			Left,
			UpLeft,

			COUNT
		}

		#endregion

		#region Member Variables

		public const char BlankChar = '\0';

		public int					rows;
		public int					cols;
		public List<string>			words;
		public List<List<char>>		boardCharacters;
		public List<WordPlacement>	wordPlacements;

		public int				difficultyIndex	= -1;
		public HashSet<string>	foundWords		= new HashSet<string>();
		public HashSet<char>	letterHintsUsed	= new HashSet<char>();

		#endregion

		public Dictionary<string, object> ToJson()
		{
			Dictionary<string, object> json = new Dictionary<string, object>();

			json["rows"] 			= rows;
			json["cols"] 			= cols;
			json["words"] 			= words;
			json["boardCharacters"]	= boardCharacters;

			List<object> wordPlacementsJson = new List<object>();

			for (int i = 0; i < wordPlacements.Count; i++)
			{
				WordPlacement				wordPlacement		= wordPlacements[i];
				Dictionary<string, object>	wordPlacementJson	= new Dictionary<string, object>();

				wordPlacementJson["word"]	= wordPlacement.word;
				wordPlacementJson["row"]	= wordPlacement.startingPosition.row;
				wordPlacementJson["col"]	= wordPlacement.startingPosition.col;
				wordPlacementJson["h"]		= wordPlacement.horizontalDirection;
				wordPlacementJson["v"]		= wordPlacement.verticalDirection;

				wordPlacementsJson.Add(wordPlacementJson);
			}

			json["wordPlacements"]	= wordPlacementsJson;

			if (difficultyIndex != -1)
			{
				json["difficultyIndex"] = difficultyIndex;
			}

			if (foundWords.Count > 0)
			{
				json["foundWords"] = new List<string>(foundWords);
			}

			if (letterHintsUsed.Count > 0)
			{
				json["letterHintsUsed"]	= new List<char>(letterHintsUsed);
			}

			return json;
		}

		public void FromJson(JSONNode json)
		{
			rows			= json["rows"].AsInt;
			cols			= json["cols"].AsInt;
			words			= new List<string>();
			boardCharacters	= new List<List<char>>();
			wordPlacements	= new List<WordPlacement>();
			difficultyIndex	= json.AsObject.HasKey("difficultyIndex") ? json["difficultyIndex"].AsInt : -1;
			foundWords		= new HashSet<string>();
			letterHintsUsed	= new HashSet<char>();

			for (int i = 0; i < json["words"].AsArray.Count; i++)
			{
				words.Add(json["words"].AsArray[i].Value);
			}

			for (int i = 0; i < json["boardCharacters"].AsArray.Count; i++)
			{
				boardCharacters.Add(new List<char>());

				for (int j = 0; j < json["boardCharacters"][i].AsArray.Count; j++)
				{
					char character = json["boardCharacters"][i][j].Value[0];

					boardCharacters[i].Add(character);
				}
			}

			for (int i = 0; i < json["wordPlacements"].AsArray.Count; i++)
			{
				JSONNode		wordPlacementJson	= json["wordPlacements"].AsArray[i];
				WordPlacement	wordPlacement		= new WordPlacement();

				wordPlacement.word					= wordPlacementJson["word"].Value;
				wordPlacement.startingPosition		= new Cell(wordPlacementJson["row"].AsInt, wordPlacementJson["col"].AsInt);
				wordPlacement.horizontalDirection	= wordPlacementJson["h"].AsInt;
				wordPlacement.verticalDirection		= wordPlacementJson["v"].AsInt;

				wordPlacements.Add(wordPlacement);
			}

			for (int i = 0; i < json["foundWords"].AsArray.Count; i++)
			{
				foundWords.Add(json["foundWords"].AsArray[i].Value);
			}

			for (int i = 0; i < json["letterHintsUsed"].AsArray.Count; i++)
			{
				letterHintsUsed.Add(json["letterHintsUsed"].AsArray[i].Value[0]);
			}
		}

		public Board Copy()
		{
			Board board = new Board();

			board.rows				= rows;
			board.cols				= cols;
			board.words				= new List<string>(words);
			board.wordPlacements	= new List<WordPlacement>(wordPlacements);

			board.boardCharacters = new List<List<char>>();

			for (int i = 0; i < boardCharacters.Count; i++)
			{
				board.boardCharacters.Add(new List<char>(boardCharacters[i]));
			}

			return board;
		}
	}
}
