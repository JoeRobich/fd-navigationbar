using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ASCompletion.Model;
using ScintillaNet;
using ASCompletion.Context;
using ASCompletion;
using ASCompletion.Completion;
using ASCompletion.Settings;
using NavigationBar.Helpers;
using PluginCore.Localization;
using PluginCore;

namespace NavigationBar.Controls
{
    public partial class NavigationBar : UserControl
    {
        private ImageList _icons = null;
        private bool _updating = false;
        private int _lastPosition = -1;
        private bool _textChanged = false;
        private bool _completeBuild = false;

        private Settings _settings = null;
        private MemberTreeNodeComparer _memberSort = null;

        private FileModel _fileModel = null;
        private ScintillaControl _scintilla = null;

        private TreeNode _lastSelectedClassNode = null;
        private TreeNode _lastSelectedMemberNode = null;

        private ToolStripMenuItem _showImportDropDownItem;
        private ToolStripMenuItem _showSuperClassesItem;
        private ToolStripMenuItem _showInheritedMembersItem;

        private ToolStripMenuItem _sortNoneItem;
        private ToolStripMenuItem _sortSortedItem;
        private ToolStripMenuItem _sortByKindItem;
        private ToolStripMenuItem _sortSmartItem;

        #region Initializing and Disposing

        public NavigationBar(Settings settings)
        {
            InitializeComponent();
            InitializeContextMenu();
            InitializeIcons();
            
            // We should remain invisible until we determine that we are in a code file.
            this.Visible = false;

            // We should be docked at the top of the container.
            this.Dock = DockStyle.Top;

            _scintilla = PluginBase.MainForm.CurrentDocument.SciControl;
            _fileModel = ASContext.Context.CurrentModel;

            _settings = settings;

            RefreshSettings();
            HookEvents();
            updateTimer.Start();
        }

        private void InitializeContextMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

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

            menu.Items.Add(_showImportDropDownItem);
            menu.Items.Add(_showSuperClassesItem);
            menu.Items.Add(_showInheritedMembersItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(sortItem);

            this.ContextMenuStrip = menu;
        }

        private void InitializeIcons()
        {
            //Pull the member icons from the resources;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ASCompletion.PluginUI));
            _icons = new ImageList();
            _icons.ImageStream = ((ImageListStreamer)(resources.GetObject("treeIcons.ImageStream")));
            _icons.TransparentColor = Color.Transparent;
            _icons.Images.SetKeyName(0, "FilePlain.png");
            _icons.Images.SetKeyName(1, "FolderClosed.png");
            _icons.Images.SetKeyName(2, "FolderOpen.png");
            _icons.Images.SetKeyName(3, "CheckAS.png");
            _icons.Images.SetKeyName(4, "QuickBuild.png");
            _icons.Images.SetKeyName(5, "Package.png");
            _icons.Images.SetKeyName(6, "Interface.png");
            _icons.Images.SetKeyName(7, "Intrinsic.png");
            _icons.Images.SetKeyName(8, "Class.png");
            _icons.Images.SetKeyName(9, "Variable.png");
            _icons.Images.SetKeyName(10, "VariableProtected.png");
            _icons.Images.SetKeyName(11, "VariablePrivate.png");
            _icons.Images.SetKeyName(12, "Const.png");
            _icons.Images.SetKeyName(13, "ConstProtected.png");
            _icons.Images.SetKeyName(14, "ConstPrivate.png");
            _icons.Images.SetKeyName(15, "Method.png");
            _icons.Images.SetKeyName(16, "MethodProtected.png");
            _icons.Images.SetKeyName(17, "MethodPrivate.png");
            _icons.Images.SetKeyName(18, "Property.png");
            _icons.Images.SetKeyName(19, "PropertyProtected.png");
            _icons.Images.SetKeyName(20, "PropertyPrivate.png");
            _icons.Images.SetKeyName(21, "Template.png");
            _icons.Images.SetKeyName(22, "Declaration.png");
        }

        private void Remove()
        {
            // Unhook events then remove and dispose of ourselves
            UnhookEvents();
            this.Parent.Controls.Remove(this);
            this.Dispose();
        }

        #endregion

        #region Context Menu Handlers

        private void ShowImportsDropDown(object sender, EventArgs e)
        {
            _settings.ShowImportedClasses = !_settings.ShowImportedClasses;
        }

        private void ShowSuperClasses(object sender, EventArgs e)
        {
            _settings.ShowSuperClasses = !_settings.ShowSuperClasses;
        }

        private void ShowInheritedMembers(object sender, EventArgs e)
        {
            _settings.ShowInheritedMembers = !_settings.ShowInheritedMembers;
        }

        private void SortNone(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.None;
        }

        private void SortSorted(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.Sorted;
        }

        private void SortByKind(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.SortedByKind;
        }

        private void SmartSort(object sender, EventArgs e)
        {
            _settings.MemberSortMethod = OutlineSorting.SortedSmart;
        }

        private void UpdateContextMenu()
        {
            _showImportDropDownItem.Checked = _settings.ShowImportedClasses;
            _showSuperClassesItem.Checked = _settings.ShowSuperClasses;
            _showInheritedMembersItem.Checked = _settings.ShowInheritedMembers;
            UpdateSortMenu();
        }

        private void UpdateSortMenu()
        {
            _sortNoneItem.Checked = _settings.MemberSortMethod == OutlineSorting.None ? true : false;
            _sortSortedItem.Checked = _settings.MemberSortMethod == OutlineSorting.Sorted ? true : false;
            _sortByKindItem.Checked = _settings.MemberSortMethod == OutlineSorting.SortedByKind ||
                                      _settings.MemberSortMethod == OutlineSorting.SortedGroup ? true : false;
            _sortSmartItem.Checked = _settings.MemberSortMethod == OutlineSorting.SortedSmart ? true : false;            
        }

        #endregion

        #region Public methods

        public void OpenImports()
        {
            if (_settings.ShowImportedClasses)
            {
                if (classComboBox.DroppedDown)
                    classComboBox.DroppedDown = false;
                else if (memberComboBox.DroppedDown)
                    memberComboBox.DroppedDown = false;

                importComboBox.Focus();
                importComboBox.DroppedDown = true;
            }
        }

        public void OpenClasses()
        {
            if (importComboBox.DroppedDown)
                importComboBox.DroppedDown = false;
            else if (memberComboBox.DroppedDown)
                memberComboBox.DroppedDown = false;

            classComboBox.Focus();
            classComboBox.DroppedDown = true;
        }

        public void OpenMembers()
        {
            if (importComboBox.DroppedDown)
                importComboBox.DroppedDown = false;
            else if (classComboBox.DroppedDown)
                classComboBox.DroppedDown = false;

            memberComboBox.Focus();
            memberComboBox.DroppedDown = true;
        }

        #endregion

        #region Event Hooks and Handlers

        private void _settings_OnSettingsChanged()
        {
            RefreshSettings();
        }

        private void HookEvents()
        {
            // Check for whether the cursor has moved
            _scintilla.UpdateUI += new UpdateUIHandler(_scintella_UpdateUI);

            // The code has changed so we will need to rebuild the dropdowns
            _scintilla.TextInserted += new TextInsertedHandler(_scintilla_TextChanged);
            _scintilla.TextDeleted += new TextDeletedHandler(_scintilla_TextChanged);

            _settings.OnSettingsChanged += new SettingsChangesEvent(_settings_OnSettingsChanged);
        }

        private void UnhookEvents()
        {
            // We are not in a code file so we should unhook
            _scintilla.UpdateUI -= new UpdateUIHandler(_scintella_UpdateUI);
            _scintilla.TextInserted -= new TextInsertedHandler(_scintilla_TextChanged);
            _scintilla.TextDeleted -= new TextDeletedHandler(_scintilla_TextChanged);

            _settings.OnSettingsChanged += new SettingsChangesEvent(_settings_OnSettingsChanged);
        }

        void _scintilla_TextChanged(ScintillaControl sender, int position, int length, int linesAdded)
        {
            // The text has changed start checking for the model to update
            _textChanged = true;
            updateTimer.Start();
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            // The text has changed and the model may have updated
            UpdateNavigationBar();
        }

        void _scintella_UpdateUI(ScintillaControl sender)
        {
            // The caret may have moved 
            UpdateNavigationBar();
        }

        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            // If we are not updating and the combobox has a selected item, then
            // navigate to it
            if (!_updating && comboBox.SelectedItem != null)
                NavigateToMemberTreeNode(comboBox.SelectedItem as MemberTreeNode);
        }

        private void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            _scintilla.Focus();
            ResetDropDowns();
        }

        private void comboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string searchKey = e.KeyChar.ToString();

            // If shift is pressed then reverse search
            if ((Control.ModifierKeys & Keys.Shift) == 0)
            {
                ForwardSearch(comboBox, searchKey);
            }
            else
            {
                ReverseSearch(comboBox, searchKey);
            }
        }

        private void comboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If we drawing an item that exists
            if (e.Index > -1)
            {
                ComboBox comboBox = sender as ComboBox;
                MemberTreeNode node = comboBox.Items[e.Index] as MemberTreeNode;

                // Clear the old background
                e.Graphics.FillRectangle(new SolidBrush(comboBox.BackColor), e.Bounds.Left + 16, e.Bounds.Top, e.Bounds.Width - 16, e.Bounds.Height);

                // Draw the item image
                e.Graphics.DrawImage(_icons.Images[node.ImageIndex], new Point(e.Bounds.Left, e.Bounds.Top));

                // Is this item being hovered over?
                if ((e.State & DrawItemState.Focus) != 0 ||
                    (e.State & DrawItemState.Selected) != 0)
                {
                    // Draw a selection box and label in the selection text color
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds.Left + 16, e.Bounds.Top, e.Bounds.Width - 17, e.Bounds.Height - 1);
                    e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), e.Bounds.Left + 16, e.Bounds.Top, e.Bounds.Width - 17, e.Bounds.Height - 1);

                    e.Graphics.DrawString(node.Label, comboBox.Font, new SolidBrush(SystemColors.HighlightText), new Point(e.Bounds.Left + 17, e.Bounds.Top));
                }
                // Is this item disabled?
                else if ((e.State & DrawItemState.Disabled) != 0)
                {
                    // Draw the label in the disabled text color
                    e.Graphics.DrawString(node.Label, comboBox.Font, new SolidBrush(SystemColors.GrayText), new Point(e.Bounds.Left + 17, e.Bounds.Top));
                }
                // Is this item inherited?
                else if (node is InheritedMemberTreeNode)
                {
                    // Draw the label in the disabled text color
                    e.Graphics.DrawString(node.Label, comboBox.Font, new SolidBrush(Color.Gray), new Point(e.Bounds.Left + 17, e.Bounds.Top));
                }
                else
                {
                    // Draw the label in the foreground color
                    e.Graphics.DrawString(node.Label, comboBox.Font, new SolidBrush(comboBox.ForeColor), new Point(e.Bounds.Left + 17, e.Bounds.Top));
                }
            }
        }

        #endregion

        #region Build DropDown Methods

        private void ShowImportDropDown()
        {
            tablePanel.ColumnStyles[0].SizeType = SizeType.Percent;
            tablePanel.ColumnStyles[0].Width = 33.33f;
        }

        private void HideImportDropDown()
        {
            tablePanel.ColumnStyles[0].SizeType = SizeType.Absolute;
            tablePanel.ColumnStyles[0].Width = 0f;
        }

        private void BuildDropDowns()
        {
            BuildImportDropDown();
            BuildClassDropDown();
            BuildMemberDropDown();
        }

        private void BuildImportDropDown()
        {
            importComboBox.Items.Clear();

            if (ASContext.Context.CurrentModel != null && _settings.ShowImportedClasses)
            {
                List<MemberTreeNode> importNodes = new List<MemberTreeNode>();

                // Add all the imported classes from this file
                foreach (MemberModel importModel in ASContext.Context.CurrentModel.Imports)
                {
                    // ignore package imports
                    if (!importModel.Type.EndsWith(".*"))
                    {
                        ClassModel classModel = ASContext.Context.ResolveType(importModel.Type, ASContext.Context.CurrentModel);
                        if (!classModel.IsVoid() && classModel.InFile != null)
                        {
                            MemberTreeNode node = GetClassTreeNode(classModel, false, true);
                            importNodes.Add(node);
                        }
                    }
                }

                // Apply member sort
                if (_memberSort != null)
                    importNodes.Sort(_memberSort);

                importComboBox.Items.AddRange(importNodes.ToArray());
            }
        }

        private void BuildClassDropDown()
        {
            classComboBox.Items.Clear();

            if (ASContext.Context.CurrentModel != null)
            {
                List<MemberTreeNode> classNodes = new List<MemberTreeNode>();
                List<string> classNames = new List<string>();

                // Add all the classes from this file
                foreach (ClassModel classModel in ASContext.Context.CurrentModel.Classes)
                {
                    MemberTreeNode node = GetClassTreeNode(classModel, false, false);
                    classNodes.Add(node);

                    if (_settings.ShowSuperClasses)
                    {
                        // While extended class is not null, Object, Void, or haXe Dynamic
                        var extendClassModel = classModel.Extends;
                        while (extendClassModel != null && 
                               extendClassModel.Name != "Object" && 
                               extendClassModel != ClassModel.VoidClass &&
                               (!extendClassModel.InFile.haXe || extendClassModel.Type != "Dynamic"))
                         {
                            // Have we already added this class? Multiple classes could extend the same base.
                            if (classNames.Contains(extendClassModel.QualifiedName))
                                break;
                            classNames.Add(extendClassModel.QualifiedName);

                            node = GetClassTreeNode(extendClassModel, true, false);
                            classNodes.Add(node);

                            extendClassModel = extendClassModel.Extends;
                        }
                    }
                }

                // Apply member sort
                if (_memberSort != null)
                    classNodes.Sort(_memberSort);

                classComboBox.Items.AddRange(classNodes.ToArray());
            }

            // Select the class that contains the caret
            UpdateClassDropDown();
        }

        private MemberTreeNode GetClassTreeNode(ClassModel classModel, bool isInherited, bool isImported)
        {
            int imageNum = ((classModel.Flags & FlagType.Intrinsic) > 0) ? ASCompletion.PluginUI.ICON_INTRINSIC_TYPE :
                           ((classModel.Flags & FlagType.Interface) > 0) ? ASCompletion.PluginUI.ICON_INTERFACE : ASCompletion.PluginUI.ICON_TYPE;
            return isInherited ? new InheritedClassTreeNode(classModel, imageNum, _settings.ShowQualifiedClassName) :
                   isImported ? new ImportTreeNode(classModel, imageNum, _settings.ShowQualifiedClassName) :
                                new ClassTreeNode(classModel, imageNum, _settings.ShowQualifiedClassName) as MemberTreeNode;
        }

        private void BuildMemberDropDown()
        {
            memberComboBox.Items.Clear();

            MemberList members = null;
            ClassTreeNode classTreeNode = classComboBox.SelectedItem as ClassTreeNode;
            ClassModel classModel = null;

            if (ASContext.Context.CurrentModel != null)
            {
                List<MemberTreeNode> memberNodes = new List<MemberTreeNode>();

                if (classTreeNode == null)
                {
                    // The caret is not within a class, so add the global members
                    members = ASContext.Context.CurrentModel.Members;
                }
                else
                {
                    // The caret is within a class, so add the classes members
                    classModel = (ClassModel)classTreeNode.Model;
                    members = classModel.Members;
                }

                // Add the local members
                foreach (MemberModel member in members)
                {
                    MemberTreeNode node = GetMemberTreeNode(member, null);

                    if (node != null)
                        memberNodes.Add(node);
                }

                // Add inherited members if applicable
                if (_settings.ShowInheritedMembers && classModel != null)
                    memberNodes.AddRange(GetInheritedMembers(classModel.Extends));

                // Apply member sort
                if (_memberSort != null)
                    memberNodes.Sort(_memberSort);

                memberComboBox.Items.AddRange(memberNodes.ToArray());
            }

            // Select the member that contains the caret
            UpdateMemberDropDown();
        }

        private List<MemberTreeNode> GetInheritedMembers(ClassModel classModel)
        {
            List<MemberTreeNode> memberNodes = new List<MemberTreeNode>();

            // Add members from our super class as long as it is not null, Object, Void, or haXe Dynamic
            while (classModel != null &&
                   classModel.Name != "Object" &&
                   classModel != ClassModel.VoidClass &&
                   (!classModel.InFile.haXe || classModel.Type != "Dynamic"))
            {
                MemberList members = classModel.Members;

                foreach (MemberModel member in members)
                {
                    MemberTreeNode node = GetMemberTreeNode(member, classModel);

                    if (node != null)
                    {
                        memberNodes.Add(node);
                    }
                }

                // Follow the inheritence chain down
                classModel = classModel.Extends;
            }

            return memberNodes;
        }

        private MemberTreeNode GetMemberTreeNode(MemberModel memberModel, ClassModel classModel)
        {
            MemberTreeNode node = null;
            int imageIndex = ASCompletion.PluginUI.GetMemberIcon(memberModel.Flags, memberModel.Access);

            if (imageIndex != 0)
            {
                node = classModel == null ? new MemberTreeNode(memberModel, imageIndex, _settings.LabelPropertiesLikeFunctions) :
                                            new InheritedMemberTreeNode(classModel, memberModel, imageIndex, _settings.LabelPropertiesLikeFunctions);
            }

            return node;
        }

        #endregion

        #region Update DropDown Methods

        private void UpdateDropDowns()
        {
            UpdateClassDropDown();
            UpdateMemberDropDown();
        }

        private void UpdateClassDropDown()
        {
            MemberTreeNode selectedNode = null;
            bool singleClassContext = false;

            // Check to see if there is only one class in this file
            if (ASContext.Context.CurrentModel != null)
            {
                if (ASContext.Context.CurrentModel.Classes.Count == 1 &&
                    ASContext.Context.CurrentModel.Members.Count == 0)
                {
                    singleClassContext = true;
                }
            }

            // get the line the caret is on
            int line = _scintilla.LineFromPosition(_scintilla.CurrentPos);

            foreach (MemberTreeNode classNode in classComboBox.Items)
            {
                // if the caret is within the lines of the class, then select it
                if (!(classNode is InheritedClassTreeNode) &&
                    (singleClassContext ||
                    (line >= classNode.Model.LineFrom && line <= classNode.Model.LineTo)))
                {
                    selectedNode = classNode;
                    break;
                }
            }

            if (_lastSelectedClassNode != selectedNode ||
                classComboBox.SelectedItem != selectedNode)
            {
                // Update the combobox with the new selected node
                _lastSelectedClassNode = selectedNode;
                classComboBox.SelectedItem = selectedNode;

                // Update the members to match the new class
                BuildMemberDropDown();
            }
        }

        private void UpdateMemberDropDown()
        {
            MemberTreeNode currentMemberNode = memberComboBox.SelectedItem as MemberTreeNode;
            MemberTreeNode selectedNode = null;

            // get the line the caret is on
            int line = _scintilla.LineFromPosition(_scintilla.CurrentPos);

            foreach (MemberTreeNode memberNode in memberComboBox.Items)
            {
                // if the member is in this code file and the caret is within the lines of the member,
                // then select it
                if (!(memberNode is InheritedMemberTreeNode) &&
                    (line >= memberNode.Model.LineFrom && line <= memberNode.Model.LineTo))
                {
                    selectedNode = memberNode;
                    break;
                }
            }

            if (_lastSelectedClassNode != selectedNode ||
                memberComboBox.SelectedItem != selectedNode)
            {
                // Update the combobox with the new selected node
                _lastSelectedMemberNode = selectedNode;
                memberComboBox.SelectedItem = selectedNode;
            }
        }

        private void ResetDropDowns()
        {
            _updating = true;
            importComboBox.SelectedItem = null;
            classComboBox.SelectedItem = _lastSelectedClassNode;
            memberComboBox.SelectedItem = _lastSelectedMemberNode;
            _updating = false;
        }

        #endregion

        #region Node Search Methods

        private void ForwardSearch(ComboBox comboBox, string searchKey)
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
        }

        private void ReverseSearch(ComboBox comboBox, string searchKey)
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
        }

        private bool NodeStartsWith(MemberTreeNode node, string searchKey)
        {
            string searchString;

            // If node navigates to a class then ignore the package name
            if (node is ClassTreeNode || node is ImportTreeNode || node is InheritedClassTreeNode)
                searchString = _settings.IgnoreUnderscore ? node.Model.Name.TrimStart('_') : node.Model.Name;
            else
                searchString = _settings.IgnoreUnderscore ? node.Label.TrimStart('_') : node.Label;

            return searchString.StartsWith(searchKey, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion

        #region Other Methods

        public void RefreshSettings()
        {
            UpdateContextMenu();

            _memberSort = MemberTreeNodeComparer.GetComparer(_settings.MemberSortMethod);

            // Show the imported dropdown if it is not visible
            if (_settings.ShowImportedClasses && tablePanel.ColumnStyles[0].SizeType == SizeType.Absolute)
                ShowImportDropDown();
            // Hide the imported dropdown if it is visible
            else if (!_settings.ShowImportedClasses && tablePanel.ColumnStyles[0].SizeType == SizeType.Percent)
                HideImportDropDown();

            // Forces a rebuild of the dropdowns
            _textChanged = true;
            updateTimer.Start();
        }

        private void UpdateNavigationBar()
        {
            // Only update if we are the visible document
            if (PluginBase.MainForm.CurrentDocument.SciControl != _scintilla)
            {
                _textChanged = false;
                updateTimer.Stop();
                return;
            }

            _updating = true;

            // If we are not visible then we should see if we belong in this document
            if (!this.Visible)
            {
                // Only display the navigation bar if we are a code file
                if (ASContext.Context.CurrentModel != FileModel.Ignore)
                {
                    this.Visible = true;
                }
                else
                {
                    Remove();
                    return;
                }
            }

            // If we haven't performed a build after dependencies have been resolved
            if (!_completeBuild &&
                ASContext.Context.CurrentModel.GetPublicClass().Extends != ClassModel.VoidClass)
            {
                _completeBuild = true;

                // If the text has changed and we need to rebuild or
                // We need to show inherited classes or members
                if (_textChanged ||
                    _settings.ShowImportedClasses ||
                    _settings.ShowInheritedMembers ||
                    _settings.ShowSuperClasses)
                {
                    _textChanged = false;
                    updateTimer.Stop();
                    BuildDropDowns();
                }
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
            if (_scintilla.CurrentPos != _lastPosition)
            {
                _lastPosition = _scintilla.CurrentPos;
                UpdateDropDowns();
            }

            _updating = false;
        }

        #endregion

        #region Navigate Methods

        private void NavigateToMemberTreeNode(MemberTreeNode node)
        {
            if (node is InheritedMemberTreeNode)
            {
                InheritedMemberTreeNode inheritedNode = (InheritedMemberTreeNode)node;
                FileModel model = ModelsExplorer.Instance.OpenFile(inheritedNode.ClassModel.InFile.FileName);

                if (!(node is InheritedClassTreeNode))
                {
                    // We have to update the Tag to reflect the line number the member starts on
                    inheritedNode.Tag = GetInheritedMemberTag(model, inheritedNode.Model.Name);
                }
            }
            else if (node is ImportTreeNode)
            {
                ImportTreeNode importNode = (ImportTreeNode)node;
                ClassModel importModel = (ClassModel)importNode.Model;
                if (!importModel.IsVoid() && importModel.InFile != null)
                    ModelsExplorer.Instance.OpenFile(importModel.InFile.FileName);
            }

            // Navigate to node location
            ASContext.Context.OnSelectOutlineNode(node);

            // If navigating to an inherited class or member, we need to reset our combobox
            if (node is InheritedMemberTreeNode || node is ImportTreeNode)
                ResetDropDowns();
        }

        private string GetInheritedMemberTag(FileModel model, string memberName)
        {
            // Search for the member and return it's location
            foreach (ClassModel classModel in model.Classes)
                foreach (MemberModel memberModel in classModel.Members)
                    if (memberName == memberModel.Name)
                        return memberModel.Name + "@" + memberModel.LineFrom;
            return string.Empty;
        }

        #endregion
    }

    #region Custom Structures

    class MemberTreeNode : TreeNode
    {
        protected string _label = "";
        protected MemberModel _model = null;

        public MemberTreeNode(MemberModel memberModel, int imageIndex, bool labelPropertiesLikeFunctions)
            : base(memberModel.ToString(), imageIndex, imageIndex)
        {
            if (labelPropertiesLikeFunctions &&
                (memberModel.Flags & (FlagType.Setter | FlagType.Getter)) != 0)
            {
                List<string> paramList = new List<string>();
                if (memberModel.Parameters != null)
                    foreach (var param in memberModel.Parameters)
                        paramList.Add(string.Format("{0}:{1}", param.Name, param.Type));

                _label = string.Format("{0} ({1}) : {2}", memberModel.Name, string.Join(", ", paramList.ToArray()), memberModel.Type);
            }
            else
            {
                _label = this.Text;
            }
            _model = memberModel;
            Tag = memberModel.Name + "@" + memberModel.LineFrom;
        }

        public MemberModel Model
        {
            get
            {
                return _model;
            }
        }

        public string Label
        {
            get
            {
                return _label;
            }
        }
    }

    class InheritedMemberTreeNode : MemberTreeNode
    {
        protected ClassModel _classModel = null;

        public InheritedMemberTreeNode(ClassModel classModel, MemberModel memberModel, int imageIndex, bool labelPropertiesLikeFunctions)
            : base(memberModel, imageIndex, labelPropertiesLikeFunctions)
        {
            _label = this.Text + " - " + classModel.Name;
            _classModel = classModel;
        }

        public ClassModel ClassModel
        {
            get
            {
                return _classModel;
            }
        }
    }

    class ImportTreeNode : MemberTreeNode
    {
        public ImportTreeNode(ClassModel importModel, int imageIndex, bool showQualifiedClassNames)
            : base(importModel, imageIndex, false)
        {
            Text = importModel.Name;
            Tag = "class";
            _label = showQualifiedClassNames ? importModel.QualifiedName : importModel.Name;
        }
    }

    class ClassTreeNode : MemberTreeNode
    {
        public ClassTreeNode(ClassModel classModel, int imageIndex, bool showQualifiedClassNames)
            : base(classModel, imageIndex, false)
        {
            Text = classModel.Name;
            Tag = "class";
            _label = showQualifiedClassNames ? classModel.QualifiedName : classModel.Name;
        }
    }

    class InheritedClassTreeNode : InheritedMemberTreeNode
    {
        public InheritedClassTreeNode(ClassModel classModel, int imageIndex, bool showQualifiedClassNames)
            : base(classModel, classModel, imageIndex, false)
        {
            Text = classModel.Name;
            Tag = "class";
            _label = showQualifiedClassNames ? classModel.QualifiedName : classModel.Name;
        }
    }

    class MemberTreeNodeComparer : IComparer<MemberTreeNode>
    {
        private static MemberTreeNodeComparer _sortedComparer = new MemberTreeNodeComparer(null);
        private static MemberTreeNodeComparer _byKindComparer = new MemberTreeNodeComparer(new ByKindMemberComparer());
        private static MemberTreeNodeComparer _smartSortComparer = new MemberTreeNodeComparer(new SmartMemberComparer());

        private IComparer<MemberModel> _memberModelComparer;

        public static MemberTreeNodeComparer GetComparer(OutlineSorting outlineSort)
        {
            MemberTreeNodeComparer memberSort = null;

            switch (outlineSort)
            {
                case OutlineSorting.Sorted:
                    memberSort = _sortedComparer;
                    break;
                case OutlineSorting.SortedByKind:
                case OutlineSorting.SortedGroup:
                    memberSort = _byKindComparer;
                    break;
                case OutlineSorting.SortedSmart:
                    memberSort = _smartSortComparer;
                    break;
            }

            return memberSort;
        }



        public MemberTreeNodeComparer(IComparer<MemberModel> memberModelComparer)
        {
            _memberModelComparer = memberModelComparer;
        }

        public int Compare(MemberTreeNode x, MemberTreeNode y)
        {
            return _memberModelComparer != null ? _memberModelComparer.Compare(x.Model, y.Model) :
                                                  x.Label.CompareTo(y.Label);
        }
    }

    #endregion
}