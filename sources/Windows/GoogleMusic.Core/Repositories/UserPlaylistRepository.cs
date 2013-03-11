﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public class UserPlaylistRepository : RepositoryBase, IUserPlaylistRepository
    {
        private readonly ILogger logger;

        private readonly IPlaylistsWebService playlistsWebService;


        public UserPlaylistRepository(
            ILogManager logManager,
            IPlaylistsWebService playlistsWebService)
        {
            this.logger = logManager.CreateLogger("UserPlaylistRepository");
            this.playlistsWebService = playlistsWebService;
        }

        public async Task<IEnumerable<UserPlaylist>> GetAllAsync()
        {
            List<UserPlaylist> userPlaylists = new List<UserPlaylist>();

            var playlists = await this.Connection.Table<UserPlaylistEntity>().ToListAsync();

            foreach (var playlist in playlists)
            {
                List<string> entrieIds = new List<string>();
                List<Song> songs = new List<Song>();

                int playlistId = playlist.PlaylistId;
                var entries = await this.Connection.Table<UserPlaylistEntryEntity>()
                                    .Where(x => x.PlaylistId == playlistId)
                                    .OrderBy(x => x.PlaylistOrder).ToListAsync();

                foreach (var entry in entries)
                {
                    var song = await this.Connection.FindAsync<SongEntity>(entry.SongId);

                    if (song != null)
                    {
                        entrieIds.Add(entry.ProviderEntryId);
                        songs.Add(new Song(song));
                    }
                    else
                    {
                        this.logger.Warning("Could not find a song with id {0}.", entry.SongId);
                    }
                }

                userPlaylists.Add(new UserPlaylist(playlist, playlist.Title, songs, entrieIds));
            }

            return userPlaylists;
        }

        public async Task<UserPlaylist> CreateAsync(string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Creating playlist '{0}'.", name);
            }

            var resp = await this.playlistsWebService.CreateAsync(name);
            if (resp.Success.HasValue && resp.Success.Value)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Playlist was created on the server with id '{0}' for name '{1}'.", resp.Id, resp.Title);
                }

                var userPlaylistEntity = new UserPlaylistEntity() { ProviderPlaylistId = resp.Id, Title = resp.Title };
                await this.Connection.InsertAsync(userPlaylistEntity);
                return new UserPlaylist(userPlaylistEntity, resp.Title, new List<Song>(), new List<string>());
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Could not create playlist for name '{0}'.", resp.Title);
                }

                return null;
            }
        }

        public async Task<bool> DeleteAsync(UserPlaylistEntity playlist)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'.", playlist.ProviderPlaylistId);
            }

            var resp = await this.playlistsWebService.DeleteAsync(playlist.ProviderPlaylistId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'. Response '{1}'.", playlist.ProviderPlaylistId, resp);
            }

            if (resp)
            {
                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                        {
                            connection.Execute("DELETE from UserPlaylistEntry where ProviderPlaylistId = ?", playlist.PlaylistId);
                            connection.Delete<UserPlaylistEntity>(playlist.PlaylistId);
                        });
            }

            return resp;
        }

        public async Task<bool> ChangeName(UserPlaylistEntity playlist, string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Changing name for playlist with Id '{0}' to '{1}'.", playlist.ProviderPlaylistId, name);
            }

            bool result = await this.playlistsWebService.ChangeNameAsync(playlist.ProviderPlaylistId, name);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("The result of name changing for playlist with id '{0}' is '{1}'.", playlist.ProviderPlaylistId, result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                    {
                        connection.Execute("UPDATE UserPlaylist SET Title = ? WHERE ProviderPlaylistId = ?", name, playlist.PlaylistId);
                    });
            }

            return result;
        }

        public async Task<bool> RemoveEntry(UserPlaylistEntity playlist, string songId, string entryId)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entry Id '{0}' from playlist '{1}'.", entryId, playlist.ProviderPlaylistId);
            }

            var result = await this.playlistsWebService.RemoveSongAsync(playlist.ProviderPlaylistId, songId, entryId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Result of entry removing '{0}' from playlist '{1}' is '{2}'.", entryId, playlist.ProviderPlaylistId, result);
            }

            if (result)
            {
                await this.Connection.RunInTransactionAsync(connection =>
                    {
                        var entry = connection.Find<UserPlaylistEntryEntity>(e => e.PlaylistId == playlist.PlaylistId && e.ProviderEntryId == entryId);
                        connection.Execute(
                            "UPDATE UserPlaylistEntry SET PlaylistOrder = (PlaylistOrder - 1) WHERE PlaylistId = ? AND PlaylistOrder > ?",
                            playlist.PlaylistId,
                            entry.PlaylistOrder);
                        connection.Delete(entry);
                    });
            }

            return result;
        }

        public async Task<bool> AddEntriesAsync(UserPlaylistEntity playlist, List<Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Adding song Ids '{0}' to playlist '{1}'.", string.Join(",", songs.Select(x => x.Metadata.ProviderSongId.ToString())), playlist.ProviderPlaylistId);
            }

            var result = await this.playlistsWebService.AddSongAsync(playlist.ProviderPlaylistId, songs.Select(s => s.Metadata.ProviderSongId));
            if (result != null && result.SongIds.Length == 1)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Successfully added entries '{0}' to playlist {1}.",
                        string.Join(",", songs.Select(x => x.Metadata.ProviderSongId.ToString())),
                        playlist.ProviderPlaylistId);
                }

                await this.Connection.RunInTransactionAsync(
                    (connection) =>
                        {
                            var lastEntry = connection.Table<UserPlaylistEntryEntity>()
                                          .Where(e => e.PlaylistId == playlist.PlaylistId)
                                          .OrderByDescending(x => x.PlaylistOrder)
                                          .FirstOrDefault();

                            int nextIndex = lastEntry == null ? 0 : (lastEntry.PlaylistOrder + 1);

                            var entries = result.SongIds
                                            .Select((x, index) => Tuple.Create(x.PlaylistEntryId, songs.FirstOrDefault(s => string.Equals(s.Metadata.ProviderSongId, x.SongId)), index))
                                            .Where(x => x.Item2 != null)
                                            .Select(x => new UserPlaylistEntryEntity()
                                               {
                                                   ProviderEntryId = x.Item1,
                                                   SongId = x.Item2.Metadata.SongId,
                                                   PlaylistId = playlist.PlaylistId,
                                                   PlaylistOrder = nextIndex + x.Item3
                                               });

                            connection.InsertAll(entries);
                        });

                return true;
            }

            if (this.logger.IsWarningEnabled)
            {
                this.logger.Warning(
                    "Result of adding entries '{0}' to playlist {1} was unsuccesefull.", string.Join(",", songs.Select(x => x.Metadata.ProviderSongId.ToString())), playlist.ProviderPlaylistId);
            }

            return false;
        }
    }
}