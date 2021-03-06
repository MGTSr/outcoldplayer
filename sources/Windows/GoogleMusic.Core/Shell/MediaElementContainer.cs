﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.Storage.Streams;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class MediaElementContainer : IMediaElementContainer
    {
        private const string MediaElementVolumeKey = "MediaElementVolume";

        private readonly MediaElement mediaElement;
        private readonly ISettingsService settingsService;
        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;

        private readonly DispatcherTimer timer = new DispatcherTimer();

        public MediaElementContainer(
            ILogManager logManager, 
            MediaElement mediaElement,
            ISettingsService settingsService,
            IDispatcher dispatcher)
        {
            this.logger = logManager.CreateLogger("MediaElementContainer");
            this.mediaElement = mediaElement;
            this.settingsService = settingsService;
            this.dispatcher = dispatcher;
            this.mediaElement.MediaFailed += this.MediaElementOnMediaFailed;
            this.mediaElement.MediaEnded += this.MediaElementOnMediaEnded;
            this.mediaElement.MediaOpened += this.MediaElementOnMediaOpened;

            this.mediaElement.Volume = this.settingsService.GetValue(MediaElementVolumeKey, 1d);

            this.timer.Interval = TimeSpan.FromMilliseconds(200);
            this.timer.Tick += this.TimerOnTick;
        }

        public event EventHandler<MediaEndedEventArgs> MediaEnded;

        public event EventHandler<PlayProgressEventArgs> PlayProgress;

        public double Volume
        {
            get
            {
                return this.mediaElement.Volume;
            }

            set
            {
                this.mediaElement.Volume = value;
                this.settingsService.SetValue(MediaElementVolumeKey, value);
            }
        }

        public async Task<bool> IsBeginning()
        {
            bool result = false;
            await this.dispatcher.RunAsync(() =>
            {
                TimeSpan position = this.mediaElement.Position;
                TimeSpan duration = this.mediaElement.NaturalDuration.HasTimeSpan
                    ? this.mediaElement.NaturalDuration.TimeSpan
                    : position;
                result = position < duration && position.TotalSeconds < 3;
            });

            return result;
        }

        public Task PlayAsync(IRandomAccessStream stream, string mimeType)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Play new media {0}.", mimeType);
            }

            return this.dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        this.mediaElement.Position = TimeSpan.FromMilliseconds(0);
                        this.mediaElement.SetSource(stream, mimeType);
                        this.mediaElement.Play();
                    });
        }

        public Task PlayAsync()
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Play current media.");
            }

            return this.dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        this.timer.Start();
                        this.mediaElement.Play();
                    });
        }


        public Task PauseAsync()
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Pausing current media.");
            }

            return this.dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        this.timer.Stop();
                        this.mediaElement.Pause();
                    });
        }

        public Task StopAsync()
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Stopping current media.");
            }

            return this.dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        this.timer.Stop();
                        this.mediaElement.Stop();
                    });
        }

        public Task SetPositionAsync(TimeSpan position)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Changing position of current song to {0}.", position);
            }

            return this.dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                () =>
                    {
                        this.mediaElement.Position = position;
                    });
        }
        
        private void TimerOnTick(object sender, object o)
        {
            this.RaisePlayProgress(new PlayProgressEventArgs(this.mediaElement.Position, this.mediaElement.NaturalDuration.TimeSpan));
        }

        private void MediaElementOnMediaOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("MediaElement opened, duration is {0}.", this.mediaElement.NaturalDuration.TimeSpan);
            }

            this.timer.Start();
            this.RaisePlayProgress(new PlayProgressEventArgs(new TimeSpan(0), this.mediaElement.NaturalDuration.TimeSpan));
        }

        private void MediaElementOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("MediaElement ended.");
            }

            this.timer.Stop();
            this.RaiseMediaEnded(new MediaEndedEventArgs(MediaEndedReason.Ended));
        }

        private void MediaElementOnMediaFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            this.logger.Error("MediaElement failed: {0}.", exceptionRoutedEventArgs.ErrorMessage);
            this.timer.Stop();
            this.RaiseMediaEnded(new MediaEndedEventArgs(MediaEndedReason.Failed));
        }

        private void RaisePlayProgress(PlayProgressEventArgs e)
        {
            var handler = this.PlayProgress;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RaiseMediaEnded(MediaEndedEventArgs e)
        {
            var handler = this.MediaEnded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}