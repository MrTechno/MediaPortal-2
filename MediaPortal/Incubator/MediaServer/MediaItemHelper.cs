#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using MediaPortal.Common.MediaManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.Common;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;

namespace MediaPortal.Plugins.MediaServer
{
  class MediaItemHelper
  {
    public static object GetAttributeValue(IDictionary<Guid, IList<MediaItemAspect>> aspects, SingleMediaItemAspectMetadata.SingleAttributeSpecification attribute)
    {
      object value = null;

      if (!MediaItemAspect.TryGetAttribute(aspects, attribute, out value))
        return null;

      return value;
    }

    public static IEnumerable<T> GetCollectionAttributeValues<T>(IDictionary<Guid, IList<MediaItemAspect>> aspects, SingleMediaItemAspectMetadata.SingleAttributeSpecification attribute)
    {
      IEnumerable<T> value = null;

      if (!MediaItemAspect.TryGetAttribute(aspects, attribute, out value))
        return null;

      return value;
    }

    public static object GetAttributeValue(IDictionary<Guid, IList<MediaItemAspect>> aspects, MediaItemAspectMetadata.MultipleAttributeSpecification attribute)
    {
      List<object> values = null;

      if (!MediaItemAspect.TryGetAttribute(aspects, attribute, out values))
        return null;

      if (values.Count == 0)
        return null;

      return values[0];
    }

    public static IEnumerable<T> GetCollectionAttributeValues<T>(IDictionary<Guid, IList<MediaItemAspect>> aspects, MediaItemAspectMetadata.MultipleAttributeSpecification attribute)
    {
      List<object> values = null;

      if (!MediaItemAspect.TryGetAttribute(aspects, attribute, out values))
        return null;

      if (values.Count == 0)
        return null;

      return (IEnumerable<T>)values[0];
    }

    public static IList<MultipleMediaItemAspect> GetAspects(IDictionary<Guid, IList<MediaItemAspect>> aspects, MultipleMediaItemAspectMetadata aspect)
    {
      IList<MultipleMediaItemAspect> values = null;

      if (!MediaItemAspect.TryGetAspects(aspects, aspect, out values))
        return null;

      return values;
    }

    private static string GetValue(MediaItemAspectMetadata.AttributeSpecification spec, object value)
    {
      if (value == null)
        return null;

      if (spec.ParentMIAM.AspectId == RelationshipAspect.ASPECT_ID && (RelationshipAspect.ATTR_ROLE.Equals(spec) || RelationshipAspect.ATTR_LINKED_ROLE.Equals(spec)))
      {
        if (EpisodeAspect.ROLE_EPISODE.Equals(value))
          return "Episode";

        if (SeasonAspect.ROLE_SEASON.Equals(value))
          return "Season";

        if (SeriesAspect.ROLE_SERIES.Equals(value))
          return "Series";
      }

      return value.ToString();
    }

    public static string ToString(MediaItem item)
    {
      IMediaItemAspectTypeRegistration registration = ServiceRegistration.Get<IMediaItemAspectTypeRegistration>();

      string itemStr = "";
      itemStr += string.Format("{0}", item.MediaItemId);
      //Logger.Info("Item {0} ([{1}):", item.MediaItemId, string.Join(",", item.Aspects.Keys.Select(x => x.ToString())));
      foreach (Guid mia in item.Aspects.Keys)
      {
        if (!registration.LocallyKnownMediaItemAspectTypes.ContainsKey(mia))
        {
          itemStr += string.Format("\n** No MIAM for {0} **", mia);
          continue;
        }
        MediaItemAspectMetadata metadata = registration.LocallyKnownMediaItemAspectTypes[mia];
        foreach (MediaItemAspect aspect in item.Aspects[mia])
        {
          itemStr += string.Format("\n {0}:", metadata.Name);
          int count = 0;
          string sb = "\n ";
          foreach (MediaItemAspectMetadata.AttributeSpecification spec in aspect.Metadata.AttributeSpecifications.Values)
          {
            string valueStr = null;
            if (spec.IsCollectionAttribute)
            {
              IEnumerable values = aspect.GetCollectionAttribute(spec);
              if (values != null)
              {
                IList<string> list = new List<string>();
                foreach (object value in values)
                  list.Add(GetValue(spec, value));
                valueStr = string.Format("[{0}]", string.Join(",", list));
              }
            }
            else
            {
              valueStr = GetValue(spec, aspect.GetAttributeValue(spec));
            }
            if (valueStr != null)
            {
              if (count > 0)
                sb += ",";
              sb += string.Format(" {0}={1}", spec.AttributeName, valueStr);
              count++;
            }
          }
          sb = sb.Replace("{", "{{").Replace("}", "}}");
          itemStr += sb;
        }
      }

      return itemStr;
    }
  }
}
