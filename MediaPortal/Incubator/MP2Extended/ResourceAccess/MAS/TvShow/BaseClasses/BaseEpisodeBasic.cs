using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;
using MediaPortal.Utilities;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.TvShow.BaseClasses
{
  class BaseEpisodeBasic
  {
    internal WebTVEpisodeBasic EpisodeBasic(MediaItem item, MediaItem showItem = null)
    {
      MediaItemAspect episodeAspects = MediaItemAspect.GetAspect(item.Aspects, EpisodeAspect.Metadata);
      ResourcePath path = ResourcePath.Deserialize((string)MP2ExtendedUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH));

      if (showItem == null)
        showItem = GetMediaItems.GetMediaItemByName((string)episodeAspects[EpisodeAspect.ATTR_SERIES_NAME], null);

      WebTVEpisodeBasic webTvEpisodeBasic = new WebTVEpisodeBasic
      {
        IsProtected = false, //??
        Rating = episodeAspects[SeriesAspect.ATTR_TOTAL_RATING] == null ? 0 : Convert.ToSingle((double)episodeAspects[SeriesAspect.ATTR_TOTAL_RATING]),
        SeasonNumber = (int)episodeAspects[EpisodeAspect.ATTR_SEASON],
        Type = WebMediaType.TVEpisode,
        Watched = ((int)(MP2ExtendedUtils.GetAttributeValue(item.Aspects, MediaAspect.ATTR_PLAYCOUNT) ?? 0) > 0),
        Path = new List<string> { (path != null && path.PathSegments.Count > 0) ? StringUtils.RemovePrefixIfPresent(path.LastPathSegment.Path, "/") : string.Empty },
        //Artwork = ,
        DateAdded = (DateTime)MP2ExtendedUtils.GetAttributeValue(item.Aspects, ImporterAspect.ATTR_DATEADDED),
        Id = item.MediaItemId.ToString(),
        PID = 0,
        Title = (string)episodeAspects[EpisodeAspect.ATTR_EPISODE_NAME],
      };
      var episodeNumber = ((HashSet<object>)MP2ExtendedUtils.GetAttributeValue(item.Aspects, EpisodeAspect.ATTR_EPISODE)).Cast<int>().ToList();
      webTvEpisodeBasic.EpisodeNumber = episodeNumber[0];
      string TvDbId;
      if (MediaItemAspect.TryGetExternalAttribute(item.Aspects, ExternalIdentifierAspect.SOURCE_TVDB, ExternalIdentifierAspect.TYPE_MOVIE, out TvDbId))
      {
        webTvEpisodeBasic.ExternalId.Add(new WebExternalId
        {
          Site = "TVDB",
          Id = TvDbId
        });
      }
      string ImdbId;
      if (MediaItemAspect.TryGetExternalAttribute(item.Aspects, ExternalIdentifierAspect.SOURCE_IMDB, ExternalIdentifierAspect.TYPE_MOVIE, out ImdbId))
      {
        webTvEpisodeBasic.ExternalId.Add(new WebExternalId
        {
          Site = "IMDB",
          Id = ImdbId
        });
      }

      var firstAired = MP2ExtendedUtils.GetAttributeValue(item.Aspects, MediaAspect.ATTR_RECORDINGTIME);
      if (firstAired != null)
        webTvEpisodeBasic.FirstAired = (DateTime)firstAired;
      
      if (showItem != null)
      {
        webTvEpisodeBasic.ShowId = showItem.MediaItemId.ToString();
        webTvEpisodeBasic.SeasonId = string.Format("{0}:{1}", showItem.MediaItemId, (int)episodeAspects[EpisodeAspect.ATTR_SEASON]);
      }
      

      return webTvEpisodeBasic;
    }
  }
}
