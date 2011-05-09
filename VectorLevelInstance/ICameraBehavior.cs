using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorLevel
{
    public abstract class ICameraBehavior
    {
        //----------------------------------------------------------------------
        public ICameraBehavior( Camera _camera )
        {
            Camera = _camera;
        }

        //----------------------------------------------------------------------
        public abstract void Update( float _fElapsedTime );

        //----------------------------------------------------------------------
        public Camera       Camera;
    }
}
