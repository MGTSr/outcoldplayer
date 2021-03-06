﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class GoogleAccountWebService : WebServiceBase, IGoogleAccountWebService
    {
        private const string ClientLoginPath = "accounts/ClientLogin";
        private const string IssueAuthTokenPath = "accounts/IssueAuthToken";
        private const string TokenAuthPath = "accounts/TokenAuth";

        private readonly ILogger logger;
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private ISettingsService settingsService;

        public GoogleAccountWebService(ILogManager logManager, ISettingsService settingsService)
        {
            this.logger = logManager.CreateLogger("GoogleAccountWebService");
            this.settingsService = settingsService;
        }

        protected override ILogger Logger
        {
            get { return this.logger; }
        }

        protected override HttpClient HttpClient
        {
            get { return this.httpClient; }
        }

        public async Task<GoogleAuthResponse> AuthenticateAsync(Uri serviceUri, string email, string password, string captchaToken, string captcha)
        {
            this.InitializeHttpClient();

            HttpResponseMessage responseMessage = null;

            try
            {
                // Check for details: https://developers.google.com/accounts/docs/AuthForInstalledApps
                var requestContent = new Dictionary<string, string>
                {
                    {"accountType", "HOSTED_OR_GOOGLE"},
                    {"Email", email},
                    {"Passwd", password},
                    {"service", "sj"}
                };

                if (!string.IsNullOrEmpty(captchaToken))
                {
                    requestContent["logintoken"] = captchaToken;
                    requestContent["logincaptcha"] = captcha;
                }

                responseMessage = await this.SendAsync(
                                                new HttpRequestMessage(HttpMethod.Post, ClientLoginPath)
                                                {
                                                    Content = new FormUrlEncodedContent(requestContent)
                                                },
                                                HttpCompletionOption.ResponseContentRead);
                if (!responseMessage.Content.IsPlainText())
                {
                    return GoogleAuthResponse.ErrorResponse(GoogleAuthResponse.ErrorResponseCode.Unknown);
                }

                var dictionary = await responseMessage.Content.ReadAsDictionaryAsync();
                if (!responseMessage.IsSuccessStatusCode
                    || string.IsNullOrEmpty(dictionary["SID"])
                    || string.IsNullOrEmpty(dictionary["LSID"]))
                {
                    var error = GoogleAuthResponse.ErrorResponseCode.Unknown;

                    string dictionaryValue;
                    if (dictionary.TryGetValue("Error", out dictionaryValue))
                    {
                        Enum.TryParse(dictionary["Error"], out error);
                    }

                    var response = GoogleAuthResponse.ErrorResponse(error);

                    if (response.Error == GoogleAuthResponse.ErrorResponseCode.CaptchaRequired)
                    {
                        response.CaptchaToken = dictionary["CaptchaToken"];
                        response.CaptchaUrl = "https://google.com/accounts/" + dictionary["CaptchaUrl"];
                    }

                    return response;
                }

                responseMessage.DisposeIfDisposable();
                responseMessage = await this.SendAsync(
                                                    new HttpRequestMessage(HttpMethod.Post, IssueAuthTokenPath)
                                                    {
                                                        Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                                                                                        {
                                                                                            { "SID", dictionary["SID"] }, 
                                                                                            { "LSID", dictionary["LSID"] }, 
                                                                                            { "service", "gaia" }
                                                                                        })
                                                    },
                                                    HttpCompletionOption.ResponseContentRead);
                if (!responseMessage.IsSuccessStatusCode || !responseMessage.Content.IsPlainText())
                {
                    return GoogleAuthResponse.ErrorResponse(GoogleAuthResponse.ErrorResponseCode.Unknown);
                }

                var token = await responseMessage.Content.ReadAsStringAsync();

                var url = new StringBuilder(TokenAuthPath);
                url.Append("?");
                url.AppendFormat("auth={0}", WebUtility.UrlEncode(token));
                url.AppendFormat("&service=sj");
                url.AppendFormat("&continue={0}", WebUtility.UrlEncode(serviceUri.ToString()));
                var requestUri = url.ToString();

                responseMessage.DisposeIfDisposable();
                responseMessage = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return GoogleAuthResponse.ErrorResponse(GoogleAuthResponse.ErrorResponseCode.Unknown);
                }

                return GoogleAuthResponse.SuccessResponse(this.httpClientHandler.CookieContainer.GetCookies(serviceUri), dictionary["Auth"]);
            }
            finally
            {
                responseMessage.DisposeIfDisposable();
                this.httpClientHandler.DisposeIfDisposable();
                this.httpClient.DisposeIfDisposable();
                this.httpClientHandler = null;
                this.httpClient = null;
            }
        }

        private void InitializeHttpClient()
        {
            this.httpClientHandler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            // Log information about Proxy settings
            if (this.logger.IsDebugEnabled)
            {
                try
                {
                    this.logger.Debug("Supports Proxy: {0}", this.httpClientHandler.SupportsProxy);
                    this.logger.Debug("UseProxy: {0}", this.httpClientHandler.UseProxy);
                    this.logger.Debug("Proxy: {0}", this.httpClientHandler.Proxy);
                    if (this.httpClientHandler.Proxy != null)
                    {
                        this.logger.Debug("Proxy: {0}", this.httpClientHandler.Proxy);
                    }

                    if (WebRequest.DefaultWebProxy != null)
                    {
                        this.logger.Debug("DefaultWebProxy: {0}", WebRequest.DefaultWebProxy);
                        this.logger.Debug("DefaultWebProxy.IsBypassed: {0}", WebRequest.DefaultWebProxy.IsBypassed(new Uri("https://www.google.com")));
                        this.logger.Debug("DefaultWebProxy.GetProxy: {0}", WebRequest.DefaultWebProxy.GetProxy(new Uri("https://www.google.com")));
                    }
                }
                catch (Exception e)
                {
                    this.logger.Warning(e, "Exception while tried to get proxy settings");
                }
            }

            HttpClientHandlerExtensionsEx.SetProxy(this.httpClientHandler, this.logger, this.settingsService);

            this.httpClient = new HttpClient(this.httpClientHandler)
            {
                BaseAddress = new Uri("https://www.google.com"),
                Timeout = TimeSpan.FromSeconds(60),
                DefaultRequestHeaders =
                                        {
                                            { HttpRequestHeader.UserAgent.ToString(), "Music Manager (1, 0, 54, 4672 - Windows)" }
                                        }
            };
        }
    }
}
