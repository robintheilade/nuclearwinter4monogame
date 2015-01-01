#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public static class TextInputEXT
	{
		#region Event

		/// <summary>
		/// Use this event to retrieve text for objects like textboxes.
		/// This event is not raised by noncharacter keys.
		/// This event also supports key repeat.
		/// For more information this event is based off:
		/// http://msdn.microsoft.com/en-AU/library/system.windows.forms.control.keypress.aspx
		/// </summary>
		public static event Action<char> TextInput;
		public static event Action<SDL2.SDL.SDL_Keycode> KeyDown;
		public static event Action<SDL2.SDL.SDL_Keycode> KeyUp;

		#endregion

		#region Internal Event Access Method

		internal static void OnTextInput(char c)
		{
			if (TextInput != null)
			{
				TextInput(c);
			}
		}

		internal static void OnKeyDown(SDL2.SDL.SDL_Keycode key)
		{
			if(KeyDown != null)
			{
				KeyDown(key);
			}
		}

		internal static void OnKeyUp(SDL2.SDL.SDL_Keycode key)
		{
			if(KeyUp != null)
			{
				KeyUp(key);
			}
		}

		#endregion
	}
}
