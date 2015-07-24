using ASCompletion.Model;

namespace NavigationBar.Controls
{
    internal class InheritedMemberTreeNode : MemberTreeNode
    {
        public ClassModel ClassModel { get; protected set; }

        public InheritedMemberTreeNode(ClassModel classModel, MemberModel memberModel, int imageIndex, bool labelPropertiesLikeFunctions)
            : base(memberModel, imageIndex, labelPropertiesLikeFunctions)
        {
            Label = Text + " - " + classModel.Name;
            ClassModel = classModel;
        }
    }
}