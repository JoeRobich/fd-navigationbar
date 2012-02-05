using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace NavigationBar
{
    public delegate void SettingsChangesEvent();

    [Serializable]
    public class Settings
    {
        [field: NonSerialized]
        public event SettingsChangesEvent OnSettingsChanged;

        private const Boolean DEFAULT_SHOW_SUPER_CLASSES = false;
        private const Boolean DEFAULT_SHOW_INHERITED_MEMBERS = false;

        private Boolean showSuperClasses = DEFAULT_SHOW_SUPER_CLASSES;
        private Boolean showInheritedMembers = DEFAULT_SHOW_INHERITED_MEMBERS;

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
        [Description("Whether the member dropdown is populated with inherited memebers.")]
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

        private void FireChanged()
        {
            if (OnSettingsChanged != null) OnSettingsChanged();
        }
    }
}
