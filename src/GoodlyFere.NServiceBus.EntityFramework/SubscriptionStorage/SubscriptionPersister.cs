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
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage
{
    public class SubscriptionPersister : ISubscriptionStorage
    {
        private static readonly ILog Logger = LogManager.GetLogger<SubscriptionPersister>();
        private readonly INServiceBusDbContextFactory _dbContextFactory;

        public SubscriptionPersister(INServiceBusDbContextFactory dbContextFactory)
        {
            if (dbContextFactory == null)
            {
                throw new ArgumentNullException("dbContextFactory");
            }

            _dbContextFactory = dbContextFactory;
        }

        public IEnumerable<Address> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes)
        {
            Logger.Debug("Getting subscriber addresses for message types");

            if (messageTypes == null)
            {
                Logger.Debug("Message types are null! Throwing argument null exception.");
                throw new ArgumentNullException("messageTypes");
            }

            MessageType[] mtArray = messageTypes as MessageType[] ?? messageTypes.ToArray();
            if (!mtArray.Any())
            {
                Logger.Debug("Message types list is empty, returning empty address list.");
                return new List<Address>();
            }

            List<string> messageTypeStrings = mtArray.Select(mt => mt.ToString()).ToList();
            Logger.DebugFormat(
                "Message types are {0}.",
                string.Join(",", messageTypeStrings));

            using (ISubscriptionDbContext dbc = _dbContextFactory.CreateSubscriptionDbContext())
            using (DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                Logger.Debug("Querying for subscriptions with message types.");
                var subscriptions = dbc.Subscriptions
                    .Where(s => messageTypeStrings.Contains(s.MessageType))
                    .ToList();
                Logger.DebugFormat("{0} subscription(s) found.", subscriptions.Count);

                Logger.Debug("Getting addresses for found subscriptions.");
                List<Address> results = subscriptions
                    .Select(s => Address.Parse(s.SubscriberEndpoint))
                    .Distinct()
                    .ToList();

                Logger.Debug("Committing transaction and returning addresses.");
                transaction.Commit();
                return results;
            }
        }

        public void Init()
        {
            Logger.Debug("Doing nothing in Init.");
        }

        public void Subscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            Logger.Debug("Subscribe called");

            if (client == null)
            {
                Logger.Debug("Client is null!  Throwing");
                throw new ArgumentNullException("client");
            }

            if (messageTypes == null)
            {
                Logger.Debug("Message types is null!  Throwing");
                throw new ArgumentNullException("messageTypes");
            }

            string clientAddress = client.ToString();
            List<string> messageTypeStrings = new List<string>();
            List<SubscriptionEntity> subscriptions = new List<SubscriptionEntity>();

            foreach (MessageType mt in messageTypes.Distinct())
            {
                string messageTypeString = mt.ToString();

                Logger.DebugFormat("Adding message type {0} to {1}.", messageTypeString, clientAddress);

                messageTypeStrings.Add(messageTypeString);
                subscriptions.Add(
                    new SubscriptionEntity
                    {
                        SubscriberEndpoint = clientAddress,
                        MessageType = messageTypeString
                    });
            }

            using (ISubscriptionDbContext dbc = _dbContextFactory.CreateSubscriptionDbContext())
            using (DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    Logger.DebugFormat("Gathering subscriptions for {0} and any of the requested message types.", clientAddress);
                    var existing = dbc.Subscriptions.Where(
                        s => s.SubscriberEndpoint == clientAddress
                             && messageTypeStrings.Contains(s.MessageType));

                    foreach (var subscription in subscriptions)
                    {
                        if (existing.Any(s => s.MessageType == subscription.MessageType))
                        {
                            Logger.DebugFormat("{0} already is subscribed to {1}", clientAddress, subscription.MessageType);
                            continue;
                        }

                        Logger.DebugFormat(
                            "{0} doesn't yet have {1}.  Subscribing...",
                            clientAddress,
                            subscription.MessageType);
                        dbc.Subscriptions.Add(subscription);
                    }

                    Logger.Debug("Saving changes and committing transaction.");
                    dbc.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Logger.Error("Some error happened while saving subscriptions", ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void Unsubscribe(Address client, IEnumerable<MessageType> messageTypes)
        {
            Logger.Debug("Unsubscribe called");

            if (client == null)
            {
                Logger.Debug("Client is null!  Throwing");
                throw new ArgumentNullException("client");
            }

            if (messageTypes == null)
            {
                Logger.Debug("MessageTypes is null!  Throwing");
                throw new ArgumentNullException("messageTypes");
            }

            using (ISubscriptionDbContext dbc = _dbContextFactory.CreateSubscriptionDbContext())
            using (DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    string clientAddress = client.ToString();
                    List<string> messageTypeStrings = messageTypes.Select(mt => mt.ToString()).ToList();

                    Logger.DebugFormat("Finding existing subscriptions for message types requested of {0}", clientAddress);
                    List<SubscriptionEntity> existing = dbc.Subscriptions.Where(
                        s => s.SubscriberEndpoint == clientAddress
                             && messageTypeStrings.Contains(s.MessageType))
                        .ToList();

                    Logger.DebugFormat("{0} subscription(s) found, removing them.", existing.Count);
                    dbc.Subscriptions.RemoveRange(existing);

                    Logger.Debug("Saving changes and committing transaction.");
                    dbc.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Logger.Error("Some error happened while removing subscriptions", ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}