﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using OutcoldSolutions.GoogleMusic.Services;

    public class PlaylistDataTemplateSelector : DataTemplateSelector 
    {
        public DataTemplate ArtistDataTemplate { get; set; }

        public DataTemplate SmallArtistDataTemplate { get; set; }

        public DataTemplate AlbumDataTemplate { get; set; }

        public DataTemplate GenreDataTemplate { get; set; }

        public DataTemplate SystemPlaylistDataTemplate { get; set; }

        public DataTemplate UserPlaylistDataTemplate { get; set; }

        public DataTemplate RadioDataTemplate { get; set; }

        public DataTemplate MixedRadioDataTemplate { get; set; }

        public DataTemplate ArtistRadioDataTemplate { get; set; }

        public DataTemplate SituationDataTemplate { get; set; }

        public DataTemplate SituationStationsDataTemplate { get; set; }

        public DataTemplate SituationRadioDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var groupPlaylistBindingModel = item as PlaylistBindingModel;
            object playlist = null;
            if (groupPlaylistBindingModel != null)
            {
                playlist = groupPlaylistBindingModel.Playlist;
            }

            if (playlist is Artist)
            {
                Debug.Assert(this.ArtistDataTemplate != null, "this.ArtistDataTemplate != null");
                return this.ArtistDataTemplate;
            }

            if (playlist is Album)
            {
                Debug.Assert(this.AlbumDataTemplate != null, "this.AlbumDataTemplate != null");
                return this.AlbumDataTemplate;
            }

            if (playlist is Genre || playlist is AllAccessGenre)
            {
                Debug.Assert(this.GenreDataTemplate != null, "this.GenreDataTemplate != null");
                return this.GenreDataTemplate;
            }

            if (playlist is SystemPlaylist)
            {
                Debug.Assert(this.SystemPlaylistDataTemplate != null, "this.SystemPlaylistDataTemplate != null");
                return this.SystemPlaylistDataTemplate;
            }

            if (playlist is UserPlaylist)
            {
                Debug.Assert(this.UserPlaylistDataTemplate != null, "this.UserPlaylistDataTemplate != null");
                return this.UserPlaylistDataTemplate;
            }

            if (playlist is SituationGroup)
            {
                Debug.Assert(this.SituationDataTemplate != null, "this.SituationDataTemplate != null");
                return this.SituationDataTemplate;
            }

            if (playlist is SituationStations)
            {
                Debug.Assert(this.SituationStationsDataTemplate != null, "this.SituationStationsDataTemplate != null");
                return this.SituationStationsDataTemplate;
            }

            if (playlist is SituationRadio)
            {
                Debug.Assert(this.SituationRadioDataTemplate != null, "this.SituationRadioDataTemplate != null");
                return this.SituationRadioDataTemplate;
            }

            if (playlist is Radio)
            {
                if (((Radio)playlist).ArtUrls.Length > 1)
                {
                    return this.MixedRadioDataTemplate;
                }

                if (!string.IsNullOrEmpty(((Radio)playlist).GoogleArtistId))
                {
                    return this.ArtistRadioDataTemplate;
                }

                Debug.Assert(this.RadioDataTemplate != null, "this.UserPlaylistDataTemplate != null");
                return this.RadioDataTemplate;
            }

            Debug.Assert(false, "Uknown playlist type.");
            return base.SelectTemplateCore(item, container);
        }
    }
}
