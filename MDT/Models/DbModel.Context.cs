﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MDT.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DbEntities : DbContext
    {
        public DbEntities()
            : base("name=DbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Balance> Balances { get; set; }
        public virtual DbSet<Description> Descriptions { get; set; }
        public virtual DbSet<DrawEntry> DrawEntries { get; set; }
        public virtual DbSet<DrawOption> DrawOptions { get; set; }
        public virtual DbSet<DrawResult> DrawResults { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
        public virtual DbSet<GroupInvite> GroupInvites { get; set; }
        public virtual DbSet<GroupUser> GroupUsers { get; set; }
        public virtual DbSet<Ledger> Ledgers { get; set; }
        public virtual DbSet<NumberSet> NumberSets { get; set; }
        public virtual DbSet<ObjectType> ObjectTypes { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionType> TransactionTypes { get; set; }
        public virtual DbSet<UserDrawTypeOption> UserDrawTypeOptions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<SentEmail> SentEmails { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<UniqueKey> UniqueKeys { get; set; }
        public virtual DbSet<VerificationKey> VerificationKeys { get; set; }
        public virtual DbSet<PendingTransaction> PendingTransactions { get; set; }
        public virtual DbSet<DrawType> DrawTypes { get; set; }
        public virtual DbSet<Draw> Draws { get; set; }
    }
}
