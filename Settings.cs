using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using ASCompletion.Settings;

namespace NavigationBar
{
    public delegate void SettingsChangesEvent();

    [Serializable]
    public class Settings
    {
        [field: NonSerialized]
        public event SettingsChangesEvent OnSettingsChanged;

        private const Boolean DEFAULT_SHOW_IMPORTED_CLASSES = false;
        private const Boolean DEFAULT_SHOW_SUPER_CLASSES = false;
        private const Boolean DEFAULT_SHOW_INHERITED_MEMBERS = false;
        private const Boolean DEFAULT_SHOW_QUALIFIED_CLASS_NAME = true;
        private const Boolean DEFAULT_LABEL_PROPERTIES_LIKE_FUNCTIONS = false;
        private const OutlineSorting DEFAULT_MEMBER_SORT_METHOD = OutlineSorting.Sorted;

        private Boolean showImportedClasses = DEFAULT_SHOW_IMPORTED_CLASSES;
        private Boolean showSuperClasses = DEFAULT_SHOW_SUPER_CLASSES;
        private Boolean showInheritedMembers = DEFAULT_SHOW_INHERITED_MEMBERS;
        private Boolean showQualifiedClassName = DEFAULT_SHOW_QUALIFIED_CLASS_NAME;
        private Boolean labelPropertiesLikeFunctions = DEFAULT_LABEL_PROPERTIES_LIKE_FUNCTIONS;
        private OutlineSorting memberSortMethod = DEFAULT_MEMBER_SORT_METHOD;

        [Category("Navigation")]
        [DisplayName("Show imported class dropdown")]
        [Description("Whether the imported class dropdown is visible.")]
        [DefaultValue(DEFAULT_SHOW_IMPORTED_CLASSES)]
        public Boolean ShowImportedClasses
        {
            get { return showImportedClasses; }
            set
            {
                if (showImportedClasses != value)
                {
                    showImportedClasses = value;
                    FireChanged();
                }
            }
        }

        [Category("Navigation")]
        [DisplayName("Show super classes")]
        [Description("Whether the class dropdown is populated with super classes.")]
        [DefaultValue(DEFAULT_SHOW_SUPER_CLASSES)]
        public Boolean ShowSuperClasses
        {
            get { return showSuperClasses; }
            set
            {
                if (showSuperClasses != value)
                {
                    showSuperClasses = value;
                    FireChanged();
                }
            }
        }

        [Category("Navigation")]
        [DisplayName("Show inherited members")]
        [Description("Whether the member dropdown is populated with inherited members.")]
        [DefaultValue(DEFAULT_SHOW_INHERITED_MEMBERS)]
        public Boolean ShowInheritedMembers
        {
            get { return showInheritedMembers; }
            set 
            { 
                if (showInheritedMembers != value)
                {
                    showInheritedMembers = value;
                    FireChanged();
                }
            }
        }

        [Category("Navigation")]
        [DisplayName("Show qualified class names")]
        [Description("Whether to show the qualified class names in the dropdowns.")]
        [DefaultValue(DEFAULT_SHOW_QUALIFIED_CLASS_NAME)]
        public Boolean ShowQualifiedClassName
        {
            get { return showQualifiedClassName; }
            set
            {
                if (showQualifiedClassName != value)
                {
                    showQualifiedClassName = value;
                    FireChanged();
                }
            }
        }

        [Category("Navigation")]
        [DisplayName("Label properties like functions")]
        [Description("Whether the labels for getters/setters look like function labels.")]
        [DefaultValue(DEFAULT_LABEL_PROPERTIES_LIKE_FUNCTIONS)]
        public Boolean LabelPropertiesLikeFunctions
        {
            get { return labelPropertiesLikeFunctions; }
            set
            {
                if (labelPropertiesLikeFunctions != value)
                {
                    labelPropertiesLikeFunctions = value;
                    FireChanged();
                }
            }
        }

        [Category("Navigation")]
        [DisplayName("Member sort method")]
        [Description("How should the members dropdown be sorted.")]
        [DefaultValue(DEFAULT_MEMBER_SORT_METHOD)]
        public OutlineSorting MemberSortMethod
        {
            get { return memberSortMethod; }
            set
            {
                if (memberSortMethod != value)
                {
                    memberSortMethod = value;
                    FireChanged();
                }
            }
        }

        private void FireChanged()
        {
            if (OnSettingsChanged != null) OnSettingsChanged();
        }
    }
}
