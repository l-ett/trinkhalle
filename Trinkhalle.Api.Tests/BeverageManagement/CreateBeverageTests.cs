using FluentAssertions;
using FluentValidation.TestHelper;
using Trinkhalle.Api.BeverageManagement;
using Trinkhalle.Api.BeverageManagement.UseCases;

namespace Trinkhalle.Api.Tests.BeverageManagement;

public class CreateBeverageTests : IClassFixture<BeverageManagementFixture>
{
    private BeverageManagementFixture _fixture;

    public CreateBeverageTests(BeverageManagementFixture beverageManagementFixture)
    {
        _fixture = beverageManagementFixture;
    }

    [Fact]
    public void CreateBeverageCommandValidator_CommandWithDefaultValues_ShouldHaveError()
    {
        //arrange
        var validator = new CreateBeverage.CreateBeverageCommandValidator();
        var command = new CreateBeverageCommand() { };

        //act
        var result = validator.TestValidate(command);

        //assert
        result.ShouldHaveValidationErrorFor(person => person.Available);
        result.ShouldHaveValidationErrorFor(person => person.Name);
        result.ShouldHaveValidationErrorFor(person => person.ImageUrl);
    }

    [Fact]
    public void CreateBeverageCommandValidator_NegativePrice_ShouldHaveError()
    {
        //arrange
        var validator = new CreateBeverage.CreateBeverageCommandValidator();
        var command = new CreateBeverageCommand() { Price = -1 };

        //act
        var result = validator.TestValidate(command);

        //assert
        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void CreateBeverageCommandValidator_ValidCommand_ShouldHaveNoError()
    {
        //arrange
        var validator = new CreateBeverage.CreateBeverageCommandValidator();
        var command = new CreateBeverageCommand()
            { Available = true, Name = "Test", Price = 5, ImageUrl = "abc.com" };

        //act
        var result = validator.TestValidate(command);

        //assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateBeverageCommandValidator_PriceZero_ShouldHaveNoError()
    {
        //arrange
        var validator = new CreateBeverage.CreateBeverageCommandValidator();
        var command = new CreateBeverageCommand()
            { Available = true, Name = "Test", Price = 0, ImageUrl = "abc.com" };

        //act
        var result = validator.TestValidate(command);

        //assert
        result.ShouldNotHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public async Task CreateBeverageCommandHandler_ValidCommand_ShouldReturnId()
    {
        //arrange
        var command = new CreateBeverageCommand()
            { Available = true, Name = "Test", Price = 0, ImageUrl = "abc.com" };

        //act
        var result = await _fixture.SendAsync(command);

        //assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}