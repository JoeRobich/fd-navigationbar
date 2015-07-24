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
                List<string> paramList = new List<string>();
                if (memberModel.Parameters != null)
                    paramList.AddRange(memberModel.Parameters.Select(param => string.Format("{0}:{1}", param.Name, param.Type)));

                Label = string.Format("{0} ({1}) : {2}", memberModel.Name, string.Join(", ", paramList.ToArray()), memberModel.Type);
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