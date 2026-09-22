namespace CulinaryBlog.Domain.Enums;

/// <summary>
/// Mức độ khó khi thực hiện công thức nấu ăn theo đặc tả SRS v1.2.0.
/// </summary>
public enum RecipeDifficulty
{
    /// <summary>
    /// Mức độ dễ, dành cho người mới bắt đầu.
    /// </summary>
    Easy = 1,

    /// <summary>
    /// Mức độ trung bình, yêu cầu kỹ năng nấu nướng cơ bản.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Mức độ khó, đòi hỏi kỹ thuật và thời gian chuẩn bị phức tạp.
    /// </summary>
    Hard = 3,

    /// <summary>
    /// Mức độ chuyên gia, dành cho đầu bếp chuyên nghiệp.
    /// </summary>
    Expert = 4
}