﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Playlist : BindingModelBase
    {
        private string title;

        protected Playlist(string name, List<Song> songs)
        {
            this.Title = name;
            this.Songs = songs;
            this.CalculateFields();
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public double Duration { get; private set; }

        public string AlbumArtUrl { get; protected set; }

        public List<Song> Songs { get; private set; }

        public void CalculateFields()
        {
            this.Duration = TimeSpan.FromSeconds(this.Songs.Sum(x => x.Duration)).TotalSeconds;

            var song = this.Songs.FirstOrDefault(x => !string.IsNullOrEmpty(x.GoogleMusicMetadata.AlbumArtUrl));
            if (song != null)
            {
                this.AlbumArtUrl = song.GoogleMusicMetadata.AlbumArtUrl;
            }
        }
    }
}