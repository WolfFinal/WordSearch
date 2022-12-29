using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CategoryInfo
{
	#region Enums

	public enum LockType
	{
		None,
		Coins,
		Keys,
		IAP
	}

	#endregion

	public string			displayName;
	public string			saveId;
	public Sprite			icon;
	public Color			categoryColor;	
	public LockType			lockType;
	public int				unlockAmount;
	public string			iapProductId;
	public TextAsset		wordFile;
	public List<TextAsset>	levelFiles;
}
