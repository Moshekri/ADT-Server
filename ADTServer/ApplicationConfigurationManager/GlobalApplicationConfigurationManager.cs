using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AdtSvrCmn.Objects;
using AdtSvrCmn.Interfaces;

namespace GlobalApplicationConfigurationManager
{
    public class ApplicationConfigurationManager :IApplicationConfigurationManager
    {
        private readonly string _configFile;
        private readonly XmlSerializer _xmlReader;
        private const string Source = "Application Configuration Manager";

        public ApplicationConfigurationManager(string configFileFullPath)
        {
            var path = Path.GetDirectoryName(configFileFullPath);
            if (!Directory.Exists(path))
            {
               Directory.CreateDirectory(path);
            }
            _configFile = configFileFullPath;
            _xmlReader = new XmlSerializer(typeof(ApplicationConfiguration));
           
        }

        /// <summary>
        /// Returns Application Configuration
        /// </summary>
        /// <returns>ApplicationConfiguration</returns>
        public ApplicationConfiguration GetConfig()
        {
            try
            {
                using (var configFileStream = OpenConfigFile())
                {
                    var xmlReader = XmlReader.Create(configFileStream);
                    if (_xmlReader.CanDeserialize(xmlReader))
                    {
                        var configuration = _xmlReader.Deserialize(xmlReader);
                        return configuration as ApplicationConfiguration;
                    }
                    else
                    {
                        return null;
                    }
                }
                
            }

            catch (Exception ex)
            {
                throw new Exception(Source, ex);
            }


        }

        public void SaveConfig(ApplicationConfiguration config)
        {
            try
            {
                var configFileSream = OpenConfigFile();
                _xmlReader.Serialize(configFileSream, config);
                configFileSream.Close();
            }
            catch (Exception e)
            {
                throw new Exception(Source, e);
            }
        }

        private FileStream OpenConfigFile()
        {

            FileStream configFile = null;
            try
            {
                if (!File.Exists(_configFile))
                {
                    CreateConfigFile();
                  
                }
                configFile = File.Open(_configFile,FileMode.Open,FileAccess.ReadWrite);
                return configFile;
            }
            catch (FileNotFoundException ex)
            {
                configFile?.Close();
                throw new Exception(Source, ex);
            }
            catch (Exception ex)
            {
                configFile?.Close();
                throw new Exception(Source, ex);
            }


        }

        private void CreateConfigFile()
        {
            var fileStream = File.Create(_configFile);
            fileStream.Close();
            using (var writeStream = File.OpenWrite(_configFile))
            {
                _xmlReader.Serialize(writeStream, new ApplicationConfiguration());

            }

        }
    }
}
