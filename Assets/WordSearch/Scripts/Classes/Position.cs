using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
	public class Cell
	{
		public int row;
		public int col;

		public Cell(int row, int col)
		{
			this.row = row;
			this.col = col;
		}
	}
}
