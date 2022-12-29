using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BBG.WordSearch
{
	public class CharacterGridItem : MonoBehaviour
	{
		#region Inspector Variables

		public Text	characterText;

		#endregion

		#region Properties

		public int	Row				{ get; set; }
		public int	Col				{ get; set; }
		public bool	IsHighlighted	{ get; set; }

		#endregion
	}
}
