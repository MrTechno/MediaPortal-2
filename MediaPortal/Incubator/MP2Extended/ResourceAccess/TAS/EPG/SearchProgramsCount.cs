using System.Collections.Generic;
using HttpServer;
using HttpServer.Sessions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.TAS.Tv;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.TAS.EPG
{
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, ReturnType = typeof(WebIntResult), Summary = "")]
  [ApiFunctionParam(Name = "searchTerm", Type = typeof(string), Nullable = false)]
  internal class SearchProgramsCount : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request, IHttpSession session)
    {
      List<WebProgramBasic> output = new SearchProgramsBasic().Process(request, session);

      return new WebIntResult { Result = output.Count };
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}