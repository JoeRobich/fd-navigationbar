using ASCompletion.Settings;
using NavigationBar.Localization;
using System;
using System.ComponentModel;

namespace NavigationBar
{
    public delegate void SettingsChangesEvent();

    [Serializable]
    public class Settings
    {
        [field: NonSerialized]
        public event SettingsChangesEvent OnSettingsChanged;

        private const bool DEFAULT_SHOW_NAVIGATION_TOOLBAR = false;
        private const bool DEFAULT_SHOW_IMPORTED_CLASSES = false;
        private const bool DEFAULT_SHOW_SUPER_CLASSES = false;
        private const bool DEFAULT_SHOW_INHERITED_MEMBERS = false;
        private const bool DEFAULT_SHOW_QUALIFIED_CLASS_NAME = true;
        private const bool DEFAULT_LABEL_PROPERTIES_LIKE_FUNCTIONS = false;
        private const bool DEFAULT_IGNORE_UNDERSCORE = false;
        private const OutlineSorting DEFAULT_MEMBER_SORT_METHOD = OutlineSorting.Sorted;
        private const bool DEFAULT_DROPDOWN_MULTIKEY_SEARCH_ENABLED = true;
        private const int DEFAULT_DROPDOWN_MULTIKEY_SEARCH_TIMER = 1000;
        private const bool DEFAULT_DROPDOWN_FULLWORD_SEARCH_ENABLED = true;

        private bool _showNavigationToolbar = DEFAULT_SHOW_NAVIGATION_TOOLBAR;
        private bool _showImportedClasses = DEFAULT_SHOW_IMPORTED_CLASSES;
        private bool _showSuperClasses = DEFAULT_SHOW_SUPER_CLASSES;
        private bool _showInheritedMembers = DEFAULT_SHOW_INHERITED_MEMBERS;
        private bool _showQualifiedClassName = DEFAULT_SHOW_QUALIFIED_CLASS_NAME;
        private bool _labelPropertiesLikeFunctions = DEFAULT_LABEL_PROPERTIES_LIKE_FUNCTIONS;
        private bool _ignoreUnderscore = DEFAULT_IGNORE_UNDERSCORE;
        private OutlineSorting _memberSortMethod = DEFAULT_MEMBER_SORT_METHOD;
        private bool _dropDownMultiKeySearchEnabled = false;
        private int _dropDownMultiKeySearchTimer = 0;
        private bool _dropDownFullWordSearchEnabled = false;

        [LocalizedCategory("NavigationBar.Category.Visibility")]
        [LocalizedDisplayName("NavigationBar.Label.ShowNavigationToolbar")]
        [LocalizedDescription("NavigationBar.Description.ShowNavigationToolbar")]
        [DefaultValue(DEFAULT_SHOW_NAVIGATION_TOOLBAR)]
        public bool ShowNavigationToolbar
        {
            get { return _showNavigationToolbar; }
            set
            {
                if (_showNavigationToolbar != value)
                {
                    _showNavigationToolbar = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Visibility")]
        [LocalizedDisplayName("NavigationBar.Label.ShowImportedClasses")]
        [LocalizedDescription("NavigationBar.Description.ShowImportedClasses")]
        [DefaultValue(DEFAULT_SHOW_IMPORTED_CLASSES)]
        public bool ShowImportedClasses
        {
            get { return _showImportedClasses; }
            set
            {
                if (_showImportedClasses != value)
                {
                    _showImportedClasses = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Visibility")]
        [LocalizedDisplayName("NavigationBar.Label.ShowSuperClasses")]
        [LocalizedDescription("NavigationBar.Description.ShowSuperClasses")]
        [DefaultValue(DEFAULT_SHOW_SUPER_CLASSES)]
        public bool ShowSuperClasses
        {
            get { return _showSuperClasses; }
            set
            {
                if (_showSuperClasses != value)
                {
                    _showSuperClasses = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Visibility")]
        [LocalizedDisplayName("NavigationBar.Label.ShowInheritedMembers")]
        [LocalizedDescription("NavigationBar.Description.ShowInheritedMembers")]
        [DefaultValue(DEFAULT_SHOW_INHERITED_MEMBERS)]
        public bool ShowInheritedMembers
        {
            get { return _showInheritedMembers; }
            set 
            { 
                if (_showInheritedMembers != value)
                {
                    _showInheritedMembers = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Labeling")]
        [LocalizedDisplayName("NavigationBar.Label.ShowQualifiedClassNames")]
        [LocalizedDescription("NavigationBar.Description.ShowQualifiedClassNames")]
        [DefaultValue(DEFAULT_SHOW_QUALIFIED_CLASS_NAME)]
        public bool ShowQualifiedClassName
        {
            get { return _showQualifiedClassName; }
            set
            {
                if (_showQualifiedClassName != value)
                {
                    _showQualifiedClassName = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Labeling")]
        [LocalizedDisplayName("NavigationBar.Label.LabelPropertiesLikeFunctions")]
        [LocalizedDescription("NavigationBar.Description.LabelPropertiesLikeFunctions")]
        [DefaultValue(DEFAULT_LABEL_PROPERTIES_LIKE_FUNCTIONS)]
        public bool LabelPropertiesLikeFunctions
        {
            get { return _labelPropertiesLikeFunctions; }
            set
            {
                if (_labelPropertiesLikeFunctions != value)
                {
                    _labelPropertiesLikeFunctions = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Navigation")]
        [LocalizedDisplayName("NavigationBar.Label.IgnoreUnderscore")]
        [LocalizedDescription("NavigationBar.Description.IgnoreUnderscore")]
        [DefaultValue(DEFAULT_IGNORE_UNDERSCORE)]
        public bool IgnoreUnderscore
        {
            get { return _ignoreUnderscore; }
            set
            {
                if (_ignoreUnderscore != value)
                {
                    _ignoreUnderscore = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Navigation")]
        [LocalizedDisplayName("NavigationBar.Label.SortingMethod")]
        [LocalizedDescription("NavigationBar.Description.SortingMethod")]
        [DefaultValue(DEFAULT_MEMBER_SORT_METHOD)]
        public OutlineSorting MemberSortMethod
        {
            get { return _memberSortMethod; }
            set
            {
                if (_memberSortMethod != value)
                {
                    _memberSortMethod = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Search")]
        [LocalizedDisplayName("NavigationBar.Label.DropDownMultiKeyEnabled")]
        [LocalizedDescription("NavigationBar.Description.DropDownMultiKeyEnabled")]
        [DefaultValue(DEFAULT_DROPDOWN_MULTIKEY_SEARCH_ENABLED)]
        public bool DropDownMultiKeyEnabled
        {
            get { return _dropDownMultiKeySearchEnabled; }
            set
            {
                if (_dropDownMultiKeySearchEnabled != value)
                {
                    _dropDownMultiKeySearchEnabled = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Search")]
        [LocalizedDisplayName("NavigationBar.Label.DropDownMultiKeyTimer")]
        [LocalizedDescription("NavigationBar.Description.DropDownMultiKeyTimer")]
        [DefaultValue(DEFAULT_DROPDOWN_MULTIKEY_SEARCH_TIMER)]
        public int DropDownMultiKeyTimer
        {
            get { return _dropDownMultiKeySearchTimer; }
            set
            {
                if (_dropDownMultiKeySearchTimer != value)
                {
                    _dropDownMultiKeySearchTimer = value;
                    FireChanged();
                }
            }
        }

        [LocalizedCategory("NavigationBar.Category.Search")]
        [LocalizedDisplayName("NavigationBar.Label.DropDownFullWordSearchEnabled")]
        [LocalizedDescription("NavigationBar.Description.DropDownFullWordSearchEnabled")]
        [DefaultValue(DEFAULT_DROPDOWN_FULLWORD_SEARCH_ENABLED)]
        public bool DropDownFullWordSearchEnabled
        {
            get { return _dropDownFullWordSearchEnabled; }
            set
            {
                if (_dropDownFullWordSearchEnabled != value)
                {
                    _dropDownFullWordSearchEnabled = value;
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
