using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRewardNotify : MonoBehaviour
{
	Vector2 start;
	public float time;
	public float Distance;
	 float vector;
	private RectTransform rect;
	private bool isRuning = false;
	private float y_Anchor;
	private float x_Anchor;
	protected void Awake()
	{
		rect = GetComponent<RectTransform>();
		start = rect.anchoredPosition;
		vector = Distance/time;
		x_Anchor = rect.anchoredPosition.x;
		isRuning = false;
	}
	// This function is called when the object becomes enabled and active.
	protected void OnEnable()
	{
		isRuning = true;
	}
    void Update()
    {
	    if(isRuning)
	    {
	    	y_Anchor = rect.anchoredPosition.y + vector*Time.deltaTime;
	    	rect.anchoredPosition = new Vector2( x_Anchor , y_Anchor);
	    	if(y_Anchor>= Distance)
	    	{
	    		
	    		this.gameObject.SetActive(false);
	    	}
	    }
    }
	// This function is called when the behaviour becomes disabled () or inactive.
	protected void OnDisable()
	{
		rect.anchoredPosition = start;
		isRuning = false;
	}
}
