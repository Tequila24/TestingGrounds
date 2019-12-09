using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputClass
{
    
    private ISet<int> buttonDownList;
    private Vector2 lastMouseDelta;

    public MouseInputClass()
    {
        buttonDownList = new HashSet<int>();
    }

    
    public void ProcessPressedKey(int key) 
	{
		if ( !buttonDownList.Contains(key)) {
			buttonDownList.Add(key);
		}
	}


	public void ProcessReleasedKey(int key) 
	{
		if (buttonDownList.Contains(key)) {
			buttonDownList.Remove(key);
		}
	}


	public bool IsKeyDown(int key) 
	{
		return buttonDownList.Contains(key);
	}


    public void ProcessNewMouseDelta(Vector2 newMouseDelta)
    {
        lastMouseDelta = newMouseDelta;
    }

    public Vector2 GetMouseDelta()
    {
        return lastMouseDelta;
    }


}
