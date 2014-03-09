﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    using Windows.ApplicationModel;
    using Windows.Networking.Connectivity;
    using Windows.System;
    using Windows.UI.Popups;

    public class StartPageViewPresenter : PagePresenterBase<IStartPageView, StartViewBindingModel>
    {
        private const int MaxItems = 16;

        private const int AskForReviewStarts = 10;
        private const string DoNotAskToReviewKey = "DoNotAskToReviewKey";
        private const string CountOfStartsBeforeReview = "CountOfStartsBeforeReview";

        private readonly IApplicationResources resources;

        private readonly ISettingsService settingsService;
        private readonly IAuthentificationService authentificationService;
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;
        private readonly IPlaylistsService playlistsService;
        private readonly IMainFrameRegionProvider mainFrameRegionProvider;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;

        private readonly ISelectedObjectsService selectedObjectsService;

        private bool initialized = false;

        public StartPageViewPresenter(
            IApplicationResources resources,
            ISettingsService settingsService,
            IAuthentificationService authentificationService,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistsService playlistsService,
            IMainFrameRegionProvider mainFrameRegionProvider,
            IGoogleMusicSessionService sessionService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService,
            ISelectedObjectsService selectedObjectsService)
        {
            this.resources = resources;
            this.settingsService = settingsService;
            this.authentificationService = authentificationService;
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.playlistsService = playlistsService;
            this.mainFrameRegionProvider = mainFrameRegionProvider;
            this.sessionService = sessionService;
            this.cachingService = cachingService;
            this.stateService = stateService;
            this.selectedObjectsService = selectedObjectsService;

            this.PlayCommand = new DelegateCommand(this.Play);

            this.sessionService.SessionCleared += async (sender, args) => 
                    {
                        await this.DeinitializeAsync();
                        this.ShowAuthentificationPopupView();
                    };
        }

        public DelegateCommand PlayCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            if (!this.initialized)
            {
                await this.InitializeOnFirstLaunchAsync();
            }
            else
            {
                await this.LoadGroupsAsync();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;

            this.EventAggregator.GetEvent<ReloadSongsEvent>()
                .Subscribe(async (e) =>
                {
                    await this.DeinitializeAsync();
                    this.ShowProgressLoadingPopupView();
                });

            this.EventAggregator.GetEvent<SelectionClearedEvent>()
               .Subscribe<SelectionClearedEvent>(async (e) => await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems()));
        }

        private async Task InitializeOnFirstLaunchAsync()
        {
            AuthentificationService.AuthentificationResult result = null;

            try
            {
                result = await this.authentificationService.CheckAuthentificationAsync();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Exception while tried to verifying authentification.");
            }

            if (result != null && result.Succeed)
            {
                var currentVersion = this.settingsService.GetValue<string>("Version", null);
                var dbContext = new DbContext();
                bool fCurrentVersion = string.Equals(currentVersion, Package.Current.Id.Version.ToVersionString(), StringComparison.OrdinalIgnoreCase);

                if (fCurrentVersion && await dbContext.CheckVersionAsync())
                {
                    await this.OnViewInitializedAsync();
                }
                else
                {
                    this.ShowProgressLoadingPopupView();
                }
            }
            else
            {
                this.ShowAuthentificationPopupView();
            }
        }

        private void ShowAuthentificationPopupView()
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.MainFrame.ShowPopup<IAuthentificationPopupView>(PopupRegion.Full).Closed += this.AuthentificationPopupView_Closed;
                    });
        }

        private void AuthentificationPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IAuthentificationPopupView)sender).Closed -= this.AuthentificationPopupView_Closed;
            this.ShowProgressLoadingPopupView();
        }

        private void ShowProgressLoadingPopupView()
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.MainFrame.ShowPopup<IProgressLoadingPopupView>(PopupRegion.Full).Closed += this.ProgressLoadingPopupView_Closed;
                    });
        }

        private async void ProgressLoadingPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IProgressLoadingPopupView)sender).Closed -= this.ProgressLoadingPopupView_Closed;

            var progressLoadingCloseEventArgs = eventArgs as ProgressLoadingCloseEventArgs;
            if (progressLoadingCloseEventArgs != null && progressLoadingCloseEventArgs.IsFailed)
            {
                await this.sessionService.ClearSession();
                return;
            }

            var currentVersion = this.settingsService.GetValue<string>("Version", null);
            bool fCurrentVersion = string.Equals(currentVersion, Package.Current.Id.Version.ToVersionString(), StringComparison.OrdinalIgnoreCase);
            bool fUpdate = !fCurrentVersion && currentVersion != null;
            
            await this.OnViewInitializedAsync();

            this.settingsService.SetValue("Version", Package.Current.Id.Version.ToVersionString());

            if (fUpdate)
            {
                this.MainFrame.ShowPopup<IReleasesHistoryPopupView>(PopupRegion.Full);
            }

            this.VerifyIfCanAskForReview();
        }

        private async Task DeinitializeAsync()
        {
            try
            {
                await this.cachingService.ClearCacheAsync();
            }
            catch (Exception e)
            {
                this.Logger.Debug("Could not clear cache", e);
            }

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, null);
                    this.BindingModel.ClearSelectedItems();
                    this.BindingModel.Groups = null;
                    this.navigationService.ClearHistory();
                    this.initialized = false;
                });
        }

        private async Task OnViewInitializedAsync()
        {
            if (!this.initialized)
            {
                if (this.stateService.CurrentState == ApplicationState.Online)
                {
                    var profile = NetworkInformation.GetInternetConnectionProfile();
                    if (profile != null)
                    {
                        var networkConnectivityLevel = profile.GetNetworkConnectivityLevel();
                        if (networkConnectivityLevel != NetworkConnectivityLevel.ConstrainedInternetAccess
                            && networkConnectivityLevel != NetworkConnectivityLevel.InternetAccess)
                        {
                            this.stateService.CurrentState = ApplicationState.Offline;
                        }
                    }
                }

                await this.Dispatcher.RunAsync(() => this.mainFrameRegionProvider.SetContent(MainFrameRegion.Links, ApplicationBase.Container.Resolve<LinksRegionView>()));

                this.cachingService.StartDownloadTask();

                this.initialized = true;
            }

            bool loadFailed = false;

            try
            {
                await this.LoadGroupsAsync();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot load groups");
                loadFailed = true;
            }

            if (loadFailed)
            {
                await this.DeinitializeAsync();
                this.ShowAuthentificationPopupView();
            }
        }

        private async Task LoadGroupsAsync()
        {
            PlaylistType[] types;

            if (this.stateService.IsOnline())
            {
                types = new[]
                        {
                            PlaylistType.SystemPlaylist, PlaylistType.UserPlaylist, PlaylistType.Radio,
                            PlaylistType.Artist, PlaylistType.Album, PlaylistType.Genre
                        };
            }
            else
            {
                types = new[]
                        {
                            PlaylistType.SystemPlaylist, PlaylistType.UserPlaylist, PlaylistType.Artist,
                            PlaylistType.Album, PlaylistType.Genre
                        };
            }

            var groups = await Task.WhenAll(
                types.Select(
                    (t) => Task.Run(
                        async () =>
                            {
                                var countTask = this.playlistsService.GetCountAsync(t);
                                var getAllTask = this.playlistsService.GetAllAsync(t, Order.LastPlayed, MaxItems);

                                await Task.WhenAll(countTask, getAllTask);

                                int count = await countTask;
                                IEnumerable<IPlaylist> playlists = await getAllTask;

                                return this.CreateGroup(this.resources.GetPluralTitle(t), count, playlists, t);
                            })));

            await this.Dispatcher.RunAsync(() => { this.BindingModel.Groups = groups.Where(g => g.Playlists.Count > 0).ToList(); });
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist),
                e.OldItems == null ? null : e.OldItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist));
        }

        private PlaylistsGroupBindingModel CreateGroup(string title, int playlistsCount, IEnumerable<IPlaylist> playlists, PlaylistType type)
        {
            List<PlaylistBindingModel> groupItems =
                playlists.Select(
                    playlist =>
                    new PlaylistBindingModel(playlist)
                        {
                            PlayCommand = this.PlayCommand
                        }).ToList();

            return new PlaylistsGroupBindingModel(
                title,
                playlistsCount,
                groupItems,
                type);
        }

        private void Play(object commandParameter)
        {
            IPlaylist playlist = commandParameter as IPlaylist;
            if (playlist != null)
            {
                this.MainFrame.IsBottomAppBarOpen = true;
                this.Logger.LogTask(this.playQueueService.PlayAsync(playlist));
                this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
            }
        }

        private void VerifyIfCanAskForReview()
        {
            bool dontAsk = this.settingsService.GetRoamingValue<bool>(DoNotAskToReviewKey);
            if (!dontAsk)
            {
                int startsCount = this.settingsService.GetRoamingValue<int>(CountOfStartsBeforeReview);
                if (startsCount >= AskForReviewStarts)
                {
                    try
                    {
                        this.Logger.LogTask(this.VerifyToReview());
                    }
                    catch (Exception e) 
                    {
                        this.Logger.Error(e, "VerifyToReview failed");
                    }
                }
                else
                {
                    this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, startsCount + 1);
                }
            }
        }

        private Task VerifyToReview()
        {
            var dialog = new MessageDialog(this.resources.GetString("MessageBox_ReviewMessage"));
            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("MessageBox_ReviewButtonRate"),
                    (cmd) =>
                    {
                        this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true);
                        this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=47286outcoldman.gMusic_z1q2m7teapq4y")).AsTask());
                    }));

            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("MessageBox_ReviewButtonNoThanks"),
                    (cmd) =>
                    this.settingsService.SetRoamingValue<bool>(DoNotAskToReviewKey, true)));

            dialog.Commands.Add(
                new UICommand(
                    this.resources.GetString("MessageBox_ReviewButtonRemind"),
                    (cmd) =>
                    this.settingsService.SetRoamingValue<int>(CountOfStartsBeforeReview, 0)));

            return dialog.ShowAsync().AsTask();
        }
    }
}