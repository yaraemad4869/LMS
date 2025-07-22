using LearningManagementSystem.Data.Enum;
using LearningManagementSystem.Data.Enum;
using LearningManagementSystem.IUOW;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;

[ApiController]
[Route("api/payments/paypal")]
public class PayPalController : ControllerBase
{
    private readonly PayPalHttpClient _payPalClient;
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;

    public PayPalController(PayPalHttpClient payPalClient, IConfiguration config, IUnitOfWork unitOfWork)
    {
        _payPalClient = payPalClient;
        _unitOfWork = unitOfWork;
        _config = config;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] LearningManagementSystem.Models.Order request)
    {
        var order = new LearningManagementSystem.Models.Order
        {
            CheckoutPaymentIntent = "CAPTURE",
            ApplicationContext = new ApplicationContext
            {
                ReturnUrl = $"{_config["FrontendUrl"]}/payment-success",
                CancelUrl = $"{_config["FrontendUrl"]}/payment-canceled",
                BrandName = "LMS"
            },
            PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = "EGP",
                        Value = request.TotalPrice.ToString("0.00")
                    },
                    Description = request.Description
                }
            }
        };

        var requestPayPal = new OrdersCreateRequest();
        requestPayPal.Prefer("return=representation");
        requestPayPal.RequestBody(order);

        try
        {
            var response = await _payPalClient.Execute(requestPayPal);
            return Ok(response.Result<LearningManagementSystem.Models.Order>());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("capture-order")]
    public async Task<IActionResult> CaptureOrder([FromBody] string orderId)
    {
        User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
        if(user != null){
            var request = new OrdersCaptureRequest(orderId);
            request.Prefer("return=representation");
            request.RequestBody(new OrderActionRequest());

            try
            {
                var response = await _payPalClient.Execute(request);
                var result = response.Result<LearningManagementSystem.Models.Order>();

                result.PaymentStatus = PaymentStatus.Completed;
                result.OrderStatus = OrderStatus.Completed;
                result.UserId=user.Id;
                await _unitOfWork.CompleteAsync();
                user.Courses.AddRange(result.Courses);
                await _unitOfWork.CompleteAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        return Unauthorized();
    }
}