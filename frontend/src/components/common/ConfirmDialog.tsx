'use client';

import React from 'react';
import { AlertTriangleIcon, TrashIcon, XIcon } from './Icons';

interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  isDestructive?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDialog({
  isOpen,
  title,
  message,
  confirmLabel = 'Xác nhận',
  cancelLabel = 'Hủy bỏ',
  isDestructive = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  if (!isOpen) return null;

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true">
      <div className="modal-card">
        <div className="modal-header">
          <div className={`modal-icon-badge ${isDestructive ? 'badge-danger' : 'badge-primary'}`}>
            {isDestructive ? <TrashIcon size={22} /> : <AlertTriangleIcon size={22} />}
          </div>
          <div>
            <h3 className="modal-title">{title}</h3>
          </div>
          <button onClick={onCancel} className="modal-close-btn" aria-label="Đóng">
            <XIcon size={18} />
          </button>
        </div>

        <div className="modal-body">
          <p className="confirm-message">{message}</p>
        </div>

        <div className="modal-footer">
          <button type="button" onClick={onCancel} className="btn btn-secondary">
            {cancelLabel}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            className={`btn ${isDestructive ? 'btn-danger' : 'btn-primary'}`}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
