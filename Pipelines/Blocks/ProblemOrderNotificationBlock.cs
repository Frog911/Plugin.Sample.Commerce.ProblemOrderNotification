namespace Plugin.Sample.Commerce.ProblemOrderNotification
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.SQL;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;


    [PipelineDisplayName("Plugin.Sample.Commerce.ProblemOrderNotification")]
    public class ProblemOrderNotificationBlock : ConditionalPipelineBlock<ListEntitiesArgument, ListEntitiesArgument, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;

        public ProblemOrderNotificationBlock(IFindEntityPipeline findEntityPipeline)
        {
            this._findEntityPipeline = findEntityPipeline;
            this.BlockCondition = new Predicate<IPipelineExecutionContext>(this.ValidatePolicy);
        }

        public override Task<ListEntitiesArgument> ContinueTask(ListEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            return Task.FromResult(arg);
        }

        public override async Task<ListEntitiesArgument> Run(ListEntitiesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: the argument cannot be null.");
            try
            {
                if (!arg.EntityIds.Any())
                {
                    string str = await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "EntitiesWereAdded", null, $"{this.Name}: No entities were added to list {arg.ListName} because no entity ids were supplied.");
                    arg.Success = false;
                    return arg;
                }

                var knownOrderListsPolicy = context.GetPolicy<KnownOrderListsPolicy>();

                if (arg.ListName != knownOrderListsPolicy.ProblemOrders)
                {
                    return arg;
                }

                ListShardingPolicy byName = ListShardingPolicy.GetByName(context.CommerceContext, context.GetPolicy<ListNamePolicy>().ListName(arg.ListName));
                bool hasVersionedEntities = byName != null && byName.HasVersionedEntities;
                foreach (string entityId in arg.EntityIds)
                {
                    context.Logger.LogDebug($"{this.Name}: ListName={arg.ListName} EntityId={entityId}");
                    int? entityVersion = new int?();
                    int num;

                    if (hasVersionedEntities && arg.EntityVersionMap.TryGetValue(entityId, out num))
                    {
                        entityVersion = new int?(num);
                    }

                    var order = await _findEntityPipeline.Run(new FindEntityArgument(typeof(Order), entityId, entityVersion, false)).ConfigureAwait(false) as Order;
                    ///
                    /// TODO: send email
                    ///
                }

                //arg.Success = true;
            }
            catch (Exception ex)
            {
                arg.Success = false;
                await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, this.Name, new object[] { ex }, this.Name + ".Exception: " + ex.Message);
            }
            return arg;
        }

        private bool ValidatePolicy(IPipelineExecutionContext obj)
        {
            return ((CommercePipelineExecutionContext)obj).CommerceContext.HasPolicy<EntityStoreSqlPolicy>();
        }
    }
}
