using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyConsent.Data.Entities;

[Table("owner", Schema = "data_inventory")]
public class Owner
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("owner_type_id")]
    public int? OwnerTypeId { get; set; }

    [Column("registration_country_id")]
    public int? CountryId { get; set; }

    [Column("owner_rank")]
    public int? OwnerRank { get; set; }

    [Column("default_language")]
    public string DefaultLanguage { get; set; } = "en";

    public ICollection<Product> Products { get; set; } = [];
}

[Table("product", Schema = "data_inventory")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("owner_id")]
    public int OwnerId { get; set; }

    [Column("product_group_id")]
    public int? ProductGroupId { get; set; }

    [Column("connect_id_name")]
    public string? ConnectIdName { get; set; }

    [Column("product_rank")]
    public int? ProductRank { get; set; }

    public Owner? Owner { get; set; }
}

[Table("purpose_category", Schema = "data_inventory")]
public class PurposeCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("use_case", Schema = "data_inventory")]
public class UseCase
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("purpose_category_id")]
    public int? PurposeCategoryId { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("state_id")]
    public int? StateId { get; set; }
}

[Table("legal_basis", Schema = "data_inventory")]
public class LegalBasis
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("is_legitimate_interest")]
    public bool? IsLegitimateInterest { get; set; }
}
