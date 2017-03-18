using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS;
using MediaPortal.Plugins.MP2Extended.MAS.Movie;
using MediaPortal.Utilities;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.Movie.BaseClasses
{
  class BaseMovieBasic
  {
    internal WebMovieBasic MovieBasic(MediaItem item)
    {
      SingleMediaItemAspect movieAspects;
      MediaItemAspect.TryGetAspect(item.Aspects, MovieAspect.Metadata, out movieAspects);
      ResourcePath path = ResourcePath.Deserialize((string)MP2ExtendedUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH));

      WebMovieBasic webMovieBasic = new WebMovieBasic
      {
        ExternalId = new List<WebExternalId>(),
        Runtime = (int)movieAspects[MovieAspect.ATTR_RUNTIME_M],
        IsProtected = false, //??
        Type = WebMediaType.Movie,
        Watched = ((int)(MP2ExtendedUtils.GetAttributeValue(item.Aspects, MediaAspect.ATTR_PLAYCOUNT) ?? 0) > 0),
        DateAdded = (DateTime)MP2ExtendedUtils.GetAttributeValue(item.Aspects, ImporterAspect.ATTR_DATEADDED),
        Id = item.MediaItemId.ToString(),
        PID = 0,
        Title = (string)movieAspects[MovieAspect.ATTR_MOVIE_NAME],
        Year = ((DateTime)MP2ExtendedUtils.GetAttributeValue(item.Aspects, MediaAspect.ATTR_RECORDINGTIME)).Year,
        Path = new List<string> { (path != null && path.PathSegments.Count > 0) ? StringUtils.RemovePrefixIfPresent(path.LastPathSegment.Path, "/") : string.Empty },
        Actors = new BaseMovieActors().MovieActors(item)
        //Artwork = 
      };
      string TMDBId;
      if (MediaItemAspect.TryGetExternalAttribute(item.Aspects, ExternalIdentifierAspect.SOURCE_TMDB, ExternalIdentifierAspect.TYPE_MOVIE, out TMDBId))
      {
        webMovieBasic.ExternalId.Add(new WebExternalId
        {
          Site = "TMDB",
          Id = TMDBId
        });
      }
      string ImdbId;
      if (MediaItemAspect.TryGetExternalAttribute(item.Aspects, ExternalIdentifierAspect.SOURCE_IMDB, ExternalIdentifierAspect.TYPE_MOVIE, out ImdbId))
      {
        webMovieBasic.ExternalId.Add(new WebExternalId
        {
          Site = "IMDB",
          Id = ImdbId
        });
      }

      
      var rating = movieAspects.GetAttributeValue(MovieAspect.ATTR_TOTAL_RATING);
      if (rating != null)
        webMovieBasic.Rating = Convert.ToSingle(rating);
      
      var movieGenres = (HashSet<object>)MP2ExtendedUtils.GetAttributeValue(item.Aspects, GenreAspect.ATTR_GENRE);
      if (movieGenres != null)
        webMovieBasic.Genres = movieGenres.Cast<string>().ToList();

      return webMovieBasic;
    }
  }
}
