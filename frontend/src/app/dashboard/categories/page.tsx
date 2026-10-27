"use client";

import React, { useState, useMemo } from "react";
import Link from "next/link";
import Image from "next/image";
import { toast } from "sonner";
import {
  useCategories,
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategory,
} from "@/hooks/useCategories";
import { Container } from "@/components/common/Container";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogAction,
  AlertDialogCancel,
} from "@/components/ui/alert-dialog";
import {
  FolderPlus,
  Search,
  X,
  ExternalLink,
  Pencil,
  Trash2,
  AlertTriangle,
  FolderKanban,
  UtensilsCrossed,
  Link2,
  RotateCw,
  Lock,
  Loader2,
  Utensils,
} from "lucide-react";
import type { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest, AppError } from "@/types/api";

// ----------------------------------------------------------------------------
// HELPER: Slugify tiếng Việt chuẩn SEO (Preview thời gian thực)
// ----------------------------------------------------------------------------
function generateSlug(text: string): string {
  return text
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[đĐ]/g, "d")
    .replace(/[^a-z0-9\s-]/g, "")
    .trim()
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-");
}

export default function AdminCategoriesPage() {
  // Queries & Mutations
  const { data: categories = [], isLoading, isError, error, refetch } = useCategories();
  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const deleteMutation = useDeleteCategory();

  // Local State: Tìm kiếm & Sắp xếp
  const [searchTerm, setSearchTerm] = useState("");
  const [sortBy, setSortBy] = useState<"orderIndex" | "name" | "recipeCount">("orderIndex");

  // Local State: Modals
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(null);
  const [deletingCategory, setDeletingCategory] = useState<CategoryDto | null>(null);

  // Lọc và Sắp xếp Danh mục trên Client
  const filteredCategories = useMemo(() => {
    let result = [...categories];

    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase().trim();
      result = result.filter(
        (c) =>
          c.name.toLowerCase().includes(q) ||
          c.slug.toLowerCase().includes(q) ||
          (c.description && c.description.toLowerCase().includes(q))
      );
    }

    result.sort((a, b) => {
      if (sortBy === "orderIndex") {
        return a.orderIndex - b.orderIndex || a.name.localeCompare(b.name, "vi");
      }
      if (sortBy === "recipeCount") {
        return b.recipeCount - a.recipeCount;
      }
      return a.name.localeCompare(b.name, "vi");
    });

    return result;
  }, [categories, searchTerm, sortBy]);

  // Thống kê Metrics
  const totalCategories = categories.length;
  const activeCategoriesWithRecipes = categories.filter((c: CategoryDto) => c.recipeCount > 0).length;
  const totalRecipesLinked = categories.reduce((sum: number, c: CategoryDto) => sum + c.recipeCount, 0);

  return (
    <div className="py-8 sm:py-12 bg-background min-h-screen">
      <Container>
        {/* Header & Tiêu đề trang */}
        <div className="mb-8">
          <div className="flex items-center gap-2 text-xs text-muted-foreground mb-2">
            <Link href="/" className="hover:text-primary transition-colors">
              Trang chủ
            </Link>
            <span>/</span>
            <span>Bảng điều khiển</span>
            <span>/</span>
            <span className="text-primary font-medium">Quản lý Danh mục</span>
          </div>

          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-foreground font-serif">
                Quản Trị Danh Mục Món Ăn
              </h1>
              <p className="text-sm text-muted-foreground mt-1">
                Quản lý danh sách, cấu hình thứ tự hiển thị và duy trì các chuyên mục ẩm thực (FR-CAT-001 đến FR-CAT-005).
              </p>
            </div>
            <Button
              onClick={() => setIsCreateOpen(true)}
              className="gap-2 shrink-0 rounded-xl"
            >
              <FolderPlus className="w-4 h-4" />
              <span>Thêm danh mục mới</span>
            </Button>
          </div>
        </div>

        {/* Metrics Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          <Card className="border border-border/80 shadow-xs">
            <CardContent className="p-5 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
                <FolderKanban className="w-6 h-6" />
              </div>
              <div>
                <div className="text-2xl font-bold text-foreground">{totalCategories}</div>
                <div className="text-xs text-muted-foreground font-medium">Tổng số danh mục</div>
              </div>
            </CardContent>
          </Card>

          <Card className="border border-border/80 shadow-xs">
            <CardContent className="p-5 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-secondary/15 text-secondary flex items-center justify-center shrink-0">
                <UtensilsCrossed className="w-6 h-6" />
              </div>
              <div>
                <div className="text-2xl font-bold text-foreground">{activeCategoriesWithRecipes}</div>
                <div className="text-xs text-muted-foreground font-medium">Danh mục có bài viết</div>
              </div>
            </CardContent>
          </Card>

          <Card className="border border-border/80 shadow-xs">
            <CardContent className="p-5 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-500 flex items-center justify-center shrink-0">
                <Link2 className="w-6 h-6" />
              </div>
              <div>
                <div className="text-2xl font-bold text-foreground">{totalRecipesLinked}</div>
                <div className="text-xs text-muted-foreground font-medium">Tổng liên kết công thức</div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Toolbar: Tìm kiếm & Sắp xếp */}
        <Card className="border border-border/80 shadow-xs mb-6">
          <CardContent className="p-4 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3">
            <div className="relative flex-1 max-w-md">
              <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
              <Input
                type="text"
                placeholder="Lọc nhanh danh mục theo tên hoặc slug..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9 pr-8 h-10 rounded-lg text-sm"
              />
              {searchTerm && (
                <button
                  type="button"
                  onClick={() => setSearchTerm("")}
                  className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground p-0.5"
                  aria-label="Xóa bộ lọc"
                >
                  <X className="w-3.5 h-3.5" />
                </button>
              )}
            </div>

            <div className="flex items-center gap-2 shrink-0">
              <Label htmlFor="sortSelect" className="text-xs text-muted-foreground whitespace-nowrap">
                Sắp xếp theo:
              </Label>
              <select
                id="sortSelect"
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value as "orderIndex" | "name" | "recipeCount")}
                className="h-10 rounded-lg border border-input bg-background px-3 py-1 text-sm text-foreground shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                <option value="orderIndex">Thứ tự hiển thị (Mặc định)</option>
                <option value="name">Tên danh mục (A - Z)</option>
                <option value="recipeCount">Số lượng công thức (Nhiều nhất)</option>
              </select>
            </div>
          </CardContent>
        </Card>

        {/* Main Data Table */}
        {isLoading ? (
          <Card className="border border-border/80 shadow-xs p-8 text-center">
            <div className="flex flex-col items-center justify-center gap-3 py-12">
              <Loader2 className="w-8 h-8 animate-spin text-primary" />
              <p className="text-sm text-muted-foreground">Đang tải danh sách danh mục...</p>
            </div>
          </Card>
        ) : isError ? (
          <Card className="border border-destructive/30 shadow-xs p-8 text-center bg-destructive/5">
            <div className="flex flex-col items-center justify-center gap-3 py-6">
              <div className="w-12 h-12 rounded-full bg-destructive/10 text-destructive flex items-center justify-center">
                <AlertTriangle className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-foreground">Không thể nạp dữ liệu danh mục</h3>
              <p className="text-sm text-muted-foreground max-w-md">
                {error?.detail || error?.message || "Đã xảy ra sự cố khi kết nối tới máy chủ API."}
              </p>
              <Button onClick={() => refetch()} variant="outline" size="sm" className="gap-2 mt-2">
                <RotateCw className="w-4 h-4" />
                Tải lại dữ liệu
              </Button>
            </div>
          </Card>
        ) : filteredCategories.length === 0 ? (
          <Card className="border border-dashed border-border p-12 text-center">
            <div className="flex flex-col items-center justify-center gap-3 py-4">
              <div className="w-14 h-14 rounded-full bg-muted flex items-center justify-center text-muted-foreground">
                <FolderKanban className="w-7 h-7" />
              </div>
              <h3 className="text-lg font-bold text-foreground">
                {searchTerm ? "Không tìm thấy danh mục phù hợp" : "Chưa có danh mục nào"}
              </h3>
              <p className="text-sm text-muted-foreground max-w-md">
                {searchTerm
                  ? `Không có kết quả nào khớp với từ khóa "${searchTerm}". Vui lòng thử từ khóa khác.`
                  : "Bắt đầu thiết lập hệ thống chuyên mục ẩm thực bằng cách tạo danh mục đầu tiên."}
              </p>
              {searchTerm ? (
                <Button onClick={() => setSearchTerm("")} variant="outline" size="sm">
                  Xóa bộ lọc tìm kiếm
                </Button>
              ) : (
                <Button onClick={() => setIsCreateOpen(true)} size="sm" className="gap-2">
                  <FolderPlus className="w-4 h-4" />
                  Tạo danh mục đầu tiên
                </Button>
              )}
            </div>
          </Card>
        ) : (
          <Card className="border border-border/80 shadow-xs overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[70px] text-center">Ảnh</TableHead>
                  <TableHead>Tên danh mục</TableHead>
                  <TableHead>Đường dẫn (Slug)</TableHead>
                  <TableHead className="w-[100px] text-center">Thứ tự</TableHead>
                  <TableHead className="w-[120px] text-center">Công thức</TableHead>
                  <TableHead className="w-[160px] text-right">Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredCategories.map((cat) => (
                  <TableRow key={cat.id}>
                    {/* Thumbnail */}
                    <TableCell className="text-center">
                      <div className="relative w-11 h-11 rounded-lg overflow-hidden bg-muted border border-border/60 mx-auto flex items-center justify-center">
                        {cat.imageUrl ? (
                          <Image
                            src={cat.imageUrl}
                            alt={cat.name}
                            fill
                            sizes="44px"
                            className="object-cover"
                          />
                        ) : (
                          <Utensils className="w-5 h-5 text-muted-foreground" />
                        )}
                      </div>
                    </TableCell>

                    {/* Tên & Mô tả */}
                    <TableCell>
                      <div className="font-semibold text-foreground text-sm">
                        {cat.name}
                      </div>
                      {cat.description ? (
                        <div className="text-xs text-muted-foreground line-clamp-1 max-w-[340px] mt-0.5">
                          {cat.description}
                        </div>
                      ) : (
                        <span className="text-xs text-muted-foreground italic">
                          Chưa có mô tả
                        </span>
                      )}
                    </TableCell>

                    {/* Slug */}
                    <TableCell>
                      <div className="inline-flex items-center gap-1.5">
                        <code className="text-xs bg-muted px-2 py-0.5 rounded font-mono text-primary font-medium">
                          {cat.slug}
                        </code>
                        <Link
                          href={`/categories/${cat.slug}`}
                          target="_blank"
                          title="Xem trang công khai"
                          className="text-muted-foreground hover:text-primary transition-colors"
                        >
                          <ExternalLink className="w-3.5 h-3.5" />
                        </Link>
                      </div>
                    </TableCell>

                    {/* Thứ tự hiển thị */}
                    <TableCell className="text-center">
                      <Badge variant="outline" className="font-mono text-xs font-semibold">
                        {cat.orderIndex}
                      </Badge>
                    </TableCell>

                    {/* Số bài viết */}
                    <TableCell className="text-center">
                      <Badge
                        variant={cat.recipeCount > 0 ? "secondary" : "outline"}
                        className={
                          cat.recipeCount > 0
                            ? "bg-secondary/15 text-secondary font-semibold"
                            : "text-muted-foreground"
                        }
                      >
                        {cat.recipeCount} bài
                      </Badge>
                    </TableCell>

                    {/* Actions */}
                    <TableCell className="text-right whitespace-nowrap">
                      <div className="inline-flex items-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setEditingCategory(cat)}
                          className="h-8 px-2.5 text-xs gap-1"
                          title="Chỉnh sửa thông tin danh mục (FR-CAT-004)"
                        >
                          <Pencil className="w-3.5 h-3.5" />
                          <span>Sửa</span>
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setDeletingCategory(cat)}
                          className="h-8 px-2.5 text-xs gap-1 text-destructive hover:text-destructive hover:bg-destructive/10 border-destructive/30"
                          title="Xóa danh mục (FR-CAT-005)"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                          <span>Xóa</span>
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Card>
        )}

        {/* ====================================================================
            MODAL 1: THÊM MỚI DANH MỤC (CreateCategoryModal)
            ==================================================================== */}
        <CreateCategoryDialog
          open={isCreateOpen}
          onOpenChange={setIsCreateOpen}
          onSuccess={(newCat) => {
            setIsCreateOpen(false);
            toast.success(`Đã tạo thành công danh mục "${newCat.name}"!`);
          }}
          onError={(msg) => {
            toast.error(msg);
          }}
          createMutation={createMutation}
        />

        {/* ====================================================================
            MODAL 2: CHỈNH SỬA DANH MỤC (EditCategoryModal)
            ==================================================================== */}
        {editingCategory && (
          <EditCategoryDialog
            category={editingCategory}
            open={!!editingCategory}
            onOpenChange={(open) => !open && setEditingCategory(null)}
            onSuccess={(updatedCat) => {
              setEditingCategory(null);
              toast.success(`Đã cập nhật danh mục "${updatedCat.name}"!`);
            }}
            onError={(msg) => {
              toast.error(msg);
            }}
            updateMutation={updateMutation}
          />
        )}

        {/* ====================================================================
            MODAL 3: XÓA AN TOÀN DANH MỤC (DeleteCategoryModal)
            ==================================================================== */}
        {deletingCategory && (
          <DeleteCategoryAlertDialog
            category={deletingCategory}
            open={!!deletingCategory}
            onOpenChange={(open) => !open && setDeletingCategory(null)}
            onSuccess={() => {
              const name = deletingCategory.name;
              setDeletingCategory(null);
              toast.success(`Đã xóa danh mục "${name}" thành công!`);
            }}
            onError={(msg) => {
              toast.error(msg);
            }}
            deleteMutation={deleteMutation}
          />
        )}
      </Container>
    </div>
  );
}

// ============================================================================
// DIALOG: CreateCategoryDialog
// ============================================================================
interface CreateCategoryDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: (created: CategoryDto) => void;
  onError: (errorMessage: string) => void;
  createMutation: ReturnType<typeof useCreateCategory>;
}

function CreateCategoryDialog({
  open,
  onOpenChange,
  onSuccess,
  onError,
  createMutation,
}: CreateCategoryDialogProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const [orderIndex, setOrderIndex] = useState(0);
  const [clientError, setClientError] = useState("");

  const slugPreview = generateSlug(name);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setClientError("");

    if (!name.trim()) {
      setClientError("Vui lòng nhập tên danh mục.");
      return;
    }
    if (name.trim().length < 2 || name.trim().length > 50) {
      setClientError("Tên danh mục phải có độ dài từ 2 đến 50 ký tự theo quy định SRS.");
      return;
    }

    const payload: CreateCategoryRequest = {
      name: name.trim(),
      description: description.trim() || undefined,
      imageUrl: imageUrl.trim() || undefined,
      orderIndex: Number(orderIndex) || 0,
    };

    createMutation.mutate(payload, {
      onSuccess: (data: CategoryDto) => {
        setName("");
        setDescription("");
        setImageUrl("");
        setOrderIndex(0);
        onSuccess(data);
      },
      onError: (err: AppError) => {
        onError(err.detail || err.message || "Tạo danh mục thất bại.");
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="text-xl font-bold font-serif">Thêm Danh Mục Mới</DialogTitle>
          <DialogDescription className="text-sm text-muted-foreground">
            Tạo mới chuyên mục ẩm thực theo đặc tả FR-CAT-001.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4 py-2">
          {clientError && (
            <div className="p-3 bg-destructive/10 text-destructive rounded-lg text-xs font-semibold flex items-center gap-2">
              <AlertTriangle className="w-4 h-4 shrink-0" />
              <span>{clientError}</span>
            </div>
          )}

          {/* Tên danh mục */}
          <div className="space-y-1.5">
            <Label htmlFor="create-name" className="text-xs font-semibold">
              Tên danh mục <span className="text-destructive">*</span>
            </Label>
            <Input
              id="create-name"
              placeholder="VD: Món Tráng Miệng, Hải Sản Tươi..."
              value={name}
              onChange={(e) => setName(e.target.value)}
              maxLength={50}
              required
              autoFocus
            />
            {/* Preview Slug */}
            <div className="text-xs text-muted-foreground">
              Slug SEO dự kiến:{" "}
              <code className="text-primary font-mono font-medium">
                {slugPreview || "..."}
              </code>
            </div>
          </div>

          {/* Mô tả */}
          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <Label htmlFor="create-desc" className="text-xs font-semibold">
                Mô tả giới thiệu
              </Label>
              <span className="text-[11px] text-muted-foreground">{description.length}/500</span>
            </div>
            <Textarea
              id="create-desc"
              rows={3}
              placeholder="Mô tả tóm tắt nội dung các công thức trong danh mục..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={500}
            />
          </div>

          {/* Ảnh đại diện URL */}
          <div className="space-y-1.5">
            <Label htmlFor="create-image" className="text-xs font-semibold">
              Đường dẫn hình ảnh (Image URL)
            </Label>
            <Input
              id="create-image"
              type="url"
              placeholder="https://images.unsplash.com/... hoặc MinIO URL"
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
            />
            {imageUrl && (
              <div className="flex items-center gap-2 pt-1">
                <span className="text-xs text-muted-foreground">Xem trước:</span>
                <div className="relative w-8 h-8 rounded-md overflow-hidden border border-border">
                  <Image src={imageUrl} alt="Preview" fill sizes="32px" className="object-cover" />
                </div>
              </div>
            )}
          </div>

          {/* Thứ tự hiển thị */}
          <div className="space-y-1.5">
            <Label htmlFor="create-order" className="text-xs font-semibold">
              Thứ tự sắp xếp (Order Index)
            </Label>
            <div className="flex items-center gap-3">
              <Input
                id="create-order"
                type="number"
                className="w-28 font-mono"
                value={orderIndex}
                onChange={(e) => setOrderIndex(parseInt(e.target.value, 10) || 0)}
                min={0}
                max={9999}
              />
              <span className="text-xs text-muted-foreground">
                Số nhỏ hơn sẽ được ưu tiên hiển thị trước.
              </span>
            </div>
          </div>

          <DialogFooter className="pt-3">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Hủy bỏ
            </Button>
            <Button
              type="submit"
              disabled={createMutation.isPending}
              className="gap-2"
            >
              {createMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
              <span>{createMutation.isPending ? "Đang xử lý..." : "Tạo danh mục"}</span>
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ============================================================================
// DIALOG: EditCategoryDialog (SRS C7: Slug bất biến)
// ============================================================================
interface EditCategoryDialogProps {
  category: CategoryDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: (updated: CategoryDto) => void;
  onError: (errorMessage: string) => void;
  updateMutation: ReturnType<typeof useUpdateCategory>;
}

function EditCategoryDialog({
  category,
  open,
  onOpenChange,
  onSuccess,
  onError,
  updateMutation,
}: EditCategoryDialogProps) {
  const [name, setName] = useState(category.name);
  const [description, setDescription] = useState(category.description || "");
  const [imageUrl, setImageUrl] = useState(category.imageUrl || "");
  const [orderIndex, setOrderIndex] = useState(category.orderIndex);
  const [clientError, setClientError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setClientError("");

    if (!name.trim()) {
      setClientError("Vui lòng nhập tên danh mục.");
      return;
    }
    if (name.trim().length < 2 || name.trim().length > 50) {
      setClientError("Tên danh mục phải có độ dài từ 2 đến 50 ký tự.");
      return;
    }

    const payload: UpdateCategoryRequest = {
      name: name.trim(),
      description: description.trim() || undefined,
      imageUrl: imageUrl.trim() || undefined,
      orderIndex: Number(orderIndex) || 0,
    };

    updateMutation.mutate(
      { id: category.id, data: payload },
      {
        onSuccess: (data: CategoryDto) => {
          onSuccess(data);
        },
        onError: (err: AppError) => {
          onError(err.detail || err.message || "Cập nhật danh mục thất bại.");
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="text-xl font-bold font-serif">Chỉnh Sửa Danh Mục</DialogTitle>
          <DialogDescription className="text-sm text-muted-foreground">
            Cập nhật thông tin danh mục theo FR-CAT-004.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4 py-2">
          {clientError && (
            <div className="p-3 bg-destructive/10 text-destructive rounded-lg text-xs font-semibold flex items-center gap-2">
              <AlertTriangle className="w-4 h-4 shrink-0" />
              <span>{clientError}</span>
            </div>
          )}

          {/* Slug (Read-only C7) */}
          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <Label htmlFor="edit-slug" className="text-xs font-semibold">
                Đường dẫn cố định (Slug)
              </Label>
              <Badge variant="outline" className="text-[11px] gap-1 text-primary border-primary/30">
                <Lock className="w-3 h-3" />
                Bất biến (SRS C7)
              </Badge>
            </div>
            <Input
              id="edit-slug"
              value={category.slug}
              readOnly
              disabled
              className="font-mono bg-muted text-muted-foreground cursor-not-allowed"
            />
            <p className="text-[11px] text-muted-foreground">
              Slug được giữ nguyên bất biến khi đổi tên để bảo toàn liên kết công khai và chỉ mục SEO.
            </p>
          </div>

          {/* Tên danh mục */}
          <div className="space-y-1.5">
            <Label htmlFor="edit-name" className="text-xs font-semibold">
              Tên danh mục <span className="text-destructive">*</span>
            </Label>
            <Input
              id="edit-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              maxLength={50}
              required
            />
          </div>

          {/* Mô tả */}
          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <Label htmlFor="edit-desc" className="text-xs font-semibold">
                Mô tả giới thiệu
              </Label>
              <span className="text-[11px] text-muted-foreground">{description.length}/500</span>
            </div>
            <Textarea
              id="edit-desc"
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={500}
            />
          </div>

          {/* Ảnh đại diện URL */}
          <div className="space-y-1.5">
            <Label htmlFor="edit-image" className="text-xs font-semibold">
              Đường dẫn hình ảnh (Image URL)
            </Label>
            <Input
              id="edit-image"
              type="url"
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
            />
            {imageUrl && (
              <div className="flex items-center gap-2 pt-1">
                <span className="text-xs text-muted-foreground">Xem trước:</span>
                <div className="relative w-8 h-8 rounded-md overflow-hidden border border-border">
                  <Image src={imageUrl} alt="Preview" fill sizes="32px" className="object-cover" />
                </div>
              </div>
            )}
          </div>

          {/* Thứ tự hiển thị */}
          <div className="space-y-1.5">
            <Label htmlFor="edit-order" className="text-xs font-semibold">
              Thứ tự sắp xếp (Order Index)
            </Label>
            <Input
              id="edit-order"
              type="number"
              className="w-28 font-mono"
              value={orderIndex}
              onChange={(e) => setOrderIndex(parseInt(e.target.value, 10) || 0)}
              min={0}
              max={9999}
            />
          </div>

          <DialogFooter className="pt-3">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Hủy bỏ
            </Button>
            <Button
              type="submit"
              disabled={updateMutation.isPending}
              className="gap-2"
            >
              {updateMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
              <span>{updateMutation.isPending ? "Đang lưu..." : "Lưu thay đổi"}</span>
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ============================================================================
// ALERT DIALOG: DeleteCategoryAlertDialog (FR-CAT-005 Ràng buộc toàn vẹn)
// ============================================================================
interface DeleteCategoryAlertDialogProps {
  category: CategoryDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
  onError: (errorMessage: string) => void;
  deleteMutation: ReturnType<typeof useDeleteCategory>;
}

function DeleteCategoryAlertDialog({
  category,
  open,
  onOpenChange,
  onSuccess,
  onError,
  deleteMutation,
}: DeleteCategoryAlertDialogProps) {
  const hasRecipes = category.recipeCount > 0;

  const handleDelete = () => {
    deleteMutation.mutate(category.id, {
      onSuccess: () => {
        onSuccess();
      },
      onError: (err: AppError) => {
        if (err.status === 409 || err.errorCode === "CATEGORY_HAS_RECIPES") {
          onError("Không thể xóa danh mục vì vẫn còn công thức liên kết (Quy tắc FR-CAT-005).");
        } else {
          onError(err.detail || err.message || "Xóa danh mục thất bại.");
        }
      },
    });
  };

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle className="text-lg font-bold text-destructive flex items-center gap-2">
            <AlertTriangle className="w-5 h-5 shrink-0" />
            Xác Nhận Xóa Danh Mục
          </AlertDialogTitle>
          <AlertDialogDescription className="text-sm space-y-3 pt-2 text-foreground">
            <p>
              Bạn có chắc chắn muốn xóa danh mục <strong>&ldquo;{category.name}&rdquo;</strong> (
              <code className="text-xs bg-muted px-1.5 py-0.5 rounded font-mono">{category.slug}</code>)?
            </p>

            {hasRecipes ? (
              <div className="p-3.5 bg-destructive/10 border border-destructive/20 rounded-lg text-xs leading-relaxed text-destructive space-y-1">
                <div className="font-bold flex items-center gap-1.5">
                  <AlertTriangle className="w-4 h-4" />
                  Chặn xóa theo quy tắc hệ thống (FR-CAT-005)
                </div>
                <p>
                  Danh mục này hiện đang có <strong>{category.recipeCount} công thức nấu ăn</strong> liên kết. Hệ thống không cho phép xóa danh mục khi chưa chuyển các công thức này sang danh mục khác.
                </p>
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">
                Hành động này sẽ thực hiện xóa mềm (Soft-delete) danh mục khỏi hệ thống.
              </p>
            )}
          </AlertDialogDescription>
        </AlertDialogHeader>

        <AlertDialogFooter>
          <AlertDialogCancel onClick={() => onOpenChange(false)}>
            Hủy bỏ
          </AlertDialogCancel>
          <AlertDialogAction
            disabled={hasRecipes || deleteMutation.isPending}
            onClick={handleDelete}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {deleteMutation.isPending ? "Đang xóa..." : "Xác nhận xóa"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
