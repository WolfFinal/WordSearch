using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBG.WordSearch
{
	public static class BoardCreator
	{
		#region Classes

		public class BoardConfig
		{
			public int				rows;
			public int				cols;
			public List<string>		words;
			public string			randomCharacters			= "abcdefghijklmnopqrstuvwxyz";
			public long				algoTimeoutInMilliseconds	= 2000;	// If this is 0 then there will be no timeout, the algorithm will run till it places all words of fails to place all words
			public int				numSamplesToGenerate		= 0;	// Specifies the number og boards to generate before the algo stops and picks the one with the most words. If set to 0 then it will try all possible combinations.
			public System.Random	random						= new System.Random();
		}

		#endregion

		#region Member Variables

		private static BoardCreatorWorker		boardCreatorWorker;
		private static BoardCreatorBehaviour	boardCreatorBehaviour;
		private static System.Action<Board>		onFinishedCallback;

		#endregion

		#region Properties

		public static bool IsRunning	{ get { return boardCreatorWorker != null; } }
		public static bool IsFinished	{ get { return boardCreatorWorker != null && boardCreatorWorker.Stopped; } }

		#endregion

		#region Public Methods

		public static void CreateBoard(BoardConfig boardConfig, System.Action<Board> callback)
		{
			// Make sure there is no other worker running
			Stop();

			onFinishedCallback = callback;

			// Create the BoardCreatorWorker to actually create the board
			boardCreatorWorker			= new BoardCreatorWorker();
			boardCreatorWorker.Config	= boardConfig;

			// Create the BoardCreatorBehaviour which will check the status in the Update loop and when it sees the worker has finished will call OnBoardWorkerFinished on the main thread
			boardCreatorBehaviour = new GameObject("board_creator_behaviour").AddComponent<BoardCreatorBehaviour>();

			boardCreatorWorker.StartWorker();
		}

		/// <summary>
		/// Invoked by BoardCreatorBehaviour
		/// </summary>
		public static void OnBoardWorkerFinished()
		{
			Board completedBoard = boardCreatorWorker.CompletedBoard;

			if (!string.IsNullOrEmpty(boardCreatorWorker.Error))
			{
				Debug.LogError(boardCreatorWorker.Error);
			}

			Stop();

			if (onFinishedCallback != null)
			{
				onFinishedCallback(completedBoard);
			}
		}

		/// <summary>
		/// Stops the worker if there is one running
		/// </summary>
		public static void Stop()
		{
			if (boardCreatorWorker != null)
			{
				boardCreatorWorker.Stop();
				boardCreatorWorker = null;
			}

			if (boardCreatorBehaviour != null)
			{
				GameObject.Destroy(boardCreatorBehaviour.gameObject);
				boardCreatorBehaviour = null;
			}
		}

		#endregion

		#region Private Methods

		#endregion
	}
}
