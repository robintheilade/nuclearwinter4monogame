using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorLevel.Puppet
{
    //--------------------------------------------------------------------------
    public class PuppetAnimation
    {
        //----------------------------------------------------------------------
        public PuppetAnimation()
        {

        }

        public PuppetAnimation( string _texName, int _iFrameWidth, int _iFrameHeight, float _fFrameDuration )
        {
            TextureName     = _texName;
            FrameWidth      = _iFrameWidth;
            FrameHeight     = _iFrameHeight;
            FrameDuration   = _fFrameDuration;
        }

        //----------------------------------------------------------------------
        public string           TextureName;
        public int              FrameWidth;
        public int              FrameHeight;
        public float            FrameDuration;
    }
}
