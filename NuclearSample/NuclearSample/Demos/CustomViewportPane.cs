using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NuclearUI = NuclearWinter.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearSample.Demos
{
    class CustomViewportPane: NuclearUI.ManagerPane<MainMenuManager>
    {
        //----------------------------------------------------------------------
        public CustomViewportPane( MainMenuManager _manager )
        : base( _manager )
        {
            var viewport = new MyCustomViewport( Manager.MenuScreen );
            AddChild( viewport );
        }
    }

    class MyCustomViewport: NuclearUI.CustomViewport
    {
        BasicEffect mEffect;
        float mfRotation;
        float mfDistance = -3f;

        public MyCustomViewport( NuclearUI.Screen _screen )
        : base( _screen )
        {
            mEffect = new BasicEffect( Screen.Game.GraphicsDevice );
            mEffect.VertexColorEnabled = true;
        }

        //----------------------------------------------------------------------
        protected override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            // Just an example of how you can interact through the CustomViewport widget
            // You can override lots of other event handlers for mouse & keyboard events
            mfDistance += _iDelta / 120f * 0.5f;
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            mfRotation += MathHelper.TwoPi / 180f;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            var fViewportRatio = (float)LayoutRect.Width / LayoutRect.Height;
            mEffect.Projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver2, fViewportRatio, 0.1f, 1000f );
            mEffect.View = Matrix.CreateRotationY( mfRotation ) * Matrix.CreateTranslation( 0, 0, mfDistance );

            BeginDraw();

            mEffect.CurrentTechnique.Passes[0].Apply();

            mEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.TriangleList, new VertexPositionColor[] {
                new VertexPositionColor( new Vector3( -1f, -1f, 0f ), Color.Red ),
                new VertexPositionColor( new Vector3(  1f,  1f, 0f ), Color.Green ),
                new VertexPositionColor( new Vector3(  1f, -1f, 0f ), Color.Blue ),
            }, 0, 1 );

            EndDraw();
        }
    }
}
