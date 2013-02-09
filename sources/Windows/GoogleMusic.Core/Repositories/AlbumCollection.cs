﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public class AlbumCollection : PlaylistCollectionBase<Album>
    {
        public AlbumCollection(ISongsRepository songsRepository)
            : base(songsRepository)
        {
        }

        protected override Task<List<Album>> LoadCollectionAsync()
        {
            return Task.FromResult(this.SongsRepository.GetAll()
                .GroupBy(
                    x =>
                    new
                        {
                            x.GoogleMusicMetadata.AlbumNorm,
                            ArtistNorm = string.IsNullOrWhiteSpace(x.GoogleMusicMetadata.AlbumArtistNorm)
                                            ? x.GoogleMusicMetadata.ArtistNorm
                                            : x.GoogleMusicMetadata.AlbumArtistNorm
                        })
                .Select(
                    x =>
                    new Album(
                        x.OrderBy(s => Math.Max(s.GoogleMusicMetadata.Disc, 1))
                         .ThenBy(s => s.GoogleMusicMetadata.Track)
                         .ToList()))
                .ToList());
        }
    }
}