namespace Plugin.Sample.Commerce.ProblemOrderNotification
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.SQL;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

               .ConfigurePipeline<IAddListEntitiesPipeline>(configure => configure.Add<ProblemOrderNotificationBlock>().Before<AddListEntitiesBlock>()));

            services.RegisterAllCommands(assembly);
        }
    }
}