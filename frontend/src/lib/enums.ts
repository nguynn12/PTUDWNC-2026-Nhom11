/**
 * Ánh xạ chuẩn hóa giữa Enum hiển thị (UI) và Enum số nguyên của Backend (C# .NET)
 * theo quyết định D2 trong docs/decisions/ và KE_HOACH_SUA_LOI_TRUOC_DEMO.md.
 *
 * Difficulty:
 *  1: Easy (Dễ)
 *  2: Medium (Trung bình)
 *  3: Hard (Khó)
 *  4: Expert (Chuyên gia)
 *
 * Status:
 *  0: Draft (Bản nháp)
 *  1: Published (Đã xuất bản)
 *  2: Archived (Đã lưu trữ)
 */

export const RecipeDifficulty = {
  Easy: 1,
  Medium: 2,
  Hard: 3,
  Expert: 4,
} as const;

export type RecipeDifficultyValue = (typeof RecipeDifficulty)[keyof typeof RecipeDifficulty];

export const RecipeStatus = {
  Draft: 0,
  Published: 1,
  Archived: 2,
} as const;

export type RecipeStatusValue = (typeof RecipeStatus)[keyof typeof RecipeStatus];

/**
 * Chuyển đổi giá trị difficulty (chuỗi hoặc số) sang số nguyên tương ứng (1..4).
 */
export function toDifficultyNumber(val: unknown): number {
  if (typeof val === 'number') {
    if (val === 0) return 1; // UI cũ dùng 0 là Easy -> 1
    if (val >= 1 && val <= 4) return val;
    return 1;
  }
  if (typeof val === 'string') {
    const lower = val.trim().toLowerCase();
    if (lower === 'easy' || lower === 'dễ') return 1;
    if (lower === 'medium' || lower === 'trung bình') return 2;
    if (lower === 'hard' || lower === 'khó') return 3;
    if (lower === 'expert' || lower === 'chuyên gia') return 4;
  }
  return 1;
}

/**
 * Chuyển đổi số nguyên difficulty sang chuỗi nhãn hiển thị tiếng Việt.
 */
export function toDifficultyLabel(val: unknown): string {
  const num = toDifficultyNumber(val);
  switch (num) {
    case 1:
      return 'Dễ';
    case 2:
      return 'Trung bình';
    case 3:
      return 'Khó';
    case 4:
      return 'Chuyên gia';
    default:
      return 'Dễ';
  }
}

/**
 * Chuyển đổi trạng thái status (chuỗi hoặc số) sang số nguyên tương ứng (0..2).
 */
export function toStatusNumber(val: unknown): number {
  if (typeof val === 'number') {
    if (val >= 0 && val <= 2) return val;
    return 0;
  }
  if (typeof val === 'string') {
    const lower = val.trim().toLowerCase();
    if (lower === 'draft' || lower === 'bản nháp') return 0;
    if (lower === 'published' || lower === 'đã xuất bản') return 1;
    if (lower === 'archived' || lower === 'đã lưu trữ') return 2;
  }
  return 0;
}

/**
 * Chuyển đổi số nguyên status sang chuỗi nhãn hiển thị tiếng Việt.
 */
export function toStatusLabel(val: unknown): string {
  const num = toStatusNumber(val);
  switch (num) {
    case 0:
      return 'Bản nháp';
    case 1:
      return 'Đã xuất bản';
    case 2:
      return 'Đã lưu trữ';
    default:
      return 'Bản nháp';
  }
}
