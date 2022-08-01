using FluentAssertions;
using Trinkhalle.DrinkManagement.Domain;

namespace Trinkhalle.DrinkManagement.Tests;

public class DrinkTests
{
    [Fact]
    public void Ctor_ValidValues_ShouldCreateInstanceWithSameValues()
    {
        //arrange
        var id = Guid.NewGuid();
        var name = "Cola";
        var price = new decimal(1.33);
        var imageUrl = "http://trinkhalle.com/image.png";
        var available = true;

        //act
        var beverage = new Drink(id, price, "Cola", imageUrl, true);

        //assert
        beverage.Id.Should().Be(id);
        beverage.Price.Should().Be(price);
        beverage.Name.Should().Be(name);
        beverage.ImageUrl.Should().Be(imageUrl);
        beverage.Available.Should().Be(available);
    }

    [Fact]
    public void Ctor_ValidValues_PartitionKeyShouldBeId()
    {
        //arrange
        var id = Guid.NewGuid();
        var name = "Cola";
        var price = new decimal(1.33);
        var imageUrl = "http://trinkhalle.com/image.png";
        var available = true;

        //act
        var beverage = new Drink(id, price, name, imageUrl, available);

        //assert
        beverage.PartitionKey.Should().Be(id.ToString());
    }

    [Fact]
    public void Ctor_EmptyGuid_ShouldThrowArguementException()
    {
        //arrange
        var id = Guid.Empty;

        //act
        Action ctor = () => new Drink(id, Decimal.One, "name", "image_url", true);

        //assert
        ctor.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_EmptyName_ShouldThrowArguementException()
    {
        //arrange
        var name = string.Empty;

        //act
        Action ctor = () => new Drink(Guid.NewGuid(), Decimal.One, name, "image_url", true);

        //assert
        ctor.Should().Throw<ArgumentException>();
    }
}