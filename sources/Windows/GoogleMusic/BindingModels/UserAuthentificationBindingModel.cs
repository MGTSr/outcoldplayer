﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    public class UserAuthentificationBindingModel : BindingModelBase
    {
        private string email;
        private string password;
        private bool rememberAccount;
        private string errorMessage;

        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {
                if (!string.Equals(this.email, value, StringComparison.CurrentCulture))
                {
                    this.email = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                if (!string.Equals(this.password, value, StringComparison.CurrentCulture))
                {
                    this.password = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }

        public bool RememberAccount
        {
            get
            {
                return this.rememberAccount;
            }

            set
            {
                if (this.rememberAccount != value)
                {
                    this.rememberAccount = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                if (!string.Equals(this.errorMessage, value, StringComparison.CurrentCulture))
                {
                    this.errorMessage = value;
                    this.RaiseCurrenntPropertyChanged();
                }
            }
        }
    }
}