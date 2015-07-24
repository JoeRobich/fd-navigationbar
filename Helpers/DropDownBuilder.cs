using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using NavigationBar.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NavigationBar.Helpers
{
    internal static class DropDownBuilder
    {
        static MemberTreeNodeComparer _memberSort = null;
        static Settings _settings;

        internal static Settings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;

                if (_memberSort == null)
                    _memberSort = MemberTreeNodeComparer.GetComparer(_settings.MemberSortMethod);
            }
        }

        internal static void BuildImportDropDown(ToolStripSpringComboBox importComboBox)
        {
            importComboBox.FlatCombo.BeginUpdate();

            var currentNodes = importComboBox.Items.OfType<MemberTreeNode>().ToList();
            importComboBox.Items.Clear();

            if (ASContext.Context.CurrentModel != null && Settings.ShowImportedClasses)
            {
                // Remove not needed imports
                var importModels = ASContext.Context.CurrentModel.Imports.OfType<MemberModel>().ToList();
                var existingNodes = currentNodes.Where(n => importModels.Any(m => m.Type == n.Model.Type));
                var newNodes = importModels.Where(importModel => !importModel.Type.EndsWith(".*") && !existingNodes.Any(importNode => importNode.Model.Type == importModel.Type))
                    .Select(importModel => ASContext.Context.ResolveType(importModel.Type, ASContext.Context.CurrentModel))
                    .Where(classModel => classModel != null && !classModel.IsVoid() && classModel.InFile != null)
                    .Select(classModel => GetClassTreeNode(classModel, false, true));

                var importNodes = existingNodes.Concat(newNodes).ToList();

                // Apply member sort
                if (_memberSort != null)
                    importNodes.Sort(_memberSort);

                importComboBox.Items.AddRange(importNodes.ToArray());
            }

            importComboBox.FlatCombo.EndUpdate();
        }

        internal static void BuildClassDropDown(ToolStripSpringComboBox classComboBox)
        {
            classComboBox.FlatCombo.BeginUpdate();

            classComboBox.Items.Clear();

            if (ASContext.Context.CurrentModel != null)
            {
                var classNodes = new List<MemberTreeNode>();
                var classNames = new List<string>();

                // Add all the classes from this file
                foreach (ClassModel classModel in ASContext.Context.CurrentModel.Classes)
                {
                    MemberTreeNode node = GetClassTreeNode(classModel, false, false);
                    classNodes.Add(node);

                    if (Settings.ShowSuperClasses)
                    {
                        var extendClassModel = classModel.Extends;
                        while (!IsRootType(extendClassModel))
                        {
                            // Have we already added this class? Multiple classes could extend the same base.
                            if (classNames.Contains(extendClassModel.QualifiedName))
                                break;

                            classNames.Add(extendClassModel.QualifiedName);
                            classNodes.Add(GetClassTreeNode(extendClassModel, true, false));

                            extendClassModel = extendClassModel.Extends;
                        }
                    }
                }

                // Apply member sort
                if (_memberSort != null)
                    classNodes.Sort(_memberSort);

                classComboBox.Items.AddRange(classNodes.ToArray());
            }

            classComboBox.FlatCombo.EndUpdate();
        }

        static MemberTreeNode GetClassTreeNode(ClassModel classModel, bool isInherited, bool isImported)
        {
            int imageNum = ((classModel.Flags & FlagType.Intrinsic) > 0) ? PluginUI.ICON_INTRINSIC_TYPE :
                           ((classModel.Flags & FlagType.Interface) > 0) ? PluginUI.ICON_INTERFACE : PluginUI.ICON_TYPE;
            return isInherited ? new InheritedClassTreeNode(classModel, imageNum, Settings.ShowQualifiedClassName) :
                   isImported ? new ImportTreeNode(classModel, imageNum, Settings.ShowQualifiedClassName) :
                                new ClassTreeNode(classModel, imageNum, Settings.ShowQualifiedClassName) as MemberTreeNode;
        }

        internal static void BuildMemberDropDown(ToolStripSpringComboBox classComboBox, ToolStripSpringComboBox memberComboBox)
        {
            memberComboBox.FlatCombo.BeginUpdate();

            memberComboBox.Items.Clear();

            if (ASContext.Context.CurrentModel != null)
            {
                ClassTreeNode classTreeNode = classComboBox.SelectedItem as ClassTreeNode;
                ClassModel classModel = (classTreeNode != null) ? (ClassModel)classTreeNode.Model : null;
                MemberList members = (classModel != null) ? classModel.Members : ASContext.Context.CurrentModel.Members;

                var memberNodes = members.OfType<MemberModel>()
                    .Select(m => GetMemberTreeNode(m, null))
                    .Where(mn => mn != null);

                // Add inherited members if applicable
                if (Settings.ShowInheritedMembers && classModel != null)
                    memberNodes = memberNodes.Concat(GetInheritedMembers(classModel.Extends));

                // Apply member sort
                if (_memberSort != null)
                    memberNodes = memberNodes.OrderBy(n => n, _memberSort);

                memberComboBox.Items.AddRange(memberNodes.ToArray());
            }

            memberComboBox.FlatCombo.EndUpdate();
        }

        static IEnumerable<MemberTreeNode> GetInheritedMembers(ClassModel classModel)
        {
            var memberNodes = Enumerable.Empty<MemberTreeNode>();

            // Add members from our super class as long as it is not null, Object, Void, or haXe Dynamic
            while (!IsRootType(classModel))
            {
                memberNodes = memberNodes.Concat(classModel.Members
                    .OfType<MemberModel>()
                    .Select(member => GetMemberTreeNode(member, classModel))
                    .Where(mn => mn != null));

                // Follow the inheritence chain down
                classModel = classModel.Extends;
            }

            return memberNodes;
        }

        static MemberTreeNode GetMemberTreeNode(MemberModel memberModel, ClassModel classModel)
        {
            MemberTreeNode node = null;
            int imageIndex = PluginUI.GetIcon(memberModel.Flags, memberModel.Access);

            if (imageIndex != 0)
                node = classModel == null ?
                    new MemberTreeNode(memberModel, imageIndex, Settings.LabelPropertiesLikeFunctions) :
                    new InheritedMemberTreeNode(classModel, memberModel, imageIndex, Settings.LabelPropertiesLikeFunctions);

            return node;
        }

        static bool IsRootType(ClassModel classModel)
        {
            return classModel == null ||
                   classModel.Name == "Object" ||
                   classModel == ClassModel.VoidClass ||
                   (classModel.InFile.haXe && classModel.Type == "Dynamic");
        }
    }
}