using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

#if !FNA
using System.Windows.Forms;
using System.Runtime.InteropServices;
#endif

namespace NuclearWinter
{
    public class NuclearGame : Game
    {
        //----------------------------------------------------------------------
        public Texture2D WhitePixelTex { get; protected set; }
        public Matrix SpriteMatrix { get; protected set; }

        //----------------------------------------------------------------------
        public GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public RasterizerState ScissorRasterizerState { get; private set; }


        // Index of the player responsible for menu navigation (or null if none yet)
        public PlayerIndex? PlayerInCharge;

        public static readonly string ApplicationDataFolderPath;
        public static readonly string DocumentsPath;

        public Storage.SaveHandler NuclearSaveHandler;

        //----------------------------------------------------------------------
        // Sound & Music
        public Song Song;

        //----------------------------------------------------------------------
        // Game States
        protected bool UseGameStateManager;
        public GameFlow.GameStateMgr<NuclearGame> GameStateMgr { get; private set; }

        //----------------------------------------------------------------------
        // Input
        public Input.InputManager InputMgr { get; private set; }

        public const float LerpMultiplier = 15f;

        //----------------------------------------------------------------------
#if !FNA
        public Form Form { get; private set; }

        [DllImport("user32.dll")]
        static extern bool IsWindowUnicode(IntPtr hWnd);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs..</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values: GWL_EXSTYLE, GWL_HINSTANCE, GWL_ID, GWL_STYLE, GWL_USERDATA, GWL_WNDPROC </param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer. 
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
        [DllImport("user32.dll")]
        static extern int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowTextW(IntPtr hwnd, string strText);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }
#endif

        static NuclearGame()
        {
#if !FNA
            ApplicationDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
            string platform = SDL2.SDL.SDL_GetPlatform();

            switch( platform )
            {
                case "Windows":
                    ApplicationDataFolderPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
                    DocumentsPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
                    break;

                case "Mac OS X": {
                    string osConfigDir = Environment.GetEnvironmentVariable("HOME");
                    if( String.IsNullOrEmpty(osConfigDir) ) osConfigDir = ApplicationDataFolderPath = "."; // Oh well.
                    else ApplicationDataFolderPath = osConfigDir + "/Library";

                    DocumentsPath = System.IO.Path.Combine( osConfigDir, "Documents" );
                    break;
                }

                case "Linux": {
                    string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                    if( String.IsNullOrEmpty(osConfigDir) )
                    {
                        osConfigDir = Environment.GetEnvironmentVariable("HOME");

                        if( String.IsNullOrEmpty(osConfigDir) ) ApplicationDataFolderPath = osConfigDir = "."; // Oh well.
                        else ApplicationDataFolderPath = osConfigDir + "/.local/share";
                    }
                    else
                    {
                        ApplicationDataFolderPath = osConfigDir;
                    }

                    DocumentsPath = Environment.GetEnvironmentVariable("XDG_DOCUMENTS_DIR");
                    if( String.IsNullOrEmpty(DocumentsPath) ) DocumentsPath = System.IO.Path.Combine( osConfigDir, "Documents" );
                    break;
                }
                
                default:
                    throw new Exception("Unhandled SDL platform: " + platform);
            }

#endif
        }

        //----------------------------------------------------------------------
        public NuclearGame(bool useGameStateManager = true)
        {
#if !FNA
            Form = (Form)Form.FromHandle(Window.Handle);

            // Is the Game window unicode-aware?
            if (!IsWindowUnicode(Window.Handle))
            {
                // No? Well, no problem, we'll just make it aware!

                int iTitleLength = GetWindowTextLength(Window.Handle);
                StringBuilder sbTitle = new StringBuilder(iTitleLength + 1);
                GetWindowText(Window.Handle, sbTitle, sbTitle.Capacity);

                SetWindowLongW(Window.Handle, (int)GWL.GWL_WNDPROC, GetWindowLong(Window.Handle, (int)GWL.GWL_WNDPROC));
                SetWindowTextW(Window.Handle, sbTitle.ToString());
            }
#endif

            Graphics = new GraphicsDeviceManager(this);
            SpriteMatrix = Matrix.Identity;

            UseGameStateManager = useGameStateManager;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Sets the window title (Unicode-aware)
        /// </summary>
        public void SetWindowTitle(string title)
        {
            Window.Title = title;
        }


        //----------------------------------------------------------------------
#if !FNA
        Dictionary<MouseCursor, Cursor> mWindowsCursors = new Dictionary<MouseCursor, Cursor> {
            { MouseCursor.Default, Cursors.Default },
            { MouseCursor.SizeWE, Cursors.SizeWE },
            { MouseCursor.SizeNS, Cursors.SizeNS },
            { MouseCursor.SizeAll, Cursors.SizeAll },

            { MouseCursor.Hand, Cursors.Hand },
            { MouseCursor.IBeam, Cursors.IBeam },
            { MouseCursor.Cross, Cursors.Cross },
        };
#else
        Dictionary<MouseCursor,IntPtr> mSDLCursors = new Dictionary<MouseCursor,IntPtr>();
#endif

        public void SetCursor(MouseCursor cursor)
        {
#if !FNA
            Form.Cursor = mWindowsCursors[cursor];
#else
            IntPtr SDLCursor;
            if( ! mSDLCursors.TryGetValue( _cursor, out SDLCursor ) )
            {
                mSDLCursors[_cursor] = SDLCursor = SDL2.SDL.SDL_CreateSystemCursor( (SDL2.SDL.SDL_SystemCursor)_cursor );
            }
            SDL2.SDL.SDL_SetCursor(SDLCursor);
#endif
        }

        public void ShowErrorMessageBox(string title, string message)
        {
            ShowErrorMessageBox(title, message, Window.Handle);
        }

        public static void ShowErrorMessageBox(string title, string message, IntPtr windowHandle)
        {
#if !FNA
            System.Windows.Forms.MessageBox.Show(message, title, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
#else
            SDL2.SDL.SDL_ShowSimpleMessageBox( SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, _strTitle, _strMessage, _windowHandle );
#endif
        }

        //----------------------------------------------------------------------
        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            ScissorRasterizerState = new RasterizerState();
            ScissorRasterizerState.ScissorTestEnable = true;

            if (UseGameStateManager)
            {
                GameStateMgr = new GameFlow.GameStateMgr<NuclearGame>(this);
                Components.Add(GameStateMgr);
            }

            InputMgr = new Input.InputManager(this);
            Components.Add(InputMgr);

            base.Initialize();
        }

        //----------------------------------------------------------------------
        protected override void LoadContent()
        {
            WhitePixelTex = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            WhitePixelTex.SetData(new[] { Color.White });
        }

        //----------------------------------------------------------------------
        protected override void OnExiting(object sender, EventArgs args)
        {
            if (NuclearSaveHandler != null)
            {
                NuclearSaveHandler.SaveGameSettings();
            }

            if (GameStateMgr != null && GameStateMgr.CurrentState != null)
            {
                GameStateMgr.CurrentState.OnExiting();
            }

            base.OnExiting(sender, args);
        }

        //----------------------------------------------------------------------
        public void DrawLine(Vector2 from, Vector2 to, Color color, float width = 1f)
        {
            float fAngle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
            float fLength = Vector2.Distance(from, to);

            Vector2 vOrigin = new Vector2(fLength / 2f, width / 2f);

            SpriteBatch.Draw(WhitePixelTex, (from + to) / 2f, null, color, fAngle, new Vector2(0.5f), new Vector2(fLength + width, width), SpriteEffects.None, 0);
        }

        public void DrawRect(Vector2 from, Vector2 to, Color color, float width = 1)
        {
            if (from.X > to.X)
            {
                float x = from.X;
                from.X = to.X;
                to.X = x;
            }

            if (from.Y > to.Y)
            {
                float y = from.Y;
                from.Y = to.Y;
                to.Y = y;
            }

            DrawLine(from + new Vector2(width / 2f, 0f), new Vector2(to.X - width, from.Y), color, width);
            DrawLine(new Vector2(from.X + width / 2f, to.Y), to - new Vector2(width, 0f), color, width);

            DrawLine(from, new Vector2(from.X, to.Y), color, width);
            DrawLine(new Vector2(to.X, from.Y), to, color, width);
        }

        //----------------------------------------------------------------------
        // TODO should this method be removed? can't find any usages/references of/to it
        public virtual string GetUIString(string id) { return id; }

        //----------------------------------------------------------------------
        public List<Tuple<string, bool>> WrapText(SpriteFont font, string text, float lineWidth)
        {
            List<Tuple<string, bool>> lText = new List<Tuple<string, bool>>();

            foreach (string strChunk in text.Split('\n'))
            {
                string strLine = string.Empty;

                if (strChunk != "")
                {
                    string[] aWords = strChunk.Split(' ');

                    bool bFirst = true;
                    foreach (string strWord in aWords)
                    {
                        if (bFirst) bFirst = false;
                        else strLine += " ";

                        if (strLine != "" && font.MeasureString(strLine + strWord).X > lineWidth)
                        {
                            lText.Add(new Tuple<string, bool>(strLine, false));
                            strLine = string.Empty;
                        }

                        strLine += strWord;
                    }
                }

                lText.Add(new Tuple<string, bool>(strLine + " ", true));
            }

            return lText;
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, string label, Vector2 position)
        {
            DrawBlurredText(blurRadius, font, label, position, Color.White, Color.Black, Vector2.Zero, 1f);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, string label, Vector2 position, Color color)
        {
            DrawBlurredText(blurRadius, font, label, position, color, Color.Black, Vector2.Zero, 1f);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, string label, Vector2 position, Color color, Vector2 origin, float scale)
        {
            DrawBlurredText(blurRadius, font, label, position, color, Color.Black, origin, scale);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, string label, Vector2 position, Color color, Color blurColor)
        {
            DrawBlurredText(blurRadius, font, label, position, color, blurColor, Vector2.Zero, 1f);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, string label, Vector2 position, Color color, Color blurColor, Vector2 origin, float scale)
        {
            if (blurRadius > 0f)
            {
                //Color blurColor = _blurColor * 0.1f * (_color.A / 255f);

                SpriteBatch.DrawString(font, label, new Vector2(position.X - blurRadius, position.Y - blurRadius), blurColor, 0f, origin, scale, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(font, label, new Vector2(position.X + blurRadius, position.Y - blurRadius), blurColor, 0f, origin, scale, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(font, label, new Vector2(position.X + blurRadius, position.Y + blurRadius), blurColor, 0f, origin, scale, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(font, label, new Vector2(position.X - blurRadius, position.Y + blurRadius), blurColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            SpriteBatch.DrawString(font, label, new Vector2(position.X, position.Y), color, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, StringBuilder label, Vector2 position)
        {
            DrawBlurredText(blurRadius, font, label, position, Color.White, Color.Black, Vector2.Zero, 1f);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, StringBuilder label, Vector2 position, Color color)
        {
            DrawBlurredText(blurRadius, font, label, position, color, Color.Black, Vector2.Zero, 1f);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, StringBuilder label, Vector2 position, Color color, Vector2 origin, float scale)
        {
            DrawBlurredText(blurRadius, font, label, position, color, Color.Black, origin, scale);
        }

        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, StringBuilder label, Vector2 position, Color color, Color blurColor)
        {
            DrawBlurredText(blurRadius, font, label, position, color, blurColor, Vector2.Zero, 1f);
        }


        //----------------------------------------------------------------------
        public void DrawBlurredText(float blurRadius, SpriteFont font, StringBuilder label, Vector2 position, Color color, Color blurColor, Vector2 origin, float scale)
        {
            if (blurRadius > 0f)
            {
                Color actualColor = blurColor * 0.1f * (color.A / 255f);

                SpriteBatch.DrawString(font, label, new Vector2(position.X - blurRadius, position.Y - blurRadius), actualColor, 0f, origin, scale, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(font, label, new Vector2(position.X + blurRadius, position.Y - blurRadius), actualColor, 0f, origin, scale, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(font, label, new Vector2(position.X + blurRadius, position.Y + blurRadius), actualColor, 0f, origin, scale, SpriteEffects.None, 0f);
                SpriteBatch.DrawString(font, label, new Vector2(position.X - blurRadius, position.Y + blurRadius), actualColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            SpriteBatch.DrawString(font, label, new Vector2(position.X, position.Y), color, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        //----------------------------------------------------------------------
        public void PlayMusic(Song song)
        {
            if (Song != song && MediaPlayer.GameHasControl)
            {
                MediaPlayer.IsRepeating = true;

                if (song != null)
                {
                    MediaPlayer.Play(song);
                }
                else
                {
                    MediaPlayer.Stop();
                }

                Song = song;
            }
        }
    }
}
