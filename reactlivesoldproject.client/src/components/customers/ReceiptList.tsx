import { useState } from 'react';
import { Receipt } from '../../types/wallet.types';
import { ReceiptDetails } from './ReceiptDetails';
import { DollarSign, Eye, CheckCircle, Send } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { usePostReceipt } from '../../hooks/useWallet';
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
  const postReceiptMutation = usePostReceipt();

  const handleViewDetails = (receipt: Receipt) => {
    setSelectedReceipt(receipt);
    setIsDetailsModalOpen(true);
  };

  const handlePostClick = (receipt: Receipt) => {
    setSelectedReceipt(receipt);
    setIsConfirmPostOpen(true);
  };

  const handleConfirmPost = async () => {
    if (!selectedReceipt) return;

    try {
      await postReceiptMutation.mutateAsync(selectedReceipt.id);
      toast.success("Receipt posted successfully!");
      setIsConfirmPostOpen(false);
      setSelectedReceipt(null);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || "Failed to post receipt.";
      toast.error(errorMessage);
      console.error("Error posting receipt:", error);
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
        No receipts found for this customer.
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
                        <CheckCircle className="h-3 w-3 mr-1" /> Posted
                      </span>
                    ) : (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                        Draft
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
                  <div className="sm:flex">
                    <p className="flex items-center text-sm text-gray-500">
                      {receipt.notes || 'No notes'}
                    </p>
                  </div>
                  <div className="mt-2 flex items-center text-sm text-gray-500 sm:mt-0">
                    <p>
                      Created on <time dateTime={receipt.createdAt}>{new Date(receipt.createdAt).toLocaleDateString()}</time>
                    </p>
                    <Button variant="ghost" size="sm" className="ml-2" onClick={() => handleViewDetails(receipt)}>
                      <Eye className="h-4 w-4 mr-1" /> View Details
                    </Button>
                    {!receipt.isPosted && (
                      <Button variant="ghost" size="sm" className="ml-2 text-blue-600" onClick={() => handlePostClick(receipt)}>
                        <Send className="h-4 w-4 mr-1" /> Post
                      </Button>
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
            <AlertDialogTitle>Are you sure you want to post this receipt?</AlertDialogTitle>
            <AlertDialogDescription>
              This action will permanently affect the customer's wallet balance and cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmPost} disabled={postReceiptMutation.isPending}>
              {postReceiptMutation.isPending ? "Posting..." : "Post Receipt"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};
