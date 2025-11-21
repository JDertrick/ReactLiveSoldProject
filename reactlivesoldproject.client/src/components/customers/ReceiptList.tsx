import { useState } from 'react';
import { Receipt } from '../../types/wallet.types';
import { ReceiptDetails } from './ReceiptDetails';
import { ArrowDownCircle, ArrowUpCircle, Eye, CheckCircle, Send, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { usePostReceipt, useRejectReceipt } from '../../hooks/useWallet';
import { toast } from 'sonner';
import { formatCurrency } from '@/utils/currencyHelper';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface ReceiptListProps {
  receipts: Receipt[];
  isLoading: boolean;
}

export const ReceiptList = ({ receipts, isLoading }: ReceiptListProps) => {
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [selectedReceipt, setSelectedReceipt] = useState<Receipt | null>(null);
  const [isConfirmPostOpen, setIsConfirmPostOpen] = useState(false);
  const [isConfirmRejectOpen, setIsConfirmRejectOpen] = useState(false);
  const postReceiptMutation = usePostReceipt();
  const rejectReceiptMutation = useRejectReceipt();

  const handleViewDetails = (receipt: Receipt) => {
    setSelectedReceipt(receipt);
    setIsDetailsModalOpen(true);
  };

  const handlePostClick = (receipt: Receipt) => {
    setSelectedReceipt(receipt);
    setIsConfirmPostOpen(true);
  };

  const handleRejectClick = (receipt: Receipt) => {
    setSelectedReceipt(receipt);
    setIsConfirmRejectOpen(true);
  };

  const handleConfirmPost = async () => {
    if (!selectedReceipt) return;

    try {
      await postReceiptMutation.mutateAsync(selectedReceipt.id);
      toast.success("¡Recibo posteado con éxito!");
      setIsConfirmPostOpen(false);
      setSelectedReceipt(null);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Error al postear el recibo.";
      toast.error(errorMessage);
      console.error("Error posting receipt:", error);
    }
  };

  const handleConfirmReject = async () => {
    if (!selectedReceipt) return;

    try {
      await rejectReceiptMutation.mutateAsync(selectedReceipt.id);
      toast.success("¡Recibo rechazado con éxito!");
      setIsConfirmRejectOpen(false);
      setSelectedReceipt(null);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Error al rechazar el recibo.";
      toast.error(errorMessage);
      console.error("Error rejecting receipt:", error);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('es-ES', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-32">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!receipts || receipts.length === 0) {
    return (
      <div className="bg-white rounded-xl border border-gray-200 text-center py-12">
        <div className="flex flex-col items-center">
          <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
            <svg className="w-8 h-8 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <p className="text-gray-500 text-sm">No se encontraron recibos para este cliente.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <ul role="list" className="divide-y divide-gray-100">
        {receipts.map((receipt) => (
          <li key={receipt.id}>
            <div className="block hover:bg-gray-50 transition-colors">
              <div className="px-6 py-4">
                <div className="flex items-center gap-4">
                  {/* Icon */}
                  <div className={`flex-shrink-0 w-12 h-12 rounded-full flex items-center justify-center ${
                    receipt.type === 'Deposit' ? 'bg-green-100' : 'bg-red-100'
                  }`}>
                    {receipt.type === 'Deposit' ? (
                      <ArrowDownCircle className="w-6 h-6 text-green-600" />
                    ) : (
                      <ArrowUpCircle className="w-6 h-6 text-red-600" />
                    )}
                  </div>

                  {/* Receipt Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <p className="text-base font-semibold text-gray-900">
                        {receipt.notes || (receipt.type === 'Deposit' ? 'Depósito' : 'Retiro')}
                      </p>
                    </div>
                    <p className="text-sm text-gray-500">
                      {receipt.type} • Ref: {receipt.id.substring(0, 8)}
                    </p>
                  </div>

                  {/* Date & Time */}
                  <div className="hidden sm:flex flex-col items-end mr-4">
                    <p className="text-sm font-medium text-gray-900">
                      {formatDate(receipt.createdAt)}
                    </p>
                    <p className="text-xs text-gray-500">
                      {formatTime(receipt.createdAt)}
                    </p>
                  </div>

                  {/* Created By */}
                  <div className="hidden md:flex items-center gap-2 mr-4">
                    <div className="w-8 h-8 bg-indigo-100 rounded-full flex items-center justify-center">
                      <span className="text-xs font-semibold text-indigo-600">
                        {receipt.createdByUserName?.substring(0, 2).toUpperCase() || 'DP'}
                      </span>
                    </div>
                    <div className="flex flex-col">
                      <p className="text-sm font-medium text-gray-700">
                        {receipt.createdByUserName || 'N/A'}
                      </p>
                    </div>
                  </div>

                  {/* Status Badge */}
                  <div className="flex-shrink-0">
                    {receipt.isPosted ? (
                      <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-semibold bg-green-100 text-green-700">
                        <CheckCircle className="h-3.5 w-3.5" />
                        Posteado
                      </span>
                    ) : receipt.isRejected ? (
                      <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-semibold bg-red-100 text-red-700">
                        <XCircle className="h-3.5 w-3.5" />
                        Rechazado
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-semibold bg-yellow-100 text-yellow-700">
                        Borrador
                      </span>
                    )}
                  </div>

                  {/* Amount */}
                  <div className="flex-shrink-0 min-w-[140px] text-right">
                    <p className={`text-lg font-bold ${
                      receipt.type === 'Deposit' ? 'text-green-600' : 'text-red-600'
                    }`}>
                      {receipt.type === 'Deposit' ? '+' : '-'}{formatCurrency(receipt.totalAmount)}
                    </p>
                  </div>

                  {/* Actions */}
                  <div className="flex-shrink-0 flex items-center gap-2 ml-4">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleViewDetails(receipt)}
                      className="text-indigo-600 hover:text-indigo-700 hover:bg-indigo-50"
                    >
                      <Eye className="h-4 w-4 mr-1" /> Ver
                    </Button>
                    {!receipt.isPosted && !receipt.isRejected && (
                      <>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-blue-600 hover:text-blue-700 hover:bg-blue-50"
                          onClick={() => handlePostClick(receipt)}
                        >
                          <Send className="h-4 w-4 mr-1" /> Postear
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                          onClick={() => handleRejectClick(receipt)}
                        >
                          <XCircle className="h-4 w-4" />
                        </Button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>

      {selectedReceipt && (
        <ReceiptDetails
          isOpen={isDetailsModalOpen}
          onClose={() => setIsDetailsModalOpen(false)}
          receipt={selectedReceipt}
        />
      )}

      <AlertDialog open={isConfirmPostOpen} onOpenChange={setIsConfirmPostOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Estás seguro de que quieres postear este recibo?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción afectará permanentemente el saldo de la billetera del cliente y no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmPost} disabled={postReceiptMutation.isPending}>
              {postReceiptMutation.isPending ? "Posteando..." : "Postear Recibo"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={isConfirmRejectOpen} onOpenChange={setIsConfirmRejectOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Estás seguro de que quieres rechazar este recibo?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción marcará el recibo como rechazado y no se podrá postear. Esto no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmReject} disabled={rejectReceiptMutation.isPending}>
              {rejectReceiptMutation.isPending ? "Rechazando..." : "Rechazar Recibo"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};
