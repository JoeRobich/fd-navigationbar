using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using NavigationBar.Controls;
using System.Linq;

namespace NavigationBar.Helpers
{
    internal static class NavigationHelper
    {
        internal static void NavigateTo(MemberTreeNode node)
        {
            if (node is InheritedMemberTreeNode)
            {
                InheritedMemberTreeNode inheritedNode = (InheritedMemberTreeNode)node;
                FileModel model = ModelsExplorer.Instance.OpenFile(inheritedNode.ClassModel.InFile.FileName);

                // We have to update the Tag to reflect the line number the member starts on
                if (!(node is InheritedClassTreeNode))
                    inheritedNode.Tag = GetInheritedMemberTag(model, inheritedNode.Model.Name) ?? string.Empty;
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
        }

        static string GetInheritedMemberTag(FileModel model, string memberName)
        {
            return model.Classes
                .SelectMany(classModel => classModel.Members.Cast<MemberModel>())
                .Where(memberModel => memberModel.Name == memberName)
                .Select(memberModel => memberModel.Name + "@" + memberModel.LineFrom)
                .FirstOrDefault();
        }
    }
}