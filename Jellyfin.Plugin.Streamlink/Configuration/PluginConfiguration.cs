﻿using System.Globalization;
using System.Collections.Generic;

using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Channels;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Common.Extensions;

using MediaBrowser.Providers.Plugin.Streamlink;

namespace Jellyfin.Plugin.Streamlink.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            StreamlinkPath = null;
            StreamQuality = "best";
            ExtraArguments = "";
            Channels = new ChannelConfig[] {};
        }

        public string StreamlinkPath { get; set; }
        public string StreamQuality { get; set; }
        public string ExtraArguments { get; set; }
        public ChannelConfig[] Channels { get; set; }
    }

    public class ChannelConfig
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }

        public string Id { get => Url.GetMD5().ToString("N", CultureInfo.InvariantCulture); }

        public virtual ChannelItemInfo CreateChannelItemInfo()
        {
            return new ChannelItemInfo
            {
                Name = Name,
                ImageUrl = Image,
                Id = Id,
                Type = ChannelItemType.Media,
                ContentType = ChannelMediaContentType.Clip,
                MediaType = ChannelMediaType.Video,
                IsLiveStream = true,
                MediaSources = new List<MediaSourceInfo>{ CreateMediaSourceInfo() }
            };
        }

        public virtual MediaSourceInfo CreateMediaSourceInfo()
        {
            var mediaSource = new MediaSourceInfo
            {
                Path = Url,
                Protocol = MediaProtocol.File,
                MediaStreams = new List<MediaStream>
                {
                    new MediaStream
                    {
                        Type = MediaStreamType.Video,
                        // Set the index to -1 because we don't know the exact index of the video stream within the container
                        // For twitch streams it seems to be 1, but that might not be consistent
                        Index = -1,
                        IsInterlaced = true
                    },
                    new MediaStream
                    {
                        Type = MediaStreamType.Audio,
                        // Set the index to -1 because we don't know the exact index of the audio stream within the container
                        // For twitch streams it seems to be 0
                        Index = -1
                    }
                },
                RequiresOpening = true,
                RequiresClosing = true,
                RequiresLooping = false,

                OpenToken = StreamlinkProvider.Prefix + Id,

                ReadAtNativeFramerate = false,

                Id = Id,
                IsInfiniteStream = true,
                IsRemote = true,

                IgnoreDts = true,
                SupportsDirectPlay = true,
                SupportsDirectStream = true
            };

            mediaSource.InferTotalBitrate();

            return mediaSource;
        }
    }
}
