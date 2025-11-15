import { useState } from 'react';
import { Receipt } from '../../types/wallet.types';
import { ReceiptDetails } from './ReceiptDetails';
import { DollarSign, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface ReceiptListProps {
  receipts: Receipt[];
  isLoading: boolean;
}

export const ReceiptList = ({ receipts, isLoading }: ReceiptListProps) => {
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [selectedReceipt, setSelectedReceipt] = useState<Receipt | null>(null);

  const handleViewDetails = (receipt: Receipt) => {
    setSelectedReceipt(receipt);
    setIsDetailsModalOpen(true);
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
                  <p className="text-sm font-medium text-indigo-600 truncate">
                    {receipt.type}
                  </p>
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
    </div>
  );
};
