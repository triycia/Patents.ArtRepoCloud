using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataFetchers;
using Patents.ArtRepoCloud.Service.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Patents.ArtRepoCloud.Service.DataFetchers.Epo;
using Patents.ArtRepoCloud.Service.DataFetchers.Ifi;
using System.Collections;
using Patents.ArtRepoCloud.Service.Code.Attributes;
using System.Reflection;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Patents.ArtRepoCloud.Service.Extensions
{
    public static class StartupExtensions
    {
        public static bool IsFetcherEnabled(this FetcherSettings settings, Type type)
        {
            IList<PropertyInfo> props = new List<PropertyInfo>(typeof(FetcherSettings).GetProperties());

            foreach (PropertyInfo prop in props)
            {
                var fetcherType = prop.GetCustomAttributes(typeof(FetcherAttribute), false).OfType<FetcherAttribute>().FirstOrDefault()?.FetcherType;

                if (fetcherType == type)
                {
                    return (bool)(prop.GetValue(settings) ?? false);
                }
            }

            return false;
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            OrderBy<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var sss = registration.ActivatorData.Filters;
            return registration;
        }
    }
}