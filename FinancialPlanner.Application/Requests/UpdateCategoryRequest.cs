using System.Text.Json.Serialization;

namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to update an existing category.</para>
/// </summary>
public sealed class UpdateCategoryRequest
{
    /// <summary>
    /// <para>Gets or sets the new name.</para>
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// <para>Gets or sets the color.</para>
    /// </summary>
    [JsonPropertyName("hexColor")]
    public required string HexColor { get; init; }

    /// <summary>
    /// <para>Gets or sets the icon identifier.</para>
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    /// <summary>
    /// <para>Gets or sets the active state. When null, the existing value is preserved.</para>
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; init; }

    /// <summary>
    /// <para>Gets or sets the parent category identifier (null for top-level category).</para>
    /// </summary>
    [JsonPropertyName("parentId")]
    public Guid? ParentId { get; init; }
}

