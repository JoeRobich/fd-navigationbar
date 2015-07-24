using NavigationBar.Helpers;
using NavigationBar.Managers;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ProjectManager;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NavigationBar
{
    public class PluginMain : IPlugin
    {
        private const int API = 1;
        private const string NAME = "NavigationBar";
        private const string GUID = "F313AE66-0C5F-4388-B281-9E9AFAD7B8F9";
        private const string HELP = "www.flashdevelop.org/community/viewtopic.php?f=4&t=9376";
        private const string DESCRIPTION = "Adds a navigation bar for quickly moving through your source.";
        private const string AUTHOR = "Joey Robichaud";

        private string _settingFilename = "";
        private Settings _settings;
        private ToolStripSplitButton _navigateBackwardButton = null;
        private ToolStripSplitButton _navigateForwardButton = null;


	    #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public int Api
        {
            get { return API; }
        }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public string Name
		{
			get { return NAME; }
		}

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid
		{
			get { return GUID; }
		}

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public string Author
		{
			get { return AUTHOR; }
		}

        /// <summary>
        /// Description of the plugin
        /// </summary>
        public string Description
		{
			get { return DESCRIPTION; }
		}

        /// <summary>
        /// Web address for help
        /// </summary>
        public string Help
		{
			get { return HELP; }
		}

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public object Settings
        {
            get { return _settings; }
        }

		#endregion

		#region Required Methods

		/// <summary>
		/// Initializes the plugin
		/// </summary>
		public void Initialize()
        {
            InitBasics();
            LoadSettings();
            AddEventHandlers();
            CreateMenuItems();
            CreateToolbarItems();
        }

		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
        {
            SaveSettings();
		}

		/// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(object sender, NotifyEvent e, HandlingPriority prority)
		{
            if (e.Type == EventType.FileOpen || e.Type == EventType.FileNew)
            {
                ITabbedDocument document = PluginBase.MainForm.CurrentDocument;
                if (document != null)
                {
                    // Check to see if we've already added an NavigationBar to the
                    // current document.
                    if (document.SciControl == null)
                        return;

                    Controls.NavigationBar bar = GetNavigationBar(document);
                    if (bar != null)
                        return;

                    // Dock a new navigation bar to the top of the current document
                    bar = new Controls.NavigationBar(document, _settings);
                    document.Controls.Add(bar);
                }
            }
            else if (e.Type == EventType.FileSwitch)
            {
                ITabbedDocument document = PluginBase.MainForm.CurrentDocument;
                if (document != null)
                {
                    // Check to see if this document contains a text editor.
                    if (document.SciControl == null)
                        return;

                    // Refresh the navigation bar
                    Controls.NavigationBar bar = GetNavigationBar(document);
                    if (bar != null)
                        bar.RefreshSettings();
                }
            }
            else if (e.Type == EventType.Command)
            {
                DataEvent de = e as DataEvent;

                if (de.Action.StartsWith("ProjectManager."))
                {
                    if (de.Action == ProjectManagerCommands.NewProject)
                        NavigationManager.Instance.Clear();
                    else if (de.Action == ProjectManagerCommands.OpenProject)
                        NavigationManager.Instance.Clear();
                }
            }
            else if (e.Type == EventType.ApplyTheme)
            {
                foreach (var document in PluginBase.MainForm.Documents)
                {
                    if (document.SciControl == null)
                        continue;

                    var bar = GetNavigationBar(document);
                    if (bar != null)
                        bar.ApplyTheme();
                }
            }
		}

        void _settings_OnSettingsChanged()
        {
            UpdateNavigationButtons();
        }

        private void NavigationManager_LocationChanged(object sender, EventArgs e)
        {
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            _navigateBackwardButton.Enabled = NavigationManager.Instance.CanNavigateBackward;
            _navigateBackwardButton.Visible = _settings.ShowNavigationToolbar;
            _navigateForwardButton.Enabled = NavigationManager.Instance.CanNavigateForward;
            _navigateForwardButton.Visible = _settings.ShowNavigationToolbar;
        }

        void NavigateForward(object sender, EventArgs e)
        {
            NavigationManager.Instance.NavigateForward();
        }

        void NavigateBackward(object sender, EventArgs e)
        {
            NavigationManager.Instance.NavigateBackward();
        }

		#endregion

        #region Plugin Methods

        private void OpenImports(object sender, EventArgs e)
        {
            Controls.NavigationBar navBar = GetNavigationBar();
            if (navBar == null || !navBar.Visible)
                return;

            navBar.OpenImports();
        }

        private void OpenClasses(object sender, EventArgs e)
        {
            Controls.NavigationBar navBar = GetNavigationBar();
            if (navBar == null || !navBar.Visible)
                return;

            navBar.OpenClasses();
        }

        private void OpenMembers(object sender, EventArgs e)
        {
            Controls.NavigationBar navBar = GetNavigationBar();
            if (navBar == null || !navBar.Visible)
                return;

            navBar.OpenMembers();
        }

        private Controls.NavigationBar GetNavigationBar()
        {
            ITabbedDocument document = PluginBase.MainForm.CurrentDocument;
            if (document == null)
                return null;

            return GetNavigationBar(document);
        }

        private Controls.NavigationBar GetNavigationBar(ITabbedDocument document)
        {
            return document.Controls
                .OfType<Controls.NavigationBar>()
                .FirstOrDefault();
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, NAME);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            _settingFilename = Path.Combine(dataPath, "Settings.fdb");
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        public void AddEventHandlers()
        {
            // Set events you want to listen (combine as flags)
            EventManager.AddEventHandler(this, EventType.FileNew | EventType.FileOpen | EventType.FileSwitch | EventType.Command | EventType.ApplyTheme);
            NavigationManager.Instance.LocationChanged += new EventHandler(NavigationManager_LocationChanged);
            _settings.OnSettingsChanged += new SettingsChangesEvent(_settings_OnSettingsChanged);
        }

        /// <summary>
        /// Adds shortcuts for manipulating the navigation bar
        /// </summary>
        public void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            ToolStripMenuItem menuItem;

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.OpenImports"), null, new EventHandler(OpenImports));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.OpenImports", menuItem);
            menu.DropDownItems.Add(menuItem);

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.OpenClasses"), null, new EventHandler(OpenClasses));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.OpenClasses", menuItem);
            menu.DropDownItems.Add(menuItem);

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.OpenMembers"), null, new EventHandler(OpenMembers));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.OpenMembers", menuItem);
            menu.DropDownItems.Add(menuItem);

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.NavigateForward"), null, new EventHandler(NavigateForward));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.NavigateForward", menuItem);
            menu.DropDownItems.Add(menuItem);

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.NavigateBackward"), null, new EventHandler(NavigateBackward));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.NavigateBackward", menuItem);
            menu.DropDownItems.Add(menuItem);
        }

        public void CreateToolbarItems()
        {
            _navigateBackwardButton = new ToolStripSplitButton(ResourceHelper.GetString("NavigationBar.Label.NavigateBackward"), PluginBase.MainForm.FindImage("315|1|-3|3"));
            _navigateBackwardButton.Name = "NavigateBackward";
            _navigateBackwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _navigateBackwardButton.ButtonClick += new EventHandler(NavigateBackward);
            _navigateBackwardButton.DropDownOpening += NavigateBackwardDropDownOpening;
            _navigateBackwardButton.DropDown.Renderer = new DockPanelStripRenderer();
            PluginBase.MainForm.ToolStrip.Items.Add(_navigateBackwardButton);

            _navigateForwardButton = new ToolStripSplitButton(ResourceHelper.GetString("NavigationBar.Label.NavigateForward"), PluginBase.MainForm.FindImage("315|9|3|3"));
            _navigateForwardButton.Name = "NavigateForward";
            _navigateForwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _navigateForwardButton.ButtonClick += new EventHandler(NavigateForward);
            _navigateForwardButton.DropDownOpening += NavigateForwardDropDownOpening;
            _navigateForwardButton.DropDown.Renderer = new DockPanelStripRenderer();
            PluginBase.MainForm.ToolStrip.Items.Add(_navigateForwardButton);

            UpdateNavigationButtons();
        }

        private void NavigateBackwardDropDownOpening(object sender, EventArgs e)
        {
            _navigateBackwardButton.DropDownItems.Clear();
            var historyItems = NavigationManager.Instance.BackwardHistory.Select(nl =>
                new ToolStripMenuItem(nl.ToString(), null, NavigateBackwardDropDownItemClick) { Tag = nl }
            );
            _navigateBackwardButton.DropDownItems.AddRange(historyItems.ToArray());
        }

        private void NavigateBackwardDropDownItemClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var navigationLocation = (NavigationLocation)item.Tag;
            NavigationManager.Instance.NavigateBackwardTo(navigationLocation);
        }

        private void NavigateForwardDropDownOpening(object sender, EventArgs e)
        {
            _navigateForwardButton.DropDownItems.Clear();
            var historyItems = NavigationManager.Instance.ForwardHistory.Select(nl =>
                new ToolStripMenuItem(nl.ToString(), null, NavigateForwardDropDownItemClick) { Tag = nl }
            );
            _navigateForwardButton.DropDownItems.AddRange(historyItems.ToArray());
        }

        private void NavigateForwardDropDownItemClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var navigationLocation = (NavigationLocation)item.Tag;
            NavigationManager.Instance.NavigateForwardTo(navigationLocation);
        }


        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            _settings = new Settings();
            if (!File.Exists(_settingFilename)) SaveSettings();
            else
            {
                object obj = ObjectSerializer.Deserialize(_settingFilename, _settings);
                _settings = (Settings)obj;
            }
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(_settingFilename, _settings);
        }

		#endregion
	}
}