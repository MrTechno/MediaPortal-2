using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.TvShow.BaseClasses
{
  class BaseEpisodesDetailed
  {
    internal WebTVEpisodeDetailed EpisodeDetailed(MediaItem item, MediaItem showItem = null)
    {
      WebTVEpisodeBasic webMovieBasic = new BaseEpisodeBasic().EpisodeBasic(item);
      MediaItemAspect episodeAspects = MediaItemAspect.GetAspect(item.Aspects, EpisodeAspect.Metadata);

      if (showItem == null)
        showItem = GetMediaItems.GetMediaItemByName((string)episodeAspects[EpisodeAspect.ATTR_SERIES_NAME], null);

      WebTVEpisodeDetailed webTvEpisodeDetailed = new WebTVEpisodeDetailed
      {
        EpisodeNumber = webMovieBasic.EpisodeNumber,
        ExternalId = webMovieBasic.ExternalId,
        FirstAired = webMovieBasic.FirstAired,
        IsProtected = webMovieBasic.IsProtected,
        Rating = webMovieBasic.Rating,
        SeasonNumber = webMovieBasic.SeasonNumber,
        ShowId = webMovieBasic.ShowId,
        SeasonId = webMovieBasic.SeasonId,
        Type = webMovieBasic.Type,
        Watched = webMovieBasic.Watched,
        Path = webMovieBasic.Path,
        DateAdded = webMovieBasic.DateAdded,
        Id = webMovieBasic.Id,
        PID = webMovieBasic.PID,
        Title = webMovieBasic.Title,
        Artwork = webMovieBasic.Artwork
      };
      
      webTvEpisodeDetailed.Summary = (string)MP2ExtendedUtils.GetAttributeValue(item.Aspects, VideoAspect.ATTR_STORYPLOT);
      webTvEpisodeDetailed.Show = (string)MP2ExtendedUtils.GetAttributeValue(item.Aspects, EpisodeAspect.ATTR_SERIES_NAME);
      var videoWriters = (HashSet<object>)MP2ExtendedUtils.GetAttributeValue(item.Aspects, VideoAspect.ATTR_WRITERS);
      if (videoWriters != null)
        webTvEpisodeDetailed.Writers = videoWriters.Cast<string>().ToList();
      var videoDirectors = (HashSet<object>)MP2ExtendedUtils.GetAttributeValue(item.Aspects, VideoAspect.ATTR_DIRECTORS);
      if (videoDirectors != null)
        webTvEpisodeDetailed.Directors = videoDirectors.Cast<string>().ToList();
      // webTVEpisodeDetailed.GuestStars not in the MP DB

      return webTvEpisodeDetailed;
    }
  }
}
