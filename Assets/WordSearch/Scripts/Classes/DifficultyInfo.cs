using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
	[System.Serializable]
	public class DifficultyInfo
	{
		public int boardRowSize;
		public int boardColumnSize;
		public int maxWords;
		public int maxWordLength;
	}
}
