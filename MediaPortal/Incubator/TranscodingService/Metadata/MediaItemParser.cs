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
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.Services.ResourceAccess.StreamedResourceToLocalFsAccessBridge;
using MediaPortal.Plugins.Transcoding.Aspects;
using MediaPortal.Plugins.Transcoding.Service.Metadata.Streams;
using MediaPortal.Plugins.Transcoding.Service.Analyzers;
using MediaPortal.Plugins.SlimTv.Interfaces.ResourceProvider;

namespace MediaPortal.Plugins.Transcoding.Service.Metadata
{
    public class MediaItemParser
  {
    public static MetadataContainer ParseAudioItem(MediaItem item)
    {
      MetadataContainer info = new MetadataContainer();
      IResourceAccessor mediaItemAccessor = item.GetResourceLocator().CreateAccessor();
      if (mediaItemAccessor is IFileSystemResourceAccessor)
      {
        using (var fsra = (IFileSystemResourceAccessor)mediaItemAccessor.Clone())
        {
          if (!fsra.IsFile)
            return null;
          using (var lfsra = StreamedResourceToLocalFsAccessBridge.GetLocalFsResourceAccessor(fsra))
          {
            info.Metadata.Source = lfsra;
            info.Metadata.Size = lfsra.Size;
          }
        }
      }
      else if (mediaItemAccessor is INetworkResourceAccessor)
      {
        using (var nra = (INetworkResourceAccessor)mediaItemAccessor.Clone())
        {
          info.Metadata.Source = nra;
        }
        info.Metadata.Size = 0;
      }
      if (item.Aspects.ContainsKey(TranscodeItemAudioAspect.ASPECT_ID) == true)
      {
        object oValue = null;
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemAudioAspect.ATTR_CONTAINER);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Metadata.AudioContainerType = (AudioContainer)Enum.Parse(typeof(AudioContainer), oValue.ToString());
        }
        AudioStream audio = new AudioStream();
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemAudioAspect.ATTR_STREAM);
        if (oValue != null)
        {
          audio.StreamIndex = Convert.ToInt32(oValue);
          oValue = (string)TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemAudioAspect.ATTR_CODEC);
          if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
          {
            audio.Codec = (AudioCodec)Enum.Parse(typeof(AudioCodec), oValue.ToString());
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemAudioAspect.ATTR_CHANNELS);
          if (oValue != null)
          {
            audio.Channels = Convert.ToInt32(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemAudioAspect.ATTR_FREQUENCY);
          if (oValue != null)
          {
            audio.Frequency = Convert.ToInt64(oValue);
          }
          if (item.Aspects.ContainsKey(AudioAspect.ASPECT_ID) == true)
          {
            oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, AudioAspect.ATTR_BITRATE);
            if (oValue != null)
            {
              audio.Bitrate = Convert.ToInt64(oValue);
            }
            oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, AudioAspect.ATTR_DURATION);
            if (oValue != null)
            {
              info.Metadata.Duration = Convert.ToDouble(oValue);
            }
          }
          if (item.Aspects.ContainsKey(ProviderResourceAspect.ASPECT_ID) == true)
          {
            oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_MIME_TYPE);
            if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
            {
              info.Metadata.Mime = oValue.ToString();
            }
          }
        }
        info.Audio.Add(audio);
        if (info.Audio.Count > 0 && info.Audio[0].Bitrate > 0)
        {
          info.Metadata.Bitrate = info.Audio[0].Bitrate;
        }
      }

      return info;
    }

    public static MetadataContainer ParseImageItem(MediaItem item)
    {
      MetadataContainer info = new MetadataContainer();
      IResourceAccessor mediaItemAccessor = item.GetResourceLocator().CreateAccessor();
      if (mediaItemAccessor is IFileSystemResourceAccessor)
      {
        using (var fsra = (IFileSystemResourceAccessor)mediaItemAccessor.Clone())
        {
          if (!fsra.IsFile)
            return null;
          using (var lfsra = StreamedResourceToLocalFsAccessBridge.GetLocalFsResourceAccessor(fsra))
          {
            info.Metadata.Source = lfsra;
            info.Metadata.Size = lfsra.Size;
          }
        }
      }

      if (item.Aspects.ContainsKey(TranscodeItemImageAspect.ASPECT_ID) == true)
      {
        object oValue = null;
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemImageAspect.ATTR_CONTAINER);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Metadata.ImageContainerType = (ImageContainer)Enum.Parse(typeof(ImageContainer), oValue.ToString());
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemImageAspect.ATTR_PIXEL_FORMAT);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Image.PixelFormatType = (PixelFormat)Enum.Parse(typeof(PixelFormat), oValue.ToString());
        }
        if (item.Aspects.ContainsKey(ImageAspect.ASPECT_ID) == true)
        {
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, ImageAspect.ATTR_HEIGHT);
          if (oValue != null)
          {
            info.Image.Height = Convert.ToInt32(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, ImageAspect.ATTR_WIDTH);
          if (oValue != null)
          {
            info.Image.Width = Convert.ToInt32(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, ImageAspect.ATTR_ORIENTATION);
          if (oValue != null)
          {
            info.Image.Orientation = Convert.ToInt32(oValue);
          }
        }
        if (item.Aspects.ContainsKey(ProviderResourceAspect.ASPECT_ID) == true)
        {
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_MIME_TYPE);
          if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
          {
            info.Metadata.Mime = oValue.ToString();
          }
        }
      }
      return info;
    }

    public static MetadataContainer ParseVideoItem(MediaItem item)
    {
      MetadataContainer info = new MetadataContainer();
      IResourceAccessor mediaItemAccessor = item.GetResourceLocator().CreateAccessor();
      if (mediaItemAccessor is IFileSystemResourceAccessor)
      {
        using (var fsra = (IFileSystemResourceAccessor)mediaItemAccessor.Clone())
        {
          if (!fsra.IsFile)
            return null;
          using (var lfsra = StreamedResourceToLocalFsAccessBridge.GetLocalFsResourceAccessor(fsra))
          {
            info.Metadata.Source = lfsra;
            info.Metadata.Size = lfsra.Size;
          }
        }
      }
      else if (mediaItemAccessor is INetworkResourceAccessor)
      {
        using (var nra = (INetworkResourceAccessor)mediaItemAccessor.Clone())
        {
          info.Metadata.Source = nra;
        }
        info.Metadata.Size = 0;
      }

      if (item.Aspects.ContainsKey(TranscodeItemVideoAspect.ASPECT_ID) == true)
      {
        object oValue = null;
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_CONTAINER);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Metadata.VideoContainerType = (VideoContainer)Enum.Parse(typeof(VideoContainer), oValue.ToString());
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_PIXEL_FORMAT);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Video.PixelFormatType = (PixelFormat)Enum.Parse(typeof(PixelFormat), oValue.ToString());
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_BRAND);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Metadata.MajorBrand = oValue.ToString();
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_CODEC);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Video.Codec = (VideoCodec)Enum.Parse(typeof(VideoCodec), oValue.ToString());
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_FOURCC);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Video.FourCC = oValue.ToString();
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_H264_PROFILE);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Video.ProfileType = (EncodingProfile)Enum.Parse(typeof(EncodingProfile), oValue.ToString());
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_H264_HEADER_LEVEL);
        if (oValue != null)
        {
          info.Video.HeaderLevel = Convert.ToSingle(oValue);
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_H264_REF_LEVEL);
        if (oValue != null)
        {
          info.Video.RefLevel = Convert.ToSingle(oValue);
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_PIXEL_ASPECTRATIO);
        if (oValue != null)
        {
          info.Video.PixelAspectRatio = Convert.ToSingle(oValue);
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_STREAM);
        if (oValue != null)
        {
          info.Video.StreamIndex = Convert.ToInt32(oValue);
        }
        oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, TranscodeItemVideoAspect.ATTR_TS_TIMESTAMP);
        if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
        {
          info.Video.TimestampType = (Timestamp)Enum.Parse(typeof(Timestamp), oValue.ToString());
        }

        oValue = TranscodingServiceUtils.GetAspects(item.Aspects, TranscodeItemVideoAudioAspect.Metadata);
        if (oValue != null)
        {
          IList<MultipleMediaItemAspect> aspects = oValue as IList<MultipleMediaItemAspect>;
          for (int iAudio = 0; iAudio < aspects.Count; iAudio++)
          {
            AudioStream audio = new AudioStream();
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_BITRATE) != null)
            {
              audio.Bitrate = Convert.ToInt64(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_BITRATE));
            }
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_CHANNEL) != null)
            {
              audio.Channels = Convert.ToInt32(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_CHANNEL));
            }
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_CODEC) != null && string.IsNullOrEmpty(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_CODEC).ToString()) == false)
            {
              audio.Codec = (AudioCodec)Enum.Parse(typeof(AudioCodec), aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_CODEC).ToString());
            }
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_FREQUENCY) != null)
            {
              audio.Frequency = Convert.ToInt64(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_FREQUENCY));
            }
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_LANGUAGE) != null && string.IsNullOrEmpty(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_LANGUAGE).ToString()) == false)
            {
              audio.Language = aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_LANGUAGE).ToString();
            }
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_STREAM) != null)
            {
              audio.StreamIndex = Convert.ToInt32(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_STREAM));
            }
            if (aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_DEFAULT) != null)
            {
              audio.Default = Convert.ToInt32(aspects[iAudio].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_DEFAULT)) > 0;
            }
            info.Audio.Add(audio);
          }
        }

        oValue = TranscodingServiceUtils.GetAspects(item.Aspects, TranscodeItemVideoEmbeddedAspect.Metadata);
        if (oValue != null)
        {
          IList<MultipleMediaItemAspect> aspects = oValue as IList<MultipleMediaItemAspect>;
          for (int iSub = 0; iSub < aspects.Count; iSub++)
          {
            SubtitleStream sub = new SubtitleStream();
            if (aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBCODEC) != null && string.IsNullOrEmpty(aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBCODEC).ToString()) == false)
            {
              sub.Codec = (SubtitleCodec)Enum.Parse(typeof(SubtitleCodec), aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBCODEC).ToString());
            }
            if (aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBLANGUAGE) != null && string.IsNullOrEmpty(aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBLANGUAGE).ToString()) == false)
            {
              sub.Language = aspects[iSub].GetAttributeValue(TranscodeItemVideoAudioAspect.ATTR_DEFAULT).ToString();
            }
            if (aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBSTREAM) != null)
            {
              sub.StreamIndex = Convert.ToInt32(aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBSTREAM));
            }
            if (aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBDEFAULT) != null)
            {
              sub.Default = Convert.ToInt32(aspects[iSub].GetAttributeValue(TranscodeItemVideoEmbeddedAspect.ATTR_SUBDEFAULT)) > 0;
            }
            info.Subtitles.Add(sub);
          }
        }

        if (item.Aspects.ContainsKey(VideoStreamAspect.ASPECT_ID) == true)
        {
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, VideoStreamAspect.ATTR_HEIGHT);
          if (oValue != null)
          {
            info.Video.Height = Convert.ToInt32(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, VideoStreamAspect.ATTR_WIDTH);
          if (oValue != null)
          {
            info.Video.Width = Convert.ToInt32(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, VideoStreamAspect.ATTR_ASPECTRATIO);
          if (oValue != null)
          {
            info.Video.AspectRatio = Convert.ToSingle(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, VideoStreamAspect.ATTR_DURATION);
          if (oValue != null)
          {
            info.Metadata.Duration = Convert.ToDouble(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, VideoStreamAspect.ATTR_FPS);
          if (oValue != null)
          {
            info.Video.Framerate = Convert.ToSingle(oValue);
          }
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, VideoStreamAspect.ATTR_VIDEOBITRATE);
          if (oValue != null)
          {
            info.Video.Bitrate = Convert.ToInt64(oValue);
          }
        }
        if (item.Aspects.ContainsKey(ProviderResourceAspect.ASPECT_ID) == true)
        {
          oValue = TranscodingServiceUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_MIME_TYPE);
          if (oValue != null && string.IsNullOrEmpty(oValue.ToString()) == false)
          {
            info.Metadata.Mime = oValue.ToString();
          }
        }
        if (info.Audio.Count > 0 && info.Audio[0].Bitrate > 0 && info.Video.Bitrate > 0)
        {
          info.Metadata.Bitrate = info.Audio[0].Bitrate + info.Video.Bitrate;
        }
      }
      return info;
    }

    public static MetadataContainer ParseLiveVideoItem(MediaItem item)
    {
      string resourcePathStr = (string)TranscodingServiceUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH);
      var resourcePath = ResourcePath.Deserialize(resourcePathStr);
      IResourceAccessor stra = SlimTvResourceAccessor.GetResourceAccessor(resourcePath.BasePathSegment.Path);

      if (stra is ILocalFsResourceAccessor)
      {
        return MediaAnalyzer.ParseVideoFile((ILocalFsResourceAccessor)stra);
      }
      else
      {
        return MediaAnalyzer.ParseVideoStream((INetworkResourceAccessor)stra);
      }
    }

    public static MetadataContainer ParseLiveAudioItem(MediaItem item)
    {
      string resourcePathStr = (string)TranscodingServiceUtils.GetAttributeValue(item.Aspects, ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH);
      var resourcePath = ResourcePath.Deserialize(resourcePathStr);
      IResourceAccessor stra = SlimTvResourceAccessor.GetResourceAccessor(resourcePath.BasePathSegment.Path);

      if (stra is ILocalFsResourceAccessor)
      {
        return MediaAnalyzer.ParseAudioFile((ILocalFsResourceAccessor)stra);
      }
      else
      {
        return MediaAnalyzer.ParseAudioStream((INetworkResourceAccessor)stra);
      }
    }
  }
}
