namespace FairHire.Domain;

public sealed class User
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }


    public IDictionary<string, string> SocialNetworks = new Dictionary<string, string>();
}
