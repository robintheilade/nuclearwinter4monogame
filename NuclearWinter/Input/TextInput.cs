using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NuclearWinter.Input
{
    //-------------------------------------------------------------------------
    // http://stackoverflow.com/questions/375316/xna-keyboard-text-input
    internal class TextInput: NativeWindow, IDisposable
    {
        //---------------------------------------------------------------------
        public Action<Keys>     KeyUpHandler;
        public Action<Keys>     KeyDownHandler;
        public Action<char>     CharacterHandler;

        //---------------------------------------------------------------------
        bool                    mbIsDisposed;

        const int               DLGC_WANTCHARS      = 0x0080;
        const int               DLGC_WANTALLKEYS    = 0x0004;

        const int               WM_GETDLGCODE       = 0x0087;

        const int               WM_CHAR             = 0x0102;
        const int               WM_KEYDOWN          = 0x0100;
        const int               WM_KEYUP            = 0x0101;

        //---------------------------------------------------------------------
        public TextInput( IntPtr _hWnd )
        {
            AssignHandle( _hWnd );
        }

        //---------------------------------------------------------------------
        ~TextInput()
        {
            Dispose();
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            if( ! mbIsDisposed )
            {
                ReleaseHandle();
                mbIsDisposed = true;
            }
            }

        //---------------------------------------------------------------------
        protected override void WndProc( ref Message _message )
        {
            base.WndProc( ref _message );

            switch( _message.Msg )
            {
                case WM_GETDLGCODE: {
	                int returnCode = _message.Result.ToInt32();
	                returnCode |= ( DLGC_WANTALLKEYS  | DLGC_WANTCHARS );
	                _message.Result = new IntPtr( returnCode );
                    break;
                }
                 case WM_KEYDOWN: {
                    int virtualKeyCode = _message.WParam.ToInt32();
                    if( KeyDownHandler != null )
                    {
                        KeyDownHandler( (Keys)virtualKeyCode );
                    }
                    break;
                }
                case WM_KEYUP: {
                    int virtualKeyCode = _message.WParam.ToInt32();
                    if( KeyUpHandler != null )
                    {
                        KeyUpHandler( (Keys)virtualKeyCode );
                    }
                    break;
                }
                case WM_CHAR: {
                    char character = (char)_message.WParam.ToInt32();
                    if( CharacterHandler != null )
                    {
                        CharacterHandler( character );
                    }

                    break;
                }
            }
        }
    }
}
