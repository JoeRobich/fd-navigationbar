using ASCompletion.Context;
using ASCompletion.Model;
using NavigationBar.Helpers;
using NavigationBar.Managers;
using PluginCore;
using PluginCore.Controls;
using PluginCore.Helpers;
using ScintillaNet;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NavigationBar.Controls
{
    public partial class NavigationBar : ToolStripEx, IDisposable
    {
        bool _updating = false;
        int _lastPosition = -1;
        bool _textChanged = false;
        bool _completeBuild = false;
        bool _disposed = false;

        NavigationBarContextMenu contextMenu;
        ToolStripSpringComboBox importComboBox;
        ToolStripSpringComboBox classComboBox;
        ToolStripSpringComboBox memberComboBox;
        Timer updateTimer;

        ITabbedDocument _document = null;
        Settings _settings = null;

        TreeNode _lastSelectedClassNode = null;
        TreeNode _lastSelectedMemberNode = null;

        public NavigationBar(ITabbedDocument document, Settings settings)
        {
            _document = document;

            _settings = settings;
            SearchHelper.Settings = settings;
            DropDownBuilder.Settings = settings;

            InitializeComponent();

            Renderer = new DockPanelStripRenderer(false);
            BackColor = PluginBase.MainForm.GetThemeColor("ToolStripComboBoxControl.BorderColor");

            RefreshSettings();
            HookEvents();

            updateTimer.Start();
        }

        private void InitializeComponent()
        {
            contextMenu = new NavigationBarContextMenu(_settings);
            importComboBox = new ToolStripSpringComboBox();
            classComboBox = new ToolStripSpringComboBox();
            memberComboBox = new ToolStripSpringComboBox();
            updateTimer = new Timer();

            SuspendLayout();

            // importComboBox
            importComboBox.FlatCombo.DrawMode = DrawMode.OwnerDrawFixed;
            importComboBox.FlatStyle = FlatStyle.System;
            importComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            importComboBox.FlatCombo.MaxDropDownItems = 25;
            importComboBox.Name = "importComboBox";
            importComboBox.FlatCombo.DrawItem += ItemRenderer.ComboBox_DrawItem;
            importComboBox.FlatCombo.SelectionChangeCommitted += new EventHandler(comboBox_SelectionChangeCommitted);
            importComboBox.KeyPress += SearchHelper.ComboBox_KeyPress;
            importComboBox.FlatCombo.DropDownClosed += new EventHandler(comboBox_DropDownClosed);
            // classComboBox
            classComboBox.FlatCombo.DrawMode = DrawMode.OwnerDrawFixed;
            classComboBox.FlatStyle = FlatStyle.System;
            classComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            classComboBox.FlatCombo.MaxDropDownItems = 25;
            classComboBox.Name = "classComboBox";
            classComboBox.FlatCombo.DrawItem += ItemRenderer.ComboBox_DrawItem;
            classComboBox.FlatCombo.SelectionChangeCommitted += new EventHandler(comboBox_SelectionChangeCommitted);
            classComboBox.KeyPress += SearchHelper.ComboBox_KeyPress;
            classComboBox.FlatCombo.DropDownClosed += new EventHandler(comboBox_DropDownClosed);
            // memberComboBox
            memberComboBox.FlatCombo.DrawMode = DrawMode.OwnerDrawFixed;
            memberComboBox.FlatStyle = FlatStyle.System;
            memberComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            memberComboBox.FlatCombo.MaxDropDownItems = 25;
            memberComboBox.Name = "memberComboBox";
            memberComboBox.FlatCombo.DrawItem += ItemRenderer.ComboBox_DrawItem;
            memberComboBox.FlatCombo.SelectionChangeCommitted += new EventHandler(comboBox_SelectionChangeCommitted);
            memberComboBox.KeyPress += SearchHelper.ComboBox_KeyPress;
            memberComboBox.FlatCombo.DropDownClosed += new EventHandler(comboBox_DropDownClosed);
            // updateTimer
            updateTimer.Interval = 500;
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            // NavigationBar
            CanOverflow = false;
            ContextMenuStrip = contextMenu;
            Dock = DockStyle.Top;
            GripStyle = ToolStripGripStyle.Hidden;
            Items.AddRange(new ToolStripItem[] { importComboBox, classComboBox, memberComboBox });
            Name = "NavigationBar";
            Padding = new Padding(0, ScaleHelper.Scale(1), 0, ScaleHelper.Scale(1));
            Stretch = true;
            Visible = false;

            ResumeLayout(false);
        }

        public void Dispose()
        {
            // Unhook events and dispose of ourselves
            updateTimer.Stop();
            UnhookEvents();
            base.Dispose();
        }

        private void Remove()
        {
            // Remove and dispose of ourselves
            if (Parent != null)
                Parent.Controls.Remove(this);
            Dispose();
        }

        public void OpenImports()
        {
            if (_settings.ShowImportedClasses)
            {
                if (classComboBox.FlatCombo.DroppedDown)
                    classComboBox.FlatCombo.DroppedDown = false;
                else if (memberComboBox.FlatCombo.DroppedDown)
                    memberComboBox.FlatCombo.DroppedDown = false;

                importComboBox.Focus();
                importComboBox.FlatCombo.DroppedDown = true;
            }
        }

        public void OpenClasses()
        {
            if (importComboBox.FlatCombo.DroppedDown)
                importComboBox.FlatCombo.DroppedDown = false;
            else if (memberComboBox.FlatCombo.DroppedDown)
                memberComboBox.FlatCombo.DroppedDown = false;

            classComboBox.Focus();
            classComboBox.FlatCombo.DroppedDown = true;
        }

        public void OpenMembers()
        {
            if (importComboBox.FlatCombo.DroppedDown)
                importComboBox.FlatCombo.DroppedDown = false;
            else if (classComboBox.FlatCombo.DroppedDown)
                classComboBox.FlatCombo.DroppedDown = false;

            memberComboBox.Focus();
            memberComboBox.FlatCombo.DroppedDown = true;
        }

        private void HookEvents()
        {
            NavigationManager.Instance.LocationChanged += NavigationManager_LocationChanged;

            // The code has changed so we will need to rebuild the dropdowns
            _document.SplitSci1.TextInserted += _scintilla_TextChanged;
            _document.SplitSci1.TextDeleted += _scintilla_TextChanged;

            _settings.OnSettingsChanged += _settings_OnSettingsChanged;
        }

        private void UnhookEvents()
        {
            NavigationManager.Instance.LocationChanged -= NavigationManager_LocationChanged;

            _document.SplitSci1.TextInserted -= _scintilla_TextChanged;
            _document.SplitSci1.TextDeleted -= _scintilla_TextChanged;

            _settings.OnSettingsChanged -= _settings_OnSettingsChanged;
        }

        private void NavigationManager_LocationChanged(object sender, EventArgs e)
        {
            UpdateNavigationBar();
        }

        private void _settings_OnSettingsChanged()
        {
            RefreshSettings();
        }

        void _scintilla_TextChanged(ScintillaControl sender, int position, int length, int linesAdded)
        {
            // The text has changed start checking for the model to update
            _textChanged = true;
            updateTimer.Stop();
            updateTimer.Start();
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            // The text has changed and the model may have updated
            UpdateNavigationBar();
        }

        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            // If we are updating or the combobox has no selected item, then return
            if (_updating || comboBox.SelectedItem == null)
                return;

            var node = comboBox.SelectedItem as MemberTreeNode;
            NavigationHelper.NavigateTo(node);

            // If navigating to an inherited class or member, we need to reset our combobox
            if (node is InheritedMemberTreeNode || node is ImportTreeNode)
                ResetDropDowns();
        }

        private void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            SearchHelper.Reset();

            _document.SciControl.Focus();
            ResetDropDowns();
        }

        private void ShowImportDropDown()
        {
            if (!Items.Contains(importComboBox))
                Items.Insert(0, importComboBox);
        }

        private void HideImportDropDown()
        {
            if (Items.Contains(importComboBox))
                Items.Remove(importComboBox);
        }

        private void BuildDropDowns()
        {
            DropDownBuilder.BuildImportDropDown(importComboBox);
            DropDownBuilder.BuildClassDropDown(classComboBox);
            DropDownBuilder.BuildMemberDropDown(classComboBox, memberComboBox);
            UpdateDropDowns();
        }

        private void UpdateDropDowns()
        {
            _updating = true;
            UpdateClassDropDown();
            UpdateMemberDropDown();
            _updating = false;
        }

        private void UpdateClassDropDown()
        {
            var singleClassContext = false;

            // Check to see if there is only one class in this file
            if (ASContext.Context.CurrentModel != null)
            {
                if (ASContext.Context.CurrentModel.Classes.Count == 1 &&
                    ASContext.Context.CurrentModel.Members.Count == 0)
                    singleClassContext = true;
            }

            // get the line the caret is on
            int line = _document.SciControl.LineFromPosition(_document.SciControl.CurrentPos);

            var selectedNode = classComboBox.Items
                .OfType<MemberTreeNode>()
                .Where(node => !(node is InheritedClassTreeNode))
                .FirstOrDefault(node => singleClassContext ||
                    (line >= node.Model.LineFrom && line <= node.Model.LineTo));

            if (_lastSelectedClassNode == selectedNode &&
                classComboBox.SelectedItem == selectedNode)
                return;

            // Update the combobox with the new selected node
            _lastSelectedClassNode = selectedNode;
            classComboBox.SelectedItem = selectedNode;

            // Update the members to match the new class
            DropDownBuilder.BuildMemberDropDown(classComboBox, memberComboBox);
            UpdateMemberDropDown();
        }

        private void UpdateMemberDropDown()
        {
            // get the line the caret is on
            int line = _document.SciControl.LineFromPosition(_document.SciControl.CurrentPos);

            var selectedNode = memberComboBox.Items
                .OfType<MemberTreeNode>()
                .Where(node => !(node is InheritedMemberTreeNode))
                .FirstOrDefault(node => line >= node.Model.LineFrom && line <= node.Model.LineTo);

            if (_lastSelectedClassNode == selectedNode &&
                memberComboBox.SelectedItem == selectedNode)
                return;

            // Update the combobox with the new selected node
            _lastSelectedMemberNode = selectedNode;
            memberComboBox.SelectedItem = selectedNode;
        }

        private void ResetDropDowns()
        {
            _updating = true;
            importComboBox.SelectedItem = null;
            classComboBox.SelectedItem = _lastSelectedClassNode;
            memberComboBox.SelectedItem = _lastSelectedMemberNode;
            _updating = false;
        }

        public void RefreshSettings()
        {
            contextMenu.UpdateContextMenu();

            if (_settings.ShowImportedClasses)
                ShowImportDropDown();
            else if (!_settings.ShowImportedClasses)
                HideImportDropDown();

            // Forces a rebuild of the dropdowns
            _textChanged = true;
            updateTimer.Start();
        }

        public void ApplyTheme()
        {
            PluginBase.MainForm.ThemeControls(this);

            var backColor = PluginBase.MainForm.GetThemeColor("ToolStripComboBoxControl.BorderColor");
            if (BackColor != Color.Empty)
                BackColor = backColor;
        }

        private void UpdateNavigationBar()
        {
            // Only update if we are the visible document
            if (PluginBase.MainForm.CurrentDocument != _document)
            {
                _textChanged = false;
                updateTimer.Stop();
                return;
            }

            // If we are not visible then we should see if we belong in this document
            if (!Visible)
            {
                // Only display the navigation bar if we are a code file
                if (ASContext.Context.CurrentModel != FileModel.Ignore)
                {
                    Visible = true;
                }
                else
                {
                    Remove();
                    return;
                }
            }

            // If we haven't performed a build after dependencies have been resolved
            if (!_completeBuild &&
                ASContext.Context.CurrentModel.GetPublicClass() != ClassModel.VoidClass)
            {
                _completeBuild = true;

                _textChanged = false;
                updateTimer.Stop();
                BuildDropDowns();
            }
            // Rebuild the dropdowns if the text changed and the model has updated
            else if (_textChanged &&
                    ASContext.Context.CurrentModel != null &&
                    !ASContext.Context.CurrentModel.OutOfDate)
            {
                _textChanged = false;
                if (_completeBuild)
                    updateTimer.Stop();
                BuildDropDowns();
            }

            // Update the dropdowns if the caret position has changed
            if (_document.SciControl.CurrentPos != _lastPosition)
            {
                _lastPosition = _document.SciControl.CurrentPos;
                UpdateDropDowns();
            }
        }
    }
}