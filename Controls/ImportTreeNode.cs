using ASCompletion.Model;

namespace NavigationBar.Controls
{
    internal class ImportTreeNode : MemberTreeNode
    {
        public ImportTreeNode(ClassModel importModel, int imageIndex, bool showQualifiedClassNames)
            : base(importModel, imageIndex, false)
        {
            Text = importModel.Name;
            Tag = "class";
            Label = showQualifiedClassNames ? importModel.QualifiedName : importModel.Name;
        }
    }
}