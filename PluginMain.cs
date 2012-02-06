using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;
using PluginCore.Localization;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;
using ASCompletion.Context;
using ASCompletion.Model;

namespace NavigationBar
{
	public class PluginMain : IPlugin
	{
        private String pluginName = "NavigationBar";
        private String pluginGuid = "F313AE66-0C5F-4388-B281-9E9AFAD7B8F9";
        private String pluginHelp = "www.flashdevelop.org/community/";
        private String pluginDesc = "Adds a navigation bar for quickly moving through your source.";
        private String pluginAuth = "Joey Robichaud";
        private String settingFilename = "";
        private Settings settingObject;

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
			get { return this.pluginName; }
		}

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
		{
			get { return this.pluginGuid; }
		}

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
		{
			get { return this.pluginAuth; }
		}

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
		{
			get { return this.pluginDesc; }
		}

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
		{
			get { return this.pluginHelp; }
		}

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public Object Settings
        {
            get { return this.settingObject; }
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
            this.AddShortCuts();
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
            if (e.Type == EventType.FileSwitch)
            {
                DockContent content = PluginBase.MainForm.CurrentDocument as DockContent;
                if (content != null)
                {
                    // Check to see if we've already added an NavigationBar to the 
                    // current document.
                    if (GetNavigationBar(content) != null)
                        return;

                    // Dock a new navigation bar to the top of the current document
                    NavigationBar bar = new NavigationBar(settingObject.ShowSuperClasses, settingObject.ShowInheritedMembers);
                    content.Controls.Add(bar);
                }
            }
		}
		
		#endregion

        #region Plugin Methods

        private void OpenClasses(object sender, EventArgs e)
        {
            DockContent content = PluginBase.MainForm.CurrentDocument as DockContent;
            if (content == null)
                return;

            NavigationBar navBar = GetNavigationBar(content);
            if (navBar == null || !navBar.Visible)
                return;

            navBar.OpenClasses();
        }

        private void OpenMembers(object sender, EventArgs e)
        {
             DockContent content = PluginBase.MainForm.CurrentDocument as DockContent;
            if (content == null)
                return;

            NavigationBar navBar = GetNavigationBar(content);
            if (navBar == null || !navBar.Visible)
                return;

            navBar.OpenMembers();
        }

        private NavigationBar GetNavigationBar(DockContent content)
        {
            foreach (Control control in content.Controls)
            {
                if (control is NavigationBar)
                {
                    return control as NavigationBar;
                }
            }
            return null;
        }

        private void UpdateNavigationBarSettings()
        {
            foreach (var document in FlashDevelop.Globals.MainForm.Documents)
            {
                DockContent content = document as DockContent;
                if (content != null)
                {
                    NavigationBar navBar = GetNavigationBar(content);
                    navBar.UpdateSettings(settingObject.ShowSuperClasses, settingObject.ShowInheritedMembers);
                }
            }
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, pluginName);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            this.settingFilename = Path.Combine(dataPath, "Settings.fdb");
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            // Set events you want to listen (combine as flags)
            EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);
        }

        /// <summary>
        /// Adds shortcuts for manipulating the navigation bar
        /// </summary>
        public void AddShortCuts()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            ToolStripMenuItem menuItem;
            
            menuItem = new ToolStripMenuItem("Open Classes", null, new EventHandler(OpenClasses));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.OpenClasses", menuItem);
            menu.DropDownItems.Add(menuItem);

            menuItem = new ToolStripMenuItem("Open Members", null,new EventHandler(OpenMembers));
            menuItem.Visible = false;
            PluginBase.MainForm.RegisterShortcutItem("NavigationBar.OpenMembers", menuItem);
            menu.DropDownItems.Add(menuItem);
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            this.settingObject = new Settings();
            if (!File.Exists(this.settingFilename)) this.SaveSettings();
            else
            {
                Object obj = ObjectSerializer.Deserialize(this.settingFilename, this.settingObject);
                this.settingObject = (Settings)obj;
            }
            this.settingObject.OnSettingsChanged += new SettingsChangesEvent(UpdateNavigationBarSettings);
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(this.settingFilename, this.settingObject);
        }

		#endregion

	}
	
}
