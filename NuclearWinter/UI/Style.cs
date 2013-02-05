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
        public UIFont           SmallFont;
        public UIFont           MediumFont;
        public UIFont           LargeFont;
        public UIFont           ExtraLargeFont;

        public Box              LabelPadding = new Box(10);

        public int              ButtonCornerSize = 30;
        public int              DefaultButtonHeight = 80;
        public Color            DefaultTextColor = Color.White;
        public Texture2D        ButtonFrame;
        public Texture2D        ButtonDownFrame;
        public Texture2D        ButtonHoverOverlay;
        public Texture2D        ButtonFocusOverlay;
        public Texture2D        ButtonDownOverlay;

        public int              ButtonVerticalPadding = 10;
        public int              ButtonHorizontalPadding = 20;

        public int              TooltipCornerSize   = 10;
        public Texture2D        TooltipFrame;
        public Color            TooltipTextColor = Color.White;

        public int              RadioButtonCornerSize = 30;
        public int              RadioButtonFrameOffset = 10;
        public Texture2D        ButtonFrameLeft;
        public Texture2D        ButtonDownFrameLeft;
        public Texture2D        ButtonFrameMiddle;
        public Texture2D        ButtonDownFrameMiddle;
        public Texture2D        ButtonFrameRight;
        public Texture2D        ButtonDownFrameRight;

        public Texture2D        EditBoxFrame;
        public Texture2D        EditBoxHoverOverlay;
        public int              EditBoxCornerSize = 30;
        public Color            EditBoxTextColor = Color.White;
        public Box              EditBoxPadding = new Box(15);

        public int              CaretWidth = 1;

        public Box              DropDownBoxPadding = new Box(10);
        public Box              DropDownBoxTextPadding = new Box(5);
        public Texture2D        DropDownArrow;

        public Texture2D        ListFrame;

        public Texture2D        ListRowInsertMarker;
        public int              ListRowInsertMarkerCornerSize = 10;

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

        public Notebook.NotebookStyle NotebookStyle = new Notebook.NotebookStyle( 10, 50, 10 );
        
        public int              PopupFrameCornerSize;
        public Texture2D        PopupFrame;
        public Color            PopupBackgroundFadeColor = Color.Black * 0.2f;

        public TreeView.TreeViewStyle TreeViewStyle = new TreeView.TreeViewStyle( 40, 0, 25 );

        public Texture2D        CheckBoxChecked;
        public Texture2D        CheckBoxUnchecked;
        public Texture2D        CheckBoxInconsistent;

        public int              CheckBoxSize = 40;
        public Texture2D        CheckBoxFrame;
        public Texture2D        CheckBoxFrameHover;
        public int              CheckBoxFrameCornerSize = 15;

        public Texture2D        VerticalScrollbar;
        public int              VerticalScrollbarCornerSize = 10;

        public int              SplitterSize                = 10;
        public int              SplitterFrameCornerSize     = 5;
        public Texture2D        SplitterFrame;
        public Texture2D        SplitterDragHandle;
        public Texture2D        SplitterCollapseArrow;

        public Texture2D        ProgressBarFrame;
        public int              ProgressBarFrameCornerSize;

        public Texture2D        ProgressBar;
        public int              ProgressBarCornerSize;

        public Texture2D        SpinningWheel;

        public int              SliderHandleSize = 30;

        public Box              TextAreaPadding = new Box(20);
        public Texture2D        TextAreaGutterFrame;
        public int              TextAreaGutterCornerSize = 15;

        public Box              RichTextAreaPadding = new Box(20);
    }
}
