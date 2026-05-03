namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a category ready to be sent to client applications.</para>
/// </summary>
public sealed record CategoryDto
{
    /// <summary>
    /// <para>Gets or sets the unique identifier.</para>
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// <para>Gets or sets the visible name.</para>
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// <para>Gets or sets the hex color code.</para>
    /// </summary>
    public required string HexColor { get; init; }

    /// <summary>
    /// <para>Gets or sets the icon identifier.</para>
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// <para>Gets or sets the parent category identifier (for subcategories).</para>
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// <para>Gets or sets the subcategories.</para>
    /// </summary>
    public IReadOnlyCollection<CategoryDto>? Subcategories { get; init; }

    /// <summary>
    /// <para>Gets or sets a flag describing whether the category is still active.</para>
    /// </summary>
    public bool IsActive { get; init; }
}

