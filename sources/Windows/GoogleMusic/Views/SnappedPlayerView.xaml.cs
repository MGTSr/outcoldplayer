﻿//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Microsoft.Advertising.WinRT.UI;

    using Windows.ApplicationModel.Store;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class SnappedPlayerView : UserControl
    {
        private AdControl adControl;

        public SnappedPlayerView()
        {
            this.InitializeComponent();

            CurrentApp.LicenseInformation.LicenseChanged += this.UpdateAdControl;
            this.UpdateAdControl();
        }

        private bool IsAdFree()
        {
            return (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("AdFree")
                && CurrentApp.LicenseInformation.ProductLicenses["AddFree"].IsActive)
                || (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentApp.LicenseInformation.ProductLicenses["Ultimate"].IsActive);
        }

        private void UpdateAdControl()
        {
            if (this.IsAdFree())
            {
                if (this.adControl != null)
                {
                    this.SnappedGrid.Children.Remove(this.adControl);
                    this.adControl = null;
                }
            }
            else
            {
                if (this.adControl == null)
                {
                    this.adControl = new AdControl
                    {
                        ApplicationId = "8eb9e14b-2133-40db-9500-14eff7c05aab",
                        AdUnitId = "111664",
                        Width = 292,
                        Height = 60,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetRow(this.adControl, 6);
                    this.SnappedGrid.Children.Add(this.adControl);
                }
            }
        }

        private void AddToQueue(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.TryUnsnap())
            {
                App.Container.Resolve<INavigationService>().NavigateTo<IStartView>();
            }
        }
    }
}