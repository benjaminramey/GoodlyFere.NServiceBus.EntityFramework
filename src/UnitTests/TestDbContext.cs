#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDbContext.cs">
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
// <created>03/30/2015 4:25 PM</created>
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using NServiceBus;
using NServiceBus.Saga;

#endregion

namespace UnitTests
{
    public class TestDbContext : NServiceBusDbContext
    {
        public TestDbContext()
            : base("TestDbContext")
        {
            //Database.Log = s => File.AppendAllText("dbcontext.log", s);

            Database.SetInitializer(new CreateDatabaseIfNotExists<TestDbContext>());

        }

        public DbSet<TestSagaDataWithRowVersion> TestSagasWithRowVersion { get; set; }
        public DbSet<TestSagaData> TestSagaDatas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestSagaDataWithRowVersion>()
                .ToTable("UnitTests_TestSagaDatasWithRowVersion");
            modelBuilder.Entity<TestSagaData>()
                .ToTable("UnitTests_TestSagaDatas");

            base.OnModelCreating(modelBuilder);
        }
    }

    public class TestSagaDataWithRowVersion : IContainSagaData
    {
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string SomeProp1 { get; set; }
        public string SomeProp2 { get; set; }

        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }
    }

    public class TestSagaData : IContainSagaData
    {
        public string SomeProp1 { get; set; }
        public string SomeProp2 { get; set; }

        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }
    }

    internal class TestMessage : IMessage
    {
    }

    internal class TestMessage2 : IMessage
    {
    }
}