﻿using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Sessions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.Extensions;
using MediaPortal.Plugins.MP2Extended.MAS.OnlineVideos;
using MediaPortal.Plugins.MP2Extended.OnlineVideos;
using Newtonsoft.Json;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.OnlineVideos
{
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, ReturnType = typeof(List<WebOnlineVideosSite>), Summary = "This function returns a list of all available Sites on the OnlineVideos Server.")]
  [ApiFunctionParam(Name = "sort", Type = typeof(WebSortField), Nullable = true)]
  [ApiFunctionParam(Name = "order", Type = typeof(WebSortOrder), Nullable = true)]
  [ApiFunctionParam(Name = "filter", Type = typeof(string), Nullable = true)]
  internal class GetOnlineVideosGlobalSites : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request, IHttpSession session)
    {
      List<WebOnlineVideosGlobalSite> output = MP2Extended.OnlineVideosManager.GetGlobalSites().Select(site => new WebOnlineVideosGlobalSite
      {
        Id = OnlineVideosIdGenerator.BuildSiteId(site.Site.Name),
        Title = site.Site.Name,
        Description = site.Site.Description,
        Creator = site.Site.Owner_FK.Substring(0, site.Site.Owner_FK.IndexOf('@')).Replace('.', ' ').Replace('_', ' '),
        Language = site.Site.Language,
        IsAdult = site.Site.IsAdult,
        State = (WebOnlineVideosSiteState)site.Site.State,
        ReportCount = site.Site.ReportCount,
        LastUpdated = site.Site.LastUpdated,
        Added = site.Added
      }).ToList();

      // sort and filter
      HttpParam httpParam = request.Param;
      string sort = httpParam["sort"].Value;
      string order = httpParam["order"].Value;
      string filter = httpParam["filter"].Value;
      if (sort != null && order != null)
      {
        WebSortField webSortField = (WebSortField)JsonConvert.DeserializeObject(sort, typeof(WebSortField));
        WebSortOrder webSortOrder = (WebSortOrder)JsonConvert.DeserializeObject(order, typeof(WebSortOrder));

        output = output.AsQueryable().Filter(filter).SortMediaItemList(webSortField, webSortOrder).ToList();
      }
      else
        output = output.Filter(filter).ToList();
      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}