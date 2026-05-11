using CloudKit.Infrastructure.Data.MongoDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using B2BAdmin.ApiDocument.Domains.Models;
using CloudKit.Domain;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using B2BAdmin.ApiDocument.Domains.Models.Tours;
using B2BAdmin.ApiDocument.Domains.Models.Hotels;
using FluentValidation.Resources;
using MongoDB.Driver;

namespace B2BAdmin.ApiDocument.Infrastructure
{
    public class ApiDocumentDbContext : MongoDbBaseContext
    {
        public ApiDocumentDbContext(IOptionsMonitor<MongoDatabaseSettings> settings, ILoggerFactory logger) : base(settings, logger)
        {
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register(
               "CamelCase",
               pack,
               t => true);

            BsonClassMap.RegisterClassMap<EntityBase<string>>(cm =>
            {
                cm.AutoMap();
                cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId)).SetIdGenerator(StringObjectIdGenerator.Instance);
                cm.SetIsRootClass(true);
            });
            BsonClassMap.RegisterClassMap<LocationModel>();
            BsonClassMap.RegisterClassMap<Country>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.FormatTypeId).SetSerializer(new StringSerializer(BsonType.ObjectId));

            });
            BsonClassMap.RegisterClassMap<LocationLevel1>();
            BsonClassMap.RegisterClassMap<LocationLevel2>();
            BsonClassMap.RegisterClassMap<LocationLevel3>();
            BsonClassMap.RegisterClassMap<LocationLevel4>();
            BsonClassMap.RegisterClassMap<LocationFormatType>();

            EnsureCustomerAccountIndexes();
        }
        public MongoDB.Driver.IMongoCollection<siteTemplates> configPages { get { return _database.GetCollection<siteTemplates>("api_document_config"); } }
        public MongoDB.Driver.IMongoCollection<DocumentMenu> DocumentMenus { get { return _database.GetCollection<DocumentMenu>("api_document_menus"); } }
        public MongoDB.Driver.IMongoCollection<DocumentContent> DocumentContents { get { return _database.GetCollection<DocumentContent>("api_document_contents"); } }
        public MongoDB.Driver.IMongoCollection<DocumentMenuchildren> Api_document_menus_view_clients { get { return _database.GetCollection<DocumentMenuchildren>("api_document_menus_view_client"); } }
        public MongoDB.Driver.IMongoCollection<GentOnlineObject> GentOnlines { get { return _database.GetCollection<GentOnlineObject>("gents_onlines"); } }
        public MongoDB.Driver.IMongoCollection<UserAdmin> AdminUsers { get { return _database.GetCollection<UserAdmin>("users"); } }
        public MongoDB.Driver.IMongoCollection<ProCodes> ProCodes { get { return _database.GetCollection<ProCodes>("procodes"); } }
        public MongoDB.Driver.IMongoCollection<lsNation> Countries { get { return _database.GetCollection<lsNation>("countries"); } }
        public MongoDB.Driver.IMongoCollection<CustomerAccount> CustomerAccounts { get { return _database.GetCollection<CustomerAccount>("customer_accounts"); } }
        public MongoDB.Driver.IMongoCollection<BsonDocument> AccountTypes { get { return _database.GetCollection<BsonDocument>("account_types"); } }
        public MongoDB.Driver.IMongoCollection<CustomerDebtTransaction> CustomerDebtTransactions { get { return _database.GetCollection<CustomerDebtTransaction>("customer_debt_transactions"); } }
        public MongoDB.Driver.IMongoCollection<CustomerAuditLogEntry> CustomerAuditLogs { get { return _database.GetCollection<CustomerAuditLogEntry>("customer_audit_logs"); } }
        public MongoDB.Driver.IMongoCollection<CustomerDebtExportHistory> CustomerDebtExportHistories { get { return _database.GetCollection<CustomerDebtExportHistory>("customer_debt_export_histories"); } }
        public MongoDB.Driver.IMongoCollection<CustomerDebtReportExportHistory> CustomerDebtReportExportHistories { get { return _database.GetCollection<CustomerDebtReportExportHistory>("customer_debt_report_export_histories"); } }

        //

        public MongoDB.Driver.IMongoCollection<Hotel> HotelInfos { get { return _database.GetCollection<Hotel>("infohotels"); } }
        public MongoDB.Driver.IMongoCollection<period_groups> PeriodGroups { get { return _database.GetCollection<period_groups>("period_groupss"); } }

        // config
        public MongoDB.Driver.IMongoCollection<TourCategorys> TourCategorys { get { return _database.GetCollection<TourCategorys>("tourcategories"); } }
        public MongoDB.Driver.IMongoCollection<Tourdurations> Tourdurations { get { return _database.GetCollection<Tourdurations>("tourdurations"); } }
        public MongoDB.Driver.IMongoCollection<Country> GeoPaths
        {
            get
            {
                return _database.GetCollection<Country>("geopaths");
            }
        }
        public MongoDB.Driver.IMongoCollection<LocationFormatType> LocationFormatType
        {
            get
            {
                return _database.GetCollection<LocationFormatType>("locationformattypes");
            }
        }
       
        public MongoDB.Driver.IMongoCollection<DetailBookRetailSalesSic> BookRetailSalesSics { get { return _database.GetCollection<DetailBookRetailSalesSic>("retail_sales_sic_books"); } }

        private void EnsureCustomerAccountIndexes()
        {
            try
            {
                var customerCollection = _database.GetCollection<CustomerAccount>("customer_accounts");

                var listBaseIndex = Builders<CustomerAccount>.IndexKeys.Combine(
                    Builders<CustomerAccount>.IndexKeys.Ascending(x => x.isDeleted),
                    Builders<CustomerAccount>.IndexKeys.Ascending("isDelete"),
                    Builders<CustomerAccount>.IndexKeys.Descending(x => x.updatedAt));

                var listStatusRiskIndex = Builders<CustomerAccount>.IndexKeys.Combine(
                    Builders<CustomerAccount>.IndexKeys.Ascending(x => x.isDeleted),
                    Builders<CustomerAccount>.IndexKeys.Ascending("isDelete"),
                    Builders<CustomerAccount>.IndexKeys.Ascending(x => x.status),
                    Builders<CustomerAccount>.IndexKeys.Ascending(x => x.riskLevel),
                    Builders<CustomerAccount>.IndexKeys.Descending(x => x.updatedAt));

                customerCollection.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<CustomerAccount>(listBaseIndex, new CreateIndexOptions { Name = "idx_customer_list_base" }),
                    new CreateIndexModel<CustomerAccount>(listStatusRiskIndex, new CreateIndexOptions { Name = "idx_customer_list_status_risk" }),
                    new CreateIndexModel<CustomerAccount>(Builders<CustomerAccount>.IndexKeys.Ascending(x => x.code), new CreateIndexOptions { Name = "idx_customer_code" }),
                    new CreateIndexModel<CustomerAccount>(Builders<CustomerAccount>.IndexKeys.Ascending(x => x.taxCode), new CreateIndexOptions { Name = "idx_customer_tax_code" }),
                    new CreateIndexModel<CustomerAccount>(Builders<CustomerAccount>.IndexKeys.Ascending(x => x.phone), new CreateIndexOptions { Name = "idx_customer_phone" }),
                });
            }
            catch
            {
                // Do not block application startup if index creation fails.
            }
        }

    }
}
