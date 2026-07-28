import React from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

export const Pagination = ({ page, pageSize = 10, totalItems = 0, onPageChange }) => {
  const totalPages = Math.ceil(totalItems / pageSize) || 1;

  if (totalPages <= 1) return null;

  return (
    <div className="pagination">
      <button
        className="page-btn"
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
      >
        <ChevronLeft style={{ width: '16px', height: '16px' }} />
      </button>

      {Array.from({ length: totalPages }, (_, i) => i + 1)
        .slice(Math.max(0, page - 3), Math.min(totalPages, page + 2))
        .map((p) => (
          <button
            key={p}
            className={`page-btn ${p === page ? 'active' : ''}`}
            onClick={() => onPageChange(p)}
          >
            {p}
          </button>
        ))}

      <button
        className="page-btn"
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
      >
        <ChevronRight style={{ width: '16px', height: '16px' }} />
      </button>
    </div>
  );
};
