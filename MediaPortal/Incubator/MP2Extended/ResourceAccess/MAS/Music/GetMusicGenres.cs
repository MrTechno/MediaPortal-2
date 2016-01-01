﻿using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Sessions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.Exceptions;
using MediaPortal.Plugins.MP2Extended.MAS;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;
using Newtonsoft.Json;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.Music
{
  // TODO: Rework after MIA Rework
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, Summary = "")]
  [ApiFunctionParam(Name = "sort", Type = typeof(WebSortField), Nullable = true)]
  [ApiFunctionParam(Name = "order", Type = typeof(WebSortOrder), Nullable = true)]
  internal class GetMusicGenres
  {
    public IList<WebGenre> Process(WebSortField? sort, WebSortOrder? order)
    {
      // we can't select only for shows, so we take all episodes and filter.
      ISet<Guid> necessaryMIATypes = new HashSet<Guid>();
      necessaryMIATypes.Add(MediaAspect.ASPECT_ID);
      necessaryMIATypes.Add(ProviderResourceAspect.ASPECT_ID);
      necessaryMIATypes.Add(ImporterAspect.ASPECT_ID);
      necessaryMIATypes.Add(AudioAspect.ASPECT_ID);

      IList<MediaItem> items = GetMediaItems.GetMediaItemsByAspect(necessaryMIATypes);

      if (items.Count == 0)
        throw new BadRequestException("No Audioitems found");

      var output = new List<WebGenre>();

      foreach (var item in items)
      {
        var videoGenres = (HashSet<object>)item[AudioAspect.Metadata][AudioAspect.ATTR_GENRES];
        List<string> videoGenresList = new List<string>();
        if (videoGenres != null)
          videoGenresList = videoGenres.Cast<string>().ToList();
        foreach (var genre in videoGenresList)
        {
          int index = output.FindIndex(x => x.Title == genre);
          if (index == -1)
          {
            WebGenre webGenre = new WebGenre { Title = genre };

            output.Add(webGenre);
          }
        }
      }

      // sort
      if (sort != null && order != null)
      {
        output = output.SortWebGenre(sort, order).ToList();
      }

      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}