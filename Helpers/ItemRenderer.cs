using ASCompletion;
using NavigationBar.Controls;
using PluginCore;
using PluginCore.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace NavigationBar.Helpers
{
    internal static class ItemRenderer
    {
        static Bitmap[] _icons = null;

        static ItemRenderer()
        {
            //Pull the member icons from the resources;
            _icons = new Bitmap[] {
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("FilePlain.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("FolderClosed.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("FolderOpen.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("CheckAS.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("QuickBuild.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Package.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Interface.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Intrinsic.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Class.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Variable.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("VariableProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("VariablePrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("VariableStatic.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("VariableStaticProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("VariableStaticPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Const.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("ConstProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("ConstPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Const.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("ConstProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("ConstPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Method.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("MethodProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("MethodPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("MethodStatic.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("MethodStaticProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("MethodStaticPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Property.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("PropertyProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("PropertyPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("PropertyStatic.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("PropertyStaticProtected.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("PropertyStaticPrivate.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Template.png"))),
                ScaleHelper.Scale(new Bitmap(PluginUI.GetStream("Declaration.png")))
            };
        }

        internal static void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Update ForeColor and BackColor if a theme overrides the defaults
            ComboBox comboBox = sender as ComboBox;

            // If we drawing an item that exists
            if (e.Index > -1)
            {
                MemberTreeNode node = comboBox.Items[e.Index] as MemberTreeNode;

                int imageWidth = _icons[0].Width;
                int imageHeight = _icons[0].Height;

                // Clear the old background
                DrawItemBackground(comboBox, e);

                var focusColor = PluginBase.MainForm.GetThemeColor("ToolStripItem.BackColor");
                focusColor = focusColor != Color.Empty ? focusColor : e.BackColor;

                // Is this item being hovered over?
                DrawItemFocusRectangle(focusColor, e);

                // Draw the item image
                var imageRectangle = new Rectangle(e.Bounds.Left + 2, e.Bounds.Top, imageWidth, imageHeight);
                e.Graphics.DrawImage(_icons[node.ImageIndex], imageRectangle);

                var textPoint = new Point(imageRectangle.Right + 1, e.Bounds.Top);
                Color textColor;

                // Is this item disabled?
                if ((e.State & DrawItemState.Disabled) != 0)
                    textColor = SystemColors.GrayText;
                // Is this item inherited?
                else if (node is InheritedMemberTreeNode)
                    textColor = Color.Gray;
                // Are we using default highlighting?
                else if (focusColor == e.BackColor)
                    textColor = e.ForeColor;
                else
                    textColor = comboBox.ForeColor;

                using (var textBrush = new SolidBrush(textColor))
                    e.Graphics.DrawString(node.Label, comboBox.Font, textBrush, textPoint);
            }
        }

        static void DrawItemBackground(ComboBox comboBox, DrawItemEventArgs e)
        {
            Color backColor = Color.Empty;

            if (e.Bounds.X == 0)
                backColor = PluginBase.MainForm.GetThemeColor("ToolStripMenu.BackColor");

            if (backColor == Color.Empty)
                backColor = e.BackColor;

            using (var backgroundBrush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
        }

        static void DrawItemFocusRectangle(Color focusColor, DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Focus) != 0)
            {
                // Draw a selection box and label in the selection text color
                var rectangle = e.Bounds;
                rectangle.Inflate(-1, -1);

                using (var focusBrush = new SolidBrush(focusColor))
                    e.Graphics.FillRectangle(focusBrush, rectangle);
            }
        }
    }
}