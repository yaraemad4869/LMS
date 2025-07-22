using Microsoft.Extensions.Configuration;
using PayPal;
using PayPal.Api;
using System;
using System.Collections.Generic;
namespace LearningManagementSystem.Services
{
        public class PayPalService
        {
            private readonly IConfiguration _configuration;

            public PayPalService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            private APIContext GetAPIContext()
            {
                var clientId = "Ac3koYF-kGg55nvORbYc1sCKbPP1Fi-4_yEocZH7S89rl5gykLbrBg6eG4sTWQeahAoQlRpouI4yVbfB";
                var clientSecret = "EF5ESeLFDkoIKZ1GgNZbhgPolxVGkQnp2t-zPW7HWeDHL9fQPhavz5V0YS12evwdvO6i8t79L27pOJqW";

                var accessToken = new OAuthTokenCredential(clientId, clientSecret).GetAccessToken();
                return new APIContext(accessToken);
            }





            public PayPal.Api.Payment CreatePayment(string redirectUrl)
            {
                var apiContext = GetAPIContext();

                var payment = new PayPal.Api.Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        description = "Transaction description.",
                        invoice_number = System.Guid.NewGuid().ToString(),
                        amount = new Amount
                        {
                            currency = "USD",
                            total = "10.00" // Amount to charge
                        }
                    }
                },
                    redirect_urls = new RedirectUrls
                    {
                        cancel_url = redirectUrl,
                        return_url = redirectUrl
                    }
                };
                var createdPayment = payment.Create(apiContext);
                return createdPayment;
            }
        }
    }
