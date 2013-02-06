﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class MusicPlaylistCollection : PlaylistCollectionBase<MusicPlaylist>
    {
        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        public MusicPlaylistCollection(
            IMusicPlaylistRepository musicPlaylistRepository,
            ISongsRepository songsRepository)
            : base(songsRepository, useCache: false)
        {
            this.musicPlaylistRepository = musicPlaylistRepository;
        }

        protected async override Task<List<MusicPlaylist>> LoadCollectionAsync()
        {
            return (await this.musicPlaylistRepository.GetAllAsync()).ToList();
        }
    }
}