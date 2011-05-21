using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorLevel.Puppet
{
    public class PuppetDef
    {
        //----------------------------------------------------------------------
        public PuppetDef()
        {
        }

        //----------------------------------------------------------------------
        public Dictionary<string,PuppetAnimation>       Animations = new Dictionary<string,PuppetAnimation>();
    }
}
