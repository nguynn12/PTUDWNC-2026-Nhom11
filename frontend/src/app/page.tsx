import React from "react";
import Link from "next/link";
import Image from "next/image";
import { getCategories } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";
import { CategorySidebar } from "@/components/category";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import { Container } from "@/components/common/Container";
import { EmptyState } from "@/components/common/EmptyState";
import { ErrorState } from "@/components/common/ErrorState";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Sparkles, Utensils, Search, ArrowRight, Clock, Salad, Zap } from "lucide-react";
import type { CategoryDto, RecipeSummaryDto } from "@/types/api";

// Cấu hình ISR: Tự động tái sinh trang tĩnh ngầm định sau mỗi 60 giây khi có dữ liệu mới
export const revalidate = 60;

export default async function HomePage() {
  // Lấy dữ liệu thực tế từ Backend API (PostgreSQL)
  let categories: CategoryDto[] = [];
  let recipes: RecipeSummaryDto[] = [];
  let fetchError = false;

  const [catResult, recResult] = await Promise.allSettled([
    getCategories(),
    getRecipes({ page: 1, pageSize: 12, sortBy: "newest" }),
  ]);

  if (catResult.status === "fulfilled") {
    categories = catResult.value || [];
  } else {
    console.error("Lỗi khi tải danh sách danh mục:", catResult.reason);
    fetchError = true;
  }

  if (recResult.status === "fulfilled") {
    recipes = recResult.value?.data || [];
  } else {
    console.error("Lỗi khi tải danh sách công thức:", recResult.reason);
    fetchError = true;
  }

  return (
    <div className="flex flex-col min-h-screen">
      {/* =====================================================================
          1. HERO BANNER TINH GỌN & HIỆN ĐẠI
          ===================================================================== */}
      <section className="relative overflow-hidden border-b border-border/60 bg-gradient-to-br from-amber-500/10 via-background to-orange-500/5 py-10 sm:py-14 lg:py-16">
        <Container>
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 lg:gap-10 items-center">
            {/* Cột chữ Hero */}
            <div className="flex flex-col gap-4 lg:col-span-7">
              <div className="inline-flex items-center gap-2">
                <Badge variant="outline" className="px-3 py-1 text-xs font-semibold uppercase tracking-wider text-primary border-primary/30 bg-primary/10 gap-1.5">
                  <Sparkles className="w-3.5 h-3.5" />
                  Kho tàng công thức nấu ăn tuyển chọn
                </Badge>
              </div>

              <h1 className="font-serif text-3xl sm:text-4xl md:text-5xl font-bold tracking-tight text-foreground leading-[1.15]">
                Nghệ Thuật Ẩm Thực &amp;{" "}
                <span className="text-primary italic">Hương Vị Đích Thực</span>
              </h1>

              <p className="text-sm sm:text-base text-muted-foreground leading-relaxed max-w-2xl">
                Khám phá hàng ngàn công thức nấu ăn chuẩn vị từ các đầu bếp tài hoa. Từng bước làm tỉ mỉ, định lượng nguyên liệu chính xác và thông tin dinh dưỡng minh bạch.
              </p>

              {/* Nút Call To Action */}
              <div className="flex flex-wrap items-center gap-3 pt-1">
                <Button asChild size="default" className="rounded-xl font-medium shadow-sm gap-2">
                  <Link href="/recipes">
                    <Utensils className="w-4 h-4" />
                    Khám phá công thức
                  </Link>
                </Button>
                <Button asChild size="default" variant="outline" className="rounded-xl font-medium gap-2">
                  <Link href="/search">
                    <Search className="w-4 h-4" />
                    Tìm theo nguyên liệu
                  </Link>
                </Button>
              </div>

              {/* Thống kê nhanh */}
              <div className="grid grid-cols-3 gap-6 pt-5 border-t border-border/60 mt-1 max-w-lg">
                <div>
                  <div className="text-xl sm:text-2xl font-extrabold text-primary font-serif">100+</div>
                  <div className="text-xs text-muted-foreground mt-0.5">Công thức chuẩn vị</div>
                </div>
                <div>
                  <div className="text-xl sm:text-2xl font-extrabold text-secondary font-serif">100%</div>
                  <div className="text-xs text-muted-foreground mt-0.5">Dinh dưỡng minh bạch</div>
                </div>
                <div>
                  <div className="text-xl sm:text-2xl font-extrabold text-amber-600 dark:text-amber-500 font-serif">20+</div>
                  <div className="text-xs text-muted-foreground mt-0.5">Danh mục phong phú</div>
                </div>
              </div>
            </div>

            {/* Cột ảnh minh họa Hero */}
            <div className="lg:col-span-5 relative">
              <div className="relative aspect-[16/10] sm:aspect-[4/3] rounded-2xl overflow-hidden shadow-lg border-2 border-card bg-muted">
                <Image
                  src="https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=800&q=85"
                  alt="Không gian bếp ẩm thực ấm cúng"
                  fill
                  priority
                  sizes="(max-width: 1024px) 100vw, 450px"
                  className="object-cover"
                />
              </div>
              <div className="absolute -bottom-3 -left-3 hidden sm:flex items-center gap-2.5 bg-card/95 backdrop-blur-md p-3 rounded-xl border border-border shadow-md">
                <div className="w-8 h-8 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
                  <Utensils className="w-4 h-4" />
                </div>
                <div>
                  <div className="text-xs font-semibold text-foreground">Ẩm thực đa sắc màu</div>
                  <div className="text-[10px] text-muted-foreground">Truyền cảm hứng mỗi bữa ăn</div>
                </div>
              </div>
            </div>
          </div>
        </Container>
      </section>

      {/* =====================================================================
          2. BỐ CỤC 2 CỘT: SIDEBAR CATEGORY (<aside>) & MAIN CONTENT (<main>)
          (Yêu cầu 3 của Giảng viên)
          ===================================================================== */}
      <section className="py-10 sm:py-14 bg-background">
        <Container>
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-8 lg:gap-10 items-start">
            {/* Cột 1 (Sidebar Danh mục): Cố định sticky trên desktop, thanh trượt ngang trên mobile */}
            <aside className="lg:col-span-1">
              <CategorySidebar categories={categories} />
            </aside>

            {/* Cột 2 (Nội dung chính): Danh sách công thức mới nhất */}
            <main className="lg:col-span-3 space-y-8">
              {/* Tiêu đề khối */}
              <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4 pb-4 border-b border-border/60">
                <div>
                  <div className="inline-flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wider text-primary mb-1">
                    <Sparkles className="w-3.5 h-3.5" />
                    Cập nhật liên tục
                  </div>
                  <h2 className="font-serif text-2xl sm:text-3xl font-bold text-foreground">
                    Công Thức Mới Nhất
                  </h2>
                </div>
                <Button asChild variant="ghost" className="text-primary hover:text-primary gap-1 self-start sm:self-auto p-0 hover:bg-transparent font-medium">
                  <Link href="/recipes" className="inline-flex items-center gap-1">
                    Duyệt toàn bộ kho công thức <ArrowRight className="w-4 h-4" />
                  </Link>
                </Button>
              </div>

              {/* Xử lý các trạng thái: Lỗi mạng, Danh sách trống, hoặc Lưới 12 công thức */}
              {fetchError && recipes.length === 0 ? (
                <ErrorState
                  title="Không thể tải công thức mới"
                  message="Hệ thống máy chủ đang bận hoặc gặp sự cố kết nối. Vui lòng thử lại sau ít phút."
                  showBackHome={false}
                />
              ) : recipes.length === 0 ? (
                <EmptyState
                  title="Chưa có công thức nào"
                  description="Các công thức nấu ăn mới đang được các đầu bếp chuẩn bị. Bạn vui lòng quay lại sau nhé!"
                  actionText="Khám phá danh mục"
                  actionHref="/categories"
                />
              ) : (
                <>
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {recipes.map((rec, idx) => (
                      <RecipeCard key={rec.id || rec.slug} recipe={rec} priority={idx < 3} />
                    ))}
                  </div>

                  {/* Chân khối: đếm số bài và nút Xem Thêm */}
                  <div className="flex flex-col sm:flex-row items-center justify-between gap-4 pt-6 border-t border-border/60">
                    <p className="text-xs text-muted-foreground">
                      Hiển thị {recipes.length} công thức nấu ăn chuẩn vị mới nhất
                    </p>
                    <Button asChild size="default" className="rounded-xl font-medium shadow-sm gap-2">
                      <Link href="/recipes">
                        Xem thêm các công thức khác <ArrowRight className="w-4 h-4" />
                      </Link>
                    </Button>
                  </div>
                </>
              )}
            </main>
          </div>
        </Container>
      </section>

      {/* =====================================================================
          3. KHỐI TIÊU CHUẨN CHẤT LƯỢNG (FEATURES)
          ===================================================================== */}
      <section className="py-14 sm:py-18 border-t border-border/60 bg-muted/20">
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
