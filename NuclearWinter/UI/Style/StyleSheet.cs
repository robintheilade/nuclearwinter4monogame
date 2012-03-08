using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter.UI.Style
{
    /// <summary>
    /// A StyleSheet defines default styles for widgets and store alternative styles
    /// 
    /// StyleSheets can (and should) be loaded as XNA XML assets
    /// </summary>
    class StyleSheet
    {
        //----------------------------------------------------------------------
        public LabelStyle                   DefaultLabelStyle;
        public ButtonStyle                  DefaultButtonStyle;
        public EditBoxStyle                 DefaultEditBoxStyle;
        // TODO: Add more...

        Dictionary<string,WidgetStyle>      StylesByName;

        //----------------------------------------------------------------------
        public StyleSheet()
        {
            StylesByName = new Dictionary<string,WidgetStyle>();
        }

        //----------------------------------------------------------------------
        public T GetStyle<T>( string _strName ) where T:WidgetStyle
        {
            return null;
        }
    }
}
