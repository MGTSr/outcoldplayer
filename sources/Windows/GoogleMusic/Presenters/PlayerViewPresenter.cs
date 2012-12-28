﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.Media;
    using Windows.System.Display;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class PlayerViewPresenter : ViewPresenterBase<IMediaElemenetContainerView>, ICurrentPlaylistService, IDisposable
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly ISongWebService songWebService;

        private readonly MediaElement mediaElement;

        private readonly List<string> playOrder = new List<string>();
        private int playIndex = 0;

        private DisplayRequest request;

        public PlayerViewPresenter(
            IDependencyResolverContainer container,
            IMediaElemenetContainerView view,
            ISongWebService songWebService)
            : base(container, view)
        {
            this.songWebService = songWebService;
            this.BindingModel = new PlayerBindingModel();

            MediaControl.PlayPauseTogglePressed += this.MediaControlPlayPauseTogglePressed;
            MediaControl.PlayPressed += this.MediaControlPlayPressed;
            MediaControl.PausePressed += this.MediaControlPausePressed;
            MediaControl.StopPressed += this.MediaControlStopPressed;

            this.BindingModel.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => !this.BindingModel.IsBusy && this.playIndex > 0);
            this.BindingModel.PlayCommand = new DelegateCommand(this.Play, () => !this.BindingModel.IsBusy && !this.BindingModel.IsPlaying && this.BindingModel.Songs.Count > 0);
            this.BindingModel.PauseCommand = new DelegateCommand(this.Pause, () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.BindingModel.SkipAheadCommand = new DelegateCommand(
                this.NextSong,
                () =>
                !this.BindingModel.IsBusy
                && ((this.playIndex < (this.playOrder.Count - 1))
                    || (this.BindingModel.IsRepeatAllEnabled && this.BindingModel.Songs.Count > 0)));

            this.BindingModel.LockScreenCommand = new DelegateCommand(() =>
                {
                    if (this.request == null)
                    {
                        this.request = new DisplayRequest();
                        this.request.RequestActive();
                        this.Logger.Debug("Request display active.");
                    }
                    else
                    {
                        this.request.RequestRelease();
                        this.request = null;
                        this.Logger.Debug("Release display active.");
                    }

                    this.BindingModel.IsLockScreenEnabled = this.request != null; 
                });

            this.BindingModel.RepeatAllCommand = new DelegateCommand(() => this.BindingModel.IsRepeatAllEnabled = !this.BindingModel.IsRepeatAllEnabled);
            this.BindingModel.ShuffleCommand = new DelegateCommand(() =>
                { 
                    this.BindingModel.IsShuffleEnabled = !this.BindingModel.IsShuffleEnabled;
                    this.UpdateOrder();
                });

            this.BindingModel.UpdateBindingModel();

            this.timer.Interval = TimeSpan.FromSeconds(1);
            this.timer.Start();

            this.BindingModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName.Equals("CurrentPosition"))
                    {
                        if (Math.Abs(this.mediaElement.Position.TotalSeconds - this.BindingModel.CurrentPosition) > 2)
                        {
                            this.mediaElement.Position = TimeSpan.FromSeconds(this.BindingModel.CurrentPosition);
                        }
                    }
                };

            this.mediaElement = this.View.GetMediaElement();

            this.mediaElement.MediaOpened += (sender, args) =>
            {
                this.Logger.Info("Media opened. Duration: {0}.", this.mediaElement.NaturalDuration.TimeSpan);

                this.BindingModel.CurrentPosition = 0;
                this.BindingModel.TotalSeconds = this.mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            };

            this.mediaElement.DownloadProgressChanged += (sender, args) =>
            {
                this.Logger.Info("Download progress changed to {0}", this.mediaElement.DownloadProgress);
                this.BindingModel.DownloadProgress = this.mediaElement.DownloadProgress;
            };

            this.mediaElement.MediaEnded += (sender, args) =>
            {
                this.Logger.Info("Media Ended");
                this.OnMediaEnded();
            };

            this.mediaElement.MediaFailed += (sender, args) =>
            {
                this.Logger.Error("Media Failed: {0}", args.ErrorMessage);
                this.Logger.Debug("Media Failed - trying to handle this like MediaEnded");
                this.OnMediaEnded();
            };

            this.timer.Tick += (sender, o) =>
            {
                if (this.BindingModel.IsPlaying)
                {
                    this.Logger.Info("Update progress bar to {0}.", this.mediaElement.Position.TotalSeconds);
                    this.BindingModel.CurrentPosition = this.mediaElement.Position.TotalSeconds;
                }
            };
        }

        ~PlayerViewPresenter()
        {
            this.Dispose(disposing: false);
        }

        public event EventHandler PlaylistChanged;

        public PlayerBindingModel BindingModel { get; private set; }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void ClearPlaylist()
        {
            this.Logger.Debug("ClearPlaylist.");
            this.playIndex = -1;
            this.BindingModel.CurrentSongId = null;
            this.BindingModel.Songs.Clear();
            this.UpdateOrder();
            this.BindingModel.UpdateBindingModel();

            this.RaisePlaylistChanged();
        }

        public void AddSongs(IEnumerable<GoogleMusicSong> songs)
        {
            this.Logger.Debug("AddSongs.");

            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            foreach (var song in songs)
            {
                this.BindingModel.Songs.Add(new SongBindingModel(song));
            }

            this.UpdateOrder();
            this.Dispatcher.RunAsync(() => this.View.Activate());

            this.RaisePlaylistChanged();
        }
 
        public IEnumerable<GoogleMusicSong> GetPlaylist()
        {
            return this.BindingModel.Songs.Select(x => x.GetSong());
        }

        public void Play(GoogleMusicSong song)
        {
            var songBindingModel = this.BindingModel.Songs.FirstOrDefault(x => x.GetSong() == song);
            if (songBindingModel == null)
            {
                this.Logger.Error("Cannot find song '{0}'", song.Id);
            }
            else
            {
                this.Dispatcher.RunAsync(
                    () =>
                        {
                            this.playIndex = this.playOrder.IndexOf(songBindingModel.GetSong().Id);
                            
                            this.PlayCurrentSong();
                        });

                this.View.Activate();
            }
        }

        public void Remove(GoogleMusicSong song)
        {
            if (song != null)
            {
                var songBindingModel = this.BindingModel.Songs.FirstOrDefault(x => x.GetSong() == song);
                if (songBindingModel == null)
                {
                    this.Logger.Error("Cannot find song '{0}'", song.Id);
                }
                else
                {
                    this.Dispatcher.RunAsync(
                        () =>
                            {
                                var index = this.playOrder.IndexOf(songBindingModel.GetSong().Id);

                                this.playOrder.RemoveAt(index);
                                if (index == this.playIndex)
                                {
                                    this.Stop();
                                    this.PlayCurrentSong();
                                }
                                else 
                                {
                                    if (index < this.playIndex)
                                    {
                                        this.playIndex--;
                                    }
                                }

                                this.BindingModel.Songs.Remove(songBindingModel);
                                if (this.BindingModel.Songs.Count == 0)
                                {
                                    this.ClearPlaylist();
                                }
                                else
                                {
                                    this.RaisePlaylistChanged();
                                    this.BindingModel.UpdateBindingModel();
                                }
                            });
                }
            }
        }

        private void NextSong()
        {
            this.Logger.Debug("NextSong.");

            if (this.playIndex == (this.playOrder.Count - 1)
                && this.BindingModel.IsRepeatAllEnabled)
            {
                this.playIndex = 0;
            }
            else
            {
                this.playIndex++;
            }

            this.PlayCurrentSong();
            this.BindingModel.UpdateBindingModel();
        }

        private void PreviousSong()
        {
            this.Logger.Debug("PreviousSong.");

            if (this.playIndex != 0)
            {
                this.playIndex--;
            }

            this.PlayCurrentSong();

            this.BindingModel.UpdateBindingModel();
        }

        private void Play()
        {
            this.Logger.Debug("Play.");

            if (this.BindingModel.State == PlayState.Stop)
            {
                this.PlayCurrentSong();
            }
            else
            {
                this.mediaElement.Play();
                this.BindingModel.State = PlayState.Play;
            }

            this.BindingModel.UpdateBindingModel();
        }

        private void Pause()
        {
            this.Logger.Debug("Pause.");

            this.mediaElement.Pause();
            this.BindingModel.State = PlayState.Pause;

            this.BindingModel.UpdateBindingModel();
        }

        private void Stop()
        {
            this.Logger.Debug("Stop.");

            this.mediaElement.Stop();
            this.BindingModel.State = PlayState.Stop;

            this.BindingModel.UpdateBindingModel();
        }

        private void PlayCurrentSong()
        {
            this.Logger.Debug("PlayCurrentSong.");

            this.BindingModel.UpdateBindingModel();

            if (this.playOrder.Count > this.playIndex)
            {
                this.BindingModel.CurrentSongId = this.playOrder[this.playIndex];
                var songBindingModel = this.BindingModel.CurrentSong;
                if (songBindingModel != null)
                {
                    this.Logger.Debug("Found current song.");
                    var song = songBindingModel.GetSong();

                    this.Logger.Debug("Getting url for song '{0}'.", song.Id);
                    this.BindingModel.IsBusy = true;
                    this.songWebService.GetSongUrlAsync(song.Id).ContinueWith(
                        (t) =>
                            {
                                if (t.Result != null)
                                {
                                    this.Logger.Debug("Found url for song '{0}'. Url is '{1}'.", song.Id, t.Result.Url);

                                    MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
                                    MediaControl.NextTrackPressed -= this.MediaControlOnPreviousTrackPressed;

                                    MediaControl.ArtistName = song.Artist;
                                    MediaControl.TrackName = song.Title;

                                    /*if (song.AlbumArtUrl != null)
                                    {
                                        MediaControl.AlbumArt = new Uri("https:" + song.AlbumArtUrl);
                                    }*/

                                    if (this.mediaElement.Source != null)
                                    {
                                        this.Logger.Info("Media Element contains source. Stop it first.");
                                        this.mediaElement.Stop();
                                    }

                                    this.Logger.Info("Set new source for media element '{0}'.", t.Result.Url);
                                    this.mediaElement.Source = new Uri(t.Result.Url);
                                    this.mediaElement.Play();

                                    if (this.BindingModel.SkipAheadCommand.CanExecute())
                                    {
                                        MediaControl.NextTrackPressed += this.MediaControlOnNextTrackPressed;
                                    }

                                    if (this.BindingModel.SkipBackCommand.CanExecute())
                                    {
                                        MediaControl.PreviousTrackPressed += this.MediaControlOnPreviousTrackPressed;
                                    }

                                    this.BindingModel.State = PlayState.Play;
                                    this.BindingModel.UpdateBindingModel();
                                }
                                else
                                {
                                    this.Logger.Debug(
                                        "Could not find url for song {0}. Trying to switch to next song.", song.Id);
                                    this.NextSong();
                                }

                                this.BindingModel.IsBusy = false;
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                MediaControl.PlayPauseTogglePressed -= this.MediaControlPlayPauseTogglePressed;
                MediaControl.PlayPressed -= this.MediaControlPlayPressed;
                MediaControl.PausePressed -= this.MediaControlPausePressed;
                MediaControl.StopPressed -= this.MediaControlStopPressed;
            }
        }

        private void MediaControlPausePressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPausePressed.");
            this.Pause();
        }

        private void MediaControlPlayPressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPlayPressed.");
            this.Play();
        }

        private void MediaControlStopPressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlStopPressed.");
            this.Stop();
        }

        private void MediaControlPlayPauseTogglePressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPlayPauseTogglePressed.");
            if (this.BindingModel.State == PlayState.Play)
            {
                this.Pause();
            }
            else
            {
                this.Play();
            }
        }

        private void MediaControlOnNextTrackPressed(object sender, object o)
        {
            this.Logger.Debug("MediaControlOnNextTrackPressed.");
            this.NextSong();
        }

        private void MediaControlOnPreviousTrackPressed(object sender, object o)
        {
            this.Logger.Debug("MediaControlOnPreviousTrackPressed.");
            this.PreviousSong();
        }

        private void UpdateOrder()
        {
            this.Logger.Debug("UpdateOrder.");
            this.playOrder.Clear();

            if (this.BindingModel.Songs.Count > 0)
            {
                var range = this.BindingModel.Songs.Select(x => x.GetSong().Id);

                if (this.BindingModel.IsShuffleEnabled)
                {
                    var random = new Random((int)DateTime.Now.Ticks);
                    this.playOrder.AddRange(
                        range.ToDictionary(
                            x =>
                            {
                                if (this.BindingModel.CurrentSong != null 
                                    && x == this.BindingModel.CurrentSong.GetSong().Id)
                                {
                                    return -1;
                                }
                                else
                                {
                                    return random.Next();
                                }
                            }, 
                            x => x)
                            .OrderBy(x => x.Key).Select(x => x.Value));
                }
                else
                {
                    this.playOrder.AddRange(range);
                }

                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.Info("Shuffle enabled: {0}", this.BindingModel.IsShuffleEnabled);
                    this.Logger.Info("Playing order: {0}", string.Join(",", this.playOrder));
                }

                if (this.BindingModel.CurrentSong != null)
                {
                    this.playIndex = this.playOrder.IndexOf(this.BindingModel.CurrentSong.GetSong().Id);
                }
                else
                {
                    this.playIndex = -1;
                }
            }
        }

        private void OnMediaEnded()
        {
            this.Logger.Debug("OnMediaEnded.");

            this.BindingModel.State = PlayState.Stop;
            if (this.BindingModel.SkipAheadCommand.CanExecute())
            {
                this.NextSong();
            }
        }

        private void RaisePlaylistChanged()
        {
            var handler = this.PlaylistChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}