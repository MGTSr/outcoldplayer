﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using SQLite;

    [Table("Song")]
    public class Song
    {
        [PrimaryKey]
        public string SongId { get; set; }

        public string ClientId { get; set; }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        public TimeSpan Duration { get; set; }

        public string Composer { get; set; }

        public uint PlayCount { get; set; }

        [Indexed]
        public byte Rating { get; set; }

        public int? Disc { get; set; }

        public int? TotalDiscs { get; set; }

        public int? Track { get; set; }

        public int? TotalTracks { get; set; }

        public int? Year { get; set; }

        [Indexed]
        public Uri AlbumArtUrl { get; set; }

        public Uri ArtistArtUrl { get; set; }

        public DateTime ServerRecent { get; set; }

        [Indexed]
        public DateTime CreationDate { get; set; }

        public string Comment { get; set; }

        public StreamType TrackType { get; set; }

        public string GoogleArtistId { get; set; }

        public string GoogleAlbumId { get; set; }

        [Reference]
        public UserPlaylistEntry UserPlaylistEntry { get; set; }

        public string AlbumArtistTitle { get; set; }

        [Indexed]
        public string AlbumArtistTitleNorm { get; set; }

        public string ArtistTitle { get; set; }

        [Indexed]
        public string ArtistTitleNorm { get; set; }

        public string AlbumTitle { get; set; }

        [Indexed]
        public string AlbumTitleNorm { get; set; }

        public string GenreTitle { get; set; }

        [Indexed]
        public string GenreTitleNorm { get; set; }

        [Indexed]
        public bool IsCached { get; set; }

        [Indexed]
        public bool IsLibrary { get; set; }

        [Indexed]
        public string StoreId { get; set; }

        public DateTime LastModified { get; set; }

        public int BeatsPerMinute { get; set; }

        public string EstimatedSize { get; set; }

        public int ContentType { get; set; }

        public bool TrackAvailableForSubscription { get; set; }

        public bool TrackAvailableForPurchase { get; set; }

        public bool AlbumAvailableForPurchase { get; set; }

        public string Nid { get; set; }

        // Statistics

        [Indexed]
        public uint StatsPlayCount { get; set; }

        public DateTime StatsRecent { get; set; }

        [Indexed]
        public DateTime Recent { get; set; }

        public uint ServerPlayCount { get; set; }

        [Indexed]
        public DateTime LastRatingChange { get; set; }

        [Ignore]
        public bool UnknownSong { get; set; }
    }

    public class SongByIdComparer : IEqualityComparer<Song>
    {
        public bool Equals(Song x, Song y)
        {
            return x.SongId.Equals(y.SongId);
        }

        public int GetHashCode(Song obj)
        {
            return obj.SongId.GetHashCode();
        }
    }
}