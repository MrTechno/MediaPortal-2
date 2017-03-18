using System;
using System.Collections.Generic;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS.Picture;
using MediaPortal.Utilities;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.Picture.BaseClasses
{
  class BasePictureBasic
  {
    internal WebPictureBasic PictureBasic(MediaItem item)
    {
      ResourcePath path = ResourcePath.Deserialize((string)MP2ExtendedUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH));

      WebPictureBasic webPictureBasic = new WebPictureBasic
      {
        Type = WebMediaType.Picture,
        DateAdded = (DateTime)MP2ExtendedUtils.GetAttributeValue(item.Aspects, ImporterAspect.ATTR_DATEADDED),
        Id = item.MediaItemId.ToString(),
        PID = 0,
        Title = (string)MP2ExtendedUtils.GetAttributeValue(item.Aspects, MediaAspect.ATTR_TITLE),
        DateTaken = (DateTime)MP2ExtendedUtils.GetAttributeValue(item.Aspects, MediaAspect.ATTR_RECORDINGTIME),
        Path = new List<string> { (path != null && path.PathSegments.Count > 0) ? StringUtils.RemovePrefixIfPresent(path.LastPathSegment.Path, "/") : string.Empty },
      };

      //webPictureBasic.Categories = imageAspects.GetAttributeValue(ImageAspect);
      //webPictureBasic.Artwork;

      return webPictureBasic;
    }
  }
}
