﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    public class AccountViewBindingModel : BindingModelBase
    {
        private string accountName;
        private string lastfmAccountName;
        private string message;
        private bool isRemembered;
        private bool hasSession;

        public string AccountName
        {
            get
            {
                return this.accountName;
            }

            set
            {
                this.accountName = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool HasSession
        {
            get
            {
                return this.hasSession;
            }

            set
            {
                this.hasSession = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public bool IsRemembered
        {
            get
            {
                return this.isRemembered;
            }

            set
            {
                this.isRemembered = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public string LastfmAccountName
        {
            get
            {
                return this.lastfmAccountName;
            }

            set
            {
                this.lastfmAccountName = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}