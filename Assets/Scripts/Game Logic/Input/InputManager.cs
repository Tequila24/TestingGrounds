using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
	public class MyKeyboardEvent : UnityEvent<KeyCode> {};
	public class MyMouseEvent : UnityEvent<int> {};
	
	private static InputManager _instance = null;
	public static InputManager Instance { get {return _instance; } }


	
	// keyboard
	public MyKeyboardEvent keyPressEvent;
	public MyKeyboardEvent keyReleaseEvent;

	private KeyboardInputClass _keyboard;
	
	// mouse
	public MyMouseEvent mouseKeyPressEvent;
	public MyMouseEvent mouseKeyReleaseEvent;

	private MouseInputClass _mouse;


	void Awake() {
		if (_instance != null && _instance != this) {
			Destroy(gameObject);
		} else {
			_instance = this;
		}


		_keyboard = new KeyboardInputClass();
		keyPressEvent = new MyKeyboardEvent();
		keyReleaseEvent = new MyKeyboardEvent();
		keyPressEvent.AddListener(_keyboard.ProcessPressedKey);
		keyReleaseEvent.AddListener(_keyboard.ProcessReleasedKey);


		_mouse = new MouseInputClass();
		mouseKeyPressEvent = new MyMouseEvent();
		mouseKeyReleaseEvent = new MyMouseEvent();
		mouseKeyPressEvent.AddListener(_mouse.ProcessPressedKey);
		mouseKeyReleaseEvent.AddListener(_mouse.ProcessReleasedKey);

	}


	void OnGUI() {

		Event currentEvent = Event.current;

		if (currentEvent.isKey) {
			if (currentEvent.keyCode != KeyCode.None) {
				if (currentEvent.type == EventType.KeyDown) {
					keyPressEvent.Invoke(currentEvent.keyCode);
				}
	
				if (currentEvent.type == EventType.KeyUp) {
					keyReleaseEvent.Invoke(currentEvent.keyCode);
				}
			}
		}
		
		if (currentEvent.isMouse) {
			if (currentEvent.type == EventType.MouseDown) {
				mouseKeyPressEvent.Invoke(currentEvent.button);
			}
			if (currentEvent.type == EventType.MouseUp) {
				mouseKeyReleaseEvent.Invoke(currentEvent.button);
			}
		}
		_mouse.ProcessNewMouseDelta( new Vector2( Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") ) );
	}


	public int IsKeyDown(KeyCode key)
	{
		return _keyboard.IsKeyDown(key) ? 1 : 0;
	}
	public Vector2 GetMouseDelta()
	{
		return _mouse.GetMouseDelta();
	}

}