#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EntityFramework.Extensions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage
{
    public class SubscriptionPersister : ISubscriptionStorage
    {
        private readonly INServiceBusDbContextFactory _dbContextFactory;

        public SubscriptionPersister(INServiceBusDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Subscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            string clientAddress = client.ToString();
            List<string> messageTypeStrings = new List<string>();
            List<SubscriptionEntity> subscriptions = new List<SubscriptionEntity>();

            foreach (MessageType mt in messageTypes)
            {
                string messageTypeString = mt.ToString();

                messageTypeStrings.Add(messageTypeString);
                subscriptions.Add(
                    new SubscriptionEntity
                    {
                        SubscriberEndpoint = clientAddress,
                        MessageType = messageTypeString
                    });
            }

            using (var dbc = _dbContextFactory.CreateSubscriptionDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var existing = dbc.Subscriptions.Where(
                            s => s.SubscriberEndpoint == clientAddress
                                 && messageTypeStrings.Contains(s.MessageType))
                            .ToList();

                        foreach (var subscription in subscriptions)
                        {
                            if (existing.Any(s => s.MessageType == subscription.MessageType))
                            {
                                return;
                            }

                            dbc.Subscriptions.Add(subscription);
                        }

                        dbc.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Unsubscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            using (var dbc = _dbContextFactory.CreateSubscriptionDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    string clientAddress = client.ToString();
                    List<string> messageTypeStrings = messageTypes.Select(mt => mt.ToString()).ToList();

                    try
                    {
                        dbc.Subscriptions.Where(
                            s => s.SubscriberEndpoint == clientAddress
                                 && messageTypeStrings.Contains(s.MessageType))
                            .Delete();

                        dbc.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes)
        {
            using (var dbc = _dbContextFactory.CreateSubscriptionDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var messageTypeStrings = messageTypes.Select(mt => mt.ToString()).ToList();
                        var subscriptions = dbc.Subscriptions
                            .Where(s => messageTypeStrings.Contains(s.MessageType))
                            .ToList();

                        transaction.Commit();

                        return subscriptions
                            .Select(s => Address.Parse(s.SubscriberEndpoint))
                            .ToList();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Init()
        {
        }
    }
}