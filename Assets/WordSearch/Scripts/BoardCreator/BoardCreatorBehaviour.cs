using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.WordSearch
{
	public class BoardCreatorBehaviour : MonoBehaviour
	{
		#region Unity Methods

		private void Update()
		{
			// All this class does is checks if BoardCreator has finished and if so call the OnBoardWorkerFinished method
			if (BoardCreator.IsFinished)
			{
				BoardCreator.OnBoardWorkerFinished();
			}
		}

		#endregion
	}
}
