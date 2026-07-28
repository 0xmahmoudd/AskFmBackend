import React from 'react';
import { useNotification } from '../context/NotificationContext';
import { X, CheckCircle, AlertCircle, Info } from 'lucide-react';

export const ToastContainer = () => {
  const { toasts, removeToast } = useNotification();

  if (!toasts.length) return null;

  return (
    <div className="fixed bottom-5 right-5 z-50 flex flex-col gap-2 max-w-sm w-full px-4">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={`flex items-center justify-between p-4 rounded-lg shadow-lg border text-sm transition-all duration-300 ${
            toast.type === 'error'
              ? 'bg-red-50 text-red-800 border-red-200'
              : toast.type === 'success'
              ? 'bg-emerald-50 text-emerald-800 border-emerald-200'
              : 'bg-indigo-50 text-indigo-800 border-indigo-200'
          }`}
        >
          <div className="flex items-center gap-3">
            {toast.type === 'error' ? (
              <AlertCircle className="w-5 h-5 text-red-500 shrink-0" />
            ) : toast.type === 'success' ? (
              <CheckCircle className="w-5 h-5 text-emerald-500 shrink-0" />
            ) : (
              <Info className="w-5 h-5 text-indigo-500 shrink-0" />
            )}
            <p className="font-medium">{toast.message}</p>
          </div>
          <button
            onClick={() => removeToast(toast.id)}
            className="p-1 hover:bg-black/5 rounded-full transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      ))}
    </div>
  );
};
