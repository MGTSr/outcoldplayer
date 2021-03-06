﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IConfigWebService
    {
        Task<bool> IsAllAccessAvailableAsync();

        Task<bool> IsAccesptedUserAsync();
    }

    public class ConfigWebService : IConfigWebService
    {
        private readonly IGoogleMusicApisService googleMusicApisService;

        private const string Config = "config?start-token=0&max-results=0";

        public ConfigWebService(IGoogleMusicApisService googleMusicApisService)
        {
            this.googleMusicApisService = googleMusicApisService;
        }

        public Task<bool> IsAllAccessAvailableAsync()
        {
            return this.IsConfigTrueAsync("isNautilusUser");
        }

        public Task<bool> IsAccesptedUserAsync()
        {
            return this.IsConfigTrueAsync("isAcceptedUser");
        }

        private async Task<bool> IsConfigTrueAsync(string config)
        {
            var response = await this.googleMusicApisService.GetAsync<GoogleConfigResponse>(Config);
            if (response != null && response.Data != null && response.Data.Entries != null)
            {
                var entity = response.Data.Entries.FirstOrDefault(
                    x => string.Equals(x.Key, config, StringComparison.OrdinalIgnoreCase));
                if (entity != null)
                {
                    bool value;
                    if (bool.TryParse(entity.Value, out value))
                    {
                        return value;
                    }
                }
            }

            return false;
        }
    }
}
