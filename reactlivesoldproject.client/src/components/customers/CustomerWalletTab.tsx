import { useState } from "react";
import { Customer } from "../../types/customer.types";
import { useGetReceipts } from "../../hooks/useWallet";
import { ReceiptList } from "./ReceiptList";
import { CreateReceiptModal } from "./CreateReceiptModal";
import { ReceiptDetails } from "./ReceiptDetails";
import { Receipt } from "../../types/wallet.types";

interface CustomerWalletTabProps {
  customer: Customer;
}

export const CustomerWalletTab = ({ customer }: CustomerWalletTabProps) => {
  const [isCreateReceiptModalOpen, setIsCreateReceiptModalOpen] =
    useState(false);
  const [isReceiptDetailsModalOpen, setIsReceiptDetailsModalOpen] =
    useState(false);
  const [selectedReceipt] = useState<Receipt | null>(null);

  const { data: receipts, isLoading } = useGetReceipts(customer.id);

  const currentBalance = customer.wallet?.balance ?? 0;
  const receiptCount = receipts?.length ?? 0;

  // Calculate some statistics based on receipts
  const totalCredits =
    receipts?.reduce(
      (sum, r) =>
        r.isPosted && r.type === "Deposit" ? sum + r.totalAmount : sum,
      0
    ) ?? 0;

  const totalDebits =
    receipts?.reduce(
      (sum, r) =>
        r.isPosted && r.type === "Withdrawal" ? sum + r.totalAmount : sum,
      0
    ) ?? 0;

  return (
    <div className="space-y-6 animate-fadeIn">
      {/* Wallet Balance Card */}
      <div className="bg-gradient-to-br from-indigo-600 via-purple-600 to-pink-600 rounded-2xl p-6 text-white shadow-2xl relative overflow-hidden">
        {/* Decorative elements */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full -mr-32 -mt-32"></div>
        <div className="absolute bottom-0 left-0 w-48 h-48 bg-white/10 rounded-full -ml-24 -mb-24"></div>

        <div className="relative z-10">
          <div className="flex items-start justify-between mb-6">
            <div>
              <p className="text-sm opacity-80 font-medium uppercase tracking-wide">
                Saldo de la Billetera
              </p>
              <p className="text-5xl font-bold mt-2 tracking-tight">
                ${currentBalance.toFixed(2)}
              </p>
            </div>
            <div className="bg-white/20 backdrop-blur-sm rounded-xl p-3">
              <svg
                className="w-8 h-8"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z"
                />
              </svg>
            </div>
          </div>

          {/* Quick Stats */}
          <div className="grid grid-cols-3 gap-4 mt-6 pt-6 border-t border-white/20">
            <div>
              <p className="text-xs opacity-80 font-medium">Total Depositos</p>
              <p className="text-xl font-bold mt-1">
                ${totalCredits.toFixed(2)}
              </p>
            </div>
            <div>
              <p className="text-xs opacity-80 font-medium">Total Debitos</p>
              <p className="text-xl font-bold mt-1">
                ${totalDebits.toFixed(2)}
              </p>
            </div>
            <div>
              <p className="text-xs opacity-80 font-medium">Recibos</p>
              <p className="text-xl font-bold mt-1">{receiptCount}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-3">
        <button
          onClick={() => setIsCreateReceiptModalOpen(true)}
          className="flex-1 inline-flex items-center justify-center rounded-xl bg-indigo-600 px-4 py-3 text-sm font-semibold text-white shadow-lg hover:bg-indigo-700 hover:shadow-xl transition-all hover:scale-105"
        >
          <svg
            className="w-5 h-5 mr-2"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 4v16m8-8H4"
            />
          </svg>
          Crear transacción
        </button>
        <button
          className="inline-flex items-center justify-center rounded-xl border-2 border-gray-300 bg-white px-4 py-3 text-sm font-semibold text-gray-700 hover:bg-gray-50 transition-colors"
          title="Exportar recibos"
        >
          <svg
            className="w-5 h-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
        </button>
      </div>

      {/* Receipt History */}
      <div>
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900 flex items-center">
            <svg
              className="w-5 h-5 mr-2 text-indigo-600"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            Historial de Recibos
          </h3>
          {receiptCount > 0 && (
            <span className="inline-flex items-center rounded-full bg-indigo-100 px-3 py-1 text-xs font-medium text-indigo-700">
              {receiptCount} {receiptCount === 1 ? "recibo" : "recibos"}
            </span>
          )}
        </div>

        <ReceiptList receipts={receipts ?? []} isLoading={isLoading} />
      </div>

      {/* Create Receipt Modal */}
      <CreateReceiptModal
        isOpen={isCreateReceiptModalOpen}
        onClose={() => setIsCreateReceiptModalOpen(false)}
        customer={customer}
      />

      {/* Receipt Details Modal */}
      {selectedReceipt && (
        <ReceiptDetails
          isOpen={isReceiptDetailsModalOpen}
          onClose={() => setIsReceiptDetailsModalOpen(false)}
          receipt={selectedReceipt}
        />
      )}
    </div>
  );
};
