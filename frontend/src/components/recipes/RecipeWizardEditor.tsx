'use client';

import React, { useState, useEffect, useCallback, useRef } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { RecipeApi, ApiError } from '@/lib/api-client';
import type {
  CategoryOptionDto,
  DifficultyLevel,
  RecipeIngredientDto,
  RecipeStepDto,
  RecipeImageDto,
  RecipeNutritionDto,
} from '@/types/recipe-content';
import { useToast } from '@/components/common/Toast';
import { ConflictDialog } from '@/components/common/ConflictDialog';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import {
  BookOpenIcon,
  PlusCircleIcon,
  TrashIcon,
  EditIcon,
  ClockIcon,
  UsersIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  UploadCloudIcon,
  ImageIcon,
  StarIcon,
  ArrowUpIcon,
  ArrowDownIcon,
  ListIcon,
  LayersIcon,
  CheckIcon,
  SaveIcon,
  CheckCircleIcon,
  FlameIcon,
} from '@/components/common/Icons';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Select } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';

interface RecipeWizardEditorProps {
  mode: 'create' | 'edit';
  initialRecipeId?: string;
}

const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/avif'];
const MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024; // 5MB theo SRS Mục 8.3

const COMMON_UNITS = [
  'g',
  'kg',
  'ml',
  'lít',
  'muỗng cà phê',
  'muỗng canh',
  'chén',
  'củ',
  'quả',
  'tép',
  'nhánh',
  'lát',
];

export function RecipeWizardEditor({ mode, initialRecipeId }: RecipeWizardEditorProps) {
  const router = useRouter();
  const { showSuccess, showError, showWarning, showInfo } = useToast();

  // Trạng thái điều hướng 4 bước Wizard
  const [activeStep, setActiveStep] = useState<number>(1);
  const [recipeId, setRecipeId] = useState<string | null>(initialRecipeId ?? null);
  const [xmin, setXmin] = useState<string | number | null>(null);
  const [recipeStatus, setRecipeStatus] = useState<string>('Draft');

  // Danh mục (Categories)
  const [categories, setCategories] = useState<CategoryOptionDto[]>([]);
  const [loadingInitial, setLoadingInitial] = useState<boolean>(mode === 'edit');

  // --- STATE BƯỚC 1: THÔNG TIN CHUNG & DINH DƯỠNG ---
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('c1');
  const [difficulty, setDifficulty] = useState<DifficultyLevel>('Medium');
  const [prepTimeMinutes, setPrepTimeMinutes] = useState<number>(20);
  const [cookTimeMinutes, setCookTimeMinutes] = useState<number>(30);
  const [servings, setServings] = useState<number>(4);
  const [hasNutrition, setHasNutrition] = useState<boolean>(false);
  const [nutrition, setNutrition] = useState<RecipeNutritionDto>({
    calories: 450,
    protein: 25,
    fat: 15,
    carbohydrates: 50,
    fiber: 4,
    sodium: 600,
  });
  const [savingStep1, setSavingStep1] = useState(false);
  const [step1Errors, setStep1Errors] = useState<Record<string, string>>({});

  // --- STATE BƯỚC 2: DANH SÁCH NGUYÊN LIỆU (3 API TV4) ---
  const [ingredients, setIngredients] = useState<RecipeIngredientDto[]>([]);
  const [ingName, setIngName] = useState('');
  const [ingIsSeasoning, setIngIsSeasoning] = useState(false); // Quyết định E1 & E2: Gia vị vừa đủ
  const [ingQuantity, setIngQuantity] = useState<string>('200');
  const [ingUnit, setIngUnit] = useState<string>('g');
  const [ingNotes, setIngNotes] = useState('');
  const [editingIngId, setEditingIngId] = useState<string | null>(null);
  const [submittingIng, setSubmittingIng] = useState(false);

  // --- STATE BƯỚC 3: CÁC BƯỚC THỰC HIỆN (4 API TV4) ---
  const [steps, setSteps] = useState<RecipeStepDto[]>([]);
  const [stepTitle, setStepTitle] = useState('');
  const [stepInstruction, setStepInstruction] = useState('');
  const [stepDuration, setStepDuration] = useState<string>('10');
  const [editingStepId, setEditingStepId] = useState<string | null>(null);
  const [submittingStep, setSubmittingStep] = useState(false);
  const [reorderingSteps, setReorderingSteps] = useState(false);

  // --- STATE BƯỚC 4: UPLOAD & QUẢN LÝ ẢNH MINIO (3 API TV4) ---
  const [images, setImages] = useState<RecipeImageDto[]>([]);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [imgAltText, setImgAltText] = useState('');
  const [imgIsPrimary, setImgIsPrimary] = useState(false);
  const [uploadingImg, setUploadingImg] = useState(false);
  const [editingAltMap, setEditingAltMap] = useState<Record<string, string>>({});
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  // --- STATE XỬ LÝ XUNG ĐỘT CONCURRENCY 409 & CONFIRM XÓA ---
  const [conflictState, setConflictState] = useState<{
    isOpen: boolean;
    detail?: string;
  }>({ isOpen: false });

  const [deleteConfirm, setDeleteConfirm] = useState<{
    isOpen: boolean;
    type: 'ingredient' | 'step' | 'image';
    id: string;
    label: string;
  } | null>(null);

  // Tải danh sách danh mục & dữ liệu công thức (nếu ở chế độ Edit)
  const loadRecipeData = useCallback(
    async (targetId: string) => {
      setLoadingInitial(true);
      try {
        const detail = await RecipeApi.getRecipeFullDetail(targetId);
        setRecipeId(detail.id);
        setTitle(detail.title);
        setDescription(detail.description);
        setCategoryId(detail.categoryId || 'c1');
        setDifficulty(
          detail.difficulty === 0 || detail.difficulty === 'Easy'
            ? 'Easy'
            : detail.difficulty === 2 || detail.difficulty === 'Hard'
              ? 'Hard'
              : 'Medium'
        );
        setPrepTimeMinutes(detail.prepTimeMinutes);
        setCookTimeMinutes(detail.cookTimeMinutes);
        setServings(detail.servings);
        setXmin(detail.xmin ?? null);
        setRecipeStatus(String(detail.status || 'Draft'));

        if (detail.nutrition) {
          setHasNutrition(true);
          setNutrition(detail.nutrition);
        }

        const loadedIngs = detail.ingredients || [];
        setIngredients(loadedIngs);

        const loadedSteps = detail.steps || [];
        setSteps(loadedSteps);

        const loadedImages = detail.images || [];
        setImages(loadedImages);
        const altInit: Record<string, string> = {};
        loadedImages.forEach(img => {
          altInit[img.id] = img.altText || '';
        });
        setEditingAltMap(altInit);
      } catch (err) {
        const apiErr = err as ApiError;
        showError('Không thể tải dữ liệu công thức', apiErr.message);
      } finally {
        setLoadingInitial(false);
      }
    },
    [showError]
  );

  useEffect(() => {
    let ignore = false;
    RecipeApi.getCategories().then(cats => {
      if (ignore) return;
      setCategories(cats);
      if (cats.length > 0 && !initialRecipeId) {
        setCategoryId(cats[0].id);
      }
    });

    if (mode === 'edit' && initialRecipeId) {
      void Promise.resolve().then(() => {
        if (!ignore) {
          void loadRecipeData(initialRecipeId);
        }
      });
    }

    return () => {
      ignore = true;
    };
  }, [mode, initialRecipeId, loadRecipeData]);

  // Dọn dẹp URL preview ảnh khi unmount hoặc đổi file
  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  // =========================================================
  // XỬ LÝ BƯỚC 1: LƯU THÔNG TIN CHUNG (POST / PUT KÈM IF-MATCH)
  // =========================================================
  const validateStep1 = (): boolean => {
    const errs: Record<string, string> = {};
    if (!title.trim() || title.trim().length < 3) {
      errs.title = 'Tiêu đề món ăn phải có từ 3 đến 250 ký tự.';
    }
    if (!description.trim() || description.trim().length < 10) {
      errs.description = 'Mô tả tổng quan món ăn cần ít nhất 10 ký tự.';
    }
    if (prepTimeMinutes < 0 || prepTimeMinutes > 1440) {
      errs.prepTime = 'Thời gian chuẩn bị phải từ 0 đến 1440 phút.';
    }
    if (cookTimeMinutes < 0 || cookTimeMinutes > 1440) {
      errs.cookTime = 'Thời gian nấu phải từ 0 đến 1440 phút.';
    }
    if (servings < 1 || servings > 100) {
      errs.servings = 'Khẩu phần phục vụ phải từ 1 đến 100 người.';
    }
    setStep1Errors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSaveStep1 = async (goNextStep = true) => {
    if (!validateStep1()) {
      showWarning('Thông tin chưa hợp lệ', 'Vui lòng kiểm tra các trường báo đỏ ở Bước 1.');
      return;
    }

    setSavingStep1(true);
    try {
      const payload = {
        title: title.trim(),
        description: description.trim(),
        categoryId,
        prepTimeMinutes: Number(prepTimeMinutes),
        cookTimeMinutes: Number(cookTimeMinutes),
        servings: Number(servings),
        difficulty,
        nutrition: hasNutrition ? nutrition : null,
      };

      if (!recipeId) {
        const created = await RecipeApi.createRecipe(payload);
        setRecipeId(created.id);
        setXmin(created.xmin ?? null);
        showSuccess(
          'Đã khởi tạo bản nháp công thức',
          `Mã công thức: ${created.id.slice(0, 8)}... Hãy tiếp tục thêm Nguyên liệu ở Bước 2.`
        );
      } else {
        const updated = await RecipeApi.updateRecipe(recipeId, payload, xmin);
        setXmin(updated.xmin ?? xmin);
        showSuccess(
          'Đã cập nhật thông tin chung',
          `Phiên bản đồng thời mới (xmin): ${updated.xmin ?? 'Updated'}`
        );
      }

      if (goNextStep) {
        setActiveStep(2);
      }
    } catch (err) {
      const apiErr = err as ApiError;
      if (apiErr.status === 409 || apiErr.errorCode === 'RECIPE_CONCURRENCY_CONFLICT') {
        setConflictState({
          isOpen: true,
          detail: apiErr.problemDetails?.detail || apiErr.message,
        });
      } else {
        showError('Lưu thông tin thất bại', apiErr.message);
      }
    } finally {
      setSavingStep1(false);
    }
  };

  // =========================================================
  // XỬ LÝ BƯỚC 2: QUẢN LÝ NGUYÊN LIỆU (POST / PUT / DELETE)
  // =========================================================
  const resetIngredientForm = () => {
    setIngName('');
    setIngIsSeasoning(false);
    setIngQuantity('200');
    setIngUnit('g');
    setIngNotes('');
    setEditingIngId(null);
  };

  const handleSelectEditIngredient = (ing: RecipeIngredientDto) => {
    setEditingIngId(ing.id);
    setIngName(ing.name);
    const isSeasoning = ing.quantity === null || ing.quantity === undefined;
    setIngIsSeasoning(isSeasoning);
    setIngQuantity(ing.quantity !== null && ing.quantity !== undefined ? String(ing.quantity) : '');
    setIngUnit(ing.unit || 'g');
    setIngNotes(ing.notes || '');
  };

  const handleSubmitIngredient = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recipeId) {
      showWarning('Chưa lưu Bước 1', 'Vui lòng lưu Thông tin chung ở Bước 1 trước.');
      setActiveStep(1);
      return;
    }

    if (!ingName.trim()) {
      showWarning('Thiếu tên nguyên liệu', 'Vui lòng nhập tên nguyên liệu (VD: Thịt bò thăn).');
      return;
    }

    let parsedQty: number | null = null;
    let parsedUnit: string | null = null;

    if (!ingIsSeasoning) {
      const num = parseFloat(ingQuantity);
      if (isNaN(num) || num <= 0) {
        showWarning(
          'Định lượng không hợp lệ',
          'Số lượng phải là số thực lớn hơn 0, hoặc chọn "Gia vị nêm nếm vừa đủ".'
        );
        return;
      }
      if (!ingUnit.trim()) {
        showWarning('Thiếu đơn vị tính', 'Vui lòng nhập đơn vị đo lường (g, kg, ml, muỗng...).');
        return;
      }
      parsedQty = num;
      parsedUnit = ingUnit.trim();
    }

    setSubmittingIng(true);
    try {
      if (editingIngId) {
        const existing = ingredients.find(i => i.id === editingIngId);
        const updated = await RecipeApi.updateIngredient(recipeId, editingIngId, {
          name: ingName.trim(),
          quantity: parsedQty,
          unit: parsedUnit,
          notes: ingNotes.trim() || null,
          orderIndex: existing?.orderIndex ?? ingredients.length + 1,
        });
        setIngredients(prev =>
          prev.map(item => (item.id === editingIngId ? updated : item))
        );
        showSuccess('Đã cập nhật nguyên liệu', `Đã lưu thay đổi cho "${updated.name}".`);
      } else {
        const created = await RecipeApi.addIngredient(recipeId, {
          name: ingName.trim(),
          quantity: parsedQty,
          unit: parsedUnit,
          notes: ingNotes.trim() || null,
          orderIndex: ingredients.length + 1,
        });
        setIngredients(prev => [...prev, created]);
        showSuccess('Đã thêm nguyên liệu', `Đã bổ sung "${created.name}" vào công thức.`);
      }
      resetIngredientForm();
    } catch (err) {
      showError('Lỗi xử lý nguyên liệu', (err as ApiError).message);
    } finally {
      setSubmittingIng(false);
    }
  };

  // =========================================================
  // XỬ LÝ BƯỚC 3: CÁC BƯỚC THỰC HIỆN (POST / PUT / REORDER / DELETE)
  // =========================================================
  const resetStepForm = () => {
    setStepTitle('');
    setStepInstruction('');
    setStepDuration('10');
    setEditingStepId(null);
  };

  const handleSelectEditStep = (step: RecipeStepDto) => {
    setEditingStepId(step.id);
    setStepTitle(step.title ?? '');
    setStepInstruction(step.description);
    setStepDuration(
      step.durationMinutes !== null && step.durationMinutes !== undefined
        ? String(step.durationMinutes)
        : ''
    );
  };

  const handleSubmitStep = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recipeId) {
      showWarning('Chưa lưu Bước 1', 'Vui lòng lưu Thông tin chung ở Bước 1 trước.');
      setActiveStep(1);
      return;
    }

    if (!stepTitle.trim()) {
      showWarning('Thiếu tiêu đề bước nấu', 'Vui lòng nhập tiêu đề ngắn gọn cho bước này.');
      return;
    }
    if (!stepInstruction.trim()) {
      showWarning('Thiếu hướng dẫn chi tiết', 'Vui lòng mô tả cách chế biến ở bước này.');
      return;
    }

    const parsedDuration =
      stepDuration.trim() !== '' && !isNaN(Number(stepDuration)) && Number(stepDuration) >= 0
        ? Number(stepDuration)
        : null;

    setSubmittingStep(true);
    try {
      if (editingStepId) {
        const updated = await RecipeApi.updateStep(recipeId, editingStepId, {
          title: stepTitle.trim(),
          description: stepInstruction.trim(),
          durationMinutes: parsedDuration,
        });
        setSteps(prev =>
          prev
            .map(s => (s.id === editingStepId ? updated : s))
            .sort((a, b) => a.stepNumber - b.stepNumber)
        );
        showSuccess('Đã cập nhật bước chế biến', `Bước ${updated.stepNumber}: ${updated.title}`);
      } else {
        const created = await RecipeApi.addStep(recipeId, {
          stepNumber: steps.length + 1,
          title: stepTitle.trim(),
          description: stepInstruction.trim(),
          durationMinutes: parsedDuration,
        });
        setSteps(prev => [...prev, created].sort((a, b) => a.stepNumber - b.stepNumber));
        showSuccess('Đã thêm bước chế biến', `Bước ${created.stepNumber}: ${created.title}`);
      }
      resetStepForm();
    } catch (err) {
      showError('Lỗi lưu bước chế biến', (err as ApiError).message);
    } finally {
      setSubmittingStep(false);
    }
  };

  // Gọi API PUT /api/v1/recipes/{id}/steps/reorder (Transaction nguyên tử)
  const handleMoveStep = async (index: number, direction: 'up' | 'down') => {
    if (!recipeId) return;
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    if (targetIndex < 0 || targetIndex >= steps.length) return;

    const cloned = [...steps];
    const temp = cloned[index];
    cloned[index] = cloned[targetIndex];
    cloned[targetIndex] = temp;

    const orderedStepIds = cloned.map(s => s.id);
    setReorderingSteps(true);
    try {
      const reordered = await RecipeApi.reorderSteps(recipeId, {
        stepIds: orderedStepIds,
      });
      setSteps(reordered);
      showInfo(
        'Đã sắp xếp lại thứ tự bước nấu',
        'Thứ tự các bước 1..N đã được đồng bộ nguyên tử qua API Reorder.'
      );
    } catch (err) {
      showError('Không thể sắp xếp lại bước nấu', (err as ApiError).message);
    } finally {
      setReorderingSteps(false);
    }
  };

  // =========================================================
  // XỬ LÝ BƯỚC 4: UPLOAD & QUẢN LÝ ẢNH MINIO (POST / PATCH / DELETE)
  // =========================================================
  const handleFileChange = (file: File | null) => {
    if (!file) {
      setSelectedFile(null);
      setPreviewUrl(null);
      return;
    }

    // Kiểm tra định dạng file theo SRS Mục 8.3
    if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
      showError(
        'Định dạng ảnh không hợp lệ',
        'Hệ thống chỉ chấp nhận các định dạng: JPEG, PNG, WebP hoặc AVIF.'
      );
      return;
    }

    // Kiểm tra dung lượng tối đa 5MB
    if (file.size > MAX_IMAGE_SIZE_BYTES) {
      const sizeMb = (file.size / (1024 * 1024)).toFixed(2);
      showError(
        'Dung lượng ảnh vượt quá 5MB',
        `File đã chọn nặng ${sizeMb}MB. Vui lòng chọn ảnh nhỏ hơn hoặc bằng 5MB.`
      );
      return;
    }

    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }
    setSelectedFile(file);
    setPreviewUrl(URL.createObjectURL(file));
    if (!imgAltText.trim()) {
      setImgAltText(`${title || 'Món ăn'} - ${file.name.replace(/\.[^/.]+$/, '')}`);
    }
    if (images.length === 0) {
      setImgIsPrimary(true);
    }
  };

  const handleUploadImage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recipeId) {
      showWarning('Chưa lưu Bước 1', 'Vui lòng lưu Thông tin chung ở Bước 1 trước.');
      setActiveStep(1);
      return;
    }
    if (!selectedFile) {
      showWarning('Chưa chọn file ảnh', 'Vui lòng chọn một bức ảnh món ăn từ máy tính.');
      return;
    }

    setUploadingImg(true);
    try {
      const uploaded = await RecipeApi.uploadImage(
        recipeId,
        selectedFile,
        imgAltText.trim() || title,
        imgIsPrimary || images.length === 0
      );

      setImages(prev => {
        const base = uploaded.isPrimary ? prev.map(i => ({ ...i, isPrimary: false })) : [...prev];
        return [...base, uploaded];
      });
      setEditingAltMap(prev => ({ ...prev, [uploaded.id]: uploaded.altText || '' }));
      setSelectedFile(null);
      setPreviewUrl(null);
      setImgAltText('');
      setImgIsPrimary(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
      showSuccess(
        'Tải ảnh lên MinIO thành công',
        uploaded.isPrimary
          ? 'Ảnh đã được đặt làm Ảnh đại diện chính (Primary).'
          : 'Ảnh đã được thêm vào thư viện món ăn.'
      );
    } catch (err) {
      showError('Upload ảnh thất bại', (err as ApiError).message);
    } finally {
      setUploadingImg(false);
    }
  };

  const handleSetPrimaryImage = async (img: RecipeImageDto) => {
    if (!recipeId || img.isPrimary) return;
    try {
      const updated = await RecipeApi.updateImage(recipeId, img.id, {
        altText: editingAltMap[img.id] ?? img.altText,
        isPrimary: true,
        orderIndex: img.orderIndex,
      });
      setImages(prev =>
        prev.map(item => ({
          ...item,
          isPrimary: item.id === updated.id,
        }))
      );
      showSuccess('Đã đổi ảnh đại diện chính', `Ảnh "${updated.altText || 'Món ăn'}" đã là Primary.`);
    } catch (err) {
      showError('Không thể đặt ảnh chính', (err as ApiError).message);
    }
  };

  const handleSaveAltText = async (img: RecipeImageDto) => {
    if (!recipeId) return;
    const newAlt = editingAltMap[img.id] ?? '';
    try {
      const updated = await RecipeApi.updateImage(recipeId, img.id, {
        altText: newAlt.trim() || null,
        isPrimary: img.isPrimary,
        orderIndex: img.orderIndex,
      });
      setImages(prev => prev.map(item => (item.id === img.id ? updated : item)));
      showSuccess('Đã lưu mô tả ảnh (Alt Text)', 'Metadata ảnh đã được cập nhật qua PATCH API.');
    } catch (err) {
      showError('Cập nhật metadata thất bại', (err as ApiError).message);
    }
  };

  // =========================================================
  // XỬ LÝ XÓA (NGUYÊN LIỆU / BƯỚC NẤU / HÌNH ẢNH)
  // =========================================================
  const handleConfirmDelete = async () => {
    if (!deleteConfirm || !recipeId) return;
    const { type, id, label } = deleteConfirm;
    setDeleteConfirm(null);

    try {
      if (type === 'ingredient') {
        await RecipeApi.deleteIngredient(recipeId, id);
        setIngredients(prev => prev.filter(i => i.id !== id));
        if (editingIngId === id) resetIngredientForm();
        showSuccess('Đã xóa nguyên liệu', `Đã xóa "${label}" khỏi danh sách.`);
      } else if (type === 'step') {
        await RecipeApi.deleteStep(recipeId, id);
        setSteps(prev =>
          prev
            .filter(s => s.id !== id)
            .map((s, idx) => ({ ...s, stepNumber: idx + 1 }))
        );
        if (editingStepId === id) resetStepForm();
        showSuccess(
          'Đã xóa bước chế biến',
          'Các bước còn lại đã được tự động đánh lại số thứ tự liên tục 1..N.'
        );
      } else if (type === 'image') {
        await RecipeApi.deleteImage(recipeId, id);
        setImages(prev => {
          const remaining = prev.filter(i => i.id !== id);
          if (remaining.length > 0 && !remaining.some(i => i.isPrimary)) {
            remaining[0] = { ...remaining[0], isPrimary: true };
          }
          return remaining;
        });
        showSuccess('Đã xóa hình ảnh', 'Ảnh đã được gỡ khỏi công thức và MinIO Storage.');
      }
    } catch (err) {
      showError('Thao tác xóa thất bại', (err as ApiError).message);
    }
  };

  // Xuất bản ngay từ Bước 4
  const handlePublishNow = async () => {
    if (!recipeId) return;
    if (ingredients.length === 0 || steps.length === 0) {
      showWarning(
        'Chưa đủ điều kiện xuất bản',
        'Công thức cần có ít nhất 1 nguyên liệu (Bước 2) và 1 bước thực hiện (Bước 3) trước khi Xuất bản.'
      );
      return;
    }

    try {
      await RecipeApi.publishRecipe(recipeId, xmin);
      showSuccess('Xuất bản công thức thành công!', `Món "${title}" đã được công khai.`);
      router.push('/dashboard/recipes');
    } catch (err) {
      const apiErr = err as ApiError;
      if (apiErr.status === 409) {
        setConflictState({ isOpen: true, detail: apiErr.message });
      } else {
        showError('Không thể xuất bản', apiErr.message);
      }
    }
  };

  if (loadingInitial) {
    return (
      <main className="dashboard-content">
        <div className="section-card">
          <div className="empty-state">
            <p className="empty-title">Đang tải dữ liệu chi tiết công thức...</p>
            <p className="empty-desc">
              Đồng bộ Thông tin chung, Nguyên liệu, Các bước chế biến, Hình ảnh MinIO và Concurrency Token (xmin).
            </p>
          </div>
        </div>
      </main>
    );
  }

  return (
    <>
      {/* HEADER */}
      <header className="dashboard-header">
        <div className="header-title-area">
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '6px' }}>
            <Link href="/dashboard/recipes" className="btn btn-secondary btn-sm">
              <ChevronLeftIcon size={16} />
              <span>Danh sách công thức</span>
            </Link>
            {recipeId && (
              <span className="status-badge badge-draft">
                ID: {recipeId.slice(0, 8)}... {xmin ? `• If-Match xmin="${xmin}"` : ''}
              </span>
            )}
          </div>
          <h1>
            {mode === 'create'
              ? 'Tạo Công Thức Nấu Ăn Mới (Multi-step Wizard)'
              : `Chỉnh Sửa Công Thức: ${title || '...'}`}
          </h1>
          <p>
            {mode === 'create'
              ? 'Quy trình 4 bước chuẩn SRS: Thông tin chung ➔ Nguyên liệu định lượng ➔ Các bước thực hiện ➔ Upload Ảnh MinIO'
              : 'Cập nhật toàn diện nội dung công thức với cơ chế kiểm soát xung đột đồng thời Optimistic Concurrency (If-Match: xmin)'}
          </p>
        </div>

        <div className="header-actions">
          {recipeId && (
            <button
              type="button"
              className="btn btn-primary btn-sm"
              onClick={handlePublishNow}
              title="Xuất bản công thức ngay"
            >
              <CheckCircleIcon size={16} />
              <span>Hoàn tất & Xuất bản</span>
            </button>
          )}
        </div>
      </header>

      <main className="dashboard-content">
        {/* Thanh Stepper 4 Bước dạng Tabs responsive */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 mb-8 p-1.5 bg-[var(--bg-surface-alt)]/60 rounded-2xl border border-[var(--border-subtle)]">
          <button
            type="button"
            className={`flex items-center gap-3 p-3.5 rounded-xl text-left transition-all duration-200 cursor-pointer ${
              activeStep === 1
                ? 'bg-[var(--bg-surface)] shadow-xs border border-[var(--border-subtle)] text-[var(--primary)]'
                : 'hover:bg-[var(--bg-surface)]/50 text-[var(--text-secondary)]'
            }`}
            onClick={() => setActiveStep(1)}
          >
            <div
              className={`w-9 h-9 rounded-xl flex items-center justify-center font-bold text-sm shrink-0 transition-colors ${
                recipeId && activeStep !== 1
                  ? 'bg-emerald-100 text-emerald-700'
                  : activeStep === 1
                  ? 'bg-[var(--primary)] text-white shadow-xs'
                  : 'bg-[var(--bg-surface-alt)] text-[var(--text-muted)]'
              }`}
            >
              {recipeId && activeStep !== 1 ? <CheckIcon size={16} /> : '1'}
            </div>
            <div className="min-w-0">
              <div className="text-sm font-bold truncate">Bước 1: Thông tin chung</div>
              <div className="text-xs text-[var(--text-muted)] truncate">Tiêu đề, thời gian, dinh dưỡng</div>
            </div>
          </button>

          <button
            type="button"
            className={`flex items-center gap-3 p-3.5 rounded-xl text-left transition-all duration-200 cursor-pointer ${
              activeStep === 2
                ? 'bg-[var(--bg-surface)] shadow-xs border border-[var(--border-subtle)] text-[var(--primary)]'
                : 'hover:bg-[var(--bg-surface)]/50 text-[var(--text-secondary)]'
            }`}
            onClick={() => {
              if (!recipeId) {
                showInfo('Lưu Bước 1 trước', 'Hãy bấm Lưu Bước 1 để khởi tạo công thức trước khi thêm nguyên liệu.');
                return;
              }
              setActiveStep(2);
            }}
          >
            <div
              className={`w-9 h-9 rounded-xl flex items-center justify-center font-bold text-sm shrink-0 transition-colors ${
                ingredients.length > 0 && activeStep !== 2
                  ? 'bg-emerald-100 text-emerald-700'
                  : activeStep === 2
                  ? 'bg-[var(--primary)] text-white shadow-xs'
                  : 'bg-[var(--bg-surface-alt)] text-[var(--text-muted)]'
              }`}
            >
              {ingredients.length > 0 && activeStep !== 2 ? <CheckIcon size={16} /> : '2'}
            </div>
            <div className="min-w-0">
              <div className="text-sm font-bold truncate">Bước 2: Nguyên liệu ({ingredients.length})</div>
              <div className="text-xs text-[var(--text-muted)] truncate">Định lượng số thực & đơn vị</div>
            </div>
          </button>

          <button
            type="button"
            className={`flex items-center gap-3 p-3.5 rounded-xl text-left transition-all duration-200 cursor-pointer ${
              activeStep === 3
                ? 'bg-[var(--bg-surface)] shadow-xs border border-[var(--border-subtle)] text-[var(--primary)]'
                : 'hover:bg-[var(--bg-surface)]/50 text-[var(--text-secondary)]'
            }`}
            onClick={() => {
              if (!recipeId) {
                showInfo('Lưu Bước 1 trước', 'Hãy bấm Lưu Bước 1 để khởi tạo công thức trước khi thêm bước nấu.');
                return;
              }
              setActiveStep(3);
            }}
          >
            <div
              className={`w-9 h-9 rounded-xl flex items-center justify-center font-bold text-sm shrink-0 transition-colors ${
                steps.length > 0 && activeStep !== 3
                  ? 'bg-emerald-100 text-emerald-700'
                  : activeStep === 3
                  ? 'bg-[var(--primary)] text-white shadow-xs'
                  : 'bg-[var(--bg-surface-alt)] text-[var(--text-muted)]'
              }`}
            >
              {steps.length > 0 && activeStep !== 3 ? <CheckIcon size={16} /> : '3'}
            </div>
            <div className="min-w-0">
              <div className="text-sm font-bold truncate">Bước 3: Bước thực hiện ({steps.length})</div>
              <div className="text-xs text-[var(--text-muted)] truncate">Hướng dẫn & sắp xếp Reorder</div>
            </div>
          </button>

          <button
            type="button"
            className={`flex items-center gap-3 p-3.5 rounded-xl text-left transition-all duration-200 cursor-pointer ${
              activeStep === 4
                ? 'bg-[var(--bg-surface)] shadow-xs border border-[var(--border-subtle)] text-[var(--primary)]'
                : 'hover:bg-[var(--bg-surface)]/50 text-[var(--text-secondary)]'
            }`}
            onClick={() => {
              if (!recipeId) {
                showInfo('Lưu Bước 1 trước', 'Hãy bấm Lưu Bước 1 để khởi tạo công thức trước khi tải ảnh.');
                return;
              }
              setActiveStep(4);
            }}
          >
            <div
              className={`w-9 h-9 rounded-xl flex items-center justify-center font-bold text-sm shrink-0 transition-colors ${
                images.length > 0 && activeStep !== 4
                  ? 'bg-emerald-100 text-emerald-700'
                  : activeStep === 4
                  ? 'bg-[var(--primary)] text-white shadow-xs'
                  : 'bg-[var(--bg-surface-alt)] text-[var(--text-muted)]'
              }`}
            >
              {images.length > 0 && activeStep !== 4 ? <CheckIcon size={16} /> : '4'}
            </div>
            <div className="min-w-0">
              <div className="text-sm font-bold truncate">Bước 4: Ảnh MinIO ({images.length})</div>
              <div className="text-xs text-[var(--text-muted)] truncate">Upload ≤ 5MB & chọn Primary</div>
            </div>
          </button>
        </div>

        {/* =========================================================
            NỘI DUNG BƯỚC 1: THÔNG TIN CHUNG & DINH DƯỠNG
           ========================================================= */}
        {activeStep === 1 && (
          <div className="bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl shadow-xs overflow-hidden">
            <div className="p-5 sm:p-6 border-b border-[var(--border-subtle)] flex items-center justify-between flex-wrap gap-3">
              <div>
                <h2 className="text-lg sm:text-xl font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
                  <BookOpenIcon size={20} />
                  <span>Bước 1: Thông tin cơ bản & Giá trị Dinh dưỡng</span>
                </h2>
                <p className="text-xs sm:text-sm text-[var(--text-muted)] mt-1 mb-0">
                  Cung cấp tên món ăn, phân loại danh mục, khẩu phần phục vụ và thông số dinh dưỡng trên mỗi khẩu phần.
                </p>
              </div>
              {recipeStatus && (
                <Badge variant={recipeStatus === 'Draft' ? 'statusDraft' : 'statusPublished'} className="text-xs">
                  Trạng thái: {recipeStatus}
                </Badge>
              )}
            </div>

            <div className="p-5 sm:p-6 space-y-6">
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
                <div className="space-y-1.5">
                  <div className="flex items-center justify-between">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Tiêu đề công thức <span className="text-[var(--danger)]">*</span>
                    </label>
                    <span className="text-xs text-[var(--text-muted)]">{title.length}/250 ký tự</span>
                  </div>
                  <Input
                    type="text"
                    placeholder="Ví dụ: Phở Bò Truyền Thống Hà Nội Nước Trong..."
                    value={title}
                    maxLength={250}
                    onChange={e => setTitle(e.target.value)}
                  />
                  {step1Errors.title && <p className="text-xs text-[var(--danger)] mt-1">{step1Errors.title}</p>}
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Danh mục món ăn <span className="text-[var(--danger)]">*</span>
                    </label>
                    <Select
                      value={categoryId}
                      onChange={e => setCategoryId(e.target.value)}
                    >
                      {categories.map(cat => (
                        <option key={cat.id} value={cat.id}>
                          {cat.name}
                        </option>
                      ))}
                    </Select>
                  </div>

                  <div className="space-y-1.5">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Độ khó thực hiện <span className="text-[var(--danger)]">*</span>
                    </label>
                    <Select
                      value={String(difficulty)}
                      onChange={e => setDifficulty(e.target.value as DifficultyLevel)}
                    >
                      <option value="Easy">Dễ (Easy) — Phù hợp người mới</option>
                      <option value="Medium">Trung bình (Medium) — Gia đình</option>
                      <option value="Hard">Khó (Hard) — Chuẩn Đầu bếp</option>
                    </Select>
                  </div>
                </div>
              </div>

              <div className="space-y-1.5">
                <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                  Mô tả tổng quan & câu chuyện món ăn <span className="text-[var(--danger)]">*</span>
                </label>
                <Textarea
                  placeholder="Giới thiệu hương vị đặc trưng, nguồn gốc hoặc bí quyết giúp món ăn tròn vị..."
                  value={description}
                  onChange={e => setDescription(e.target.value)}
                  className="min-h-[100px]"
                />
                {step1Errors.description && (
                  <p className="text-xs text-[var(--danger)] mt-1">{step1Errors.description}</p>
                )}
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                    <ClockIcon size={14} /> Thời gian sơ chế (phút)
                  </label>
                  <Input
                    type="number"
                    min={0}
                    max={1440}
                    value={prepTimeMinutes}
                    onChange={e => setPrepTimeMinutes(Number(e.target.value))}
                  />
                  {step1Errors.prepTime && <p className="text-xs text-[var(--danger)] mt-1">{step1Errors.prepTime}</p>}
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                    <FlameIcon size={14} /> Thời gian nấu (phút)
                  </label>
                  <Input
                    type="number"
                    min={0}
                    max={1440}
                    value={cookTimeMinutes}
                    onChange={e => setCookTimeMinutes(Number(e.target.value))}
                  />
                  {step1Errors.cookTime && <p className="text-xs text-[var(--danger)] mt-1">{step1Errors.cookTime}</p>}
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                    <UsersIcon size={14} /> Khẩu phần (người) <span className="text-[var(--danger)]">*</span>
                  </label>
                  <Input
                    type="number"
                    min={1}
                    max={100}
                    value={servings}
                    onChange={e => setServings(Number(e.target.value))}
                  />
                  <p className="text-[11px] text-[var(--text-muted)] mt-1">
                    Cơ sở tính toán tự động khẩu phần (Serving Scaler).
                  </p>
                  {step1Errors.servings && <p className="text-xs text-[var(--danger)] mt-1">{step1Errors.servings}</p>}
                </div>
              </div>

              {/* Bảng thông tin Dinh dưỡng tùy chọn (RecipeNutrition) */}
              <div className="p-4 sm:p-5 rounded-2xl bg-[var(--bg-surface-alt)]/60 border border-[var(--border-subtle)] space-y-4">
                <div className="flex items-center justify-between flex-wrap gap-2">
                  <label className="flex items-center gap-2 cursor-pointer text-sm font-semibold text-[var(--text-primary)]">
                    <input
                      type="checkbox"
                      checked={hasNutrition}
                      onChange={e => setHasNutrition(e.target.checked)}
                      className="w-4 h-4 rounded text-[var(--primary)] accent-[var(--primary)] cursor-pointer"
                    />
                    <span>Bổ sung thông số Dinh dưỡng trên 1 khẩu phần (RecipeNutrition)</span>
                  </label>
                  <Badge variant="outline" className="text-xs">
                    Tùy chọn theo SRS
                  </Badge>
                </div>

                {hasNutrition && (
                  <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3 pt-2">
                    <div className="space-y-1">
                      <label className="text-xs text-[var(--text-muted)] font-medium">Calo (kcal)</label>
                      <Input
                        type="number"
                        min={0}
                        value={nutrition.calories ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, calories: Number(e.target.value) }))
                        }
                        className="text-xs py-1"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-xs text-[var(--text-muted)] font-medium">Chất đạm (g)</label>
                      <Input
                        type="number"
                        step="0.1"
                        min={0}
                        value={nutrition.protein ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, protein: Number(e.target.value) }))
                        }
                        className="text-xs py-1"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-xs text-[var(--text-muted)] font-medium">Chất béo (g)</label>
                      <Input
                        type="number"
                        step="0.1"
                        min={0}
                        value={nutrition.fat ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, fat: Number(e.target.value) }))
                        }
                        className="text-xs py-1"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-xs text-[var(--text-muted)] font-medium">Tinh bột (g)</label>
                      <Input
                        type="number"
                        step="0.1"
                        min={0}
                        value={nutrition.carbohydrates ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, carbohydrates: Number(e.target.value) }))
                        }
                        className="text-xs py-1"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-xs text-[var(--text-muted)] font-medium">Chất xơ (g)</label>
                      <Input
                        type="number"
                        step="0.1"
                        min={0}
                        value={nutrition.fiber ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, fiber: Number(e.target.value) }))
                        }
                        className="text-xs py-1"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-xs text-[var(--text-muted)] font-medium">Natri (mg)</label>
                      <Input
                        type="number"
                        step="1"
                        min={0}
                        value={nutrition.sodium ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, sodium: Number(e.target.value) }))
                        }
                        className="text-xs py-1"
                      />
                    </div>
                  </div>
                )}
              </div>

              {/* Thanh điều hướng footer Bước 1 */}
              <div className="flex items-center justify-between pt-4 border-t border-[var(--border-subtle)] flex-wrap gap-3">
                <div className="text-xs text-[var(--text-muted)]">
                  {recipeId
                    ? `Đang chỉnh sửa công thức #${recipeId.slice(0, 8)} (Header If-Match: "${xmin ?? 'N/A'}")`
                    : 'Bước 1 sẽ khởi tạo bản nháp (Draft) để lấy ID cho các bước tiếp theo.'}
                </div>
                <div className="flex items-center gap-3">
                  {recipeId && (
                    <Button
                      type="button"
                      variant="outline"
                      disabled={savingStep1}
                      onClick={() => handleSaveStep1(false)}
                      className="gap-2"
                    >
                      <SaveIcon size={16} />
                      <span>{savingStep1 ? 'Đang lưu...' : 'Lưu thông tin'}</span>
                    </Button>
                  )}
                  <Button
                    type="button"
                    variant="default"
                    disabled={savingStep1}
                    onClick={() => handleSaveStep1(true)}
                    className="gap-2"
                  >
                    <span>
                      {savingStep1
                        ? 'Đang xử lý...'
                        : recipeId
                          ? 'Lưu & Tiếp tục Bước 2'
                          : 'Khởi tạo Công thức & Sang Bước 2'}
                    </span>
                    <ChevronRightIcon size={16} />
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* =========================================================
            NỘI DUNG BƯỚC 2: QUẢN LÝ NGUYÊN LIỆU (3 API TV4)
           ========================================================= */}
        {activeStep === 2 && (
          <div className="bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl shadow-xs overflow-hidden">
            <div className="p-5 sm:p-6 border-b border-[var(--border-subtle)] flex items-center justify-between flex-wrap gap-3">
              <div>
                <h2 className="text-lg sm:text-xl font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
                  <ListIcon size={20} />
                  <span>Bước 2: Danh sách Nguyên liệu & Định lượng ({ingredients.length} nguyên liệu)</span>
                </h2>
                <p className="text-xs sm:text-sm text-[var(--text-muted)] mt-1 mb-0">
                  Sử dụng 3 API: <code>POST /ingredients</code>, <code>PUT /ingredients/&#123;id&#125;</code>,{' '}
                  <code>DELETE /ingredients/&#123;id&#125;</code>. Hỗ trợ số thực (decimal) và gia vị &ldquo;vừa đủ&rdquo; (nullable).
                </p>
              </div>
              <Badge variant="secondary" className="text-xs">
                Khẩu phần gốc: {servings} người
              </Badge>
            </div>

            <div className="p-5 sm:p-6 space-y-6">
              {/* Form thêm / cập nhật nguyên liệu */}
              <form
                onSubmit={handleSubmitIngredient}
                className="p-4 sm:p-5 rounded-2xl bg-[var(--bg-surface-alt)]/60 border border-[var(--border-subtle)] space-y-4"
              >
                <div className="flex items-center justify-between flex-wrap gap-2">
                  <h3 className="text-sm font-bold text-[var(--text-primary)] m-0">
                    {editingIngId ? 'Chỉnh sửa nguyên liệu đang chọn' : 'Thêm nguyên liệu mới'}
                  </h3>
                  <label className="flex items-center gap-2 cursor-pointer text-xs font-semibold text-[var(--text-primary)]">
                    <input
                      type="checkbox"
                      checked={ingIsSeasoning}
                      onChange={e => setIngIsSeasoning(e.target.checked)}
                      className="w-4 h-4 rounded text-[var(--primary)] accent-[var(--primary)] cursor-pointer"
                    />
                    <span>Gia vị nêm nếm &ldquo;vừa đủ&rdquo; (Quantity & Unit = null theo Quyết định E1, E2)</span>
                  </label>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Tên nguyên liệu <span className="text-[var(--danger)]">*</span>
                    </label>
                    <Input
                      type="text"
                      placeholder="VD: Thịt thăn bò, Xương ống..."
                      value={ingName}
                      onChange={e => setIngName(e.target.value)}
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Số lượng {!ingIsSeasoning && <span className="text-[var(--danger)]">*</span>}
                    </label>
                    <Input
                      type="number"
                      step="0.01"
                      min="0.01"
                      placeholder={ingIsSeasoning ? 'Vừa đủ (Nullable)' : 'VD: 1.5 hoặc 500'}
                      disabled={ingIsSeasoning}
                      value={ingIsSeasoning ? '' : ingQuantity}
                      onChange={e => setIngQuantity(e.target.value)}
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Đơn vị {!ingIsSeasoning && <span className="text-[var(--danger)]">*</span>}
                    </label>
                    <Input
                      type="text"
                      list="common-units-list"
                      placeholder={ingIsSeasoning ? 'Không áp dụng' : 'g, kg, ml, muỗng...'}
                      disabled={ingIsSeasoning}
                      value={ingIsSeasoning ? '' : ingUnit}
                      onChange={e => setIngUnit(e.target.value)}
                    />
                    <datalist id="common-units-list">
                      {COMMON_UNITS.map(u => (
                        <option key={u} value={u} />
                      ))}
                    </datalist>
                  </div>

                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Ghi chú sơ chế
                    </label>
                    <Input
                      type="text"
                      placeholder="VD: Thái lát mỏng ngang thớ..."
                      value={ingNotes}
                      onChange={e => setIngNotes(e.target.value)}
                    />
                  </div>
                </div>

                <div className="flex items-center justify-end gap-2.5 pt-2">
                  {editingIngId && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={resetIngredientForm}
                    >
                      Hủy sửa
                    </Button>
                  )}
                  <Button
                    type="submit"
                    variant="default"
                    size="sm"
                    disabled={submittingIng}
                    className="gap-2"
                  >
                    <PlusCircleIcon size={16} />
                    <span>
                      {submittingIng
                        ? 'Đang lưu...'
                        : editingIngId
                          ? 'Cập nhật nguyên liệu (PUT)'
                          : 'Thêm vào danh sách (POST)'}
                    </span>
                  </Button>
                </div>
              </form>

              {/* Danh sách nguyên liệu đã thêm */}
              {ingredients.length === 0 ? (
                <div className="text-center py-10 px-4 rounded-2xl border border-dashed border-[var(--border-subtle)] bg-[var(--bg-surface)]">
                  <p className="text-base font-bold text-[var(--text-primary)] mb-1">Chưa có nguyên liệu nào</p>
                  <p className="text-xs text-[var(--text-muted)] max-w-sm mx-auto mb-0">
                    Hãy nhập tên nguyên liệu, số lượng định lượng và bấm &ldquo;Thêm vào danh sách&rdquo; ở biểu mẫu trên.
                  </p>
                </div>
              ) : (
                <div className="space-y-2.5">
                  {ingredients.map((ing, idx) => (
                    <div
                      key={ing.id}
                      className="flex items-center justify-between p-3.5 sm:p-4 rounded-xl border border-[var(--border-subtle)] bg-[var(--bg-surface)] hover:border-[var(--primary-subtle)] transition-colors gap-3"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="w-7 h-7 rounded-full bg-[var(--bg-surface-alt)] text-[var(--text-secondary)] font-bold text-xs flex items-center justify-center shrink-0">
                          {idx + 1}
                        </div>
                        <div className="min-w-0">
                          <div className="font-bold text-sm text-[var(--text-primary)] flex items-center gap-2 flex-wrap">
                            <span>{ing.name}</span>
                            <Badge
                              variant={ing.quantity !== null && ing.quantity !== undefined ? "subtle" as const : "secondary"}
                              className="text-xs"
                            >
                              {ing.quantity !== null && ing.quantity !== undefined
                                ? `${ing.quantity} ${ing.unit || ''}`
                                : 'Gia vị vừa đủ'}
                            </Badge>
                          </div>
                          {ing.notes && (
                            <p className="text-xs text-[var(--text-muted)] mt-0.5 mb-0 truncate">
                              Ghi chú: {ing.notes}
                            </p>
                          )}
                        </div>
                      </div>

                      <div className="flex items-center gap-1.5 shrink-0">
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => handleSelectEditIngredient(ing)}
                          className="h-8 px-2.5 gap-1 text-xs"
                        >
                          <EditIcon size={14} />
                          <span>Sửa</span>
                        </Button>
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          onClick={() =>
                            setDeleteConfirm({
                              isOpen: true,
                              type: 'ingredient',
                              id: ing.id,
                              label: ing.name,
                            })
                          }
                          className="h-8 px-2.5 gap-1 text-xs"
                        >
                          <TrashIcon size={14} />
                          <span>Xóa</span>
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Thanh điều hướng footer Bước 2 */}
              <div className="flex items-center justify-between pt-4 border-t border-[var(--border-subtle)] flex-wrap gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setActiveStep(1)}
                  className="gap-2"
                >
                  <ChevronLeftIcon size={16} />
                  <span>Quay lại Bước 1</span>
                </Button>
                <Button
                  type="button"
                  variant="default"
                  onClick={() => setActiveStep(3)}
                  className="gap-2"
                >
                  <span>Tiếp tục Bước 3: Các bước thực hiện</span>
                  <ChevronRightIcon size={16} />
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* =========================================================
            NỘI DUNG BƯỚC 3: CÁC BƯỚC THỰC HIỆN & REORDER (4 API TV4)
           ========================================================= */}
        {activeStep === 3 && (
          <div className="bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl shadow-xs overflow-hidden">
            <div className="p-5 sm:p-6 border-b border-[var(--border-subtle)] flex items-center justify-between flex-wrap gap-3">
              <div>
                <h2 className="text-lg sm:text-xl font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
                  <LayersIcon size={20} />
                  <span>Bước 3: Các bước Chế biến & Sắp xếp Reorder ({steps.length} bước)</span>
                </h2>
                <p className="text-xs sm:text-sm text-[var(--text-muted)] mt-1 mb-0">
                  Sử dụng 4 API: <code>POST /steps</code>, <code>PUT /steps/&#123;id&#125;</code>,{' '}
                  <code>PUT /steps/reorder</code> (Transaction nguyên tử), và <code>DELETE /steps/&#123;id&#125;</code>.
                </p>
              </div>
            </div>

            <div className="p-5 sm:p-6 space-y-6">
              <form
                onSubmit={handleSubmitStep}
                className="p-4 sm:p-5 rounded-2xl bg-[var(--bg-surface-alt)]/60 border border-[var(--border-subtle)] space-y-4"
              >
                <h3 className="text-sm font-bold text-[var(--text-primary)] m-0">
                  {editingStepId
                    ? 'Chỉnh sửa bước chế biến đang chọn'
                    : `Thêm Bước thứ ${steps.length + 1}`}
                </h3>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                      Tiêu đề bước làm <span className="text-[var(--danger)]">*</span>
                    </label>
                    <Input
                      type="text"
                      placeholder="VD: Sơ chế & chần xương bò khử mùi..."
                      value={stepTitle}
                      onChange={e => setStepTitle(e.target.value)}
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)] flex items-center gap-1.5">
                      <ClockIcon size={14} /> Thời gian hẹn giờ (phút - tùy chọn)
                    </label>
                    <Input
                      type="number"
                      min={0}
                      max={1440}
                      placeholder="VD: 15 (Hỗ trợ đồng hồ đếm ngược)"
                      value={stepDuration}
                      onChange={e => setStepDuration(e.target.value)}
                    />
                  </div>
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                    Hướng dẫn chi tiết thao tác <span className="text-[var(--danger)]">*</span>
                  </label>
                  <Textarea
                    placeholder="Mô tả chi tiết nhiệt độ lửa, thời gian canh chỉnh và mẹo nhận biết khi đạt yêu cầu..."
                    value={stepInstruction}
                    onChange={e => setStepInstruction(e.target.value)}
                    className="min-h-[90px]"
                  />
                </div>

                <div className="flex items-center justify-end gap-2.5 pt-2">
                  {editingStepId && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={resetStepForm}
                    >
                      Hủy sửa
                    </Button>
                  )}
                  <Button
                    type="submit"
                    variant="default"
                    size="sm"
                    disabled={submittingStep}
                    className="gap-2"
                  >
                    <PlusCircleIcon size={16} />
                    <span>
                      {submittingStep
                        ? 'Đang lưu...'
                        : editingStepId
                          ? 'Cập nhật bước làm (PUT)'
                          : `Thêm Bước ${steps.length + 1} (POST)`}
                    </span>
                  </Button>
                </div>
              </form>

              {/* Danh sách các bước & nút Reorder Lên/Xuống */}
              {steps.length === 0 ? (
                <div className="text-center py-10 px-4 rounded-2xl border border-dashed border-[var(--border-subtle)] bg-[var(--bg-surface)]">
                  <p className="text-base font-bold text-[var(--text-primary)] mb-1">Chưa có bước thực hiện nào</p>
                  <p className="text-xs text-[var(--text-muted)] max-w-sm mx-auto mb-0">
                    Hãy chia nhỏ quy trình nấu ăn thành từng bước rõ ràng kèm thời gian hẹn giờ đếm ngược.
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {steps.map((step, index) => (
                    <div
                      key={step.id}
                      className="flex items-start justify-between p-4 sm:p-5 rounded-2xl border border-[var(--border-subtle)] bg-[var(--bg-surface)] hover:border-[var(--primary-subtle)] transition-colors gap-3"
                    >
                      <div className="flex items-start gap-3.5 flex-1 min-w-0">
                        <div className="w-8 h-8 rounded-full bg-[var(--primary)] text-white font-extrabold text-sm flex items-center justify-center shrink-0 shadow-xs">
                          {step.stepNumber}
                        </div>
                        <div className="flex-1 min-w-0 space-y-1.5">
                          <div className="flex items-center gap-2 flex-wrap">
                            <span className="font-bold text-sm sm:text-base text-[var(--text-primary)]">
                              Bước {step.stepNumber}: {step.title}
                            </span>
                            {step.durationMinutes !== null && step.durationMinutes !== undefined && (
                              <Badge variant="medium" className="text-xs">
                                <ClockIcon size={12} className="mr-1" /> Hẹn giờ: {step.durationMinutes} phút
                              </Badge>
                            )}
                          </div>
                          <p className="text-xs sm:text-sm text-[var(--text-secondary)] leading-relaxed m-0 whitespace-pre-line">
                            {step.description}
                          </p>
                        </div>
                      </div>

                      <div className="flex items-center gap-1 shrink-0 pt-0.5">
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          title="Di chuyển bước lên"
                          disabled={index === 0 || reorderingSteps}
                          onClick={() => handleMoveStep(index, 'up')}
                          className="h-8 w-8 text-xs text-[var(--text-muted)]"
                        >
                          <ArrowUpIcon size={14} />
                        </Button>
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          title="Di chuyển bước xuống"
                          disabled={index === steps.length - 1 || reorderingSteps}
                          onClick={() => handleMoveStep(index, 'down')}
                          className="h-8 w-8 text-xs text-[var(--text-muted)]"
                        >
                          <ArrowDownIcon size={14} />
                        </Button>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => handleSelectEditStep(step)}
                          className="h-8 px-2.5 gap-1 text-xs"
                        >
                          <EditIcon size={14} />
                          <span>Sửa</span>
                        </Button>
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          onClick={() =>
                            setDeleteConfirm({
                              isOpen: true,
                              type: 'step',
                              id: step.id,
                              label: `Bước ${step.stepNumber}: ${step.title}`,
                            })
                          }
                          className="h-8 px-2.5 gap-1 text-xs"
                        >
                          <TrashIcon size={14} />
                          <span>Xóa</span>
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Thanh điều hướng footer Bước 3 */}
              <div className="flex items-center justify-between pt-4 border-t border-[var(--border-subtle)] flex-wrap gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setActiveStep(2)}
                  className="gap-2"
                >
                  <ChevronLeftIcon size={16} />
                  <span>Quay lại Bước 2</span>
                </Button>
                <Button
                  type="button"
                  variant="default"
                  onClick={() => setActiveStep(4)}
                  className="gap-2"
                >
                  <span>Tiếp tục Bước 4: Upload Ảnh MinIO</span>
                  <ChevronRightIcon size={16} />
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* =========================================================
            NỘI DUNG BƯỚC 4: UPLOAD ẢNH MINIO & GALLERY (3 API TV4)
           ========================================================= */}
        {activeStep === 4 && (
          <div className="bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl shadow-xs overflow-hidden">
            <div className="p-5 sm:p-6 border-b border-[var(--border-subtle)] flex items-center justify-between flex-wrap gap-3">
              <div>
                <h2 className="text-lg sm:text-xl font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
                  <ImageIcon size={20} />
                  <span>Bước 4: Quản lý Hình ảnh & MinIO Object Storage ({images.length} ảnh)</span>
                </h2>
                <p className="text-xs sm:text-sm text-[var(--text-muted)] mt-1 mb-0">
                  Sử dụng 3 API: <code>POST /images</code> (Multipart upload ≤ 5MB),{' '}
                  <code>PATCH /images/&#123;imageId&#125;</code> (Đặt ảnh chính Primary & AltText), và{' '}
                  <code>DELETE /images/&#123;imageId&#125;</code>.
                </p>
              </div>
            </div>

            <div className="p-5 sm:p-6 space-y-6">
              {/* Form Upload Multipart/Form-Data */}
              <form onSubmit={handleUploadImage}>
                <div
                  className="p-8 sm:p-10 border-2 border-dashed border-[var(--border-subtle)] hover:border-[var(--primary)] rounded-3xl bg-[var(--bg-surface-alt)]/50 text-center cursor-pointer transition-colors"
                  onClick={() => fileInputRef.current?.click()}
                  onDragOver={e => e.preventDefault()}
                  onDrop={e => {
                    e.preventDefault();
                    const droppedFile = e.dataTransfer.files?.[0] || null;
                    handleFileChange(droppedFile);
                  }}
                >
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/jpeg,image/png,image/webp,image/avif"
                    className="hidden"
                    onChange={e => handleFileChange(e.target.files?.[0] || null)}
                  />
                  <div className="w-12 h-12 rounded-full bg-[var(--primary-light)] text-[var(--primary)] flex items-center justify-center mx-auto mb-3 shadow-xs">
                    <UploadCloudIcon size={24} />
                  </div>
                  <p className="text-sm sm:text-base font-bold text-[var(--text-primary)] mb-1">
                    {selectedFile
                      ? `Đã chọn: ${selectedFile.name} (${(selectedFile.size / 1024).toFixed(1)} KB)`
                      : 'Nhấn để chọn ảnh hoặc kéo thả ảnh món ăn vào đây'}
                  </p>
                  <p className="text-xs text-[var(--text-muted)] mb-0">
                    Định dạng hỗ trợ: JPEG, PNG, WebP, AVIF • Dung lượng tối đa: 5MB / ảnh
                  </p>
                </div>

                {selectedFile && (
                  <div className="mt-4 p-4 sm:p-5 rounded-2xl bg-[var(--bg-surface-alt)]/60 border border-[var(--border-subtle)] flex flex-col sm:flex-row gap-5 items-center">
                    <div className="relative w-44 h-32 rounded-xl overflow-hidden border border-[var(--border-subtle)] bg-stone-100 shrink-0">
                      {previewUrl && (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img src={previewUrl} alt="Xem trước ảnh tải lên" className="w-full h-full object-cover" />
                      )}
                    </div>

                    <div className="flex-1 w-full space-y-3">
                      <div className="space-y-1">
                        <label className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary)]">
                          Mô tả ảnh (Alt Text chuẩn SEO & Accessibility)
                        </label>
                        <Input
                          type="text"
                          placeholder="VD: Tô phở bò truyền thống Hà Nội nóng hổi..."
                          value={imgAltText}
                          onChange={e => setImgAltText(e.target.value)}
                        />
                      </div>

                      <div className="flex items-center justify-between flex-wrap gap-3">
                        <label className="flex items-center gap-2 cursor-pointer text-xs font-semibold text-[var(--text-primary)]">
                          <input
                            type="checkbox"
                            checked={imgIsPrimary || images.length === 0}
                            onChange={e => setImgIsPrimary(e.target.checked)}
                            className="w-4 h-4 rounded text-[var(--primary)] accent-[var(--primary)] cursor-pointer"
                          />
                          <span>Đặt làm Ảnh đại diện chính (IsPrimary) của công thức</span>
                        </label>

                        <div className="flex items-center gap-2">
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={() => handleFileChange(null)}
                          >
                            Hủy chọn
                          </Button>
                          <Button
                            type="submit"
                            variant="default"
                            size="sm"
                            disabled={uploadingImg}
                            className="gap-2"
                          >
                            <UploadCloudIcon size={16} />
                            <span>{uploadingImg ? 'Đang tải lên MinIO...' : 'Tải ảnh lên MinIO (POST)'}</span>
                          </Button>
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </form>

              {/* Thư viện ảnh đã tải lên */}
              {images.length === 0 ? (
                <div className="text-center py-10 px-4 rounded-2xl border border-dashed border-[var(--border-subtle)] bg-[var(--bg-surface)]">
                  <p className="text-base font-bold text-[var(--text-primary)] mb-1">Chưa có hình ảnh nào trong Album</p>
                  <p className="text-xs text-[var(--text-muted)] max-w-sm mx-auto mb-0">
                    Tải lên ít nhất 1 bức ảnh chất lượng cao để làm ảnh bìa (Primary Image) thu hút độc giả.
                  </p>
                </div>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {images.map(img => (
                    <div
                      key={img.id}
                      className={`rounded-2xl border overflow-hidden bg-[var(--bg-surface)] flex flex-col transition-all ${
                        img.isPrimary
                          ? 'border-[var(--primary)] ring-2 ring-[var(--primary-subtle)] shadow-sm'
                          : 'border-[var(--border-subtle)]'
                      }`}
                    >
                      <div className="relative w-full pt-[60%] overflow-hidden bg-[var(--bg-surface-alt)]">
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img
                          src={img.originalUrl}
                          alt={img.altText || title}
                          className="absolute inset-0 w-full h-full object-cover"
                          loading="lazy"
                        />
                        {img.isPrimary && (
                          <div className="absolute top-2.5 left-2.5">
                            <Badge variant="default" className="text-xs font-bold gap-1 shadow-sm">
                              <StarIcon size={12} /> Ảnh chính
                            </Badge>
                          </div>
                        )}
                      </div>

                      <div className="p-4 flex-1 flex flex-col justify-between gap-3">
                        <div className="space-y-1">
                          <label className="text-[11px] font-bold uppercase tracking-wider text-[var(--text-muted)]">
                            Alt Text (SEO):
                          </label>
                          <div className="flex gap-2">
                            <Input
                              type="text"
                              className="text-xs py-1 h-8"
                              value={editingAltMap[img.id] ?? img.altText ?? ''}
                              onChange={e =>
                                setEditingAltMap(prev => ({ ...prev, [img.id]: e.target.value }))
                              }
                            />
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              className="h-8 px-2.5 text-xs shrink-0"
                              title="Lưu Alt Text (PATCH)"
                              onClick={() => handleSaveAltText(img)}
                            >
                              Lưu
                            </Button>
                          </div>
                        </div>

                        <div className="flex items-center justify-between pt-2 border-t border-[var(--border-subtle)]">
                          {!img.isPrimary ? (
                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={() => handleSetPrimaryImage(img)}
                              className="h-8 px-2 text-xs text-[var(--text-secondary)] gap-1"
                            >
                              <StarIcon size={13} />
                              <span>Đặt làm ảnh chính</span>
                            </Button>
                          ) : (
                            <span className="text-xs font-bold text-[var(--primary)]">
                              ★ Ảnh đại diện
                            </span>
                          )}

                          <Button
                            type="button"
                            variant="destructive"
                            size="sm"
                            onClick={() =>
                              setDeleteConfirm({
                                isOpen: true,
                                type: 'image',
                                id: img.id,
                                label: img.altText || 'Hình ảnh món ăn',
                              })
                            }
                            className="h-8 px-2 text-xs"
                          >
                            <TrashIcon size={13} />
                          </Button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Thanh điều hướng footer Bước 4 */}
              <div className="flex items-center justify-between pt-4 border-t border-[var(--border-subtle)] flex-wrap gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setActiveStep(3)}
                  className="gap-2"
                >
                  <ChevronLeftIcon size={16} />
                  <span>Quay lại Bước 3</span>
                </Button>

                <div className="flex items-center gap-3">
                  <Link href="/dashboard/recipes">
                    <Button variant="outline">Lưu Nháp & Về Danh sách</Button>
                  </Link>
                  <Button
                    type="button"
                    variant="default"
                    onClick={handlePublishNow}
                    className="gap-2"
                  >
                    <CheckCircleIcon size={18} />
                    <span>Hoàn tất & Xuất bản Công thức</span>
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}
      </main>

      {/* Modal xử lý xung đột 409 Concurrency Conflict */}
      <ConflictDialog
        isOpen={conflictState.isOpen}
        recipeTitle={title}
        errorMessage={conflictState.detail}
        currentXmin={xmin}
        onClose={() => setConflictState({ isOpen: false })}
        onReload={() => {
          setConflictState({ isOpen: false });
          if (recipeId) {
            loadRecipeData(recipeId);
          }
        }}
      />

      {/* Modal xác nhận xóa Nguyên liệu / Bước nấu / Hình ảnh */}
      <ConfirmDialog
        isOpen={Boolean(deleteConfirm?.isOpen)}
        title="Xác nhận xóa mục này?"
        message={`Bạn có chắc chắn muốn xóa "${deleteConfirm?.label ?? ''}" khỏi công thức không? Hành động này sẽ gọi trực tiếp API DELETE lên máy chủ.`}
        confirmLabel="Xác nhận xóa"
        isDestructive={true}
        onCancel={() => setDeleteConfirm(null)}
        onConfirm={handleConfirmDelete}
      />
    </>
  );
}
