import type { ReactNode } from 'react';
import Button from './Button';
import Modal from './Modal';

interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  description?: string;
  children?: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  confirmVariant?: 'primary' | 'secondary' | 'danger';
  confirmDisabled?: boolean;
  loading?: boolean;
  onConfirm: () => void | Promise<void>;
  onClose: () => void;
}

const ConfirmDialog = ({
  isOpen,
  title,
  description,
  children,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  confirmVariant = 'danger',
  confirmDisabled = false,
  loading = false,
  onConfirm,
  onClose,
}: ConfirmDialogProps) => {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="md">
      <div className="space-y-4">
        {description ? <p className="text-sm text-(--app-text-muted)">{description}</p> : null}
        {children}
        <div className="flex flex-wrap justify-end gap-3 pt-2">
          <Button variant="secondary" onClick={onClose} disabled={loading}>
            {cancelLabel}
          </Button>
          <Button
            variant={confirmVariant}
            onClick={onConfirm}
            disabled={confirmDisabled || loading}
          >
            {loading ? 'Working...' : confirmLabel}
          </Button>
        </div>
      </div>
    </Modal>
  );
};

export default ConfirmDialog;
