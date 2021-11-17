using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.Sns
{
    public interface ISnsEventVerifier : IDisposable
    {
        Exception LastException { get; }

        Task<bool> VerifySnsEventRaised<T>(Action<T> verifyFunction) where T : class;
        Task PurgeQueueMessages();
    }

    /// <summary>
    /// Helper class used to verify that the correct Sns event gets raised during an E2E test
    /// </summary>
    public class SnsEventVerifier : ISnsEventVerifier
    {
        private readonly JsonSerializerOptions _jsonOptions;

        private readonly IAmazonSQS _amazonSQS;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly string _topicArn;
        private readonly string _queueUrl;
        private readonly string _subscriptionArn;

        private readonly string _sqsQueueName = "test-messages";

        public Exception LastException { get; private set; }

        /// <summary>
        /// Constructor
        /// * Constructs a temporary queue to receive a copy of all events raised during a test.
        /// * Configures a subscription to ensure events raised to the topic get set to the queue.
        /// </summary>
        /// <param name="amazonSQS">The SQS client</param>
        /// <param name="snsClient">The SNS client</param>
        /// <param name="topicArn">The arn of the topic</param>
        public SnsEventVerifier(IAmazonSQS amazonSQS, IAmazonSimpleNotificationService snsClient, string topicArn)
        {
            _amazonSQS = amazonSQS;
            _snsClient = snsClient;
            _topicArn = topicArn;
            _jsonOptions = CreateJsonOptions();

            var queueResponse = _amazonSQS.CreateQueueAsync(_sqsQueueName).GetAwaiter().GetResult();
            _queueUrl = queueResponse.QueueUrl;

            _subscriptionArn = _snsClient.SubscribeQueueAsync(_topicArn, _amazonSQS, _queueUrl)
                                        .GetAwaiter().GetResult();
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _snsClient.UnsubscribeAsync(_subscriptionArn).GetAwaiter().GetResult();
                _snsClient.DeleteTopicAsync(_topicArn).GetAwaiter().GetResult();
                _amazonSQS.DeleteQueueAsync(_queueUrl).GetAwaiter().GetResult();

                _disposed = true;
            }
        }

        public async Task PurgeQueueMessages()
        {
            var request = new PurgeQueueRequest()
            {
                QueueUrl = _queueUrl
            };
            await _amazonSQS.PurgeQueueAsync(request);
        }

        /// <summary>
        /// Verifies that the an expected event has been raised.
        /// </summary>
        /// <typeparam name="T">The event class used to create the event.</typeparam>
        /// <param name="verifyFunction">A function that will receive a copy of each event raised.
        /// This function should attempt to verify that the contents of the message match what is expected.
        /// Throw an exception should then contents not match.
        /// </param>
        /// <returns>true if a message in the temporary queue satisfies the verification function.
        /// false if no message in the temporary queue satisfies the verification function</returns>
        public async Task<bool> VerifySnsEventRaised<T>(Action<T> verifyFunction) where T : class
        {
            bool eventFound = false;
            var request = new ReceiveMessageRequest(_queueUrl)
            {
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 2
            };
            var response = await _amazonSQS.ReceiveMessageAsync(request);
            foreach (var msg in response.Messages)
            {
                eventFound = IsExpectedMessage(msg, verifyFunction);
                if (eventFound) break;
            }

            return eventFound;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private bool IsExpectedMessage<T>(Message msg, Action<T> verifyFunction) where T : class
        {
            // Here we are assuming the message is not in raw format
            // (which is the case when using SubscribeQueueAsync() in the constructor above)
            var payloadString = JsonDocument.Parse(msg.Body).RootElement.GetProperty("Message").GetString();
            var eventObject = JsonSerializer.Deserialize<T>(payloadString, _jsonOptions);
            try
            {
                verifyFunction(eventObject);
                return true;
            }
            catch (Exception e)
            {
                LastException = e;
                return false;
            }
        }
    }
}
