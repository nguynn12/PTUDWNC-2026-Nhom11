namespace CulinaryBlog.Infrastructure.Seeders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Lớp sinh dữ liệu chi tiết công thức nấu ăn: Nguyên liệu (RecipeIngredient),
/// Các bước thực hiện (RecipeStep) và Hình ảnh minh họa (RecipeImage).
/// Đáp ứng đầy đủ yêu cầu Lab 2 cho Thành viên 4:
/// - Mỗi Recipe có ít nhất 10 nguyên liệu, thứ tự tăng dần.
/// - Mỗi Recipe có ít nhất 5 bước chế biến, StepNumber tăng dần liên tục.
/// - Mỗi Recipe có ít nhất 1 ảnh chính và các ảnh phụ gallery.
/// </summary>
public static class RecipeDetailSeeder
{
    // Ngân hàng nguyên liệu thực phẩm Việt Nam phong phú
    public static readonly (string Name, string Unit, string? Notes)[] Proteins =
    [
        ("Thịt bò thăn", "gram", "thái lát mỏng ngang thớ"),
        ("Thịt ba chỉ heo", "gram", "rửa sạch, cắt miếng vuông vừa ăn"),
        ("Thịt đùi gà ta", "gram", "chặt miếng vừa ăn, để ráo"),
        ("Cá lóc đồng", "gram", "làm sạch vảy, khứa nhẹ thân cá"),
        ("Tôm sú tươi", "gram", "rút chỉ đen trên lưng, bóc vỏ bỏ đầu"),
        ("Mực ống tươi", "gram", "làm sạch túi mực, khứa vảy rồng"),
        ("Sườn non heo", "gram", "chặt khúc 3cm, chần sơ nước sôi"),
        ("Đậu phụ mơ", "bìa", "cắt khối vuông 2cm, chiên vàng sơ"),
        ("Nấm đùi gà", "gram", "ngâm nước muối loãng, cắt lát chéo"),
        ("Trứng gà ta", "quả", "để ở nhiệt độ phòng trước khi nấu")
    ];

    public static readonly (string Name, string Unit, string? Notes)[] Vegetables =
    [
        ("Hành tây Đà Lạt", "củ", "bóc vỏ, bổ múi cau mỏng"),
        ("Cà rốt tươi", "củ", "gọt vỏ, tỉa hoa hoặc cắt khúc tròn"),
        ("Cà chua chín mọng", "quả", "rửa sạch, bổ múi cau"),
        ("Nấm hương khô", "gram", "ngâm nước ấm cho nở mềm, cắt chân"),
        ("Giá đỗ sạch", "gram", "nhặt bỏ rễ sâu, rửa sạch để ráo"),
        ("Bắp ngọt", "bắp", "lột vỏ, cắt khúc 3-4cm"),
        ("Khoai tây vàng", "củ", "gọt vỏ, ngâm nước muối tránh thâm"),
        ("Ớt chuông đỏ", "quả", "bỏ hạt, thái miếng vuông vừa ăn"),
        ("Bông cải xanh", "gram", "tách nhánh nhỏ, ngâm rửa sạch"),
        ("Măng tươi luộc", "gram", "tước sợi, luộc kỹ 2 lần với muối")
    ];

    public static readonly (string Name, string Unit, string? Notes)[] Aromatics =
    [
        ("Hành tím", "củ", "bóc vỏ, băm nhuyễn"),
        ("Tỏi cô đơn", "tép", "đập dập, băm nhỏ"),
        ("Sả tươi", "nhánh", "đập dập, cắt khúc 5cm"),
        ("Gừng tươi", "nhánh", "cạo vỏ, thái sợi chỉ nhỏ"),
        ("Hành lá", "nhánh", "rửa sạch, phần đầu băm nhỏ, lá cắt khúc"),
        ("Ngò rí (rau mùi)", "nhánh", "rửa sạch, cắt nhỏ để trang trí"),
        ("Ớt hiểm đỏ", "trái", "bỏ cuống, thái lát chéo"),
        ("Rau húng quế", "nhánh", "ngắt lấy ngọn non, rửa sạch"),
        ("Lá chanh tươi", "lá", "rửa sạch, vò nhẹ hoặc thái chỉ mỏng"),
        ("Tiêu xanh Phú Quốc", "nhánh", "đập dập nhẹ cho dậy mùi")
    ];

    public static readonly (string Name, string Unit, string? Notes)[] Seasonings =
    [
        ("Nước mắm cá cơm Phú Quốc", "thìa canh", "loại 40 độ đạm truyền thống"),
        ("Hạt nêm thịt thăn xương ống", "thìa cà phê", "nêm vừa khẩu vị"),
        ("Đường phèn kết tinh", "thìa cà phê", "giúp món ăn ngọt thanh"),
        ("Hạt tiêu đen xay nhuyễn", "thìa cà phê", "rắc thơm lúc tắt bếp"),
        ("Dầu hào Maggi", "thìa canh", "tạo độ sánh bóng và đậm đà"),
        ("Dầu màu điều", "thìa canh", "tạo màu vàng cam óng ả bắt mắt"),
        ("Dầu ăn thực vật", "thìa canh", "dùng để phi thơm hương liệu"),
        ("Giấm gạo lên men", "thìa canh", "tạo độ chua dịu cân bằng"),
        ("Nước cốt dừa xiêm", "ml", "tạo vị béo bùi ngậy đặc trưng"),
        ("Nước tương đậu nành", "thìa canh", "dùng nêm nếm gia tăng hương đậu")
    ];

    // Ngân hàng các bước nấu ăn chi tiết
    public static readonly (string Title, string Description, int Duration)[] StepTemplates =
    [
        (
            "Sơ chế và làm sạch nguyên liệu",
            "Rửa sạch toàn bộ rau củ dưới vòi nước chảy. Thịt và hải sản được rửa cùng muối hạt và vài lát gừng tươi để khử hoàn toàn mùi tanh, sau đó dùng khăn giấy chuyên dụng thấm khô ráo bề mặt rồi cắt thái theo đúng kích thước hướng dẫn.",
            15
        ),
        (
            "Tẩm ướp gia vị nền tảng",
            "Cho phần thịt/cá vào tô lớn cùng 1 thìa canh nước mắm truyền thống, 1 thìa cà phê hạt nêm, tiêu đen xay, hành tím và tỏi đã băm nhuyễn. Trộn đều nhẹ tay và để nghỉ khoảng 20 phút cho gia vị ngấm sâu vào từng thớ thịt.",
            20
        ),
        (
            "Phi thơm hương liệu kích vị",
            "Đặt chảo sâu lòng hoặc nồi gang lên bếp, bật lửa vừa và cho 2 thìa canh dầu ăn vào đun nóng. Cho sả đập dập, gừng thái sợi và phần hành tỏi còn lại vào đảo đều tay đến khi dậy mùi thơm ngào ngạt và chuyển sang màu vàng óng.",
            5
        ),
        (
            "Xào săn nguyên liệu chính",
            "Trút toàn bộ phần thịt/hải sản đã ướp vào chảo ở mức lửa lớn. Đảo nhanh và dứt khoát trong 5-7 phút cho đến khi bề mặt ngoài săn chắc lại, giữ trọn vẹn nước ngọt tự nhiên bên trong.",
            10
        ),
        (
            "Tiến hành ninh nấu và hầm chín",
            "Đổ nước dùng xương hầm (hoặc nước dừa tươi) vào ngập xăm xắp mặt thức ăn, đun sôi bùng rồi hạ nhỏ lửa liu riu. Vớt sạch bọt trắng nổi lên để nước được trong veo, đậy vung hầm khoảng 25-30 phút đến khi nguyên liệu đạt độ mềm mọng hoàn hảo.",
            30
        ),
        (
            "Phối trộn rau củ và hoàn thiện sốt",
            "Cho tiếp các loại rau củ lâu chín như cà rốt, bắp ngọt vào nồi nấu trước 7 phút, sau đó cho nấm và hành tây vào đảo nhẹ. Nêm nếm lại nước dùng cho thật vừa vặn khẩu vị gia đình.",
            10
        ),
        (
            "Trình bày đĩa ăn và thưởng thức",
            "Tắt bếp, rắc đều tiêu đen xay mịn, hành lá, ngò rí và ớt cắt lát lên bề mặt. Múc món ăn ra tô sứ sâu lòng hoặc đĩa lớn, dùng kèm cơm trắng nóng dẻo hoặc bún tươi khi món ăn còn nghi ngút khói.",
            5
        )
    ];

    /// <summary>
    /// Sinh danh sách nguyên liệu cho một công thức (đảm bảo >= 10 nguyên liệu, thứ tự tăng dần).
    /// </summary>
    public static List<RecipeIngredient> GenerateIngredients(Guid recipeId, Random random)
    {
        var ingredients = new List<RecipeIngredient>();
        int targetCount = random.Next(10, 14); // Tối thiểu 10, tối đa 13 nguyên liệu
        int orderIndex = 1;

        // Đạm
        var chosenProteins = Proteins.OrderBy(_ => random.Next()).Take(random.Next(1, 3));
        foreach (var item in chosenProteins)
        {
            ingredients.Add(new RecipeIngredient
            {
                RecipeId = recipeId,
                Name = item.Name,
                Quantity = random.Next(2, 6) * 100m,
                Unit = item.Unit,
                Notes = item.Notes,
                OrderIndex = orderIndex++
            });
        }

        // Rau củ
        var chosenVegetables = Vegetables.OrderBy(_ => random.Next()).Take(random.Next(3, 5));
        foreach (var item in chosenVegetables)
        {
            ingredients.Add(new RecipeIngredient
            {
                RecipeId = recipeId,
                Name = item.Name,
                Quantity = random.Next(1, 4),
                Unit = item.Unit,
                Notes = item.Notes,
                OrderIndex = orderIndex++
            });
        }

        // Hương liệu & rau thơm
        var chosenAromatics = Aromatics.OrderBy(_ => random.Next()).Take(random.Next(2, 4));
        foreach (var item in chosenAromatics)
        {
            ingredients.Add(new RecipeIngredient
            {
                RecipeId = recipeId,
                Name = item.Name,
                Quantity = random.Next(2, 6),
                Unit = item.Unit,
                Notes = item.Notes,
                OrderIndex = orderIndex++
            });
        }

        // Gia vị nêm nếm bổ sung đảm bảo tổng số lượng >= 10
        var chosenSeasonings = Seasonings.OrderBy(_ => random.Next()).ToList();
        int remainingNeeded = Math.Max(0, targetCount - ingredients.Count);
        for (int k = 0; k < remainingNeeded && k < chosenSeasonings.Count; k++)
        {
            var item = chosenSeasonings[k];
            bool isAsNeeded = (k == 0 && random.Next(2) == 0);

            ingredients.Add(new RecipeIngredient
            {
                RecipeId = recipeId,
                Name = item.Name,
                Quantity = isAsNeeded ? null : random.Next(1, 3),
                Unit = isAsNeeded ? null : item.Unit,
                Notes = isAsNeeded ? "nêm nếm vừa đủ theo khẩu vị" : item.Notes,
                OrderIndex = orderIndex++
            });
        }

        return ingredients;
    }

    /// <summary>
    /// Sinh danh sách các bước nấu cho một công thức (đảm bảo >= 5 bước, StepNumber tăng dần từ 1).
    /// </summary>
    public static List<RecipeStep> GenerateSteps(Guid recipeId, string recipeTitle)
    {
        var steps = new List<RecipeStep>();
        int stepCount = 5 + (Math.Abs(recipeTitle.GetHashCode()) % 3);

        for (int s = 1; s <= stepCount; s++)
        {
            var template = StepTemplates[(s - 1) % StepTemplates.Length];
            steps.Add(new RecipeStep
            {
                RecipeId = recipeId,
                StepNumber = s,
                Title = $"Bước {s}: {template.Title}",
                Description = $"{template.Description} Áp dụng cho món {recipeTitle}.",
                DurationMinutes = template.Duration,
                ImageUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/steps/step_{s}.jpg",
                ParentStepId = null
            });
        }

        return steps;
    }

    /// <summary>
    /// Sinh danh sách hình ảnh minh họa cho một công thức (1 ảnh chính + các ảnh phụ).
    /// </summary>
    public static List<RecipeImage> GenerateImages(Guid recipeId, string recipeTitle)
    {
        return
        [
            new RecipeImage
            {
                RecipeId = recipeId,
                OriginalUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/primary.jpg",
                MediumUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/medium_primary.jpg",
                ThumbnailUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/thumb_primary.jpg",
                AltText = $"Hình ảnh đại diện món ăn {recipeTitle}",
                IsPrimary = true,
                OrderIndex = 0
            },
            new RecipeImage
            {
                RecipeId = recipeId,
                OriginalUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/gallery_1.jpg",
                MediumUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/medium_gallery_1.jpg",
                ThumbnailUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/thumb_gallery_1.jpg",
                AltText = $"Công đoạn sơ chế nguyên liệu món {recipeTitle}",
                IsPrimary = false,
                OrderIndex = 1
            },
            new RecipeImage
            {
                RecipeId = recipeId,
                OriginalUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/gallery_2.jpg",
                MediumUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/medium_gallery_2.jpg",
                ThumbnailUrl = $"https://storage.culinaryblog.com/recipes/{recipeId}/thumb_gallery_2.jpg",
                AltText = $"Thành phẩm thơm ngon món {recipeTitle}",
                IsPrimary = false,
                OrderIndex = 2
            }
        ];
    }

    /// <summary>
    /// Nạp dữ liệu vào cơ sở dữ liệu PostgreSQL qua DbContext.
    /// </summary>
    public static async Task SeedAsync(CulinaryBlogDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.RecipeIngredients.AnyAsync(cancellationToken))
        {
            return;
        }

        var recipes = await context.Recipes
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        if (recipes.Count == 0)
        {
            return;
        }

        var random = new Random(202611);
        var allIngredients = new List<RecipeIngredient>();
        var allSteps = new List<RecipeStep>();
        var allImages = new List<RecipeImage>();

        foreach (var recipe in recipes)
        {
            allIngredients.AddRange(GenerateIngredients(recipe.Id, random));
            allSteps.AddRange(GenerateSteps(recipe.Id, recipe.Title));
            allImages.AddRange(GenerateImages(recipe.Id, recipe.Title));
        }

        await context.RecipeIngredients.AddRangeAsync(allIngredients, cancellationToken);
        await context.RecipeSteps.AddRangeAsync(allSteps, cancellationToken);
        await context.RecipeImages.AddRangeAsync(allImages, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
