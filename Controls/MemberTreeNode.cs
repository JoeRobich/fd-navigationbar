using ASCompletion.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NavigationBar.Controls
{
    internal class MemberTreeNode : TreeNode
    {
        public MemberModel Model { get; protected set; }
        public string Label { get; protected set; }

        public MemberTreeNode(MemberModel memberModel, int imageIndex, bool labelPropertiesLikeFunctions)
            : base(memberModel.ToString(), imageIndex, imageIndex)
        {
            if (labelPropertiesLikeFunctions &&
                (memberModel.Flags & (FlagType.Setter | FlagType.Getter)) != 0)
            {
                var paramList = string.Empty;
                if (memberModel.Parameters != null)
                    paramList = string.Join(", ", memberModel.Parameters.Select(p => string.Format("{0}:{1}", p.Name, p.Type)).ToArray());

                Label = string.Format("{0} ({1}) : {2}", memberModel.Name, paramList, memberModel.Type);
            }
            else
            {
                Label = Text;
            }

            Model = memberModel;
            Tag = memberModel.Name + "@" + memberModel.LineFrom;
        }
    }
}