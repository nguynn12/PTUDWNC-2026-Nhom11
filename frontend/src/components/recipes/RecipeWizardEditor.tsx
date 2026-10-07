'use client';

import React, { useState, useEffect, useCallback, useRef } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { RecipeApi, ApiError } from '@/lib/api-client';
import {
  CategoryOptionDto,
  DifficultyLevel,
  RecipeIngredientDto,
  RecipeStepDto,
  RecipeImageDto,
  RecipeNutritionDto,
} from '@/lib/types';
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
    RecipeApi.getCategories().then(cats => {
      setCategories(cats);
      if (cats.length > 0 && !initialRecipeId) {
        setCategoryId(cats[0].id);
      }
    });

    if (mode === 'edit' && initialRecipeId) {
      loadRecipeData(initialRecipeId);
    }
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
    setStepTitle(step.title);
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
        {/* Thanh Stepper 4 Bước */}
        <div className="wizard-stepper">
          <button
            type="button"
            className={`wizard-step-tab ${activeStep === 1 ? 'active' : recipeId ? 'completed' : ''}`}
            onClick={() => setActiveStep(1)}
          >
            <div className="wizard-step-num">{recipeId && activeStep !== 1 ? <CheckIcon size={16} /> : '1'}</div>
            <div>
              <div className="wizard-step-label">Bước 1: Thông tin chung</div>
              <div className="wizard-step-sub">Tiêu đề, thời gian, dinh dưỡng</div>
            </div>
          </button>

          <button
            type="button"
            className={`wizard-step-tab ${activeStep === 2 ? 'active' : ingredients.length > 0 ? 'completed' : ''}`}
            onClick={() => {
              if (!recipeId) {
                showInfo('Lưu Bước 1 trước', 'Hãy bấm Lưu Bước 1 để khởi tạo công thức trước khi thêm nguyên liệu.');
                return;
              }
              setActiveStep(2);
            }}
          >
            <div className="wizard-step-num">
              {ingredients.length > 0 && activeStep !== 2 ? <CheckIcon size={16} /> : '2'}
            </div>
            <div>
              <div className="wizard-step-label">Bước 2: Nguyên liệu ({ingredients.length})</div>
              <div className="wizard-step-sub">Định lượng số thực & đơn vị</div>
            </div>
          </button>

          <button
            type="button"
            className={`wizard-step-tab ${activeStep === 3 ? 'active' : steps.length > 0 ? 'completed' : ''}`}
            onClick={() => {
              if (!recipeId) {
                showInfo('Lưu Bước 1 trước', 'Hãy bấm Lưu Bước 1 để khởi tạo công thức trước khi thêm bước nấu.');
                return;
              }
              setActiveStep(3);
            }}
          >
            <div className="wizard-step-num">
              {steps.length > 0 && activeStep !== 3 ? <CheckIcon size={16} /> : '3'}
            </div>
            <div>
              <div className="wizard-step-label">Bước 3: Bước thực hiện ({steps.length})</div>
              <div className="wizard-step-sub">Hướng dẫn & sắp xếp Reorder</div>
            </div>
          </button>

          <button
            type="button"
            className={`wizard-step-tab ${activeStep === 4 ? 'active' : images.length > 0 ? 'completed' : ''}`}
            onClick={() => {
              if (!recipeId) {
                showInfo('Lưu Bước 1 trước', 'Hãy bấm Lưu Bước 1 để khởi tạo công thức trước khi tải ảnh.');
                return;
              }
              setActiveStep(4);
            }}
          >
            <div className="wizard-step-num">
              {images.length > 0 && activeStep !== 4 ? <CheckIcon size={16} /> : '4'}
            </div>
            <div>
              <div className="wizard-step-label">Bước 4: Ảnh MinIO ({images.length})</div>
              <div className="wizard-step-sub">Upload ≤ 5MB & chọn Primary</div>
            </div>
          </button>
        </div>

        {/* =========================================================
            NỘI DUNG BƯỚC 1: THÔNG TIN CHUNG & DINH DƯỠNG
           ========================================================= */}
        {activeStep === 1 && (
          <div className="section-card">
            <div className="section-header">
              <div>
                <h2 className="section-title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <BookOpenIcon size={20} />
                  <span>Bước 1: Thông tin cơ bản & Giá trị Dinh dưỡng</span>
                </h2>
                <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: '4px' }}>
                  Cung cấp tên món ăn, phân loại danh mục, khẩu phần phục vụ và thông số dinh dưỡng trên mỗi khẩu phần.
                </p>
              </div>
              {recipeStatus && (
                <span className="status-badge badge-draft">Trạng thái: {recipeStatus}</span>
              )}
            </div>

            <div style={{ padding: '24px' }}>
              <div className="form-grid-2">
                <div className="form-group">
                  <label className="form-label">
                    <span>
                      Tiêu đề công thức <span className="form-required">*</span>
                    </span>
                    <span className="form-hint">{title.length}/250 ký tự</span>
                  </label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: Phở Bò Truyền Thống Hà Nội Nước Trong..."
                    value={title}
                    maxLength={250}
                    onChange={e => setTitle(e.target.value)}
                  />
                  {step1Errors.title && <span className="form-error">{step1Errors.title}</span>}
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">
                      <span>
                        Danh mục món ăn <span className="form-required">*</span>
                      </span>
                    </label>
                    <select
                      className="form-select"
                      value={categoryId}
                      onChange={e => setCategoryId(e.target.value)}
                    >
                      {categories.map(cat => (
                        <option key={cat.id} value={cat.id}>
                          {cat.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="form-group">
                    <label className="form-label">
                      <span>
                        Độ khó thực hiện <span className="form-required">*</span>
                      </span>
                    </label>
                    <select
                      className="form-select"
                      value={String(difficulty)}
                      onChange={e => setDifficulty(e.target.value as DifficultyLevel)}
                    >
                      <option value="Easy">Dễ (Easy) — Phù hợp người mới</option>
                      <option value="Medium">Trung bình (Medium) — Gia đình</option>
                      <option value="Hard">Khó (Hard) — Chuẩn Đầu bếp</option>
                    </select>
                  </div>
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">
                  <span>
                    Mô tả tổng quan & câu chuyện món ăn <span className="form-required">*</span>
                  </span>
                </label>
                <textarea
                  className="form-textarea"
                  placeholder="Giới thiệu hương vị đặc trưng, nguồn gốc hoặc bí quyết giúp món ăn tròn vị..."
                  value={description}
                  onChange={e => setDescription(e.target.value)}
                />
                {step1Errors.description && (
                  <span className="form-error">{step1Errors.description}</span>
                )}
              </div>

              <div className="form-grid-3">
                <div className="form-group">
                  <label className="form-label">
                    <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                      <ClockIcon size={15} /> Thời gian sơ chế (phút)
                    </span>
                  </label>
                  <input
                    type="number"
                    min={0}
                    max={1440}
                    className="form-input"
                    value={prepTimeMinutes}
                    onChange={e => setPrepTimeMinutes(Number(e.target.value))}
                  />
                  {step1Errors.prepTime && <span className="form-error">{step1Errors.prepTime}</span>}
                </div>

                <div className="form-group">
                  <label className="form-label">
                    <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                      <FlameIcon size={15} /> Thời gian nấu (phút)
                    </span>
                  </label>
                  <input
                    type="number"
                    min={0}
                    max={1440}
                    className="form-input"
                    value={cookTimeMinutes}
                    onChange={e => setCookTimeMinutes(Number(e.target.value))}
                  />
                  {step1Errors.cookTime && <span className="form-error">{step1Errors.cookTime}</span>}
                </div>

                <div className="form-group">
                  <label className="form-label">
                    <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                      <UsersIcon size={15} /> Số khẩu phần (người) <span className="form-required">*</span>
                    </span>
                  </label>
                  <input
                    type="number"
                    min={1}
                    max={100}
                    className="form-input"
                    value={servings}
                    onChange={e => setServings(Number(e.target.value))}
                  />
                  <span className="form-hint">
                    Dùng làm cơ sở tính toán lại định lượng nguyên liệu tự động (Serving Scaler).
                  </span>
                  {step1Errors.servings && <span className="form-error">{step1Errors.servings}</span>}
                </div>
              </div>

              {/* Bảng thông tin Dinh dưỡng tùy chọn (RecipeNutrition) */}
              <div
                style={{
                  marginTop: '12px',
                  padding: '18px',
                  borderRadius: 'var(--radius-lg)',
                  background: '#FAF9F6',
                  border: '1px solid var(--border-color)',
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <label className="checkbox-row">
                    <input
                      type="checkbox"
                      checked={hasNutrition}
                      onChange={e => setHasNutrition(e.target.checked)}
                    />
                    <span>Bổ sung thông số Dinh dưỡng trên 1 khẩu phần (RecipeNutrition)</span>
                  </label>
                  <span className="status-badge badge-draft">Tùy chọn theo SRS</span>
                </div>

                {hasNutrition && (
                  <div className="form-grid-3" style={{ marginTop: '16px' }}>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label className="form-label">Năng lượng (Calories - kcal)</label>
                      <input
                        type="number"
                        min={0}
                        className="form-input"
                        value={nutrition.calories ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, calories: Number(e.target.value) }))
                        }
                      />
                    </div>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label className="form-label">Chất đạm (Protein - g)</label>
                      <input
                        type="number"
                        step="0.1"
                        min={0}
                        className="form-input"
                        value={nutrition.protein ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, protein: Number(e.target.value) }))
                        }
                      />
                    </div>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label className="form-label">Chất béo (Fat - g)</label>
                      <input
                        type="number"
                        step="0.1"
                        min={0}
                        className="form-input"
                        value={nutrition.fat ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, fat: Number(e.target.value) }))
                        }
                      />
                    </div>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label className="form-label">Tinh bột (Carbs - g)</label>
                      <input
                        type="number"
                        step="0.1"
                        min={0}
                        className="form-input"
                        value={nutrition.carbohydrates ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, carbohydrates: Number(e.target.value) }))
                        }
                      />
                    </div>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label className="form-label">Chất xơ (Fiber - g)</label>
                      <input
                        type="number"
                        step="0.1"
                        min={0}
                        className="form-input"
                        value={nutrition.fiber ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, fiber: Number(e.target.value) }))
                        }
                      />
                    </div>
                    <div className="form-group" style={{ marginBottom: 0 }}>
                      <label className="form-label">Muối khoáng (Sodium - mg)</label>
                      <input
                        type="number"
                        step="1"
                        min={0}
                        className="form-input"
                        value={nutrition.sodium ?? 0}
                        onChange={e =>
                          setNutrition(prev => ({ ...prev, sodium: Number(e.target.value) }))
                        }
                      />
                    </div>
                  </div>
                )}
              </div>

              <div className="wizard-footer-bar">
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                  {recipeId
                    ? `Đang chỉnh sửa công thức #${recipeId.slice(0, 8)} (Header If-Match: "${xmin ?? 'N/A'}")`
                    : 'Bước 1 sẽ khởi tạo bản nháp (Draft) để lấy ID cho 10 API Nguyên liệu, Bước làm & Ảnh.'}
                </div>
                <div style={{ display: 'flex', gap: '10px' }}>
                  {recipeId && (
                    <button
                      type="button"
                      className="btn btn-secondary"
                      disabled={savingStep1}
                      onClick={() => handleSaveStep1(false)}
                    >
                      <SaveIcon size={17} />
                      <span>{savingStep1 ? 'Đang lưu...' : 'Lưu thông tin chung'}</span>
                    </button>
                  )}
                  <button
                    type="button"
                    className="btn btn-primary"
                    disabled={savingStep1}
                    onClick={() => handleSaveStep1(true)}
                  >
                    <span>
                      {savingStep1
                        ? 'Đang xử lý...'
                        : recipeId
                          ? 'Lưu & Tiếp tục Bước 2'
                          : 'Khởi tạo Công thức & Sang Bước 2'}
                    </span>
                    <ChevronRightIcon size={17} />
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* =========================================================
            NỘI DUNG BƯỚC 2: QUẢN LÝ NGUYÊN LIỆU (3 API TV4)
           ========================================================= */}
        {activeStep === 2 && (
          <div className="section-card">
            <div className="section-header">
              <div>
                <h2 className="section-title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <ListIcon size={20} />
                  <span>Bước 2: Danh sách Nguyên liệu & Định lượng ({ingredients.length} nguyên liệu)</span>
                </h2>
                <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: '4px' }}>
                  Sử dụng 3 API: <code>POST /ingredients</code>, <code>PUT /ingredients/&#123;id&#125;</code>,{' '}
                  <code>DELETE /ingredients/&#123;id&#125;</code>. Hỗ trợ số thực (decimal) và gia vị &ldquo;vừa đủ&rdquo; (nullable).
                </p>
              </div>
              <span className="status-badge badge-published">Khẩu phần gốc: {servings} người</span>
            </div>

            <div style={{ padding: '24px' }}>
              {/* Form thêm / cập nhật nguyên liệu */}
              <form
                onSubmit={handleSubmitIngredient}
                style={{
                  background: '#FAF9F6',
                  padding: '20px',
                  borderRadius: 'var(--radius-lg)',
                  border: '1px solid var(--border-color)',
                }}
              >
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    marginBottom: '14px',
                    flexWrap: 'wrap',
                    gap: '8px',
                  }}
                >
                  <h3 style={{ fontSize: '0.96rem', fontWeight: 700 }}>
                    {editingIngId ? 'Chỉnh sửa nguyên liệu đang chọn' : 'Thêm nguyên liệu mới'}
                  </h3>
                  <label className="checkbox-row">
                    <input
                      type="checkbox"
                      checked={ingIsSeasoning}
                      onChange={e => setIngIsSeasoning(e.target.checked)}
                    />
                    <span>Gia vị nêm nếm &ldquo;vừa đủ&rdquo; (Quantity & Unit = null theo Quyết định E1, E2)</span>
                  </label>
                </div>

                <div className="form-grid-4">
                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label">
                      <span>
                        Tên nguyên liệu <span className="form-required">*</span>
                      </span>
                    </label>
                    <input
                      type="text"
                      className="form-input"
                      placeholder="VD: Thịt thăn bò, Xương ống..."
                      value={ingName}
                      onChange={e => setIngName(e.target.value)}
                    />
                  </div>

                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label">
                      <span>Số lượng (decimal) {!ingIsSeasoning && <span className="form-required">*</span>}</span>
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      min="0.01"
                      className="form-input"
                      placeholder={ingIsSeasoning ? 'Vừa đủ (Nullable)' : 'VD: 1.5 hoặc 500'}
                      disabled={ingIsSeasoning}
                      value={ingIsSeasoning ? '' : ingQuantity}
                      onChange={e => setIngQuantity(e.target.value)}
                    />
                  </div>

                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label">
                      <span>Đơn vị tính {!ingIsSeasoning && <span className="form-required">*</span>}</span>
                    </label>
                    <input
                      type="text"
                      list="common-units-list"
                      className="form-input"
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

                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label">Ghi chú sơ chế</label>
                    <input
                      type="text"
                      className="form-input"
                      placeholder="VD: Thái lát mỏng ngang thớ..."
                      value={ingNotes}
                      onChange={e => setIngNotes(e.target.value)}
                    />
                  </div>
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '16px' }}>
                  {editingIngId && (
                    <button type="button" className="btn btn-secondary btn-sm" onClick={resetIngredientForm}>
                      Hủy sửa
                    </button>
                  )}
                  <button type="submit" className="btn btn-primary btn-sm" disabled={submittingIng}>
                    <PlusCircleIcon size={16} />
                    <span>
                      {submittingIng
                        ? 'Đang lưu...'
                        : editingIngId
                          ? 'Cập nhật nguyên liệu (PUT)'
                          : 'Thêm vào danh sách (POST)'}
                    </span>
                  </button>
                </div>
              </form>

              {/* Danh sách nguyên liệu đã thêm */}
              {ingredients.length === 0 ? (
                <div className="empty-state" style={{ padding: '40px 20px' }}>
                  <p className="empty-title">Chưa có nguyên liệu nào</p>
                  <p className="empty-desc">
                    Hãy nhập tên nguyên liệu, số lượng định lượng và bấm &ldquo;Thêm vào danh sách&rdquo; ở biểu mẫu trên.
                  </p>
                </div>
              ) : (
                <div className="dynamic-list">
                  {ingredients.map((ing, idx) => (
                    <div key={ing.id} className="dynamic-card">
                      <div style={{ display: 'flex', alignItems: 'center', gap: '14px' }}>
                        <div className="step-badge-circle">{idx + 1}</div>
                        <div>
                          <div style={{ fontWeight: 700, fontSize: '0.95rem', color: 'var(--text-main)' }}>
                            {ing.name}
                            <span
                              style={{
                                marginLeft: '10px',
                                padding: '3px 10px',
                                borderRadius: '999px',
                                fontSize: '0.78rem',
                                fontWeight: 700,
                                background: ing.quantity !== null ? '#FFEDE6' : '#F3F4F6',
                                color: ing.quantity !== null ? 'var(--primary)' : 'var(--text-muted)',
                              }}
                            >
                              {ing.quantity !== null && ing.quantity !== undefined
                                ? `${ing.quantity} ${ing.unit || ''}`
                                : 'Gia vị vừa đủ'}
                            </span>
                          </div>
                          {ing.notes && (
                            <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: '4px' }}>
                              Ghi chú: {ing.notes}
                            </div>
                          )}
                        </div>
                      </div>

                      <div className="table-actions">
                        <button
                          type="button"
                          className="btn btn-secondary btn-sm"
                          onClick={() => handleSelectEditIngredient(ing)}
                        >
                          <EditIcon size={15} />
                          <span>Sửa</span>
                        </button>
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          onClick={() =>
                            setDeleteConfirm({
                              isOpen: true,
                              type: 'ingredient',
                              id: ing.id,
                              label: ing.name,
                            })
                          }
                        >
                          <TrashIcon size={15} />
                          <span>Xóa</span>
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              <div className="wizard-footer-bar">
                <button type="button" className="btn btn-secondary" onClick={() => setActiveStep(1)}>
                  <ChevronLeftIcon size={17} />
                  <span>Quay lại Bước 1</span>
                </button>
                <button type="button" className="btn btn-primary" onClick={() => setActiveStep(3)}>
                  <span>Tiếp tục Bước 3: Các bước thực hiện</span>
                  <ChevronRightIcon size={17} />
                </button>
              </div>
            </div>
          </div>
        )}

        {/* =========================================================
            NỘI DUNG BƯỚC 3: CÁC BƯỚC THỰC HIỆN & REORDER (4 API TV4)
           ========================================================= */}
        {activeStep === 3 && (
          <div className="section-card">
            <div className="section-header">
              <div>
                <h2 className="section-title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <LayersIcon size={20} />
                  <span>Bước 3: Các bước Chế biến & Sắp xếp Reorder ({steps.length} bước)</span>
                </h2>
                <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: '4px' }}>
                  Sử dụng 4 API: <code>POST /steps</code>, <code>PUT /steps/&#123;id&#125;</code>,{' '}
                  <code>PUT /steps/reorder</code> (Transaction nguyên tử), và <code>DELETE /steps/&#123;id&#125;</code> (Tự động đánh lại số 1..N).
                </p>
              </div>
            </div>

            <div style={{ padding: '24px' }}>
              <form
                onSubmit={handleSubmitStep}
                style={{
                  background: '#FAF9F6',
                  padding: '20px',
                  borderRadius: 'var(--radius-lg)',
                  border: '1px solid var(--border-color)',
                }}
              >
                <h3 style={{ fontSize: '0.96rem', fontWeight: 700, marginBottom: '14px' }}>
                  {editingStepId
                    ? 'Chỉnh sửa bước chế biến đang chọn'
                    : `Thêm Bước thứ ${steps.length + 1}`}
                </h3>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">
                      <span>
                        Tiêu đề bước làm <span className="form-required">*</span>
                      </span>
                    </label>
                    <input
                      type="text"
                      className="form-input"
                      placeholder="VD: Sơ chế & chần xương bò khử mùi..."
                      value={stepTitle}
                      onChange={e => setStepTitle(e.target.value)}
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">
                      <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <ClockIcon size={15} /> Thời gian hẹn giờ bước này (phút - tùy chọn)
                      </span>
                    </label>
                    <input
                      type="number"
                      min={0}
                      max={1440}
                      className="form-input"
                      placeholder="VD: 15 (Hỗ trợ đồng hồ đếm ngược ở Chế độ Nấu ăn)"
                      value={stepDuration}
                      onChange={e => setStepDuration(e.target.value)}
                    />
                  </div>
                </div>

                <div className="form-group" style={{ marginBottom: 0 }}>
                  <label className="form-label">
                    <span>
                      Hướng dẫn chi tiết thao tác <span className="form-required">*</span>
                    </span>
                  </label>
                  <textarea
                    className="form-textarea"
                    placeholder="Mô tả chi tiết nhiệt độ lửa, thời gian canh chỉnh và mẹo nhận biết khi đạt yêu cầu..."
                    value={stepInstruction}
                    onChange={e => setStepInstruction(e.target.value)}
                  />
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '16px' }}>
                  {editingStepId && (
                    <button type="button" className="btn btn-secondary btn-sm" onClick={resetStepForm}>
                      Hủy sửa
                    </button>
                  )}
                  <button type="submit" className="btn btn-primary btn-sm" disabled={submittingStep}>
                    <PlusCircleIcon size={16} />
                    <span>
                      {submittingStep
                        ? 'Đang lưu...'
                        : editingStepId
                          ? 'Cập nhật bước làm (PUT)'
                          : `Thêm Bước ${steps.length + 1} (POST)`}
                    </span>
                  </button>
                </div>
              </form>

              {/* Danh sách các bước & nút Reorder Lên/Xuống */}
              {steps.length === 0 ? (
                <div className="empty-state" style={{ padding: '40px 20px' }}>
                  <p className="empty-title">Chưa có bước thực hiện nào</p>
                  <p className="empty-desc">
                    Hãy chia nhỏ quy trình nấu ăn thành từng bước rõ ràng kèm thời gian hẹn giờ đếm ngược.
                  </p>
                </div>
              ) : (
                <div className="dynamic-list">
                  {steps.map((step, index) => (
                    <div key={step.id} className="dynamic-card">
                      <div style={{ display: 'flex', alignItems: 'flex-start', gap: '14px', flex: 1 }}>
                        <div className="step-badge-circle">{step.stepNumber}</div>
                        <div style={{ flex: 1 }}>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', flexWrap: 'wrap' }}>
                            <span style={{ fontWeight: 800, fontSize: '0.96rem', color: 'var(--text-main)' }}>
                              Bước {step.stepNumber}: {step.title}
                            </span>
                            {step.durationMinutes !== null && step.durationMinutes !== undefined && (
                              <span className="status-badge badge-archived">
                                <ClockIcon size={12} /> Hẹn giờ: {step.durationMinutes} phút
                              </span>
                            )}
                          </div>
                          <p
                            style={{
                              fontSize: '0.88rem',
                              color: 'var(--text-muted)',
                              marginTop: '6px',
                              lineHeight: 1.6,
                            }}
                          >
                            {step.description}
                          </p>
                        </div>
                      </div>

                      <div className="table-actions">
                        <button
                          type="button"
                          className="btn btn-icon-only"
                          title="Di chuyển bước lên (PUT /steps/reorder)"
                          disabled={index === 0 || reorderingSteps}
                          onClick={() => handleMoveStep(index, 'up')}
                        >
                          <ArrowUpIcon size={16} />
                        </button>
                        <button
                          type="button"
                          className="btn btn-icon-only"
                          title="Di chuyển bước xuống (PUT /steps/reorder)"
                          disabled={index === steps.length - 1 || reorderingSteps}
                          onClick={() => handleMoveStep(index, 'down')}
                        >
                          <ArrowDownIcon size={16} />
                        </button>
                        <button
                          type="button"
                          className="btn btn-secondary btn-sm"
                          onClick={() => handleSelectEditStep(step)}
                        >
                          <EditIcon size={15} />
                          <span>Sửa</span>
                        </button>
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          onClick={() =>
                            setDeleteConfirm({
                              isOpen: true,
                              type: 'step',
                              id: step.id,
                              label: `Bước ${step.stepNumber}: ${step.title}`,
                            })
                          }
                        >
                          <TrashIcon size={15} />
                          <span>Xóa</span>
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              <div className="wizard-footer-bar">
                <button type="button" className="btn btn-secondary" onClick={() => setActiveStep(2)}>
                  <ChevronLeftIcon size={17} />
                  <span>Quay lại Bước 2 (Nguyên liệu)</span>
                </button>
                <button type="button" className="btn btn-primary" onClick={() => setActiveStep(4)}>
                  <span>Tiếp tục Bước 4: Upload Ảnh MinIO</span>
                  <ChevronRightIcon size={17} />
                </button>
              </div>
            </div>
          </div>
        )}

        {/* =========================================================
            NỘI DUNG BƯỚC 4: UPLOAD ẢNH MINIO & GALLERY (3 API TV4)
           ========================================================= */}
        {activeStep === 4 && (
          <div className="section-card">
            <div className="section-header">
              <div>
                <h2 className="section-title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <ImageIcon size={20} />
                  <span>Bước 4: Quản lý Hình ảnh & MinIO Object Storage ({images.length} ảnh)</span>
                </h2>
                <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: '4px' }}>
                  Sử dụng 3 API: <code>POST /images</code> (Multipart upload ≤ 5MB),{' '}
                  <code>PATCH /images/&#123;imageId&#125;</code> (Đặt ảnh chính Primary & AltText), và{' '}
                  <code>DELETE /images/&#123;imageId&#125;</code>.
                </p>
              </div>
            </div>

            <div style={{ padding: '24px' }}>
              {/* Form Upload Multipart/Form-Data */}
              <form onSubmit={handleUploadImage}>
                <div
                  className="upload-dropzone"
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
                    style={{ display: 'none' }}
                    onChange={e => handleFileChange(e.target.files?.[0] || null)}
                  />
                  <div
                    style={{
                      width: '52px',
                      height: '52px',
                      borderRadius: '999px',
                      background: '#FFEDE6',
                      color: 'var(--primary)',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto 12px',
                    }}
                  >
                    <UploadCloudIcon size={26} />
                  </div>
                  <p style={{ fontWeight: 700, fontSize: '0.96rem', color: 'var(--text-main)' }}>
                    {selectedFile
                      ? `Đã chọn: ${selectedFile.name} (${(selectedFile.size / 1024).toFixed(1)} KB)`
                      : 'Nhấn để chọn ảnh hoặc kéo thả ảnh món ăn vào đây'}
                  </p>
                  <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '4px' }}>
                    Định dạng hỗ trợ: JPEG, PNG, WebP, AVIF • Dung lượng tối đa: 5MB / ảnh
                  </p>
                </div>

                {selectedFile && (
                  <div
                    style={{
                      marginTop: '16px',
                      padding: '18px',
                      borderRadius: 'var(--radius-lg)',
                      background: '#FAF9F6',
                      border: '1px solid var(--border-color)',
                      display: 'grid',
                      gridTemplateColumns: '180px 1fr',
                      gap: '18px',
                      alignItems: 'center',
                    }}
                  >
                    <div className="media-thumb-wrapper" style={{ borderRadius: 'var(--radius-md)' }}>
                      {previewUrl && (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img src={previewUrl} alt="Xem trước ảnh tải lên" className="media-thumb-img" />
                      )}
                    </div>

                    <div>
                      <div className="form-group">
                        <label className="form-label">
                          <span>Mô tả ảnh (Alt Text chuẩn SEO & Accessibility)</span>
                        </label>
                        <input
                          type="text"
                          className="form-input"
                          placeholder="VD: Tô phở bò truyền thống Hà Nội nóng hổi..."
                          value={imgAltText}
                          onChange={e => setImgAltText(e.target.value)}
                        />
                      </div>

                      <div
                        style={{
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'space-between',
                          flexWrap: 'wrap',
                          gap: '12px',
                        }}
                      >
                        <label className="checkbox-row">
                          <input
                            type="checkbox"
                            checked={imgIsPrimary || images.length === 0}
                            onChange={e => setImgIsPrimary(e.target.checked)}
                          />
                          <span>Đặt làm Ảnh đại diện chính (IsPrimary) của công thức</span>
                        </label>

                        <div style={{ display: 'flex', gap: '8px' }}>
                          <button
                            type="button"
                            className="btn btn-secondary btn-sm"
                            onClick={() => handleFileChange(null)}
                          >
                            Hủy chọn
                          </button>
                          <button type="submit" className="btn btn-primary btn-sm" disabled={uploadingImg}>
                            <UploadCloudIcon size={16} />
                            <span>{uploadingImg ? 'Đang tải lên MinIO...' : 'Tải ảnh lên MinIO (POST)'}</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </form>

              {/* Thư viện ảnh đã tải lên */}
              {images.length === 0 ? (
                <div className="empty-state" style={{ padding: '40px 20px' }}>
                  <p className="empty-title">Chưa có hình ảnh nào trong Album</p>
                  <p className="empty-desc">
                    Tải lên ít nhất 1 bức ảnh chất lượng cao để làm ảnh bìa (Primary Image) thu hút độc giả.
                  </p>
                </div>
              ) : (
                <div className="media-gallery-grid">
                  {images.map(img => (
                    <div key={img.id} className={`media-card ${img.isPrimary ? 'is-primary' : ''}`}>
                      <div className="media-thumb-wrapper">
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img
                          src={img.originalUrl}
                          alt={img.altText || title}
                          className="media-thumb-img"
                          loading="lazy"
                        />
                        {img.isPrimary && (
                          <span className="primary-ribbon">
                            <StarIcon size={12} /> Ảnh chính (Primary)
                          </span>
                        )}
                      </div>

                      <div className="media-card-body">
                        <div>
                          <label className="form-label" style={{ fontSize: '0.76rem', marginBottom: '4px' }}>
                            Alt Text (SEO):
                          </label>
                          <div style={{ display: 'flex', gap: '6px' }}>
                            <input
                              type="text"
                              className="form-input"
                              style={{ padding: '6px 10px', fontSize: '0.82rem' }}
                              value={editingAltMap[img.id] ?? img.altText ?? ''}
                              onChange={e =>
                                setEditingAltMap(prev => ({ ...prev, [img.id]: e.target.value }))
                              }
                            />
                            <button
                              type="button"
                              className="btn btn-secondary btn-sm"
                              title="Lưu Alt Text (PATCH)"
                              onClick={() => handleSaveAltText(img)}
                            >
                              Lưu
                            </button>
                          </div>
                        </div>

                        <div
                          style={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                            marginTop: 'auto',
                            paddingTop: '8px',
                            borderTop: '1px solid var(--border-light)',
                          }}
                        >
                          {!img.isPrimary ? (
                            <button
                              type="button"
                              className="btn btn-secondary btn-sm"
                              onClick={() => handleSetPrimaryImage(img)}
                            >
                              <StarIcon size={14} />
                              <span>Đặt làm ảnh chính</span>
                            </button>
                          ) : (
                            <span style={{ fontSize: '0.78rem', fontWeight: 700, color: 'var(--primary)' }}>
                              ★ Đang là ảnh bìa
                            </span>
                          )}

                          <button
                            type="button"
                            className="btn btn-danger btn-sm"
                            onClick={() =>
                              setDeleteConfirm({
                                isOpen: true,
                                type: 'image',
                                id: img.id,
                                label: img.altText || 'Hình ảnh món ăn',
                              })
                            }
                          >
                            <TrashIcon size={14} />
                            <span>Xóa</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              <div className="wizard-footer-bar">
                <button type="button" className="btn btn-secondary" onClick={() => setActiveStep(3)}>
                  <ChevronLeftIcon size={17} />
                  <span>Quay lại Bước 3 (Các bước làm)</span>
                </button>

                <div style={{ display: 'flex', gap: '10px' }}>
                  <Link href="/dashboard/recipes" className="btn btn-secondary">
                    Lưu Nháp & Về Danh sách
                  </Link>
                  <button type="button" className="btn btn-primary" onClick={handlePublishNow}>
                    <CheckCircleIcon size={18} />
                    <span>Hoàn tất & Xuất bản Công thức</span>
                  </button>
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
