using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Process.Application.Clients.Cards;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.Repositories.ErrorMessages;
using System.Process.Infrastructure.Repositories.EntityFramework;
using System.Process.Infrastructure.Repositories.MongoDb;
using System.Process.Infrastructure.Repositories.MongoDb.ErrorMessages;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class RepositoriesServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMongoClient<Transaction, string>(configuration);
            services.AddMongoClient<Customer, string>(configuration);
            services.AddMongoClient<Statement, string>(configuration);
            services.AddMongoClient<ErrorMessages, string>(configuration);
            services.AddMongoClient<Company, ObjectId>(configuration);

            BsonClassMap.RegisterClassMap<Transaction>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Customer>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Statement>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<ErrorMessages>(cm => cm.AutoMap());

            services.AddTransient<ITransactionReadRepository, TransactionReadRepository>();
            services.AddTransient<ICustomerReadRepository, CustomerReadRepository>();
            services.AddTransient<IReceiverReadRepository, ReceiverReadRepository>();
            services.AddTransient<IReceiverWriteRepository, ReceiverWriteRepository>();
            services.AddTransient<IStatementReadRepository, StatementReadRepository>();
            services.AddTransient<ICardReadRepository, CardReadRepository>();
            services.AddTransient<ICardWriteRepository, CardWriteRepository>();
            services.AddTransient<ITransferReadRepository, TransferReadRepository>();
            services.AddTransient<ITransferWriteRepository, TransferWriteRepository>();
            services.AddTransient<ITransferItemReadRepository, TransferItemReadRepository>();
            services.AddTransient<ITransferItemWriteRepository, TransferItemWriteRepository>();
            services.AddTransient<ITransferWriteRepository, TransferWriteRepository>();
            services.AddTransient<ICardService, CardService>();
            services.AddScoped<IErrorMessagesRepository, ErrorMessagesRepository>();
            services.AddScoped<IErrorMessagesReadRepository, ErrorMessagesReadRepository>();
            services.AddTransient<ICompanyReadRepository, CompanyReadRepository>();
            services.AddTransient<ICompanyWriteRepository, CompanyWriteRepository>();

            return services;
        }
    }
}
