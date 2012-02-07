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

namespace NavigationBar
{
    public partial class NavigationBar : UserControl
    {
        private ImageList _icons = null;
        private bool _updating = false;
        private int _lastPosition = -1;
        private bool _textChanged = false;
        private bool _completeBuild = false;

        private bool _showImportedClasses = false;
        private bool _showSuperClasses = false;
        private bool _showInheritedMembers = false;
        private bool _showQualifiedClassNames = true;
        private MemberTreeNodeComparer _memberSort = null;
        private OutlineSorting _sortMethod = OutlineSorting.Sorted;

        private TreeNode _lastSelectedClassNode = null;
        private TreeNode _lastSelectedMemberNode = null;

        public NavigationBar(bool showImportedClasses, bool showSuperClasses, bool showInheritedMembers, bool showQualifiedClassNames, OutlineSorting sortMethod)
        {
            InitializeComponent();
            InitializeIcons();
            
            // We should remain invisible until we determine that we are in a code file.
            this.Visible = false;

            // We should be docked at the top of the container.
            this.Dock = DockStyle.Top;

            UpdateSettings(showImportedClasses, showSuperClasses, showInheritedMembers, showQualifiedClassNames, sortMethod);
            HookEvents();
        }

        public void OpenImports()
        {
            if (_showImportedClasses)
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

        public void UpdateSettings(bool showImportedClasses, bool showSuperClasses, bool showInheritedMembers, bool showQualifiedClassNames, OutlineSorting sortMethod)
        {
            _showImportedClasses = showImportedClasses;
            _showSuperClasses = showSuperClasses;
            _showInheritedMembers = showInheritedMembers;
            _showQualifiedClassNames = showQualifiedClassNames;
            _sortMethod = sortMethod;

            _memberSort = null;
            switch (_sortMethod)
            {
                case OutlineSorting.Sorted:
                    _memberSort = new MemberTreeNodeComparer(null);
                    break;
                case OutlineSorting.SortedByKind:
                case OutlineSorting.SortedGroup:
                    _memberSort = new MemberTreeNodeComparer(new ByKindMemberComparer());
                    break;
                case OutlineSorting.SortedSmart:
                    _memberSort = new MemberTreeNodeComparer(new SmartMemberComparer());
                    break;
            }

            // Forces a rebuild of the dropdowns
            _textChanged = true;
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

        private void HookEvents()
        {
            if (ASContext.CurSciControl != null)
            {
                // Check for whether the cursor has moved
                ASContext.CurSciControl.UpdateUI += new UpdateUIHandler(_scintella_UpdateUI);

                // The code has changed so we will need to rebuild the dropdowns
                ASContext.CurSciControl.TextInserted += new TextInsertedHandler(CurSciControl_TextInserted);
                ASContext.CurSciControl.TextDeleted += new TextDeletedHandler(CurSciControl_TextDeleted);
            }
        }

        private void UnhookEvents()
        {
            if (ASContext.CurSciControl != null)
            {
                // We are not in a code file so we should unhook
                ASContext.CurSciControl.UpdateUI -= new UpdateUIHandler(_scintella_UpdateUI);
                ASContext.CurSciControl.TextInserted -= new TextInsertedHandler(CurSciControl_TextInserted);
                ASContext.CurSciControl.TextDeleted -= new TextDeletedHandler(CurSciControl_TextDeleted);
            }
        }

        void CurSciControl_TextInserted(ScintillaControl sender, int position, int length, int linesAdded)
        {
            // The text has changed start checking for the model to update
            _textChanged = true;
            updateTimer.Start();
        }

        void CurSciControl_TextDeleted(ScintillaControl sender, int position, int length, int linesAdded)
        {
            // The text has changed start checking for the model to update
            _textChanged = true;
            updateTimer.Start();
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            // The text has changed and the model may have updated
            UpdateUI();
        }

        void _scintella_UpdateUI(ScintillaControl sender)
        {
            // The caret may have moved 
            UpdateUI();
        }

        private void UpdateUI()
        {
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
                    // Unhook events then remove and dispose of ourselves
                    UnhookEvents();
                    this.Parent.Controls.Remove(this);
                    this.Dispose();
                    return;
                }
            }

            // Show the imported dropdown if it is not visible
            if (_showImportedClasses && tablePanel.ColumnStyles[0].SizeType == SizeType.Absolute)
            {
                tablePanel.ColumnStyles[0].SizeType = SizeType.Percent;
                tablePanel.ColumnStyles[0].Width = 33.33f;
            }
            // Hide the imported dropdown if it is visible
            else if (!_showImportedClasses && tablePanel.ColumnStyles[0].SizeType == SizeType.Percent)
            {
                tablePanel.ColumnStyles[0].SizeType = SizeType.Absolute;
                tablePanel.ColumnStyles[0].Width = 0f;
            }

            // If we haven't performed a build after dependencies have been resolved
            if (!_completeBuild &&
                ASContext.Context.CurrentModel.GetPublicClass().Extends != ClassModel.VoidClass)
            {
                _completeBuild = true;

                // If the text has changed and we need to rebuild or
                // We need to show inherited classes or members
                if (_textChanged || _showImportedClasses || _showInheritedMembers || _showSuperClasses)
                {
                    _textChanged = false;
                    updateTimer.Stop();
                    BuildDropDowns();
                }
            }
            // Rebuild the dropdowns if the text changed and the model has updated
            else if (_textChanged &&
                ASContext.Context.CurrentModel != null && !ASContext.Context.CurrentModel.OutOfDate)
            {
                _textChanged = false;
                updateTimer.Stop();
                BuildDropDowns();
            }

            // Update the dropdowns if the caret position has changed
            if (ASContext.CurSciControl.CurrentPos != _lastPosition)
            {
                _lastPosition = ASContext.CurSciControl.CurrentPos;
                UpdateDropDowns();
            }

            _updating = false;
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

            if (ASContext.Context.CurrentModel != null)
            {
                List<MemberTreeNode> importNodes = new List<MemberTreeNode>();

                // Add all the imported classes from this file
                foreach (MemberModel importModel in ASContext.Context.CurrentModel.Imports)
                {
                    int imageNum = importModel.Type.EndsWith(".*") ? ASCompletion.PluginUI.ICON_PACKAGE :
                        ((importModel.Flags & FlagType.Intrinsic) > 0) ? ASCompletion.PluginUI.ICON_INTRINSIC_TYPE : 
                        ASCompletion.PluginUI.ICON_TYPE;

                    MemberTreeNode node = new ImportTreeNode(importModel, imageNum, _showQualifiedClassNames);
                    importNodes.Add(node);
                }

                // Apply member sort
                if (_sortMethod != OutlineSorting.None)
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
                    MemberTreeNode node = GetClassTreeNode(classModel, false);
                    classNodes.Add(node);

                    if (_showSuperClasses)
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

                            node = GetClassTreeNode(extendClassModel, true);
                            classNodes.Add(node);

                            extendClassModel = extendClassModel.Extends;
                        }
                    }
                }

                // Apply member sort
                if (_sortMethod != OutlineSorting.None)
                    classNodes.Sort(_memberSort);

                classComboBox.Items.AddRange(classNodes.ToArray());
            }

            // Select the class that contains the caret
            UpdateClassDropDown();
        }

        private MemberTreeNode GetClassTreeNode(ClassModel classModel, bool isInherited)
        {
            int imageNum = ((classModel.Flags & FlagType.Intrinsic) > 0) ? ASCompletion.PluginUI.ICON_INTRINSIC_TYPE :
                           ((classModel.Flags & FlagType.Interface) > 0) ? ASCompletion.PluginUI.ICON_INTERFACE : ASCompletion.PluginUI.ICON_TYPE;
            return isInherited ? new InheritedClassTreeNode(classModel, imageNum, _showQualifiedClassNames) :
                                 new ClassTreeNode(classModel, imageNum, _showQualifiedClassNames) as MemberTreeNode;
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
                if (_showInheritedMembers && classModel != null)
                    memberNodes.AddRange(GetInheritedMembers(classModel.Extends));

                // Apply member sort
                if (_sortMethod != OutlineSorting.None)
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
                node = classModel == null ? new MemberTreeNode(memberModel, imageIndex) :
                                            new InheritedMemberTreeNode(classModel, memberModel, imageIndex);
            }

            return node;
        }

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
            int line = ASContext.CurSciControl.LineFromPosition(ASContext.CurSciControl.CurrentPos);

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
            int line = ASContext.CurSciControl.LineFromPosition(ASContext.CurSciControl.CurrentPos);

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

        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            // If we are not updating and the combobox has a selected item, then
            // navigate to it
            if (!_updating && comboBox.SelectedItem != null)
            {
                TreeNode selectedNode = (TreeNode)comboBox.SelectedItem;
                if (selectedNode is InheritedMemberTreeNode)
                {
                    InheritedMemberTreeNode inheritedNode = (InheritedMemberTreeNode)selectedNode;
                    FileModel model = ModelsExplorer.Instance.OpenFile(inheritedNode.ClassModel.InFile.FileName);

                    if (!(selectedNode is InheritedClassTreeNode))
                    {
                        // We have to update the Tag to reflect the line number the member starts on
                        inheritedNode.Tag = GetInheritedMemberTag(model, inheritedNode.Model.Name);
                    }
                }

                // Navigate to node location
                ASContext.Context.OnSelectOutlineNode(selectedNode);

                // If navigating to an inherited class or member, we need to reset our combobox
                if (selectedNode is InheritedMemberTreeNode || selectedNode is ImportTreeNode)
                {
                    ResetDropDowns();
                }
            }
        }

        private string GetInheritedMemberTag(FileModel model, string memberName)
        {
            foreach (ClassModel classModel in model.Classes)
            {
                foreach (MemberModel memberModel in classModel.Members)
                {
                    if (memberName == memberModel.Name)
                    {
                        return memberModel.Name + "@" + memberModel.LineFrom;
                    }
                }
            }
            return string.Empty;
        }

        private void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (ASContext.CurSciControl != null)
                ASContext.CurSciControl.Focus();

            ResetDropDowns();
        }

        private void ResetDropDowns()
        {
            _updating = true;
            importComboBox.SelectedItem = null;
            classComboBox.SelectedItem = _lastSelectedClassNode;
            memberComboBox.SelectedItem = _lastSelectedMemberNode;
            _updating = false;
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
    }
}

class MemberTreeNode : TreeNode
{
    protected string _label = "";
    protected MemberModel _model = null;

    public MemberTreeNode(MemberModel memberModel, int imageIndex)
        : base(memberModel.ToString(), imageIndex, imageIndex)
    {
        _label = this.Text;
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

    public InheritedMemberTreeNode(ClassModel classModel, MemberModel memberModel, int imageIndex)
        : base(memberModel, imageIndex)
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
    public ImportTreeNode(MemberModel importModel, int imageIndex, bool showQualifiedClassNames)
        : base(importModel, imageIndex)
    {
        Text = importModel.Type;
        Tag = "import";
        _label = showQualifiedClassNames ? importModel.Type : importModel.Name;
    }
}

class ClassTreeNode : MemberTreeNode
{
    public ClassTreeNode(ClassModel classModel, int imageIndex, bool showQualifiedClassNames)
        : base(classModel, imageIndex)
    {
        Text = classModel.Name;
        Tag = "class";
        _label = showQualifiedClassNames ? classModel.QualifiedName : classModel.Name;
    }
}

class InheritedClassTreeNode : InheritedMemberTreeNode
{
    public InheritedClassTreeNode(ClassModel classModel, int imageIndex, bool showQualifiedClassNames)
        : base(classModel, classModel, imageIndex)
    {
        Text = classModel.Name;
        Tag = "class";
        _label = showQualifiedClassNames ? classModel.QualifiedName : classModel.Name;
    }
}

class MemberTreeNodeComparer : IComparer<MemberTreeNode>
{
    private IComparer<MemberModel> _memberModelComparer;

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