'use client';

import React, { createContext, useContext, useCallback, ReactNode } from 'react';
import { toast as sonnerToast } from 'sonner';
import { Toaster } from '@/components/ui/sonner';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: string;
  type: ToastType;
  title: string;
  message?: string;
  duration?: number;
}

interface ToastContextType {
  toasts: ToastMessage[];
  showToast: (toast: Omit<ToastMessage, 'id'>) => void;
  showSuccess: (title: string, message?: string) => void;
  showError: (title: string, message?: string) => void;
  showWarning: (title: string, message?: string) => void;
  showInfo: (title: string, message?: string) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

/**
 * ToastProvider tích hợp thư viện Sonner hiện đại (Lab 6 - TV3).
 * Giữ nguyên 100% hợp đồng API của `useToast()` để các trang Dashboard, Auth, Recipe không bị vỡ.
 */
export function ToastProvider({ children }: { children: ReactNode }) {
  const showToast = useCallback(
    ({ type, title, message, duration }: Omit<ToastMessage, 'id'>) => {
      const options = {
        description: message,
        duration: duration ?? 4000,
      };

      switch (type) {
        case 'success':
          sonnerToast.success(title, options);
          break;
        case 'error':
          sonnerToast.error(title, { ...options, duration: duration ?? 6000 });
          break;
        case 'warning':
          sonnerToast.warning(title, { ...options, duration: duration ?? 5000 });
          break;
        case 'info':
        default:
          sonnerToast.info(title, options);
          break;
      }
    },
    []
  );

  const showSuccess = useCallback(
    (title: string, message?: string) => showToast({ type: 'success', title, message }),
    [showToast]
  );
  const showError = useCallback(
    (title: string, message?: string) => showToast({ type: 'error', title, message, duration: 6000 }),
    [showToast]
  );
  const showWarning = useCallback(
    (title: string, message?: string) => showToast({ type: 'warning', title, message, duration: 5000 }),
    [showToast]
  );
  const showInfo = useCallback(
    (title: string, message?: string) => showToast({ type: 'info', title, message }),
    [showToast]
  );
  const removeToast = useCallback((id: string) => {
    sonnerToast.dismiss(id);
  }, []);

  return (
    <ToastContext.Provider
      value={{
        toasts: [],
        showToast,
        showSuccess,
        showError,
        showWarning,
        showInfo,
        removeToast,
      }}
    >
      {children}
      <Toaster richColors position="top-right" closeButton />
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextType {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast phải được sử dụng bên trong <ToastProvider>');
  }
  return context;
}

export default ToastProvider;
