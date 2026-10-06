'use client';

import React from 'react';
import { AlertTriangleIcon, RefreshCwIcon, XIcon } from './Icons';

interface ConflictDialogProps {
  isOpen: boolean;
  recipeTitle?: string;
  errorMessage?: string;
  currentXmin?: string | number | null;
  onReload: () => void;
  onClose: () => void;
}

export function ConflictDialog({
  isOpen,
  recipeTitle,
  errorMessage,
  currentXmin,
  onReload,
  onClose,
}: ConflictDialogProps) {
  if (!isOpen) return null;

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="conflict-title">
      <div className="modal-card modal-conflict">
        <div className="modal-header">
          <div className="modal-icon-badge badge-warning">
            <AlertTriangleIcon size={24} />
          </div>
          <div>
            <h3 id="conflict-title" className="modal-title text-danger">
              Xung đột Dữ liệu Đồng thời (HTTP 409)
            </h3>
            <p className="modal-subtitle">
              Phát hiện thay đổi ngoài ý muốn từ phiên làm việc khác
            </p>
          </div>
          <button onClick={onClose} className="modal-close-btn" aria-label="Đóng">
            <XIcon size={18} />
          </button>
        </div>

        <div className="modal-body">
          <div className="conflict-alert-box">
            <p className="conflict-main-text">
              Công thức <strong>&ldquo;{recipeTitle || 'này'}&rdquo;</strong> đã được chỉnh sửa hoặc cập nhật trạng thái bởi một phiên làm việc khác trong khi bạn đang thao tác.
            </p>
            {errorMessage && (
              <div className="conflict-detail-code">
                <code>{errorMessage}</code>
              </div>
            )}
            {currentXmin && (
              <div className="conflict-token-info">
                <span>Concurrency Token (xmin) trước đó:</span>
                <span className="token-tag">&ldquo;{currentXmin}&rdquo;</span>
              </div>
            )}
          </div>

          <p className="conflict-guide-text">
            Để đảm bảo tính toàn vẹn dữ liệu và tránh ghi đè thông tin cũ, hệ thống yêu cầu đồng bộ lại phiên bản mới nhất từ máy chủ trước khi thực hiện tiếp.
          </p>
        </div>

        <div className="modal-footer">
          <button type="button" onClick={onClose} className="btn btn-secondary">
            Hủy bỏ
          </button>
          <button
            type="button"
            onClick={() => {
              onReload();
              onClose();
            }}
            className="btn btn-primary btn-with-icon"
          >
            <RefreshCwIcon size={18} />
            <span>Tải lại dữ liệu mới nhất</span>
          </button>
        </div>
      </div>
    </div>
  );
}
