using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace VectorLevel.Puppet
{
    //--------------------------------------------------------------------------
    public class PuppetAnimation
    {
        //----------------------------------------------------------------------
        public PuppetAnimation()
        {

        }

        public PuppetAnimation( string _strName, string _strTexName, int _iFrameWidth, int _iFrameHeight, float _fFrameDuration, bool _bIsLooping )
        {
            Name            = _strName;
            TextureName     = _strTexName;
            FrameWidth      = _iFrameWidth;
            FrameHeight     = _iFrameHeight;
            FrameDuration   = _fFrameDuration;
            IsLooping       = _bIsLooping;
        }

        //----------------------------------------------------------------------
        public string           Name;
        public string           TextureName;
        public int              FrameWidth;
        public int              FrameHeight;
        public float            FrameDuration;
        [ContentSerializer(Optional = true)]
        public bool             IsLooping = true;
    }
}
