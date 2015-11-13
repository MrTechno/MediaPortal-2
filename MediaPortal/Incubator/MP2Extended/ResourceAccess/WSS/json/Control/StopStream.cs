﻿using HttpServer;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.Common;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.json.Control
{
  internal class StopStream : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      HttpParam httpParam = request.Param;
      
      string identifier = httpParam["identifier"].Value;
      bool result = true;

      if (identifier == null)
      {
        Logger.Debug("StopStream: identifier is null");
        result = false;
      }

      if (!StreamControl.ValidateIdentifie(identifier))
      {
        Logger.Debug("StopStream: unknown identifier: {0}", identifier);
        result = false;
      }

      StreamControl.StopStreaming(identifier);

     return new WebBoolResult { Result = result };
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}
