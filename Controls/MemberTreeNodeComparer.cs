using ASCompletion.Model;
using ASCompletion.Settings;
using System.Collections.Generic;

namespace NavigationBar.Controls
{
    internal class MemberTreeNodeComparer : IComparer<MemberTreeNode>
    {
        private static MemberTreeNodeComparer _sortedComparer = new MemberTreeNodeComparer(null);
        private static MemberTreeNodeComparer _byKindComparer = new MemberTreeNodeComparer(new ByKindMemberComparer());
        private static MemberTreeNodeComparer _smartSortComparer = new MemberTreeNodeComparer(new SmartMemberComparer());

        private IComparer<MemberModel> _memberModelComparer;

        public static MemberTreeNodeComparer GetComparer(OutlineSorting outlineSort)
        {
            MemberTreeNodeComparer memberSort = null;

            switch (outlineSort)
            {
                case OutlineSorting.Sorted:
                    memberSort = _sortedComparer;
                    break;
                case OutlineSorting.SortedByKind:
                case OutlineSorting.SortedGroup:
                    memberSort = _byKindComparer;
                    break;
                case OutlineSorting.SortedSmart:
                    memberSort = _smartSortComparer;
                    break;
            }

            return memberSort;
        }

        public MemberTreeNodeComparer(IComparer<MemberModel> memberModelComparer)
        {
            _memberModelComparer = memberModelComparer;
        }

        public int Compare(MemberTreeNode x, MemberTreeNode y)
        {
            return _memberModelComparer != null ? _memberModelComparer.Compare(x.Model, y.Model) :
                                                  x.Label.CompareTo(y.Label);
        }
    }
}