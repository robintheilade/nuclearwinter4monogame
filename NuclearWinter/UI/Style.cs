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
        // Global
        public UIFont           SmallFont;
        public UIFont           MediumFont;
        public UIFont           ParagraphFont;
        public UIFont           LargeFont;
        public UIFont           ExtraLargeFont;

        public Color            DefaultTextColor = Color.White;
        public Color            DefaultLinkColor = Color.Blue;
        public int              DefaultButtonHeight = 80; // FIXME: Rename to DefaultWidgetHeight?

        // Panel
        public Texture2D        Panel;
        public int              PanelCornerSize = 10;

        // Label
        public Box              LabelPadding = new Box(10);

        // Button
        public int              ButtonCornerSize = 30;
        public Texture2D        ButtonFrame;
        public Texture2D        ButtonDownFrame;
        public Texture2D        ButtonHoverOverlay;
        public Texture2D        ButtonFocusOverlay;
        public Texture2D        ButtonDownOverlay;

        public int              ButtonVerticalPadding = 10;
        public int              ButtonHorizontalPadding = 20;

        // Radio buttons
        public int              RadioButtonCornerSize = 30;
        public int              RadioButtonFrameOffset = 10;
        public Texture2D        ButtonFrameLeft;
        public Texture2D        ButtonDownFrameLeft;
        public Texture2D        ButtonFrameMiddle;
        public Texture2D        ButtonDownFrameMiddle;
        public Texture2D        ButtonFrameRight;
        public Texture2D        ButtonDownFrameRight;

        // Edit box
        public Texture2D        EditBoxFrame;
        public Texture2D        EditBoxHoverOverlay;
        public int              EditBoxCornerSize = 30;
        public Color            EditBoxTextColor = Color.White;
        public Box              EditBoxPadding = new Box(15);

        public int              CaretWidth = 1;

        // Tooltip
        public int              TooltipCornerSize   = 10;
        public Texture2D        TooltipFrame;
        public Color            TooltipTextColor = Color.White;
        public Box              TooltipPadding = new Box(10);

        // Drop-down box
        public Box              DropDownBoxPadding = new Box(10);
        public Box              DropDownBoxTextPadding = new Box(5);
        public Texture2D        DropDownArrow;

        public Texture2D        DropDownBoxEntryHoverOverlay;

        // List view
        public Texture2D        ListRowInsertMarker;
        public int              ListRowInsertMarkerCornerSize = 10;

        public ListView.ListViewStyle ListViewStyle = new ListView.ListViewStyle {
            RowHeight=40,
            RowSpacing=0,
            CellHorizontalPadding=10,
            IndicatorHorizontalSpacing=10,
            IndicatorVerticalPadding=10,
            ListViewFrameCornerSize=10,
            CellCornerSize=10,
            NewRowFrameCornerSize=10
        };
        public Box              ListViewPadding = new Box(10);

        // Tree view
        public TreeView.TreeViewStyle TreeViewStyle = new TreeView.TreeViewStyle( 40 );
        public Box              TreeViewPadding = new Box(10);

        // Notebook
        public Notebook.NotebookStyle NotebookStyle = new Notebook.NotebookStyle( 10, 50, 10 );
        
        // Popup
        public int              PopupFrameCornerSize;
        public Texture2D        PopupFrame;
        public Color            PopupBackgroundFadeColor = Color.Black * 0.2f;

        // Check box
        public Texture2D        CheckBoxChecked;
        public Texture2D        CheckBoxUnchecked;
        public Texture2D        CheckBoxInconsistent;

        public int              CheckBoxSize = 40;
        public Texture2D        CheckBoxFrame;
        public Texture2D        CheckBoxFrameHover;
        public int              CheckBoxFrameCornerSize = 15;

        public Box              CheckBoxPadding = new Box(10);
        public int              CheckBoxLabelSpacing = 10;

        // Scroll bar
        public Texture2D        VerticalScrollbar;
        public int              VerticalScrollbarCornerSize = 10;

        // Splitter
        public int              SplitterSize                = 10;
        public int              SplitterFrameCornerSize     = 5;
        public Texture2D        SplitterFrame;
        public Texture2D        SplitterDragHandle;
        public Texture2D        SplitterCollapseArrow;

        // Progress bar
        public Texture2D        ProgressBarFrame;
        public int              ProgressBarFrameCornerSize;

        public Texture2D        ProgressBar;
        public int              ProgressBarCornerSize;

        // Spinning wheel
        public Texture2D        SpinningWheel;

        // Slider
        public int              SliderFrameCornerSize = 10;
        public Texture2D        SliderFrame;
        public int              SliderHandleSize = 30;

        // Text area
        public int              TextAreaFrameCornerSize = 10;
        public Texture2D        TextAreaFrame;
        public Box              TextAreaPadding = new Box(20);
        public Texture2D        TextAreaGutterFrame;
        public int              TextAreaGutterCornerSize = 15;
        public int              TextAreaScissorOffset = 10;

        // Rich text area
        public int              RichTextAreaFrameCornerSize = 10;
        public Texture2D        RichTextAreaFrame;
        public Box              RichTextAreaPadding = new Box(20);
        public int              RichTextAreaIndentOffset    = 30;
        public int              RichTextAreaScissorOffset = 10;
    }
}
