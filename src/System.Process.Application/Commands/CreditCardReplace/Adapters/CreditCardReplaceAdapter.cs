using System;
using System.Reflection;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Proxy.Rtdx.OrderNewPlastic.Messages;

namespace System.Process.Application.Commands.CreditCardReplace.Adapters
{
    public class CreditCardReplaceAdapter
    {
        #region Properties
        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        #region Constructor

        public CreditCardReplaceAdapter(ProcessConfig config)
        {
            ProcessConfig = config;
        }

        public CreditCardReplaceAdapter()
        {

        }

        public OrderNewPlasticParams Adapt(Card input, string token)
        {
            var orderNewPlasticParams = new OrderNewPlasticParams
            {
                SecurityToken = token,
                AccountNumber = input.Pan,
                PlasticTypeTwo = ProcessConfig.RtdxParamsConfig.OrderNewPlastic.PlasticTypeTwo,
                NumberCardTwoIssueNameTwo = ProcessConfig.RtdxParamsConfig.OrderNewPlastic.NumberCardTwoIssueNameTwo,
                CreatePlastics = ProcessConfig.RtdxParamsConfig.OrderNewPlastic.CreatePlastics,
                AddressChangeWarning = ProcessConfig.RtdxParamsConfig.OrderNewPlastic.AddressChangeWarning
            };

            orderNewPlasticParams = FillProps(orderNewPlasticParams);

            return orderNewPlasticParams;
        }

        private OrderNewPlasticParams FillProps(OrderNewPlasticParams orderNewPlasticParams)
        {
            PropertyInfo[] properties = orderNewPlasticParams.GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyType == typeof(string) && (String)propertyInfo.GetValue(orderNewPlasticParams, null) == null)
                {
                    propertyInfo.SetValue(orderNewPlasticParams, String.Empty, null);
                }
            }

            return orderNewPlasticParams;
        }

        #endregion
    }
}