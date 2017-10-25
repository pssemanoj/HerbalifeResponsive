using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using HL.Common.Logging;
using Microsoft.Mef.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using MyHerbalife3.Shared.Infrastructure.MEF;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;
using IFilterProvider = System.Web.Http.Filters.IFilterProvider;


namespace MyHerbalife3.Web
{
    public static class MefConfig
    {
        public static void RegisterMef()
        {
            var filterProviders = FilterProviders.Providers;
            var httpConfiguration = GlobalConfiguration.Configuration;
            // RegisterILoaders(catalog);

            var mvcContentions = GetMvcConventions();

            // regular MVC controllers
            var mvcControllerCatalog = new DirectoryCatalog(@".\bin", mvcContentions);
            var mvcControllerContainer = new CompositionContainer(mvcControllerCatalog,
                                                                  CompositionOptions.Default);
            ControllerBuilder.Current.SetControllerFactory(new MefMvcControllerFactory(mvcControllerContainer));

            // MVC filter attributes

            filterProviders.Remove(filterProviders.Single(f => f is FilterAttributeFilterProvider));
            filterProviders.Add(new MefFilterAttributeProvider(mvcControllerContainer));

            // api controllers
            var apiConventions = GetApiConventions();
            var apiControllerCatalog = new DirectoryCatalog(@".\bin", apiConventions);
            var apiControllerContainer = new CompositionContainer(apiControllerCatalog,
                                                                  CompositionOptions.Default);
            ServiceLocator.SetLocatorProvider(() => new MefServiceLocator(apiControllerContainer));

            httpConfiguration.DependencyResolver = new MefMvcDependencyResolver(apiControllerContainer);

            // api filter provider

            var apiFilterType = typeof(IFilterProvider);

            try
            {
                foreach (
                    var existing in httpConfiguration.Services
                        .GetServices(apiFilterType)
                        .OfType<ActionDescriptorFilterProvider>())
                {
                    httpConfiguration.Services.Remove(apiFilterType, existing);
                }

            }
            catch (ReflectionTypeLoadException loadException)
            {
                LoggerHelper.Error("MEF config encountered Type Load Exception");
                if (loadException.LoaderExceptions != null)
                {
                    foreach (var error in loadException.LoaderExceptions)
                    {
                        LoggerHelper.Error(error.ToString());
                    }
                }
                throw;
            }

            httpConfiguration.Services.Add(apiFilterType,
                                           new WebApiActionDescriptionFilterProvider(apiControllerContainer));
        }

        internal static RegistrationBuilder GetApiConventions()
        {
            var conventions = new RegistrationBuilder();

            AddComponentsDerivedFromMyHerbalife3Interfaces(conventions);

            conventions.ForTypesDerivedFrom<ApiController>()
                       .Export()
                       .SetCreationPolicy(CreationPolicy.NonShared);

            conventions.ForTypesDerivedFrom<ActionFilterAttribute>()
                       .Export()
                       .SetCreationPolicy(CreationPolicy.NonShared);
            return conventions;
        }

        internal static RegistrationBuilder GetMvcConventions()
        {
            var conventions = new RegistrationBuilder();

            AddComponentsDerivedFromMyHerbalife3Interfaces(conventions);

            conventions.ForTypesDerivedFrom<IController>().Export().SetCreationPolicy(CreationPolicy.NonShared);

            conventions.ForTypesDerivedFrom<System.Web.Mvc.ActionFilterAttribute>()
                       .Export()
                       .SetCreationPolicy(CreationPolicy.NonShared);

            return conventions;
        }

        private static void AddComponentsDerivedFromMyHerbalife3Interfaces(RegistrationBuilder conventions)
        {
            conventions.ForTypesMatching(t => t.GetInterfaces()
                                               .Any(IsAnHlType))
                       .ExportInterfaces(IsAnHlType)
                       .SetCreationPolicy(CreationPolicy.NonShared);
        }

        private static bool IsAnHlType(Type type)
        {
            var result = type.Namespace != null && type.Namespace.StartsWith("MyHerbalife3.") && type.IsPublic;
            return result;
        }
    }
}