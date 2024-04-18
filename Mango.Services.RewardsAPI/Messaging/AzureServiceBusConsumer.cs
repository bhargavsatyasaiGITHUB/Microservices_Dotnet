using Azure.Messaging.ServiceBus;
using Mango.Services.RewardsAPI.Message;
using Mango.Services.RewardsAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardsAPI.Messaging
{
    public class AzureServiceBusConsumer:IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string orderCreatedTopic;
        private readonly string orderCreatedRewardSubsciption;
        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;
        private ServiceBusProcessor _rewardProcessor;
        

        public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _rewardService=rewardService;
                _configuration = configuration;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedRewardSubsciption = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Reward_Subcription");
            var client=new ServiceBusClient(serviceBusConnectionString);
            _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubsciption);
        }

        public async Task Start()
        {
            _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
            _rewardProcessor.ProcessErrorAsync += ErrorHandler;
            await _rewardProcessor.StopProcessingAsync();

          
        }

      

        public async Task Stop()
        {
           await _rewardProcessor.StopProcessingAsync();
            await _rewardProcessor.DisposeAsync();

            
        }

        private  Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
        {
            // this is where we will receive message
            var message=args.Message;
            var body=Encoding.UTF8.GetString(message.Body);

            RewardsMessage objMessage =JsonConvert.DeserializeObject<RewardsMessage>(body);
            try
            {
                await _rewardService.UpdateRewards(objMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        


    }
}
