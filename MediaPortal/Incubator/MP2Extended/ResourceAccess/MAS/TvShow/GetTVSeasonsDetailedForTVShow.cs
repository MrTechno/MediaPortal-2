using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Exceptions;
using HttpServer.Sessions;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;
using Newtonsoft.Json;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.TvShow
{
  // This is a work around -> wait for MIA rework
  // TODO: Add more details
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, Summary = "")]
  [ApiFunctionParam(Name = "id", Type = typeof(string), Nullable = false)]
  [ApiFunctionParam(Name = "sort", Type = typeof(WebSortField), Nullable = true)]
  [ApiFunctionParam(Name = "order", Type = typeof(WebSortOrder), Nullable = true)]
  internal class GetTVSeasonsDetailedForTVShow : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request, IHttpSession session)
    {
      // we can't select only for seasons, so we take all episodes from a tv show and filter.

      HttpParam httpParam = request.Param;
      string showId = httpParam["id"].Value;
      if (showId == null)
        throw new BadRequestException("GetTVEpisodeCountForSeason: no id is null");


      ISet<Guid> necessaryMIATypes = new HashSet<Guid>();
      necessaryMIATypes.Add(MediaAspect.ASPECT_ID);

      // this is the MediaItem for the show
      MediaItem showItem = GetMediaItems.GetMediaItemById(showId, necessaryMIATypes);

      if (showItem == null)
        throw new BadRequestException(String.Format("GetTVSeasonsBasicForTVShow: No MediaItem found with id: {0}", showId));

      string showName;
      try
      {
        showName = (string)MP2ExtendedUtils.GetAttributeValue(showItem.Aspects, MediaAspect.ATTR_TITLE);
      }
      catch (Exception ex)
      {
        throw new BadRequestException(String.Format("GetTVSeasonsBasicForTVShow: Couldn't convert Title: {0}", ex.Message));
      }

      // Get all episodes for this
      ISet<Guid> necessaryMIATypesEpisodes = new HashSet<Guid>();
      necessaryMIATypesEpisodes.Add(MediaAspect.ASPECT_ID);
      necessaryMIATypesEpisodes.Add(SeriesAspect.ASPECT_ID);

      IFilter searchFilter = new RelationalFilter(EpisodeAspect.ATTR_SERIES_NAME, RelationalOperator.EQ, showName);
      MediaItemQuery searchQuery = new MediaItemQuery(necessaryMIATypesEpisodes, null, searchFilter);

      IList<MediaItem> episodes = ServiceRegistration.Get<IMediaLibrary>().Search(searchQuery, false, null, false);

      if (episodes.Count == 0)
        throw new BadRequestException("No Tv Episodes found");

      var output = new List<WebTVSeasonDetailed>();

      foreach (var epsisode in episodes)
      {
        var episodeAspect = MediaItemAspect.GetAspect(epsisode.Aspects, EpisodeAspect.Metadata);
        int index = output.FindIndex(x => x.Id == String.Format("{0}:{1}", showItem.MediaItemId, (int)episodeAspect[EpisodeAspect.ATTR_SEASON]));
        if (index == -1)
        {
          var episodesInThisSeason = episodes.ToList().FindAll(x => (int)MP2ExtendedUtils.GetAttributeValue(x.Aspects, EpisodeAspect.ATTR_SEASON) == (int)episodeAspect[EpisodeAspect.ATTR_SEASON]);
          var episodesInThisSeasonUnwatched = episodesInThisSeason.FindAll(x => MP2ExtendedUtils.GetAttributeValue(x.Aspects, MediaAspect.ATTR_PLAYCOUNT) == null || (int)MP2ExtendedUtils.GetAttributeValue(x.Aspects, MediaAspect.ATTR_PLAYCOUNT) == 0);

          WebTVSeasonDetailed webTVSeasonDetailed = new WebTVSeasonDetailed
          {
            Title = (string)episodeAspect[EpisodeAspect.ATTR_SERIES_SEASON],
            Id = String.Format("{0}:{1}", showItem.MediaItemId, (int)episodeAspect[EpisodeAspect.ATTR_SEASON]),
            ShowId = showItem.MediaItemId.ToString(),
            SeasonNumber = (int)episodeAspect[EpisodeAspect.ATTR_SEASON],
            EpisodeCount = episodesInThisSeason.Count,
            UnwatchedEpisodeCount = episodesInThisSeasonUnwatched.Count,
            IsProtected = false,
            PID = 0
          };
          //webTVSeasonBasic.DateAdded;
          //webTVSeasonBasic.Year;
          // Artwork

          output.Add(webTVSeasonDetailed);
        }
      }

      // sort
      string sort = httpParam["sort"].Value;
      string order = httpParam["order"].Value;
      if (sort != null && order != null)
      {
        WebSortField webSortField = (WebSortField)JsonConvert.DeserializeObject(sort, typeof(WebSortField));
        WebSortOrder webSortOrder = (WebSortOrder)JsonConvert.DeserializeObject(order, typeof(WebSortOrder));

        output = output.SortWebTVSeasonDetailed(webSortField, webSortOrder).ToList();
      }

      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}