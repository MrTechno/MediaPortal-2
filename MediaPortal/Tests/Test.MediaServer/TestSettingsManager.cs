using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Common.Services.ResourceAccess.Settings;
using MediaPortal.Common.Settings;

namespace Test.MediaServer
{
  class TestSettingsManager : ISettingsManager
  {
    public SettingsType Load<SettingsType>() where SettingsType : class
    {
      return (SettingsType)Load(typeof(SettingsType));
    }

    public object Load(Type settingsType)
    {
      if (settingsType == typeof(ServerSettings))
      {
        return new ServerSettings();
      }

      throw new NotImplementedException("Don't know how to Load " + settingsType.Name);
    }

    public void Save(object settingsObject)
    {
      throw new NotImplementedException();
    }

    public void StartBatchUpdate()
    {
      throw new NotImplementedException();
    }

    public void EndBatchUpdate()
    {
      throw new NotImplementedException();
    }

    public void CancelBatchUpdate()
    {
      throw new NotImplementedException();
    }

    public void ClearCache()
    {
      throw new NotImplementedException();
    }

    public void RemoveSettingsData(Type settingsType, bool user, bool global)
    {
      throw new NotImplementedException();
    }

    public void RemoveAllSettingsData(bool user, bool global)
    {
      throw new NotImplementedException();
    }
  }
}
