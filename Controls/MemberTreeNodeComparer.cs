using ASCompletion.Model;
using ASCompletion.Settings;
using System.Collections.Generic;

namespace NavigationBar.Controls
{
    internal class MemberTreeNodeComparer : IComparer<MemberTreeNode>
    {
        static Dictionary<OutlineSorting, MemberTreeNodeComparer> _comparerMap  = new Dictionary<OutlineSorting, MemberTreeNodeComparer>
        {
            { OutlineSorting.Sorted, new MemberTreeNodeComparer(null) },
            { OutlineSorting.SortedByKind, new MemberTreeNodeComparer(new ByKindMemberComparer()) },
            { OutlineSorting.SortedGroup, new MemberTreeNodeComparer(new ByKindMemberComparer()) },
            { OutlineSorting.SortedSmart, new MemberTreeNodeComparer(new SmartMemberComparer()) }
        };

        public static MemberTreeNodeComparer GetComparer(OutlineSorting outlineSort)
        {
            return _comparerMap[outlineSort];
        }

        IComparer<MemberModel> _memberModelComparer;

        public MemberTreeNodeComparer(IComparer<MemberModel> memberModelComparer)
        {
            _memberModelComparer = memberModelComparer;
        }

        public int Compare(MemberTreeNode x, MemberTreeNode y)
        {
            return _memberModelComparer != null ?
                _memberModelComparer.Compare(x.Model, y.Model) :
                x.Label.CompareTo(y.Label);
        }
    }
}