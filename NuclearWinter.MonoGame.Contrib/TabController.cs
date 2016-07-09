using NuclearWinter.UI;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NuclearWinter.Contrib
{
    /// <summary>
    /// Controls the behavior when the tab key is pressed.
    /// 
    /// Default behavior of the widget is to navigate using the position
    /// of the other widgets, this controller uses the order in which the
    /// widgets are added to the controller.
    /// </summary>
    public class TabController
    {
        /// <summary>
        /// The widgets to control tab key behavior for.
        /// </summary>
        private readonly List<Widget> widgets = new List<Widget>();

        /// <summary>
        /// Adds a widget to the controller.
        /// 
        /// The order of the widgets being added directly effects in
        /// which order they will receive focus when tab key is pressed.
        /// </summary>
        /// <param name="widget">
        /// The widget to control tab key behavior for.
        /// </param>
        public void Add(Widget widget)
        {
            widget.KeyPress += this.HandleKeyPress;
            this.widgets.Add(widget);
        }

        /// <summary>
        /// Adds multiple widgets to the controller.
        /// 
        /// The order of the widgets being added directly effects in
        /// which order they will receive focus when tab key is pressed.
        /// </summary>
        /// <param name="widgets">
        /// The widgets to control tab key behavior for.
        /// </param>
        public void AddRange(params Widget[] widgets)
        {
            foreach (var widget in widgets)
            {
                this.Add(widget);
            }
        }

        /// <summary>
        /// Handles the event if tab key is pressed.
        /// </summary>
        /// <param name="sender">
        /// The widget raising the event.
        /// </param>
        /// <param name="args">
        /// The details of the event.
        /// </param>
        private void HandleKeyPress(object sender, KeyPressEventArgs args)
        {
            var key = args.Key;
            if (key != Keys.Tab)
            {
                return;
            }

            if (args.IsLeftControl || args.IsRightControl)
            {
                return;
            }

            if (args.IsLeftAlt || args.IsRightAlt)
            {
                return;
            }

            var widget = (Widget)sender;
            var index = this.widgets.IndexOf(widget);
            if (index == -1)
            {
                return;
            }

            var nextIndex
                = args.IsLeftShift || args.IsRightShift
                ? ((index - 1) + this.widgets.Count) % this.widgets.Count
                : (index + 1) % this.widgets.Count
                ;
            var nextWidget = this.widgets[nextIndex];
            widget.Screen.Focus(nextWidget);

            args.Handled = true;
        }
    }
}
