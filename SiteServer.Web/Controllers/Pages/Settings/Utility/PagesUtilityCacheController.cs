﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using SiteServer.Abstractions;
using SiteServer.CMS.Context;
using SiteServer.CMS.Core;
using SiteServer.CMS.Repositories;

namespace SiteServer.API.Controllers.Pages.Settings.Utility
{
    [RoutePrefix("pages/settings/utilityCache")]
    public class PagesUtilityCacheController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public async Task<IHttpActionResult> Get()
        {
            var auth = await AuthenticatedRequest.GetAuthAsync();
            if (!auth.IsAdminLoggin ||
                !await auth.AdminPermissionsImpl.HasSystemPermissionsAsync(Constants.AppPermissions.SettingsUtilityCache))
            {
                return Unauthorized();
            }

            var parameterList = new List<KeyValuePair<string, string>>();
            foreach (var key in CacheUtils.AllKeys)
            {
                var value = GetCacheValue(key);
                if (!string.IsNullOrEmpty(value))
                {
                    parameterList.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            return Ok(new
            {
                Value = parameterList,
                parameterList.Count
            });
        }

        private static string GetCacheValue(string key)
        {
            var value = CacheUtils.Get(key);

            var valueType = value?.GetType().FullName;
            if (valueType == null)
            {
                return null;
            }

            if (valueType == "System.String")
            {
                return $"String, Length:{value.ToString().Length}";
            }

            if (valueType == "System.Int32")
            {
                return value.ToString();
            }

            if (valueType.StartsWith("System.Collections.Generic.List"))
            {
                return $"List, Count:{((ICollection)value).Count}";
            }

            return valueType;
        }

        [HttpPost, Route(Route)]
        public async Task<IHttpActionResult> ClearCache()
        {
            var auth = await AuthenticatedRequest.GetAuthAsync();
            if (!auth.IsAdminLoggin ||
                !await auth.AdminPermissionsImpl.HasSystemPermissionsAsync(Constants.AppPermissions.SettingsUtilityCache))
            {
                return Unauthorized();
            }

            await DataProvider.ConfigRepository.ClearAllCache();

            CacheUtils.ClearAll();
            await DataProvider.DbCacheRepository.ClearAsync();

            var parameterList = new List<KeyValuePair<string, string>>();
            foreach (var key in CacheUtils.AllKeys)
            {
                var value = GetCacheValue(key);
                if (!string.IsNullOrEmpty(value))
                {
                    parameterList.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            return Ok(new
            {
                Value = parameterList,
                parameterList.Count
            });
        }
    }
}