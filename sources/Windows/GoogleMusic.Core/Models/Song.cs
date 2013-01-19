﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class Song : BindingModelBase
    {
        private GoogleMusicSong googleMusicSong;

        private int rating;
        private int playCount;
        private bool isPlaying;

        public Song(GoogleMusicSong googleMusicSong)
        {
            this.googleMusicSong = googleMusicSong;
            this.Title = googleMusicSong.Title;
            this.Duration = TimeSpan.FromMilliseconds(googleMusicSong.DurationMillis).TotalSeconds;
            this.Artist = googleMusicSong.Artist;
            this.Album = googleMusicSong.Album;
            this.PlayCount = googleMusicSong.PlayCount;
            this.Rating = googleMusicSong.Rating;
        }

        public string Title { get; set; }

        public double Duration { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public int PlayCount
        {
            get
            {
                return this.playCount;
            }

            set
            {
                this.playCount = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public int Rating
        {
            get
            {
                return this.rating;
            }

            set
            {
                if (this.rating != value)
                {
                    this.rating = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public GoogleMusicSong GoogleMusicMetadata
        {
            get
            {
                return this.googleMusicSong;
            }

            set
            {
                this.googleMusicSong = value;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.isPlaying;
            }

            set
            {
                this.isPlaying = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}