﻿using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Exceptions;
using HttpServer.Sessions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.Extensions;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;
using Newtonsoft.Json;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.TvShow
{
  // This is a work around -> wait for MIA rework
  // Add more details
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, Summary = "")]
  [ApiFunctionParam(Name = "start", Type = typeof(string), Nullable = false)]
  [ApiFunctionParam(Name = "end", Type = typeof(string), Nullable = false)]
  [ApiFunctionParam(Name = "sort", Type = typeof(WebSortField), Nullable = true)]
  [ApiFunctionParam(Name = "order", Type = typeof(WebSortOrder), Nullable = true)]
  [ApiFunctionParam(Name = "filter", Type = typeof(string), Nullable = true)]
  internal class GetTVShowsBasicByRange : IRequestMicroModuleHandler
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
        throw new BadRequestException(String.Format("GetTVShowsBasicByRange: Couldn't convert start to int: {0}", start));
      }

      int endInt;
      if (!Int32.TryParse(end, out endInt))
      {
        throw new BadRequestException(String.Format("GetTVShowsBasicByRange: Couldn't convert end to int: {0}", end));
      }

      // we can't select only for shows, so we take all episodes and filter the shows.
      ISet<Guid> necessaryMIATypes = new HashSet<Guid>();
      necessaryMIATypes.Add(MediaAspect.ASPECT_ID);
      necessaryMIATypes.Add(ProviderResourceAspect.ASPECT_ID);
      necessaryMIATypes.Add(ImporterAspect.ASPECT_ID);
      necessaryMIATypes.Add(EpisodeAspect.ASPECT_ID);

      IList<MediaItem> items = GetMediaItems.GetMediaItemsByAspect(necessaryMIATypes);

      if (items.Count == 0)
        throw new BadRequestException("No Tv Episodes found");

      var output = new List<WebTVShowBasic>();

      foreach (var item in items)
      {
        var seriesAspect = MediaItemAspect.GetAspect(item.Aspects, EpisodeAspect.Metadata);
        int index = output.FindIndex(x => x.Title == (string)seriesAspect[EpisodeAspect.ATTR_SERIES_NAME]);
        if (index == -1)
        {
          var episodesInThisShow = items.ToList().FindAll(x => (string)MP2ExtendedUtils.GetAttributeValue(x.Aspects, EpisodeAspect.ATTR_SERIES_NAME) == (string)seriesAspect[EpisodeAspect.ATTR_SERIES_NAME]);
          var episodesInThisShowUnwatched = episodesInThisShow.FindAll(x => MP2ExtendedUtils.GetAttributeValue(x.Aspects, MediaAspect.ATTR_PLAYCOUNT) == null || (int)MP2ExtendedUtils.GetAttributeValue(x.Aspects, MediaAspect.ATTR_PLAYCOUNT) == 0);
          necessaryMIATypes = new HashSet<Guid>();
          necessaryMIATypes.Add(MediaAspect.ASPECT_ID);
          MediaItem show = GetMediaItems.GetMediaItemByName((string)seriesAspect[EpisodeAspect.ATTR_SERIES_NAME], necessaryMIATypes);

          if (show == null)
          {
            Logger.Warn("GetTVShowsBasic: Couldn't find show: {0}", (string)seriesAspect[EpisodeAspect.ATTR_SERIES_NAME]);
            continue;
          }

          WebTVShowBasic webTVShowBasic = new WebTVShowBasic();
          webTVShowBasic.Id = show.MediaItemId.ToString();
          webTVShowBasic.Title = (string)seriesAspect[EpisodeAspect.ATTR_SERIES_NAME];
          webTVShowBasic.EpisodeCount = episodesInThisShow.Count;
          webTVShowBasic.UnwatchedEpisodeCount = episodesInThisShowUnwatched.Count;

          output.Add(webTVShowBasic);
        }
      }

      // sort and filter
      string sort = httpParam["sort"].Value;
      string order = httpParam["order"].Value;
      string filter = httpParam["filter"].Value;
      if (sort != null && order != null)
      {
        WebSortField webSortField = (WebSortField)JsonConvert.DeserializeObject(sort, typeof(WebSortField));
        WebSortOrder webSortOrder = (WebSortOrder)JsonConvert.DeserializeObject(order, typeof(WebSortOrder));

        output = output.Filter(filter).SortWebTVShowBasic(webSortField, webSortOrder).ToList();
      }
      else
        output = output.Filter(filter).ToList();

      // get range
      output = output.TakeRange(startInt, endInt).ToList();

      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}