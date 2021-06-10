namespace System.Process.IntegrationTests.Backend.Api.Process
{
    public class ConsultProcessTests
    {
        //#region Properties

        //private static ServiceProvider Provider;
        //private static Mock<IHttpContextAccessor> HttpContextAccessor;
        //private static ServiceCollection Services;

        //#endregion

        //#region Constructor
        //static ConsultProcessTests()
        //{                    
        //    Services = new ServiceCollection();
        //    Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
        //       .AddFluentValidation(options =>
        //       {
        //           options.RegisterValidatorsFromAssemblyContaining<ValidateAccountIdAttribute>();
        //       });
        //    Services.AddMediator();
        //    Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
        //    Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

        //    Provider = Services.BuildServiceProvider();
        //}

        //#endregion

        //#region Main Flow

        //[Fact(DisplayName = "AcctStat = 0 | 9 - Should Do Not Return The Account")]
        //public async Task ShouldDoNotReturnTheAccount()
        //{
        //    var operation = new Mock<IInquiryOperation>();
        //    var response = await Task.FromResult(new ProcessearchResponse
        //    {
        //        ProcessearchRecInfo = new List<ProcessearchRecInfo>
        //        {
        //            new ProcessearchRecInfo
        //            { 
        //                Processtatus = "0",
        //                ProcesstatusDesc = "Escheat"
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "9",
        //                ProcesstatusDesc = "No credits"
        //            }
        //        }
        //    });
        //    operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).ReturnsAsync(response);
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();
        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var cancellationToken = new CancellationToken();

        //    var result = await accountController.ConsultProcess("string", cancellationToken);
        //    var objectResult = Assert.IsType<OkObjectResult>(result);
        //    var consultProcessResponse = Assert.IsAssignableFrom<ConsultProcessByCustomerIdResponse>(objectResult.Value);

        //    Assert.Null(consultProcessResponse.ProcessearchRecords);
        //}

        //[Fact(DisplayName = "AcctStat = 1 | 3 | 4 | 5 | 6 | 7 - Should Return The Account")]
        //public async Task ShouldReturnTheAccount()
        //{
        //    var operation = new Mock<IInquiryOperation>();
        //    var response = await Task.FromResult(new ProcessearchResponse
        //    {
        //        ProcessearchRecInfo = new List<ProcessearchRecInfo>
        //        {
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "1",
        //                ProcesstatusDesc = "Active",
        //                Amount = 123456,
        //                AvailableBalance = 123456
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "3",
        //                ProcesstatusDesc = "Dormant",
        //                Amount = 123456,
        //                AvailableBalance = 123456
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "4",
        //                ProcesstatusDesc = "NewToday",
        //                Amount = 123456,
        //                AvailableBalance = 123456
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "5",
        //                ProcesstatusDesc = "PendingClosed",
        //                Amount = 123456,
        //                AvailableBalance = 123456
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "6",
        //                ProcesstatusDesc = "Restricted",
        //                Amount = 123456,
        //                AvailableBalance = 123456
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "7",
        //                ProcesstatusDesc = "NoPost",
        //                Amount = 123456,
        //                AvailableBalance = 123456
        //            }
        //        }
        //    });
        //    operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).ReturnsAsync(response);
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();

        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var cancellationToken = new CancellationToken();

        //    var result = await accountController.ConsultProcess("string", cancellationToken);
        //    var objectResult = Assert.IsType<OkObjectResult>(result);
        //    var consultProcessResponse = Assert.IsAssignableFrom<ConsultProcessByCustomerIdResponse>(objectResult.Value);
        //    var maximumRecords = response.ProcessearchRecInfo.Count();

        //    Assert.Equal(maximumRecords, consultProcessResponse.ProcessearchRecords.Count());
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.CurrentBalance != null).Count().Should().Equals(maximumRecords);
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.AvailableBalance != null).Count().Should().Equals(maximumRecords);
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.CurrentBalanceCurrency!= null).Count().Should().Equals(maximumRecords);
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.AvailableBalanceCurrency != null).Count().Should().Equals(maximumRecords);
        //}

        //[Fact(DisplayName = "AcctStat = 2 | 8 - Should Return The Account But Omitting Balance Information")]
        //public async Task ShouldReturnTheAccountOmittingBalanceInformation()
        //{
        //    var operation = new Mock<IInquiryOperation>();
        //    var response = await Task.FromResult(new ProcessearchResponse
        //    {
        //        ProcessearchRecInfo = new List<ProcessearchRecInfo>
        //        {
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "2",
        //                ProcesstatusDesc = "Closed"
        //            },
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "8",
        //                ProcesstatusDesc = "ChargedOff"
        //            }
        //        }
        //    });
        //    operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).ReturnsAsync(response);
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();
        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var cancellationToken = new CancellationToken();

        //    var result = await accountController.ConsultProcess("string", cancellationToken);
        //    var objectResult = Assert.IsType<OkObjectResult>(result);
        //    var consultProcessResponse = Assert.IsAssignableFrom<ConsultProcessByCustomerIdResponse>(objectResult.Value);
        //    var maximumRecords = response.ProcessearchRecInfo.Count();

        //    Assert.Equal(maximumRecords, consultProcessResponse.ProcessearchRecords.Count());
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.CurrentBalance is null).Count().Should().Equals(maximumRecords);
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.CurrentBalanceCurrency is null).Count().Should().Equals(maximumRecords);
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.AvailableBalance is null).Count().Should().Equals(maximumRecords);
        //    consultProcessResponse.ProcessearchRecords.Where(x => x.AvailableBalanceCurrency is null).Count().Should().Equals(maximumRecords);
        //}

        //#endregion

        //#region Alternative Flows

        //[Fact(DisplayName = "AF1 – Required Information Not Provided - Should Returns Errors", Skip = "true")]
        //public void ShouldIndicatesWhichFieldsIsNotProperlyFulfilled()
        //{
        //    var httpContext = new DefaultHttpContext();
        //    var routeData = new RouteData();
        //    var action = typeof(ProcessController).GetMethod(nameof(ProcessController.ConsultProcess));
        //    var actionDescriptor = new ControllerActionDescriptor()
        //    {
        //        ActionName = action.Name,
        //        ControllerName = typeof(ProcessController).Name,
        //        DisplayName = action.Name,
        //        MethodInfo = action,
        //    };
        //    var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        //    var operation = new Mock<IInquiryOperation>();
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();
        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var atrributeFilter = action.GetCustomAttribute<ValidateAccountIdAttribute>();
        //    var actionArguments = new Dictionary<string, object>
        //    {
        //        ["customerId"] = string.Empty
        //    };
        //    var filterMetadata = new List<IFilterMetadata>() { atrributeFilter };
        //    var actionExecutedContext = new ActionExecutingContext(actionContext, filterMetadata, actionArguments, accountController);
        //    var attribute = new ValidateAccountIdAttribute();

        //    attribute.OnActionExecuting(actionExecutedContext);

        //    actionExecutedContext.Result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>("The CustomerId cannot be null or empty. Please ensure you have entered a valid CustomerId ");
        //}

        //[Fact(DisplayName = "Should Not Return Any Error")]
        //public void ShouldNotReturnAnyError()
        //{
        //    var httpContext = new DefaultHttpContext();
        //    var routeData = new RouteData();
        //    var action = typeof(ProcessController).GetMethod(nameof(ProcessController.ConsultProcess));
        //    var actionDescriptor = new ControllerActionDescriptor()
        //    {
        //        ActionName = action.Name,
        //        ControllerName = typeof(ProcessController).Name,
        //        DisplayName = action.Name,
        //        MethodInfo = action,
        //    };
        //    var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        //    var operation = new Mock<IInquiryOperation>();
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();
        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var atrributeFilter = action.GetCustomAttribute<ValidateAccountIdAttribute>();
        //    var actionArguments = new Dictionary<string, object>
        //    {
        //        ["customerId"] = "string"
        //    };
        //    var filterMetadata = new List<IFilterMetadata>() { atrributeFilter };
        //    var actionExecutedContext = new ActionExecutingContext(actionContext, filterMetadata, actionArguments, accountController);
        //    var attribute = new ValidateAccountIdAttribute();

        //    attribute.OnActionExecuting(actionExecutedContext);

        //    actionExecutedContext.Result.Should().BeNull();
        //}

        //#endregion

        //#region Exception Flows

        //[Fact(DisplayName = "EF1 – Failed Connection With Jack Henry - Should Throws Exception")]
        //public async Task ShouldThrowsNotFoundException()
        //{
        //    var operation = new Mock<IInquiryOperation>();
        //    operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).Throws(new Exception());
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = ConsultProcessTests.GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();
        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var cancellationToken = new CancellationToken();

        //    await Assert.ThrowsAsync<NotFoundException>(() => accountController.ConsultProcess("string", cancellationToken));
        //}

        //[Fact(DisplayName = "EF2 – No connection with Core Banking - Should Throws Exception")]
        //public async Task ShouldThrowsArgumentException()
        //{
        //    var operation = new Mock<IInquiryOperation>();
        //    var response = await Task.FromResult(new ProcessearchResponse
        //    {
        //        ProcessearchRecInfo = new List<ProcessearchRecInfo>
        //        {
        //            new ProcessearchRecInfo
        //            {
        //                Processtatus = "X",
        //                ProcesstatusDesc = "Closed"
        //            }
        //        }
        //    });
        //    operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>())).ReturnsAsync(response);
        //    Services.AddSingleton(operation.Object);
        //    Provider = Services.BuildServiceProvider();
        //    var mediator = ConsultProcessTests.GetInstance<IMediator>();
        //    var logger = new Mock<ILogger<ProcessController>>();
        //    var accountController = new ProcessController(mediator, logger.Object);
        //    var cancellationToken = new CancellationToken();

        //    await Assert.ThrowsAsync<ArgumentException>(() => accountController.ConsultProcess("string", cancellationToken));
        //}

        //#endregion

        //#region Methods

        //public static T GetInstance<T>()
        //{
        //    T result = Provider.GetRequiredService<T>();
        //    ControllerBase controllerBase = result as ControllerBase;
        //    if (controllerBase != null)
        //    {
        //        SetControllerContext(controllerBase);
        //    }
        //    Controller controller = result as Controller;
        //    if (controller != null)
        //    {
        //        SetControllerContext(controller);
        //    }
        //    return result;
        //}

        //private static void SetControllerContext(Controller controller)
        //{
        //    controller.ControllerContext = new ControllerContext
        //    {
        //        HttpContext = HttpContextAccessor.Object.HttpContext
        //    };
        //}

        //private static void SetControllerContext(ControllerBase controller)
        //{
        //    controller.ControllerContext = new ControllerContext
        //    {
        //        HttpContext = HttpContextAccessor.Object.HttpContext
        //    };
        //}

        //#endregion
    }
}
