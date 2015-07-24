using ASCompletion.Model;

namespace NavigationBar.Controls
{
    internal class ClassTreeNode : MemberTreeNode
    {
        public ClassModel ClassModel { get { return (ClassModel)Model; } }

        public ClassTreeNode(ClassModel classModel, int imageIndex, bool showQualifiedClassNames)
            : base(classModel, imageIndex, false)
        {
            Text = classModel.Name;
            Tag = "class";
            Label = showQualifiedClassNames ? classModel.QualifiedName : classModel.Name;
        }
    }
}