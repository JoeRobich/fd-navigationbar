using NavigationBar.Controls;
using System;
using System.Windows.Forms;

namespace NavigationBar.Helpers
{
    internal static class SearchHelper
    {
        static string _dropDownSearchKey;
        static Timer _dropDownSearchTimer;

        internal static Settings Settings {get; set; }

        static SearchHelper()
        {
            _dropDownSearchTimer = new Timer();
            _dropDownSearchTimer.Tick += dropDownSearchTimer_Tick;
            _dropDownSearchKey = "";
        }

        internal static void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            ComboBox comboBox = ((ToolStripSpringComboBox)sender).FlatCombo;
            string searchKey = e.KeyChar.ToString();

            if (Settings.DropDownMultiKeyEnabled)
            {
                _dropDownSearchTimer.Stop();
                _dropDownSearchKey += searchKey;
                _dropDownSearchTimer.Interval = Settings.DropDownMultiKeyTimer;
                _dropDownSearchTimer.Start();
            }
            else
            {
                _dropDownSearchKey = searchKey;
            }

            // If shift is pressed then reverse search
            if ((Control.ModifierKeys & Keys.Shift) == 0)
                ForwardSearch(comboBox, _dropDownSearchKey);
            else
                ReverseSearch(comboBox, _dropDownSearchKey);
        }

        internal static void Reset()
        {
            _dropDownSearchTimer.Stop();
            _dropDownSearchKey = "";
        }

        static void ForwardSearch(ComboBox comboBox, string searchKey)
        {
            MemberTreeNode node;
            int currentIndex = comboBox.SelectedIndex;
            int searchIndex;

            // Search from the current index to the end of the items
            for (searchIndex = currentIndex + 1; searchIndex < comboBox.Items.Count; searchIndex++)
            {
                node = (MemberTreeNode)comboBox.Items[searchIndex];

                if (NodeStartsWith(node, searchKey))
                {
                    comboBox.SelectedIndex = searchIndex;
                    return;
                }
            }

            // Search from the beginning of the items to the current index
            for (searchIndex = 0; searchIndex < currentIndex; searchIndex++)
            {
                node = (MemberTreeNode)comboBox.Items[searchIndex];

                if (NodeStartsWith(node, searchKey))
                {
                    comboBox.SelectedIndex = searchIndex;
                    return;
                }
            }

            // If searching the beginning of the nodes' names has failed, then run through
            // the full list from top to bottom and search through nodes' full name.
            if (Settings.DropDownFullWordSearchEnabled)
            {
                int itemCount = comboBox.Items.Count;
                for (searchIndex = 0; searchIndex < itemCount; ++searchIndex)
                {
                    node = (MemberTreeNode)comboBox.Items[searchIndex];

                    if (NodeContains(node, searchKey))
                    {
                        comboBox.SelectedIndex = searchIndex;
                        return;
                    }
                }
            }
        }

        static void ReverseSearch(ComboBox comboBox, string searchKey)
        {
            MemberTreeNode node;
            int currentIndex = comboBox.SelectedIndex;
            int searchIndex;

            // Search from the current index to the beginning of the items
            for (searchIndex = currentIndex - 1; searchIndex >= 0; searchIndex--)
            {
                node = (MemberTreeNode)comboBox.Items[searchIndex];

                if (NodeStartsWith(node, searchKey))
                {
                    comboBox.SelectedIndex = searchIndex;
                    return;
                }
            }

            // Search from the end of the items to the current index
            for (searchIndex = comboBox.Items.Count - 1; searchIndex > currentIndex; searchIndex--)
            {
                node = (MemberTreeNode)comboBox.Items[searchIndex];

                if (NodeStartsWith(node, searchKey))
                {
                    comboBox.SelectedIndex = searchIndex;
                    return;
                }
            }

            // If searching the beginning of the nodes' names has failed, then run through
            // the full list from bottom to top and search through nodes' full name.
            if (Settings.DropDownFullWordSearchEnabled)
            {
                for (searchIndex = comboBox.Items.Count; searchIndex >= 0; --searchIndex)
                {
                    node = (MemberTreeNode)comboBox.Items[searchIndex];

                    if (NodeContains(node, searchKey))
                    {
                        comboBox.SelectedIndex = searchIndex;
                        return;
                    }
                }
            }
        }

        static string GetNodeSearchString(MemberTreeNode node)
        {
            // If node navigates to a class then ignore the package name
            if (node is ClassTreeNode || node is ImportTreeNode || node is InheritedClassTreeNode)
                return Settings.IgnoreUnderscore ? node.Model.Name.TrimStart('_') : node.Model.Name;

            return Settings.IgnoreUnderscore ? node.Label.TrimStart('_') : node.Label;
        }

        static bool NodeStartsWith(MemberTreeNode node, string searchKey)
        {
            return GetNodeSearchString(node).StartsWith(searchKey, StringComparison.CurrentCultureIgnoreCase);
        }

        static bool NodeContains(MemberTreeNode node, string searchKey)
        {
            // Using IndexOf instead of Contains so that the IgnoreCase can be included
            return GetNodeSearchString(node).IndexOf(searchKey, 0, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        static void dropDownSearchTimer_Tick(object sender, EventArgs e)
        {
            _dropDownSearchKey = "";
            _dropDownSearchTimer.Stop();
        }
    }
}