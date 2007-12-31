using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverStunts
{
	public class Keyboard
	{
		public bool[] keys = new bool[100];
		public bool[] platformKeys = new bool[256];

		public delegate void KeyEventDelegate(KeyboardEventArgs e, bool down);
		event KeyEventDelegate keyEvent;

		public Keyboard()
		{
			for (int i = 0; i < 100; i++) keys[i] = false;
		}

		void UpdateKeyState(KeyboardEventArgs e, bool press)
		{
			if (e.PlatformKeyCode <= 255)
			{
				platformKeys[e.PlatformKeyCode] = press;
			}

			if (e.Key <= 100)
			{
				keys[e.Key] = press;
			}
		}

		void OnKeyDown(Object o, KeyboardEventArgs e)
		{
			UpdateKeyState(e, true);
			keyEvent(e, true);
			//if (HandleFrameworkKey(e)) return;
			//game.ProcessKey(e, true);
		}

		void OnKeyUp(Object o, KeyboardEventArgs e)
		{
			UpdateKeyState(e, false);
			keyEvent(e, false);
			//game.ProcessKey(e, false);
		}

		public void Init(Canvas root, KeyEventDelegate keyEvent)
		{
			this.keyEvent = keyEvent;
			root.KeyDown += OnKeyDown;
			root.KeyUp += OnKeyUp;
		}
	}
}
