using System.Process.Application.AuthorizationHandlers;
using System.Process.Domain.Enums;
using Xunit;

namespace System.Process.UnitTests.Application.AuthorizationHandlers
{
    public class UserDataRequirementTests
    {
        #region Properties
        public ValidationProcess Validation { get; set; }
        public UserDataRequirement Requirement { get; set; }
        #endregion

        #region Constructor
        public UserDataRequirementTests()
        {
            Validation = new ValidationProcess();
            Validation = ValidationProcess.ValidateCardOwner;
            Requirement = new UserDataRequirement(Validation);
        }
        #endregion

        #region Tests
        [Trait("Unit", "Success")]
        [Fact(DisplayName = "Should Send User Data Requirement Success")]
        public void ShouldSendUserDataRequirementSuccess()
        {
            //Arrange
            Validation = new ValidationProcess();
            Validation = ValidationProcess.ValidateCardOwner;
            Validation = ValidationProcess.ValidateRemoteDeposit;
            Validation = ValidationProcess.ValidateSystemId;
            Validation = ValidationProcess.ValidateTransfer;

            //Act
            Requirement = new UserDataRequirement(Validation);

            //Assert
            Assert.NotNull(Requirement);
        }
        #endregion
    }
}
