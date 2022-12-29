using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.WordSearch
{
	public class GameScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private Text wordHintCostText		= null;
		[SerializeField] private Text letterHintCostText	= null;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			base.Initialize();

			wordHintCostText.text = "x" + GameManager.Instance.CoinCostWordHint;
			letterHintCostText.text = "x" + GameManager.Instance.CoinCostLetterHint;
		}

		#endregion
	}
}
