using Trinkhalle.Api.Shared.Abstractions;

namespace Trinkhalle.Api.CustomerManagement.Domain;

public class User : Aggregate
{
    public User(string email, string firstname, string lastname)
    {
        Email = email;
        Firstname = firstname;
        Lastname = lastname;
        PartitionKey = Id.ToString();
    }

    public string Email { get; private set; }
    public string Firstname { get; private set; }
    public string Lastname { get; private set; }
}