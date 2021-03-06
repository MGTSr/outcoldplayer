﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Presenters.Popups;

    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    public sealed partial class LastfmAuthentificationPageView : DisposablePopupViewBase, ILastfmAuthentificationView
    {
        private LastfmAuthentificationPresenter presenter;

        public LastfmAuthentificationPageView()
        {
            this.InitializeComponent();

            Window.Current.Activated += this.CurrentOnActivated;
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();
            Window.Current.Activated -= this.CurrentOnActivated;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<LastfmAuthentificationPresenter>();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CurrentOnActivated(object sender, WindowActivatedEventArgs windowActivatedEventArgs)
        {
            if (windowActivatedEventArgs.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                Window.Current.Activated -= this.CurrentOnActivated;
                this.presenter.GetSession();
            }
        }

        private void NavigateToLastfm(object sender, RoutedEventArgs e)
        {
            Window.Current.Activated -= this.CurrentOnActivated;
            Window.Current.Activated += this.CurrentOnActivated;
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri(this.presenter.BindingModel.LinkUrl)).AsTask());
        }
    }
}
