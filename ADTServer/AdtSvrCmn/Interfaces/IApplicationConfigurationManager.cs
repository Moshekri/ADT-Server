using AdtSvrCmn.Objects;

namespace AdtSvrCmn.Interfaces
{
    public interface IApplicationConfigurationManager
    {
        ApplicationConfiguration GetConfig();
        void SaveConfig(ApplicationConfiguration config);
    }
}