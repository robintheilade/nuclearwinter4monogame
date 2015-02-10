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
using System.Collections.Generic;
using System.ComponentModel;

using SDL2;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework
{
	class SDL2_GameWindow : GameWindow
	{
		#region Public GameWindow Properties

		[DefaultValue(false)]
		public override bool AllowUserResizing
		{
			/* FIXME: This change should happen immediately. However, SDL2 does
			 * not yet have an SDL_SetWindowResizable, so we mostly just have
			 * this for the #define we've got at the top of this file.
			 * -flibit
			 */
			get
			{
				return INTERNAL_allowUserResizing;
			}
			set
			{
				INTERNAL_allowUserResizing = value;
				INTERNAL_SetWindowMinMaxSize();
			}
		}

		public override Rectangle ClientBounds
		{
			get
			{
				Rectangle result;
				if (INTERNAL_isFullscreen)
				{
					/* FIXME: SDL2 bug!
					 * SDL's a little weird about SDL_GetWindowSize.
					 * If you call it early enough (for example,
					 * Game.Initialize()), it reports outdated ints.
					 * So you know what, let's just use this.
					 * -flibit
					 */
					SDL.SDL_DisplayMode mode;
					SDL.SDL_GetCurrentDisplayMode(
						SDL.SDL_GetWindowDisplayIndex(
							INTERNAL_sdlWindow
						),
						out mode
					);
					result.X = 0;
					result.Y = 0;
					result.Width = mode.w;
					result.Height = mode.h;
				}
				else
				{
					SDL.SDL_GetWindowPosition(
						INTERNAL_sdlWindow,
						out result.X,
						out result.Y
					);
					SDL.SDL_GetWindowSize(
						INTERNAL_sdlWindow,
						out result.Width,
						out result.Height
					);
				}
				return result;
			}
		}

		public override DisplayOrientation CurrentOrientation
		{
			get
			{
				// SDL2 has no orientation.
				return DisplayOrientation.LandscapeLeft;
			}
		}

		public override IntPtr Handle
		{
			get
			{
				return INTERNAL_sdlWindow;
			}
		}

		public override IntPtr WindowsHandleEXT
		{
			get
			{
				var info = new SDL.SDL_SysWMinfo();
				SDL.SDL_GetWindowWMInfo(INTERNAL_sdlWindow, ref info);
				return info.info.win.window;
			}
		}

		public override bool IsBorderlessEXT
		{
			get
			{
				return (SDL.SDL_GetWindowFlags(INTERNAL_sdlWindow) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0;
			}
			set
			{
				SDL.SDL_SetWindowBordered(
					INTERNAL_sdlWindow,
					value ? SDL.SDL_bool.SDL_FALSE : SDL.SDL_bool.SDL_TRUE
				);
			}
		}

		public override string ScreenDeviceName
		{
			get
			{
				return INTERNAL_deviceName;
			}
		}

		#endregion

		#region Private SDL2 Window Variables

		private IntPtr INTERNAL_sdlWindow;

		private bool INTERNAL_isFullscreen;
		private bool INTERNAL_wantsFullscreen;

		private string INTERNAL_deviceName;

		private bool INTERNAL_allowUserResizing;

		#endregion

		#region Internal Constructor

		internal SDL2_GameWindow()
		{
			SDL.SDL_WindowFlags initFlags = (
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL |
				SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
				SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
				SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS |
				SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
			);

			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
#if DEBUG
			SDL.SDL_GL_SetAttribute(
				SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS,
				(int) SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG
			);
#endif

			string title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();
			INTERNAL_sdlWindow = SDL.SDL_CreateWindow(
				title,
				SDL.SDL_WINDOWPOS_CENTERED,
				SDL.SDL_WINDOWPOS_CENTERED,
				GraphicsDeviceManager.DefaultBackBufferWidth,
				GraphicsDeviceManager.DefaultBackBufferHeight,
				initFlags
			);
			INTERNAL_SetIcon(title);

			INTERNAL_SetWindowMinMaxSize();

			INTERNAL_isFullscreen = false;
			INTERNAL_wantsFullscreen = false;
		}

		#endregion

		#region Public GameWindow Methods

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			INTERNAL_wantsFullscreen = willBeFullScreen;
		}

		public override void EndScreenDeviceChange(
			string screenDeviceName,
			int clientWidth,
			int clientHeight
		) {
			// Set screen device name, not that we use it...
			INTERNAL_deviceName = screenDeviceName;

			// Fullscreen
			SDL.SDL_SetWindowFullscreen(
				INTERNAL_sdlWindow,
				INTERNAL_wantsFullscreen ?
					(uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP :
					0
			);

			/* Because Mac windows resize from the bottom, we have to get the
			 * position before changing the size so we can keep the window
			 * centered when resizing in windowed mode.
			 * -Nick
			 */
			Rectangle prevBounds = Rectangle.Empty;
			if (!INTERNAL_wantsFullscreen)
			{
				prevBounds = ClientBounds;
			}

			// Window bounds
			INTERNAL_ClearWindowMinMaxSize();
			SDL.SDL_SetWindowSize(INTERNAL_sdlWindow, clientWidth, clientHeight);
			INTERNAL_SetWindowMinMaxSize();

			// Window position
			if (INTERNAL_isFullscreen && !INTERNAL_wantsFullscreen)
			{
				// If exiting fullscreen, just center the window on the desktop.
				SDL.SDL_SetWindowPosition(
					INTERNAL_sdlWindow,
					SDL.SDL_WINDOWPOS_CENTERED,
					SDL.SDL_WINDOWPOS_CENTERED
				);
			}
			else if (!INTERNAL_wantsFullscreen)
			{
				SDL.SDL_SetWindowPosition(
					INTERNAL_sdlWindow,
					Math.Max(
						prevBounds.X + ((prevBounds.Width - clientWidth) / 2),
						0
					),
					Math.Max(
						prevBounds.Y + ((prevBounds.Height - clientHeight) / 2),
						0
					)
				);
			}

			// Current window state has just been updated.
			INTERNAL_isFullscreen = INTERNAL_wantsFullscreen;
		}

		#endregion

		#region Internal Methods

		internal void INTERNAL_ClientSizeChanged()
		{
			OnClientSizeChanged();
		}

		internal void INTERNAL_StateChangedNUCLEAR()
		{
			OnStateChangedNUCLEAR();
		}

		internal void INTERNAL_ClearWindowMinMaxSize()
		{
			SDL2.SDL.SDL_SetWindowMinimumSize(Handle, 1, 1);

			// FIXME: SDL_SetWindowMaximumSize doesn't allow resetting back to zero
			// after setting a maximum size once.
			// https://github.com/spurious/SDL-mirror/blob/c2c793b74c2c9478814e4d49ea44577a6829c154/src/video/SDL_video.c#L1793
			SDL2.SDL.SDL_SetWindowMaximumSize(Handle, 100000, 100000);
		}

		internal void INTERNAL_SetWindowMinMaxSize()
		{
			if (!INTERNAL_allowUserResizing)
			{
				var clientBounds = ClientBounds;
				SDL2.SDL.SDL_SetWindowMinimumSize(Handle, clientBounds.Width, clientBounds.Height);
				SDL2.SDL.SDL_SetWindowMaximumSize(Handle, clientBounds.Width, clientBounds.Height);
			}
			else
			{
				INTERNAL_ClearWindowMinMaxSize();
			}
		}

		#endregion

		#region Protected GameWindow Methods

		protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			// No-op. SDL2 has no orientation.
		}

		protected override void SetTitle(string title)
		{
			SDL.SDL_SetWindowTitle(
				INTERNAL_sdlWindow,
				title
			);
		}

		#endregion

		#region Private Window Icon Method

		private void INTERNAL_SetIcon(string title)
		{
			string fileIn = String.Empty;

			/* If the game's using SDL2_image, provide the option to use a PNG
			 * instead of a BMP. Nice for anyone who cares about transparency.
			 * -flibit
			 */
			try
			{
				fileIn = INTERNAL_GetIconName(title, ".png");
				if (!String.IsNullOrEmpty(fileIn))
				{
					IntPtr icon = SDL_image.IMG_Load(fileIn);
					SDL.SDL_SetWindowIcon(INTERNAL_sdlWindow, icon);
					SDL.SDL_FreeSurface(icon);
					return;
				}
			}
			catch(DllNotFoundException)
			{
				// Not that big a deal guys.
			}

			fileIn = INTERNAL_GetIconName(title, ".bmp");
			if (!String.IsNullOrEmpty(fileIn))
			{
				IntPtr icon = SDL.SDL_LoadBMP(fileIn);
				SDL.SDL_SetWindowIcon(INTERNAL_sdlWindow, icon);
				SDL.SDL_FreeSurface(icon);
			}
		}

		#endregion

		#region Private Static Icon Filename Method

		private static string INTERNAL_GetIconName(string title, string extension)
		{
			string fileIn = String.Empty;
			if (System.IO.File.Exists(title + extension))
			{
				// If the title and filename work, it just works. Fine.
				fileIn = title + extension;
			}
			else
			{
				// But sometimes the title has invalid characters inside.

				/* In addition to the filesystem's invalid charset, we need to
				 * blacklist the Windows standard set too, no matter what.
				 * -flibit
				 */
				char[] hardCodeBadChars = new char[]
				{
					'<',
					'>',
					':',
					'"',
					'/',
					'\\',
					'|',
					'?',
					'*'
				};
				List<char> badChars = new List<char>();
				badChars.AddRange(System.IO.Path.GetInvalidFileNameChars());
				badChars.AddRange(hardCodeBadChars);

				string stripChars = title;
				foreach (char c in badChars)
				{
					stripChars = stripChars.Replace(c.ToString(), "");
				}
				stripChars += extension;

				if (System.IO.File.Exists(stripChars))
				{
					fileIn = stripChars;
				}
			}
			return fileIn;
		}

		#endregion
	}
}
