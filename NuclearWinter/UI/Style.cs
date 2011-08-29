using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    /*
     * Holds styles for widgets
     */
    public class Style
    {
        public int              BlurRadius = 4;

        public SpriteFont       SmallFont;
        public SpriteFont       MediumFont;
        public SpriteFont       BigFont;

        public int              ButtonCornerSize = 30;
        public int              DefaultButtonHeight = 80;
        public Color            ButtonTextColor;
        public Texture2D        ButtonFrame;

        public Texture2D        ButtonFrameHover;

        public Texture2D        ButtonFrameDown;
        public Texture2D        ButtonFramePressed;

        public Texture2D        ButtonFrameFocus;
        public Texture2D        ButtonFrameDownFocus;

        public Texture2D        ButtonFrameLeftFocus;
        public Texture2D        ButtonFrameLeftDownFocus;
        public Texture2D        ButtonFrameMiddleFocus;
        public Texture2D        ButtonFrameMiddleDownFocus;
        public Texture2D        ButtonFrameRightFocus;
        public Texture2D        ButtonFrameRightDownFocus;

        public int              ButtonVerticalPadding = 10;
        public int              ButtonHorizontalPadding = 20;

        public Texture2D        ButtonFrameLeft;
        public Texture2D        ButtonFrameLeftDown;
        public Texture2D        ButtonFrameMiddle;
        public Texture2D        ButtonFrameMiddleDown;
        public Texture2D        ButtonFrameRight;
        public Texture2D        ButtonFrameRightDown;

        public Texture2D        EditBoxFrame;
        public int              EditBoxCornerSize = 30;

        public Texture2D        DropDownArrow;

        public Texture2D        GridFrame;
        public Texture2D        GridHeaderFrame;

        public int              GridBoxFrameCornerSize = 30;

        public Texture2D        GridBoxFrame;
        public Texture2D        GridBoxFrameHover;
        public Texture2D        GridBoxFrameFocus;

        public Texture2D        GridBoxFrameSelected;
        public Texture2D        GridBoxFrameSelectedFocus;
        public Texture2D        GridBoxFrameSelectedHover;

        public Texture2D        Panel;
        public int              PanelCornerSize = 10;

        public int              NotebookTabCornerSize = 10;
        public Texture2D        NotebookTab;
        public Texture2D        NotebookActiveTab;
        public Texture2D        NotebookTabFocus;
        public Texture2D        NotebookActiveTabFocus;

        public Texture2D        NotebookTabClose;
        public Texture2D        NotebookTabCloseHover;
        public Texture2D        NotebookTabCloseDown;
        
        public int              PopupFrameCornerSize;
        public Texture2D        PopupFrame;

        public Texture2D        TreeViewBranchClosed;
        public Texture2D        TreeViewBranchOpen;
        public Texture2D        TreeViewBranchOpenEmpty;
        public Texture2D        TreeViewBranch;
        public Texture2D        TreeViewBranchLast;
    }
}
