#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubscriptionPersister.cs">
//  Copyright 2015 Benjamin S. Ramey
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </copyright>
// <created>03/25/2015 7:55 PM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (messageTypes == null)
            {
                throw new ArgumentNullException("messageTypes");
            }

            string clientAddress = client.ToString();
            List<string> messageTypeStrings = new List<string>();
            List<SubscriptionEntity> subscriptions = new List<SubscriptionEntity>();

            foreach (MessageType mt in messageTypes.Distinct())
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
                                 && messageTypeStrings.Contains(s.MessageType));

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
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (messageTypes == null)
            {
                throw new ArgumentNullException("messageTypes");
            }

            using (var dbc = _dbContextFactory.CreateSubscriptionDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    string clientAddress = client.ToString();
                    List<string> messageTypeStrings = messageTypes.Select(mt => mt.ToString()).ToList();

                    try
                    {
                        List<SubscriptionEntity> existing = dbc.Subscriptions.Where(
                            s => s.SubscriberEndpoint == clientAddress
                                 && messageTypeStrings.Contains(s.MessageType))
                            .ToList();

                        dbc.Subscriptions.RemoveRange(existing);

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
            if (messageTypes == null)
            {
                throw new ArgumentNullException("messageTypes");
            }

            MessageType[] mtArray = messageTypes as MessageType[] ?? messageTypes.ToArray();
            if (!mtArray.Any())
            {
                return new List<Address>();
            }

            using (var dbc = _dbContextFactory.CreateSubscriptionDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var messageTypeStrings = mtArray.Select(mt => mt.ToString()).ToList();
                        var subscriptions = dbc.Subscriptions
                            .Where(s => messageTypeStrings.Contains(s.MessageType))
                            .ToList();

                        transaction.Commit();

                        return subscriptions
                            .Select(s => Address.Parse(s.SubscriberEndpoint))
                            .Distinct()
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