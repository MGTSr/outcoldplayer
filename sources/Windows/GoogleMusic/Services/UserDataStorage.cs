﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Net;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Security.Credentials;

    public class UserDataStorage : IUserDataStorage
    {
        private const string GoogleAccountsResource = "OutcoldSolutions.GoogleMusic";

        private readonly ILogger logger;

        private UserSession userSession;

        public UserDataStorage(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("UserDataStorage");
        }

        public void SaveUserInfo(UserInfo userInfo)
        {
            var passwordCredential = new PasswordCredential(GoogleAccountsResource, userInfo.Email, userInfo.Password);
            PasswordVault vault = new PasswordVault();

            // Remove old
            try
            {
                var all = vault.FindAllByResource(GoogleAccountsResource);
                foreach (var credential in all)
                {
                    vault.Remove(credential);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogException(exception);
            }
            
            vault.Add(passwordCredential);
        }

        public UserInfo GetUserInfo()
        {
            PasswordVault vault = new PasswordVault();

            try
            {
                var list = vault.FindAllByResource(GoogleAccountsResource);
                if (list.Count > 0)
                {
                    list[0].RetrievePassword();
                    return new UserInfo(list[0].UserName, list[0].Password);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogException(exception);
            }

            return null;
        }

        public void SetUserSession(UserSession session)
        {
            this.userSession = session;
        }

        public UserSession GetUserSession()
        {
            return this.userSession;
        }
    }
}