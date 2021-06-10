using MediatR;
using System.Process.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace System.Process.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryRequest : IRequest<GetAccountHistoryResponse>
    {
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string AccountType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TransactionsType { get; set; }
        public SortMethod? SortMethod { get; set; }
        public string TransactionGroupName { get; set; }
        public string Filter { get; set; }
    }
}