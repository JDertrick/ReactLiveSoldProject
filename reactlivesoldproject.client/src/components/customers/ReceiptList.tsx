import { useState } from 'react';
import { Receipt } from '../../types/wallet.types';
import { ReceiptDetails } from './ReceiptDetails';
import { DollarSign, Eye, CheckCircle, Send, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { usePostReceipt, useRejectReceipt } from '../../hooks/useWallet';
import { toast } from 'sonner';
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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-32">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!receipts || receipts.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        No se encontraron recibos para este cliente.
      </div>
    );
  }

  return (
    <div className="bg-white shadow overflow-hidden sm:rounded-md">
      <ul role="list" className="divide-y divide-gray-200">
        {receipts.map((receipt) => (
          <li key={receipt.id}>
            <div className="block hover:bg-gray-50">
              <div className="px-4 py-4 sm:px-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-indigo-600 truncate">
                      {receipt.type}
                    </p>
                    {receipt.isPosted ? (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                        <CheckCircle className="h-3 w-3 mr-1" /> Posteado
                      </span>
                    ) : receipt.isRejected ? (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                        <XCircle className="h-3 w-3 mr-1" /> Rechazado
                      </span>
                    ) : (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                        Borrador
                      </span>
                    )}
                  </div>
                  <div className="ml-2 flex-shrink-0 flex">
                    <p className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      receipt.type === 'Deposit' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                    }`}>
                      <DollarSign className="h-3 w-3 mr-1" /> {receipt.totalAmount.toFixed(2)}
                    </p>
                  </div>
                </div>
                <div className="mt-2 sm:flex justify-between">
                  <div className="sm:flex flex-col">
                    <p className="flex items-center text-sm text-gray-500">
                      {receipt.notes || 'Sin notas'}
                    </p>
                    {receipt.isPosted && (
                      <p className="text-xs text-gray-400">
                        Posteado por {receipt.postedByUserName} el {new Date(receipt.postedAt!).toLocaleDateString()}
                      </p>
                    )}
                    {receipt.isRejected && (
                      <p className="text-xs text-gray-400">
                        Rechazado por {receipt.rejectedByUserName} el {new Date(receipt.rejectedAt!).toLocaleDateString()}
                      </p>
                    )}
                  </div>
                  <div className="mt-2 flex items-center text-sm text-gray-500 sm:mt-0">
                    <p>
                      Creado el <time dateTime={receipt.createdAt}>{new Date(receipt.createdAt).toLocaleDateString()}</time>
                    </p>
                    <Button variant="ghost" size="sm" className="ml-2" onClick={() => handleViewDetails(receipt)}>
                      <Eye className="h-4 w-4 mr-1" /> Ver Detalles
                    </Button>
                    {!receipt.isPosted && !receipt.isRejected && (
                      <>
                        <Button variant="ghost" size="sm" className="ml-2 text-blue-600" onClick={() => handlePostClick(receipt)}>
                          <Send className="h-4 w-4 mr-1" /> Postear
                        </Button>
                        <Button variant="ghost" size="sm" className="ml-2 text-red-600" onClick={() => handleRejectClick(receipt)}>
                          <XCircle className="h-4 w-4 mr-1" /> Rechazar
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
