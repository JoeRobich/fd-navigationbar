using ASCompletion.Model;
using System.IO;

namespace NavigationBar.Managers
{
    public class NavigationLocation
    {
        public string FilePath { get; set; }
        public int Position { get; set; }
        public string ClassName { get; set; }
        public string MemberName { get; set; }
        public FlagType MemberFlags { get; set; }
        public int LineFrom { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(MemberName))
                return string.Format("{0} {1} Line: {2}", Path.GetFileName(FilePath), ClassName, LineFrom);
            else
                return string.Format("{0} {1}.{2} Line: {3}", Path.GetFileName(FilePath), ClassName, MemberName, LineFrom);
        }
    }
}