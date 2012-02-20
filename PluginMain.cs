using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using PluginCore.Localization;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;
using ASCompletion.Context;
using ASCompletion.Model;
using NavigationBar.Controls;
using NavigationBar.Helpers;
using NavigationBar.Managers;
using ProjectManager;

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
        private ToolStripButton _navigateForwardButton = null;
        private ToolStripButton _navigateBackwardButton = null;
        private ToolStripSeparator _navigateSeparator = null;


	    #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public Int32 Api
        {
            get { return 1; }
        }

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public String Name
		{
			get { return NAME; }
		}

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
		{
			get { return GUID; }
		}

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
		{
			get { return AUTHOR; }
		}

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
		{
			get { return DESCRIPTION; }
		}

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
		{
			get { return HELP; }
		}

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public Object Settings
        {
            get { return this._settings; }
        }
		
		#endregion
		
		#region Required Methods
		
		/// <summary>
		/// Initializes the plugin
		/// </summary>
		public void Initialize()
		{
            this.InitBasics();
            this.LoadSettings();
            this.AddEventHandlers();
            this.CreateMenuItems();
            this.CreateToolbarItems();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
		{
            this.SaveSettings();
		}
		
		/// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
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
                    bar = new Controls.NavigationBar(_settings);
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
                    if (de.Action == ProjectManagerCommands.NewProject)
                        NavigationManager.Instance.Clear();
                    else if (de.Action == ProjectManagerCommands.OpenProject)
                        NavigationManager.Instance.Clear();
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
            _navigateSeparator.Visible = _settings.ShowNavigationToolbar;
        }

        void _navigateForwardButton_Click(object sender, EventArgs e)
        {
            NavigationManager.Instance.NavigateForward();
        }

        void _navigateBackwardButton_Click(object sender, EventArgs e)
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
            foreach (Control control in document.Controls)
            {
                if (control is Controls.NavigationBar)
                {
                    return control as Controls.NavigationBar;
                }
            }
            return null;
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, NAME);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            this._settingFilename = Path.Combine(dataPath, "Settings.fdb");
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            // Set events you want to listen (combine as flags)
            EventManager.AddEventHandler(this, EventType.FileNew | EventType.FileOpen | EventType.FileSwitch | EventType.Command);
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

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.NavigateForward"), null, new EventHandler(OpenClasses));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.NavigateForward", menuItem);
            menu.DropDownItems.Add(menuItem);

            menuItem = new ToolStripMenuItem(ResourceHelper.GetString("NavigationBar.Label.NavigateBackward"), null, new EventHandler(OpenMembers));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.NavigateBackward", menuItem);
            menu.DropDownItems.Add(menuItem);
        }

        public void CreateToolbarItems()
        {
            ToolStripButton navigateButton;

            navigateButton = (ToolStripButton)PluginBase.MainForm.FindMenuItem("ToggleBookmark");
            int index = PluginBase.MainForm.ToolStrip.Items.IndexOf(navigateButton);

            _navigateBackwardButton = new ToolStripButton(ResourceHelper.GetString("NavigationBar.Label.NavigateBackward"), PluginBase.MainForm.FindImage("315|1|-3|3"));
            _navigateBackwardButton.Name = "NavigateBackward";
            _navigateBackwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _navigateBackwardButton.Click += new EventHandler(_navigateBackwardButton_Click);
            PluginBase.MainForm.ToolStrip.Items.Insert(index, _navigateBackwardButton);

            _navigateForwardButton = new ToolStripButton(ResourceHelper.GetString("NavigationBar.Label.NavigateForward"), PluginBase.MainForm.FindImage("315|9|3|3"));
            _navigateForwardButton.Name = "NavigateForward";
            _navigateForwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _navigateForwardButton.Click += new EventHandler(_navigateForwardButton_Click);
            PluginBase.MainForm.ToolStrip.Items.Insert(index + 1, _navigateForwardButton);

            _navigateSeparator = new ToolStripSeparator();
            PluginBase.MainForm.ToolStrip.Items.Insert(index + 2, _navigateSeparator);

            UpdateNavigationButtons();
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            this._settings = new Settings();
            if (!File.Exists(this._settingFilename)) this.SaveSettings();
            else
            {
                Object obj = ObjectSerializer.Deserialize(this._settingFilename, this._settings);
                this._settings = (Settings)obj;
            }
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(this._settingFilename, this._settings);
        }

		#endregion
	}
}