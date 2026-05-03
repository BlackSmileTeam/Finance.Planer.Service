namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to create a category.</para>
/// </summary>
public sealed class CreateCategoryRequest
{
    /// <summary>
    /// <para>Gets or sets the visible name.</para>
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// <para>Gets or sets the preferred color.</para>
    /// </summary>
    public string HexColor { get; init; } = "#3B82F6";

    /// <summary>
    /// <para>Gets or sets the icon identifier.</para>
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// <para>Gets or sets the parent category identifier (for subcategories).</para>
    /// </summary>
    public Guid? ParentId { get; init; }
}

