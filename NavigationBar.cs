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

namespace NavigationBar
{
    public partial class NavigationBar : UserControl
    {
        public const int ICON_FILE = 0;
        public const int ICON_FOLDER_CLOSED = 1;
        public const int ICON_FOLDER_OPEN = 2;
        public const int ICON_CHECK_SYNTAX = 3;
        public const int ICON_QUICK_BUILD = 4;
        public const int ICON_PACKAGE = 5;
        public const int ICON_INTERFACE = 6;
        public const int ICON_INTRINSIC_TYPE = 7;
        public const int ICON_TYPE = 8;
        public const int ICON_VAR = 9;
        public const int ICON_PROTECTED_VAR = 10;
        public const int ICON_PRIVATE_VAR = 11;
        public const int ICON_CONST = 12;
        public const int ICON_PROTECTED_CONST = 13;
        public const int ICON_PRIVATE_CONST = 14;
        public const int ICON_FUNCTION = 15;
        public const int ICON_PROTECTED_FUNCTION = 16;
        public const int ICON_PRIVATE_FUNCTION = 17;
        public const int ICON_PROPERTY = 18;
        public const int ICON_PROTECTED_PROPERTY = 19;
        public const int ICON_PRIVATE_PROPERTY = 20;
        public const int ICON_TEMPLATE = 21;
        public const int ICON_DECLARATION = 22;

        private ImageList _icons = null;
        private bool _updating = false;
        private int _lastPosition = -1;
        private bool _textChanged = true;
        private bool _completeBuild = false;
        private bool _showSuperClasses = false;
        private bool _showInheritedMembers = false;
        private TreeNode _lastSelectedClassNode = null;
        private TreeNode _lastSelectedMemberNode = null;

        public NavigationBar(bool showSuperClasses, bool showInheritedMembers)
        {
            InitializeComponent();
            InitializeIcons();
            
            // We should remain invisible until we determine that we are in a code file.
            this.Visible = false;

            // We should be docked at the top of the container.
            this.Dock = DockStyle.Top;

            _showSuperClasses = showSuperClasses;
            _showInheritedMembers = showInheritedMembers;
            
            HookEvents();
        }

        public void OpenClasses()
        {
            classComboBox.Focus();
            classComboBox.DroppedDown = true;
        }

        public void OpenMembers()
        {
            memberComboBox.Focus();
            memberComboBox.DroppedDown = true;
        }

        public void UpdateSettings(bool showSuperClasses, bool showInheritedMembers)
        {
            _showSuperClasses = showSuperClasses;
            _showInheritedMembers = showInheritedMembers;

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
            // Check for whether the cursor has moved
            ASContext.CurSciControl.UpdateUI += new UpdateUIHandler(_scintella_UpdateUI);

            // The code has changed so we will need to rebuild the dropdowns
            ASContext.CurSciControl.TextInserted += new TextInsertedHandler(CurSciControl_TextInserted);
            ASContext.CurSciControl.TextDeleted += new TextDeletedHandler(CurSciControl_TextDeleted);
        }

        private void UnhookEvents()
        {
            // We are not in a code file so we should unhook
            ASContext.CurSciControl.UpdateUI -= new UpdateUIHandler(_scintella_UpdateUI);
            ASContext.CurSciControl.TextInserted -= new TextInsertedHandler(CurSciControl_TextInserted);
            ASContext.CurSciControl.TextDeleted -= new TextDeletedHandler(CurSciControl_TextDeleted);
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

            // If we haven't performed a build after dependencies have been resolved
            if (!_completeBuild &&
                ASContext.Context.CurrentModel.GetPublicClass().Extends != ClassModel.VoidClass)
            {
                _completeBuild = true;

                // If the text has changed and we need to rebuild or
                // We need to show inherited classes or members
                if (_textChanged || _showInheritedMembers || _showSuperClasses)
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
            BuildClassDropDown();
            BuildMemberDropDown();
        }

        private void UpdateDropDowns()
        {
            UpdateClassDropDown();
            UpdateMemberDropDown();
        }

        private void BuildClassDropDown()
        {
            classComboBox.Items.Clear();

            if (ASContext.Context.CurrentModel != null)
            {
                List<string> classNames = new List<string>();

                // Add all the classes from this file
                foreach (ClassModel classModel in ASContext.Context.CurrentModel.Classes)
                {
                    TreeNode node = GetClassTreeNode(classModel, false);
                    classComboBox.Items.Add(node);

                    if (_showSuperClasses)
                    {
                        // While extended class is not Object or Void
                        var extendClassModel = classModel.Extends;
                        while (extendClassModel.Name != "Object" && 
                               extendClassModel != ClassModel.VoidClass)
                        {
                            // Have we already added this class? Multiple classes could extend the same base.
                            if (classNames.Contains(extendClassModel.QualifiedName))
                                break;

                            node = GetClassTreeNode(extendClassModel, true);

                            classComboBox.Items.Add(node);
                            classNames.Add(extendClassModel.QualifiedName);

                            extendClassModel = extendClassModel.Extends;
                        }
                    }
                }
            }

            // Select the class that contains the caret
            UpdateClassDropDown();
        }

        private MemberTreeNode GetClassTreeNode(ClassModel classModel, bool isInherited)
        {
            int imageNum = ((classModel.Flags & FlagType.Intrinsic) > 0) ? ICON_INTRINSIC_TYPE :
                           ((classModel.Flags & FlagType.Interface) > 0) ? ICON_INTERFACE : ICON_TYPE;
            return isInherited ? new InheritedClassTreeNode(classModel, imageNum) :
                                                new ClassTreeNode(classModel, imageNum) as MemberTreeNode;
        }

        private void BuildMemberDropDown()
        {
            memberComboBox.Items.Clear();

            MemberList members = null;
            ClassTreeNode classTreeNode = classComboBox.SelectedItem as ClassTreeNode;
            ClassModel classModel = null;

            if (ASContext.Context.CurrentModel != null)
            {
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

                    // Set the class model to the base class so we can add those members as well
                    classModel = classModel.Extends;
                }

                // Add the local members
                foreach (MemberModel member in members)
                {
                    MemberTreeNode node = GetMemberTreeNode(member, null);

                    if (node != null)
                    {
                        memberComboBox.Items.Add(node);
                    }
                }

                if (_showInheritedMembers)
                {
                    // Add members from our base class as long as it isn't Object or Void
                    while (classModel != null && 
                           classModel != ClassModel.VoidClass && 
                           classModel.Name != "Object")
                    {
                        members = classModel.Members;

                        foreach (MemberModel member in members)
                        {
                            MemberTreeNode node = GetMemberTreeNode(member, classModel);

                            if (node != null)
                            {
                                memberComboBox.Items.Add(node);
                            }
                        }

                        // Follow the inheritence chain down
                        classModel = classModel.Extends;
                    }
                }
            }

            // Select the member that contains the caret
            UpdateMemberDropDown();
        }

        private MemberTreeNode GetMemberTreeNode(MemberModel memberModel, ClassModel classModel)
        {
            MemberTreeNode node = null;
            int imageIndex = int.MinValue;

            if ((memberModel.Flags & FlagType.Constant) > 0)
            {
                imageIndex = ((memberModel.Access & Visibility.Private) > 0) ? ICON_PRIVATE_CONST :
                    ((memberModel.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_CONST : ICON_CONST;
            }
            else if ((memberModel.Flags & FlagType.Variable) > 0)
            {
                imageIndex = ((memberModel.Access & Visibility.Private) > 0) ? ICON_PRIVATE_VAR :
                    ((memberModel.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_VAR : ICON_VAR;
            }
            else if ((memberModel.Flags & (FlagType.Getter | FlagType.Setter)) > 0)
            {
                if (node != null && node.Text == memberModel.ToString()) // "collapse" properties
                    return null;
                imageIndex = ((memberModel.Access & Visibility.Private) > 0) ? ICON_PRIVATE_PROPERTY :
                    ((memberModel.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_PROPERTY : ICON_PROPERTY;
            }
            else if ((memberModel.Flags & FlagType.Function) > 0)
            {
                imageIndex = ((memberModel.Access & Visibility.Private) > 0) ? ICON_PRIVATE_FUNCTION :
                    ((memberModel.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_FUNCTION : ICON_FUNCTION;
            }

            if (imageIndex != int.MinValue)
            {
                node = classModel == null ? new MemberTreeNode(memberModel, imageIndex) :
                                            new InheritedMemberTreeNode(classModel, memberModel, imageIndex);
            }

            return node;
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
                        foreach (ClassModel classModel in model.Classes)
                        {
                            foreach (MemberModel memberModel in classModel.Members)
                            {
                                if (inheritedNode.Model.Name == memberModel.Name)
                                {
                                    inheritedNode.Tag = memberModel.Name + "@" + memberModel.LineFrom;
                                    break;
                                }
                            }
                        }
                    }
                }

                ASContext.Context.OnSelectOutlineNode(selectedNode);

                // If navigating to an inherited class or member, we need to reset our combobox
                if (selectedNode is InheritedMemberTreeNode)
                {
                    _updating = true;
                    classComboBox.SelectedItem = _lastSelectedClassNode;
                    memberComboBox.SelectedItem = _lastSelectedMemberNode;
                    _updating = false;
                }
            }
        }

        private void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (ASContext.CurSciControl != null)
                ASContext.CurSciControl.Focus();

            _updating = true;
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

class ClassTreeNode : MemberTreeNode
{
    public ClassTreeNode(ClassModel classModel, int imageIndex)
        : base(classModel, imageIndex)
    {
        Text = classModel.Name;
        Tag = "class";
        _label = classModel.QualifiedName;
    }
}

class InheritedClassTreeNode : InheritedMemberTreeNode
{
    public InheritedClassTreeNode(ClassModel classModel, int imageIndex)
        : base(classModel, classModel, imageIndex)
    {
        Text = classModel.Name;
        Tag = "class";
        _label = classModel.QualifiedName;
    }
}