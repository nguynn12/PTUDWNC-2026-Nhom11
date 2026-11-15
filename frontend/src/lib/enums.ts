/**
 * Chuẩn hóa Enum qua wire format (FE-03, API-12, D2).
 * Backend serialize enum dạng số:
 * - RecipeDifficulty: Easy=1, Medium=2, Hard=3, Expert=4
 * - RecipeStatus: Draft=0, Published=1, Archived=2
 */

export enum RecipeDifficultyEnum {
  Easy = 1,
  Medium = 2,
  Hard = 3,
  Expert = 4,
}

export enum RecipeStatusEnum {
  Draft = 0,
  Published = 1,
  Archived = 2,
}

export const DIFFICULTY_LABELS: Record<number, string> = {
  [RecipeDifficultyEnum.Easy]: "Dễ",
  [RecipeDifficultyEnum.Medium]: "Vừa",
  [RecipeDifficultyEnum.Hard]: "Khó",
  [RecipeDifficultyEnum.Expert]: "Chuyên gia",
};

export const DIFFICULTY_VARIANTS: Record<number, "easy" | "medium" | "hard"> = {
  [RecipeDifficultyEnum.Easy]: "easy",
  [RecipeDifficultyEnum.Medium]: "medium",
  [RecipeDifficultyEnum.Hard]: "hard",
  [RecipeDifficultyEnum.Expert]: "hard",
};

/**
 * Chuyển đổi bất kỳ giá trị difficulty (chuỗi tên hoặc số) sang enum số (1..4) hoặc undefined
 */
export function toDifficultyNumber(val: unknown): number | undefined {
  if (val === undefined || val === null || val === "") return undefined;
  if (typeof val === "number" && val >= 1 && val <= 4) return val;
  const s = String(val).trim().toLowerCase();
  if (s === "1" || s === "easy") return RecipeDifficultyEnum.Easy;
  if (s === "2" || s === "medium") return RecipeDifficultyEnum.Medium;
  if (s === "3" || s === "hard") return RecipeDifficultyEnum.Hard;
  if (s === "4" || s === "expert") return RecipeDifficultyEnum.Expert;
  return undefined;
}

/**
 * Lấy nhãn hiển thị tiếng Việt cho độ khó
 */
export function getDifficultyLabel(diff: unknown): string {
  const num = toDifficultyNumber(diff);
  return num ? DIFFICULTY_LABELS[num] || "Vừa" : "Vừa";
}

/**
 * Lấy badge variant ("easy" | "medium" | "hard") cho độ khó
 */
export function getDifficultyVariant(diff: unknown): {
  label: string;
  variant: "easy" | "medium" | "hard";
} {
  const num = toDifficultyNumber(diff);
  if (num === RecipeDifficultyEnum.Easy) return { label: "Dễ", variant: "easy" };
  if (num === RecipeDifficultyEnum.Hard) return { label: "Khó", variant: "hard" };
  if (num === RecipeDifficultyEnum.Expert) return { label: "Chuyên gia", variant: "hard" };
  return { label: "Vừa", variant: "medium" };
}
