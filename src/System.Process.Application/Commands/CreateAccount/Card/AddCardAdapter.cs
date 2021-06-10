using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;

namespace System.Process.Application.Commands.CreateAccount.Card
{
    public class AddCardAdapter : IAdapter<Domain.Entities.Card, AddCardRequest>
    {
        private ProcessConfig ProcessConfig { get; set; }

        public AddCardAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        public Domain.Entities.Card Adapt(AddCardRequest input)
        {
            return new Domain.Entities.Card
            {
                CustomerId = input.CustomerId,
                Pan = input.Pan,
                AccountBalance = ProcessConfig.AccountBalance,
                Bin = input.Bin,
                BusinessName = input.BusinessName,
                CardHolder = input.CardHolder,
                CardStatus = input.CardStatus,
                CardType = input.CardType,
                ExpirationDate = input.ExpirationDate,
                LastFour = input.LastFour,
                Locked = int.Parse(ProcessConfig.Locked)
            };
        }
    }
}
