using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MRShop.API.Models;

public class Address
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("country")]
    public string Country { get; set; } = "Bangladesh";

    [BsonElement("division")]
    public string? Division { get; set; }

    [BsonElement("district")]
    public string? District { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("area")]
    public string? Area { get; set; }

    [BsonElement("postalCode")]
    public string? PostalCode { get; set; }

    [BsonElement("addressLine")]
    public string AddressLine { get; set; } = string.Empty;

    [BsonElement("addressType")]
    public string AddressType { get; set; } = "home"; // home, work, other

    [BsonElement("isDefault")]
    public bool IsDefault { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
