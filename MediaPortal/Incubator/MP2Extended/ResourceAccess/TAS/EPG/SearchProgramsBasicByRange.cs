﻿using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Exceptions;
using HttpServer.Sessions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Extensions;
using MediaPortal.Plugins.MP2Extended.TAS.Tv;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.TAS.EPG
{
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, ReturnType = typeof(List<WebProgramBasic>), Summary = "")]
  [ApiFunctionParam(Name = "searchTerm", Type = typeof(string), Nullable = false)]
  [ApiFunctionParam(Name = "start", Type = typeof(int), Nullable = false)]
  [ApiFunctionParam(Name = "end", Type = typeof(int), Nullable = false)]
  internal class SearchProgramsBasicByRange : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request, IHttpSession session)
    {
      HttpParam httpParam = request.Param;
      string start = httpParam["start"].Value;
      string end = httpParam["end"].Value;

      if (start == null || end == null)
        throw new BadRequestException("start or end parameter is missing");

      int startInt;
      if (!Int32.TryParse(start, out startInt))
      {
        throw new BadRequestException(String.Format("SearchProgramsBasicByRange: Couldn't convert start to int: {0}", start));
      }

      int endInt;
      if (!Int32.TryParse(end, out endInt))
      {
        throw new BadRequestException(String.Format("SearchProgramsBasicByRange: Couldn't convert end to int: {0}", end));
      }

      List<WebProgramBasic> output = new SearchProgramsBasic().Process(request, session);

      // Get Range
      output = output.TakeRange(startInt, endInt).ToList();

      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}