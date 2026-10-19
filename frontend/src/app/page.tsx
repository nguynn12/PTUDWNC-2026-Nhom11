import React from "react";
import Link from "next/link";
import Image from "next/image";
import { getCategories } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";
import { CategoryCard } from "@/components/category/CategoryCard";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { Container } from "@/components/common/Container";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Sparkles, Utensils, Search, ArrowRight, Clock, Salad, Zap } from "lucide-react";
import type { CategoryDto, RecipeSummaryDto } from "@/types/api";

// Cấu hình ISR: Tự động tái sinh trang tĩnh ngầm định sau mỗi 3600 giây (1 giờ)
export const revalidate = 3600;

// Dữ liệu mẫu dự phòng khi Backend đang khởi động hoặc chưa có bài viết
const fallbackCategories: CategoryDto[] = [
  {
    id: "cat-1",
    name: "Món Khai Vị",
    slug: "mon-khai-vi",
    description: "Các món salad thanh mát, gỏi cuốn và súp khởi đầu bữa ăn.",
    imageUrl: "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=400&q=80",
    orderIndex: 1,
    recipeCount: 12,
  },
  {
    id: "cat-2",
    name: "Món Chính",
    slug: "mon-chinh",
    description: "Món kho, xào, nướng đậm đà phong vị bữa cơm gia đình.",
    imageUrl: "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=400&q=80",
    orderIndex: 2,
    recipeCount: 36,
  },
  {
    id: "cat-3",
    name: "Món Canh & Súp",
    slug: "mon-canh-sup",
    description: "Nước dùng ngọt lành, thanh nhiệt và bồi bổ sức khỏe.",
    imageUrl: "https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=400&q=80",
    orderIndex: 3,
    recipeCount: 18,
  },
  {
    id: "cat-4",
    name: "Món Tráng Miệng",
    slug: "mon-trang-mieng",
    description: "Bánh ngọt, chè thanh và các món ăn chơi hấp dẫn.",
    imageUrl: "https://images.unsplash.com/photo-1551024709-8f23befc6f87?auto=format&fit=crop&w=400&q=80",
    orderIndex: 4,
    recipeCount: 24,
  },
  {
    id: "cat-5",
    name: "Đồ Uống & Trà",
    slug: "do-uong-tra",
    description: "Nước ép tươi ngon, sinh tố và trà hoa thanh lọc.",
    imageUrl: "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=400&q=80",
    orderIndex: 5,
    recipeCount: 15,
  },
  {
    id: "cat-6",
    name: "Ăn Chay Thực Dưỡng",
    slug: "an-chay-thuc-duong",
    description: "Nguyên liệu tự nhiên thuần khiết từ rau củ tươi non.",
    imageUrl: "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=400&q=80",
    orderIndex: 6,
    recipeCount: 20,
  },
];

const fallbackRecipes: RecipeSummaryDto[] = [
  {
    id: "rec-1",
    title: "Phở Bò Tái Lăn Hà Nội Truyền Thống",
    slug: "pho-bo-tai-lan-ha-noi",
    description: "Nước dùng trong veo ngọt thanh ninh từ xương bò, thịt bò xào lăn thơm lừng tỏi gừng.",
    prepTime: 30,
    cookTime: 90,
    totalTime: 120,
    servings: 4,
    difficulty: "Medium",
    primaryImageUrl: "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-2",
    categoryName: "Món Chính",
    categorySlug: "mon-chinh",
    calories: 520,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-2",
    title: "Cá Hồi Áp Chảo Sốt Bơ Tỏi Măng Tây",
    slug: "ca-hoi-ap-chao-sot-bo-toi",
    description: "Miếng cá hồi giòn da béo ngậy, đẫm sốt bơ tỏi chanh tươi cùng măng tây giòn ngọt.",
    prepTime: 15,
    cookTime: 15,
    totalTime: 30,
    servings: 2,
    difficulty: "Easy",
    primaryImageUrl: "https://images.unsplash.com/photo-1467003909585-2f8a72700288?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-2",
    categoryName: "Món Chính",
    categorySlug: "mon-chinh",
    calories: 460,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-3",
    title: "Salad Ức Gà Sốt Mè Rang Thanh Mát",
    slug: "salad-uc-ga-sot-me-rang",
    description: "Ức gà áp chảo xé sợi trộn cùng xà lách giòn rụm, cà chua bi và sốt mè rang béo ngậy.",
    prepTime: 10,
    cookTime: 15,
    totalTime: 25,
    servings: 2,
    difficulty: "Easy",
    primaryImageUrl: "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-1",
    categoryName: "Món Khai Vị",
    categorySlug: "mon-khai-vi",
    calories: 320,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-4",
    title: "Bò Kho Nước Dừa Sả Ớt Ăn Kèm Bánh Mì",
    slug: "bo-kho-nuoc-dua-sa-ot",
    description: "Nạm bò mềm nhừ thấm đượm gia vị cà ri và nước dừa tươi ngọt béo, thơm nồng mùi sả quế.",
    prepTime: 20,
    cookTime: 60,
    totalTime: 80,
    servings: 6,
    difficulty: "Medium",
    primaryImageUrl: "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-2",
    categoryName: "Món Chính",
    categorySlug: "mon-chinh",
    calories: 580,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-5",
    title: "Bánh Mousse Xoài Chanh Leo Mịn Màng",
    slug: "banh-mousse-xoai-chanh-leo",
    description: "Lớp kem mousse chua ngọt thanh nhẹ từ xoài cát tươi và chanh leo, tan ngay nơi đầu lưỡi.",
    prepTime: 40,
    cookTime: 0,
    totalTime: 40,
    servings: 8,
    difficulty: "Hard",
    primaryImageUrl: "https://images.unsplash.com/photo-1551024709-8f23befc6f87?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-4",
    categoryName: "Món Tráng Miệng",
    categorySlug: "mon-trang-mieng",
    calories: 290,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-6",
    title: "Súp Bí Đỏ Hạt Sen Nấu Nấm Đông Cô",
    slug: "sup-bi-do-hat-sen-nam-dong-co",
    description: "Món súp chay bồi bổ thể lực, vị ngọt thanh mát tự nhiên từ bí đỏ và hạt sen bùi thơm.",
    prepTime: 15,
    cookTime: 25,
    totalTime: 40,
    servings: 4,
    difficulty: "Easy",
    primaryImageUrl: "https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-3",
    categoryName: "Món Canh & Súp",
    categorySlug: "mon-canh-sup",
    calories: 210,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
];

export default async function HomePage() {
  // Tải dữ liệu song song từ Backend với Fallback an toàn
  let categories: CategoryDto[] = [];
  let recipes: RecipeSummaryDto[] = [];

  try {
    const [catRes, recRes] = await Promise.all([
      getCategories().catch(() => []),
      getRecipes({ pageSize: 6, sortBy: "newest" }).catch(() => ({ data: [], meta: null })),
    ]);

    categories = catRes && catRes.length > 0 ? catRes.slice(0, 6) : fallbackCategories;
    recipes = recRes?.data && recRes.data.length > 0 ? recRes.data.slice(0, 6) : fallbackRecipes;
  } catch {
    categories = fallbackCategories;
    recipes = fallbackRecipes;
  }

  return (
    <div className="flex flex-col min-h-screen">
      {/* =====================================================================
          1. HERO BANNER ẤM CÚNG & NỔI BẬT
          ===================================================================== */}
      <section className="relative overflow-hidden border-b border-border/60 bg-gradient-to-br from-amber-500/10 via-background to-orange-500/5 py-16 sm:py-20 lg:py-24">
        <Container>
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-10 lg:gap-12 items-center">
            {/* Cột chữ Hero */}
            <div className="flex flex-col gap-6 lg:col-span-7">
              <div className="inline-flex items-center gap-2">
                <Badge variant="outline" className="px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary border-primary/30 bg-primary/10 gap-1.5">
                  <Sparkles className="w-3.5 h-3.5" />
                  Kho tàng công thức nấu ăn tuyển chọn
                </Badge>
              </div>

              <h1 className="font-serif text-3xl sm:text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight text-foreground leading-[1.15]">
                Nghệ Thuật Ẩm Thực &amp;{" "}
                <span className="text-primary italic">Hương Vị Đích Thực</span>
              </h1>

              <p className="text-base sm:text-lg text-muted-foreground leading-relaxed max-w-2xl">
                Khám phá hàng ngàn công thức nấu ăn chuẩn vị từ các đầu bếp tài hoa. Từng bước làm tỉ mỉ, định lượng nguyên liệu chính xác và thông tin dinh dưỡng minh bạch.
              </p>

              {/* Nút Call To Action */}
              <div className="flex flex-wrap items-center gap-3 pt-2">
                <Button asChild size="lg" className="rounded-xl font-medium shadow-sm gap-2">
                  <Link href="/recipes">
                    <Utensils className="w-4 h-4" />
                    Khám phá công thức
                  </Link>
                </Button>
                <Button asChild size="lg" variant="outline" className="rounded-xl font-medium gap-2">
                  <Link href="/search">
                    <Search className="w-4 h-4" />
                    Tìm theo nguyên liệu
                  </Link>
                </Button>
              </div>

              {/* Thống kê nhanh */}
              <div className="grid grid-cols-3 gap-6 pt-6 border-t border-border/60 mt-2 max-w-lg">
                <div>
                  <div className="text-2xl sm:text-3xl font-extrabold text-primary font-serif">100+</div>
                  <div className="text-xs sm:text-sm text-muted-foreground mt-0.5">Công thức chuẩn vị</div>
                </div>
                <div>
                  <div className="text-2xl sm:text-3xl font-extrabold text-secondary font-serif">100%</div>
                  <div className="text-xs sm:text-sm text-muted-foreground mt-0.5">Dinh dưỡng minh bạch</div>
                </div>
                <div>
                  <div className="text-2xl sm:text-3xl font-extrabold text-amber-600 dark:text-amber-500 font-serif">20+</div>
                  <div className="text-xs sm:text-sm text-muted-foreground mt-0.5">Danh mục phong phú</div>
                </div>
              </div>
            </div>

            {/* Cột ảnh minh họa Hero */}
            <div className="lg:col-span-5 relative">
              <div className="relative aspect-[4/3] rounded-2xl overflow-hidden shadow-xl border-4 border-card bg-muted">
                <Image
                  src="https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1000&q=85"
                  alt="Không gian bếp ẩm thực ấm cúng"
                  fill
                  priority
                  sizes="(max-width: 1024px) 100vw, 500px"
                  className="object-cover"
                />
              </div>
              <div className="absolute -bottom-4 -left-4 hidden sm:flex items-center gap-3 bg-card/95 backdrop-blur-md p-3.5 rounded-xl border border-border shadow-lg">
                <div className="w-10 h-10 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
                  <Utensils className="w-5 h-5" />
                </div>
                <div>
                  <div className="text-xs font-semibold text-foreground">Ẩm thực đa sắc màu</div>
                  <div className="text-[11px] text-muted-foreground">Truyền cảm hứng mỗi bữa ăn</div>
                </div>
              </div>
            </div>
          </div>
        </Container>
      </section>

      {/* =====================================================================
          2. KHỐI DANH MỤC MÓN ĂN NỔI BẬT (FEATURED CATEGORIES)
          ===================================================================== */}
      <section className="py-14 sm:py-18 bg-background">
        <Container>
          <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4 mb-8">
            <div>
              <span className="text-xs font-semibold uppercase tracking-wider text-primary">
                Khám phá theo chủ đề
              </span>
              <h2 className="font-serif text-2xl sm:text-3xl font-bold text-foreground mt-1">
                Danh Mục Món Ăn Nổi Bật
              </h2>
            </div>
            <Button asChild variant="ghost" className="text-primary hover:text-primary gap-1 self-start sm:self-auto p-0 hover:bg-transparent">
              <Link href="/categories" className="inline-flex items-center gap-1 font-semibold">
                Xem tất cả danh mục <ArrowRight className="w-4 h-4" />
              </Link>
            </Button>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-4">
            {categories.map((cat) => (
              <CategoryCard key={cat.id || cat.slug} category={cat} />
            ))}
          </div>
        </Container>
      </section>

      {/* =====================================================================
          3. KHỐI CÔNG THỨC MỚI NHẤT (RECENT RECIPES)
          ===================================================================== */}
      <section className="py-14 sm:py-18 border-y border-border/60 bg-muted/30">
        <Container>
          <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4 mb-8">
            <div>
              <span className="text-xs font-semibold uppercase tracking-wider text-primary">
                Cập nhật mỗi ngày
              </span>
              <h2 className="font-serif text-2xl sm:text-3xl font-bold text-foreground mt-1">
                Công Thức Mới Nhất
              </h2>
            </div>
            <Button asChild variant="ghost" className="text-primary hover:text-primary gap-1 self-start sm:self-auto p-0 hover:bg-transparent">
              <Link href="/recipes" className="inline-flex items-center gap-1 font-semibold">
                Duyệt toàn bộ kho công thức <ArrowRight className="w-4 h-4" />
              </Link>
            </Button>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {recipes.map((rec, idx) => (
              <RecipeCard key={rec.id || rec.slug} recipe={rec} priority={idx < 3} />
            ))}
          </div>

          <div className="text-center mt-12">
            <Button asChild size="lg" className="rounded-xl font-medium shadow-sm gap-2">
              <Link href="/recipes">
                Xem thêm các công thức khác <ArrowRight className="w-4 h-4" />
              </Link>
            </Button>
          </div>
        </Container>
      </section>

      {/* =====================================================================
          4. KHỐI TIÊU CHUẨN CHẤT LƯỢNG (FEATURES)
          ===================================================================== */}
      <section className="py-16 sm:py-20 bg-background">
        <Container>
          <div className="text-center max-w-2xl mx-auto mb-12">
            <span className="text-xs font-semibold uppercase tracking-wider text-primary">
              Tại sao chọn Culinary Blog
            </span>
            <h2 className="font-serif text-2xl sm:text-3xl font-bold text-foreground mt-1 mb-3">
              Nâng Tầm Kỹ Năng Nấu Nướng
            </h2>
            <p className="text-muted-foreground text-sm sm:text-base">
              Mang đến trải nghiệm vào bếp dễ dàng, hứng khởi và chuẩn chỉnh dinh dưỡng cho mọi gia đình.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <Card className="border border-border/80 shadow-xs hover:shadow-md transition-shadow">
              <CardContent className="p-6 sm:p-8 flex flex-col items-center text-center">
                <div className="w-14 h-14 rounded-2xl bg-primary/10 text-primary flex items-center justify-center mb-5">
                  <Clock className="w-7 h-7" />
                </div>
                <h3 className="font-semibold text-lg text-foreground mb-2">Định Lượng Chuẩn Xác</h3>
                <p className="text-muted-foreground text-sm leading-relaxed">
                  Thời gian chuẩn bị, thời gian nấu và tỷ lệ gia vị được kiểm chứng kỹ lưỡng từ các chuyên gia.
                </p>
              </CardContent>
            </Card>

            <Card className="border border-border/80 shadow-xs hover:shadow-md transition-shadow">
              <CardContent className="p-6 sm:p-8 flex flex-col items-center text-center">
                <div className="w-14 h-14 rounded-2xl bg-secondary/15 text-secondary flex items-center justify-center mb-5">
                  <Salad className="w-7 h-7" />
                </div>
                <h3 className="font-semibold text-lg text-foreground mb-2">Dinh Dưỡng Rõ Ràng</h3>
                <p className="text-muted-foreground text-sm leading-relaxed">
                  Bảng dinh dưỡng chi tiết về calo, chất béo, đạm và carbs hỗ trợ ăn uống khoa học và lành mạnh.
                </p>
              </CardContent>
            </Card>

            <Card className="border border-border/80 shadow-xs hover:shadow-md transition-shadow">
              <CardContent className="p-6 sm:p-8 flex flex-col items-center text-center">
                <div className="w-14 h-14 rounded-2xl bg-amber-500/10 text-amber-600 dark:text-amber-500 flex items-center justify-center mb-5">
                  <Zap className="w-7 h-7" />
                </div>
                <h3 className="font-semibold text-lg text-foreground mb-2">Tìm Kiếm Toàn Văn Bản</h3>
                <p className="text-muted-foreground text-sm leading-relaxed">
                  Hỗ trợ tìm kiếm tiếng Việt thông minh có dấu hoặc không dấu với tốc độ phản hồi tính bằng mili-giây.
                </p>
              </CardContent>
            </Card>
          </div>
        </Container>
      </section>
    </div>
  );
}
