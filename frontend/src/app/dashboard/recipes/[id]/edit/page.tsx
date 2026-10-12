'use client';

import React, { use } from 'react';
import { RecipeWizardEditor } from '@/components/recipes/RecipeWizardEditor';

interface EditRecipePageProps {
  params: Promise<{ id: string }>;
}

export default function EditRecipePage({ params }: EditRecipePageProps) {
  const resolvedParams = use(params);
  return <RecipeWizardEditor mode="edit" initialRecipeId={resolvedParams.id} />;
}
