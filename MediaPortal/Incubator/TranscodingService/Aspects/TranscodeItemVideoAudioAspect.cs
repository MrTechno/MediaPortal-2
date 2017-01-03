#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
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

using System;
using MediaPortal.Common.MediaManagement;

namespace MediaPortal.Plugins.Transcoding.Aspects
{
  public class TranscodeItemVideoAudioAspect
  {
    /// <summary>
    /// Media item aspect id of the recording aspect.
    /// </summary>
    public static readonly Guid ASPECT_ID = new Guid("49E98E8F-A115-4648-A245-1BBA34934CB5");

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_LANGUAGE =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Language", 2, Cardinality.Inline, false);

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_CODEC =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Codec", 20, Cardinality.Inline, false);

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_STREAM =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Stream", 2, Cardinality.Inline, false);

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_BITRATE =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Bitrate", 10, Cardinality.Inline, false);

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_CHANNEL =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Channel", 2, Cardinality.Inline, false);

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_DEFAULT =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Default", 1, Cardinality.Inline, false);

    public static readonly MediaItemAspectMetadata.MultipleAttributeSpecification ATTR_FREQUENCY =
        MediaItemAspectMetadata.CreateMultipleStringAttributeSpecification("Frequency", 5, Cardinality.Inline, false);

    public static readonly MultipleMediaItemAspectMetadata Metadata = new MultipleMediaItemAspectMetadata(
        ASPECT_ID, "TranscodeItemVideoAudio", new[] {
            ATTR_LANGUAGE,
            ATTR_CODEC,
            ATTR_STREAM,
            ATTR_BITRATE,
            ATTR_CHANNEL,
            ATTR_DEFAULT,
            ATTR_FREQUENCY,
        });
  }
}
