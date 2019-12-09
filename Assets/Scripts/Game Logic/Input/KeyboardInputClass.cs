using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputClass
{
	
	public ISet<KeyCode> keyDownList;
	
	public KeyboardInputClass()
	{
		keyDownList = new HashSet<KeyCode>();
	}


	public void ProcessPressedKey(KeyCode key) 
	{
		if ( !keyDownList.Contains(key)) {
			keyDownList.Add(key);
		}
	}


	public void ProcessReleasedKey(KeyCode key) 
	{
		if (keyDownList.Contains(key)) {
			keyDownList.Remove(key);
		}
	}


	public bool IsKeyDown(KeyCode key) 
	{
		return keyDownList.Contains(key);
	}


}