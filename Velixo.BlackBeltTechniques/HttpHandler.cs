using Autofac;
using PX.Data.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using PX.Export.Authentication;
using System.Web;
using System.Web.Routing;

namespace Velixo.BlackBeltTechniques
{
    public class ServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(RouteHandler<>)).SingleInstance();
            builder.RegisterType<SampleWebHookRequestHandler>().SingleInstance();
            builder.ActivateOnApplicationStart<RouteInitializer>(e => e.InitializeRoutes());
            
            builder
                .RegisterInstance(new LocationSettings
                {
                    Path = "/" + RouteInitializer.SampleRoute,
                    Providers =
                    {
                        new ProviderSettings
                        {
                            Name = "basic",
                            Type = typeof (BasicAuthenticationModule).AssemblyQualifiedName
                        }
                    }
                });
        }
    }

    public class RouteInitializer
    {
        private readonly ILifetimeScope _container;
        internal const string SampleRoute = "SampleWebHook/test";

        public RouteInitializer(ILifetimeScope container)
        {
            _container = container;
        }

        public void InitializeRoutes()
        {
            RouteTable.Routes.Add(new Route($"{SampleRoute}", _container.Resolve<RouteHandler<SampleWebHookRequestHandler>>()));
        }
    }

    internal class RouteHandler<T> : IRouteHandler 
        where T : IHttpHandler
    {
        private readonly ILifetimeScope _container;

        public RouteHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return _container.Resolve<T>();
        }
    }

    internal class SampleWebHookRequestHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("Hello, world!");
            context.Response.StatusCode = 200;
            context.Response.End();
        }
    }
}