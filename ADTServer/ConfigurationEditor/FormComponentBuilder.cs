using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using static System.Windows.Forms.Control;

namespace ConfigurationEditor
{
    public class FormComponentBuilder
    {
        object _config;
        Type _configType;
        IApplicationConfigurationManager _configManager;


        public FormComponentBuilder(object configuration, IApplicationConfigurationManager configManager)
        {
            _config = configuration;
            _configType = configuration.GetType();
            _configManager = configManager;
        }

        public  List<Control> GetFormControlsFromConfig(int XCoordinates, int YCoordinates)
        {
            List<Control> controls = new List<Control>();

            var xCoordinate = XCoordinates;
            var yCoordinate = XCoordinates;

            int windowWidth = 0;


            var props = _config.GetType().GetProperties();


            foreach (var propertyInfo in props)
            {
                Label lbl = new Label();
                lbl.Text = propertyInfo.Name;
                lbl.Location = new Point(xCoordinate, yCoordinate);
                lbl.Name = "lbl" + propertyInfo.Name;
                lbl.AutoSize = true;

                TextBox txtBox = new TextBox();
                txtBox.Text = propertyInfo.GetValue(_config).ToString();
                txtBox.Location = new Point(lbl.Location.X + lbl.Size.Width + 100, lbl.Location.Y);
                txtBox.AutoSize = false;
                txtBox.Name = propertyInfo.Name;
                txtBox.Size = new Size(txtBox.Text.Length * 10, txtBox.Size.Height+5);
                if (windowWidth < txtBox.Text.Length * 10)
                {
                    windowWidth = txtBox.Text.Length * 10;
                }

                yCoordinate = txtBox.Location.Y + 30;

                controls.Add(lbl);
                controls.Add(txtBox);

                
            }
            return controls;

        }

        public string SaveSettings(ControlCollection controls, ApplicationConfiguration config)
        {
            var props = _config.GetType().GetProperties();

          
            foreach (Control control in controls)
            {
                PropertyInfo prop = props.FirstOrDefault(p => p.Name == control.Name);
                if (prop != null)
                {
                    prop.SetValue(_config, control.Text);
                }
            }
            try
            {
                var con = _config as ApplicationConfiguration;
                _configManager.SaveConfig(con);
                return "Data saved succsessfuly to configuration file";
            }
            catch (Exception exception)
            {
                return exception.Message;
                throw;
            }
        }
    }

}
