using ASCompletion.Model;

namespace NavigationBar.Controls
{
    internal class InheritedClassTreeNode : InheritedMemberTreeNode
    {
        public InheritedClassTreeNode(ClassModel classModel, int imageIndex, bool showQualifiedClassNames)
            : base(classModel, classModel, imageIndex, false)
        {
            Text = classModel.Name;
            Tag = "class";
            Label = showQualifiedClassNames ? classModel.QualifiedName : classModel.Name;
        }
    }
}