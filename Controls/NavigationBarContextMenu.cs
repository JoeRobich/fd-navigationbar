using ASCompletion.Settings;
using NavigationBar.Helpers;
using PluginCore.Localization;
using System;
using System.Windows.Forms;

namespace NavigationBar.Controls
{
    internal class NavigationBarContextMenu : ContextMenuStrip
    {
        Settings _settings = null;

        ToolStripMenuItem _showImportDropDownItem;
        ToolStripMenuItem _showSuperClassesItem;
        ToolStripMenuItem _showInheritedMembersItem;

        ToolStripMenuItem _sortNoneItem;
        ToolStripMenuItem _sortSortedItem;
        ToolStripMenuItem _sortByKindItem;
        ToolStripMenuItem _sortSmartItem;

        public NavigationBarContextMenu(Settings settings)
        {
            _settings = settings;

            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            Renderer = new DockPanelStripRenderer();

            _showImportDropDownItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.ShowImportedClasses"), null, new EventHandler(ShowImportsDropDown));
            _showSuperClassesItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.ShowSuperClasses"), null, new EventHandler(ShowSuperClasses));
            _showInheritedMembersItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.ShowInheritedMembers"), null, new EventHandler(ShowInheritedMembers));

            ToolStripMenuItem sortItem = new ToolStripMenuItem(TextHelper.GetString("ASCompletion.Outline.SortingMode"));

            _sortNoneItem = new ToolStripMenuItem(TextHelper.GetString("ASCompletion.Outline.SortNone"), null, new EventHandler(SortNone));
            _sortSortedItem = new ToolStripMenuItem(TextHelper.GetString("ASCompletion.Outline.SortDefault"), null, new EventHandler(SortSorted));
            _sortByKindItem = new ToolStripMenuItem(TextHelper.GetString("ASCompletion.Outline.SortedByKind"), null, new EventHandler(SortByKind));
            _sortSmartItem = new ToolStripMenuItem(TextHelper.GetString("ASCompletion.Outline.SortedSmart"), null, new EventHandler(SmartSort));

            sortItem.DropDownItems.Add(_sortNoneItem);
            sortItem.DropDownItems.Add(_sortSortedItem);
            sortItem.DropDownItems.Add(_sortByKindItem);
            sortItem.DropDownItems.Add(_sortSmartItem);

            Items.Add(_showImportDropDownItem);
            Items.Add(_showSuperClassesItem);
            Items.Add(_showInheritedMembersItem);
            Items.Add(new ToolStripSeparator());
            Items.Add(sortItem);
        }

        void ShowImportsDropDown(object sender, EventArgs e)
        {
            _settings.ShowImportedClasses = !_settings.ShowImportedClasses;
        }

        void ShowSuperClasses(object sender, EventArgs e)
        {
            _settings.ShowSuperClasses = !_settings.ShowSuperClasses;
        }

        void ShowInheritedMembers(object sender, EventArgs e)
        {
            _settings.ShowInheritedMembers = !_settings.ShowInheritedMembers;
        }

        void SortNone(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.None;
        }

        void SortSorted(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.Sorted;
        }

        void SortByKind(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.SortedByKind;
        }

        void SmartSort(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.SortedSmart;
        }

        public void UpdateContextMenu()
        {
            _showImportDropDownItem.Checked = _settings.ShowImportedClasses;
            _showSuperClassesItem.Checked = _settings.ShowSuperClasses;
            _showInheritedMembersItem.Checked = _settings.ShowInheritedMembers;
            UpdateSortMenu();
        }

        void UpdateSortMenu()
        {
            _sortNoneItem.Checked = _settings.MemberSortMethod == OutlineSorting.None ? true : false;
            _sortSortedItem.Checked = _settings.MemberSortMethod == OutlineSorting.Sorted ? true : false;
            _sortByKindItem.Checked = _settings.MemberSortMethod == OutlineSorting.SortedByKind ||
                                      _settings.MemberSortMethod == OutlineSorting.SortedGroup ? true : false;
            _sortSmartItem.Checked = _settings.MemberSortMethod == OutlineSorting.SortedSmart ? true : false;
        }
    }
}