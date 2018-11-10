// converted to use nlog


using System;
using System.Linq;
using AdtSvrCmn.Interfaces;
using AdtSvrCmn.Objects;
using System.IO;
using System.Reflection;
using NLog;
using AdtSvrCmn;

namespace PlugInManager
{
    public class PlugInManager
    {
        string pluginpath;
        Logger logger;
        ApplicationConfiguration _config;


        public PlugInManager(string plugInFolder, ApplicationConfiguration config)
        {
            _config = config;
            logger = LogManager.GetCurrentClassLogger();
            this.pluginpath = plugInFolder;
        }
        public IPatientIdHandler GetIdPatientHandlers()
        {
            IPatientIdHandler pidHandler;
            Assembly assy = null;
            Type[] types = null;
            var files = Directory.GetFiles(pluginpath, "*.dll");
            foreach (var fileName in files)
            {
                try
                {
                    assy = Assembly.LoadFile(fileName);
                    types = assy.GetTypes();
                    foreach (var t in types)
                    {
                        var interfaces = t.GetInterfaces();
                        if (interfaces.Contains(typeof(IPatientIdHandler)))
                        {
                            pidHandler = assy.CreateInstance(t.ToString()) as IPatientIdHandler;
                            logger.Info($"Loaded patient id parser plug-in {Path.GetFileName(fileName)}");
                            return pidHandler;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message);
                }
            }
            logger.Info($"No Sutible Patient id parser plug-inswere found in {Path.GetDirectoryName(pluginpath)}");
            return null;
        }
        public IPatientInfoSoapSource GetWebServiceClientApp()
        {
            IPatientInfoSoapSource webServiceClient;
            var files = Directory.GetFiles(pluginpath, "*.dll");
            foreach (var fileName in files)
            {
                try
                {
                    var assy = Assembly.LoadFile(fileName);
                    var types = assy.GetTypes();
                    foreach (Type t in types)
                    {
                        var interfaces = t.GetInterfaces();
                        if (interfaces.Contains(typeof(IPatientInfoSoapSource)))
                        {

                            webServiceClient = Activator.CreateInstance(t, _config) as IPatientInfoSoapSource;
                            logger.Info($"Loaded  patient dempgraphics web service plug-in{Path.GetFileName(fileName)}");
                            return webServiceClient;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NlogHelper.LogException(ex, logger);
                }

            }
            logger.Info($"No Sutible patient dempgraphics web service client plug-ins were found in {Path.GetDirectoryName(pluginpath)}");

            return null;
        }


    }
}
