using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using GlobalApplicationConfigurationManager;

namespace ConfigurationEditor
{
    public partial class Form1 : Form
    {
        private ApplicationConfiguration _config;
        private int windowWidth;
        private IApplicationConfigurationManager configurationManager;
        FormComponentBuilder componentBuilder;
        int xCoordinate = 20;
        int yCoordinate = 20;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
            CreateFormControlsFromConfig();
            AddButtonsToForm();
            SetFormProperties(windowWidth);
        }

        private void Initialize()
        {
            windowWidth = this.Size.Width;
            var configFilePath = ConfigurationManager.AppSettings["ConfigFileFullPath"];
            configurationManager = new ApplicationConfigurationManager(configFilePath);
        }

        private void CreateFormControlsFromConfig()
        {
            try
            {
                _config = configurationManager.GetConfig();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            componentBuilder = new FormComponentBuilder(_config, configurationManager);
            
            this.Controls.AddRange(componentBuilder.GetFormControlsFromConfig(xCoordinate, yCoordinate).ToArray());
        }

        private void SetFormProperties(int windowWidth)
        {
            this.Size = new Size(windowWidth+200, this.Controls[this.Controls.Count - 1].Location.Y + 100);
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
        }

        private void AddButtonsToForm()
        {
            Button exitButton = new Button();
            exitButton.Text = "Quit";
            exitButton.Click += ExitButton_Click;
            exitButton.Location = new Point(this.Size.Width - 100 - exitButton.Size.Width,
            this.Size.Height - 20 - exitButton.Size.Height);
            this.Controls.Add(exitButton);

            Button saveSettings = new Button();
            saveSettings.Click += SaveSettings_Click;
            saveSettings.Text = "Save Settings";
            saveSettings.Width = saveSettings.Text.Length * 10;
            saveSettings.Location = new Point(exitButton.Location.Y - exitButton.Width - 50, exitButton.Location.Y);
            this.Controls.Add(saveSettings);
        }

        private void SaveSettings_Click(object sender, EventArgs e)
        {
            var result = componentBuilder.SaveSettings(this.Controls, _config);
            MessageBox.Show(result);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
