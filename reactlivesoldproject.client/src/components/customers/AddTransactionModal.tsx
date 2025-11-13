import { useState } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import { useCreateWalletTransaction } from "../../hooks/useWallet";
import { Customer } from "../../types/customer.types";

interface AddTransactionModalProps {
  isOpen: boolean;
  onClose: () => void;
  customer: Customer;
}

export const AddTransactionModal = ({
  isOpen,
  onClose,
  customer,
}: AddTransactionModalProps) => {
  const [transactionType, setTransactionType] = useState<
    "Deposit" | "Withdrawal"
  >("Deposit");
  const [amount, setAmount] = useState("");
  const [notes, setNotes] = useState("");
  const [errors, setErrors] = useState<{ amount?: string; notes?: string }>({});

  const createTransaction = useCreateWalletTransaction();

  const validateForm = () => {
    const newErrors: { amount?: string; notes?: string } = {};

    if (!amount || parseFloat(amount) <= 0) {
      newErrors.amount = "Please enter a valid amount greater than 0";
    }

    if (parseFloat(amount) > 1000000) {
      newErrors.amount = "Amount cannot exceed $1,000,000";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      await createTransaction.mutateAsync({
        customerId: customer.id,
        type: transactionType,
        amount: parseFloat(amount),
        notes: notes.trim() || undefined,
      });

      // Reset form and close modal
      setAmount("");
      setNotes("");
      setErrors({});
      setTransactionType("Deposit");
      onClose();
    } catch (error) {
      console.error("Error creating transaction:", error);
    }
  };

  const handleClose = () => {
    if (!createTransaction.isPending) {
      setAmount("");
      setNotes("");
      setErrors({});
      setTransactionType("Deposit");
      onClose();
    }
  };

  const currentBalance = customer.wallet?.balance ?? 0;
  const projectedBalance =
    transactionType === "Deposit"
      ? currentBalance + (parseFloat(amount) || 0)
      : currentBalance - (parseFloat(amount) || 0);

  return (
    <Dialog open={isOpen} onClose={handleClose} className="relative z-50">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-gray-900/75 backdrop-blur-sm transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
      />

      <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
        <div className="flex min-h-full items-center justify-center p-4">
          <DialogPanel
            transition
            className="relative transform overflow-hidden rounded-2xl bg-white shadow-2xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:w-full sm:max-w-lg data-closed:sm:translate-y-0 data-closed:sm:scale-95"
          >
            <form onSubmit={handleSubmit}>
              {/* Header */}
              <div className="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 pt-6 pb-8 text-white">
                <div className="flex items-start justify-between">
                  <DialogTitle className="text-xl font-bold">
                    Add Transaction
                  </DialogTitle>
                  <button
                    type="button"
                    onClick={handleClose}
                    disabled={createTransaction.isPending}
                    className="text-white/80 hover:text-white transition-colors disabled:opacity-50"
                  >
                    <svg
                      className="h-6 w-6"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </div>
                <p className="mt-2 text-sm text-white/80">
                  {customer.firstName} {customer.lastName}
                </p>
              </div>

              <div className="px-6 py-6 space-y-6">
                {/* Transaction Type Selector */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-3">
                    Transaction Type
                  </label>
                  <div className="grid grid-cols-2 gap-3">
                    <button
                      type="button"
                      onClick={() => setTransactionType("Deposit")}
                      className={`relative flex items-center justify-center rounded-lg px-4 py-3 text-sm font-semibold transition-all ${
                        transactionType === "Deposit"
                          ? "bg-green-600 text-white shadow-lg shadow-green-600/50 scale-105"
                          : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                      }`}
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
                      Add Funds
                      {transactionType === "Deposit" && (
                        <span className="absolute -top-1 -right-1 flex h-3 w-3">
                          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                          <span className="relative inline-flex rounded-full h-3 w-3 bg-green-500"></span>
                        </span>
                      )}
                    </button>

                    <button
                      type="button"
                      onClick={() => setTransactionType("Withdrawal")}
                      className={`relative flex items-center justify-center rounded-lg px-4 py-3 text-sm font-semibold transition-all ${
                        transactionType === "Withdrawal"
                          ? "bg-red-600 text-white shadow-lg shadow-red-600/50 scale-105"
                          : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                      }`}
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
                          d="M20 12H4"
                        />
                      </svg>
                      Deduct Funds
                      {transactionType === "Withdrawal" && (
                        <span className="absolute -top-1 -right-1 flex h-3 w-3">
                          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                          <span className="relative inline-flex rounded-full h-3 w-3 bg-red-500"></span>
                        </span>
                      )}
                    </button>
                  </div>
                </div>

                {/* Amount Input */}
                <div>
                  <label
                    htmlFor="amount"
                    className="block text-sm font-medium text-gray-700 mb-2"
                  >
                    Amount
                  </label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                      <span className="text-gray-500 text-lg font-semibold">
                        $
                      </span>
                    </div>
                    <input
                      type="number"
                      id="amount"
                      name="amount"
                      step="0.01"
                      min="0"
                      value={amount}
                      onChange={(e) => {
                        setAmount(e.target.value);
                        setErrors({ ...errors, amount: undefined });
                      }}
                      className={`block w-full pl-8 pr-4 py-3 text-lg font-semibold rounded-lg border-2 transition-colors ${
                        errors.amount
                          ? "border-red-300 focus:border-red-500 focus:ring-red-500"
                          : "border-gray-300 focus:border-indigo-500 focus:ring-indigo-500"
                      }`}
                      placeholder="0.00"
                      disabled={createTransaction.isPending}
                    />
                  </div>
                  {errors.amount && (
                    <p className="mt-2 text-sm text-red-600 flex items-center">
                      <svg
                        className="w-4 h-4 mr-1"
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path
                          fillRule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                          clipRule="evenodd"
                        />
                      </svg>
                      {errors.amount}
                    </p>
                  )}
                </div>

                {/* Balance Preview */}
                {amount && parseFloat(amount) > 0 && (
                  <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 space-y-2 animate-fadeIn">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Current Balance:</span>
                      <span className="font-semibold text-gray-900">
                        ${currentBalance.toFixed(2)}
                      </span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">
                        {transactionType === "Deposit"
                          ? "Adding:"
                          : "Deducting:"}
                      </span>
                      <span
                        className={`font-semibold ${
                          transactionType === "Deposit"
                            ? "text-green-600"
                            : "text-red-600"
                        }`}
                      >
                        {transactionType === "Deposit" ? "+" : "-"}$
                        {parseFloat(amount).toFixed(2)}
                      </span>
                    </div>
                    <div className="pt-2 border-t border-gray-300 flex justify-between">
                      <span className="text-sm font-semibold text-gray-900">
                        New Balance:
                      </span>
                      <span
                        className={`text-lg font-bold ${
                          projectedBalance >= 0
                            ? "text-indigo-600"
                            : "text-red-600"
                        }`}
                      >
                        ${projectedBalance.toFixed(2)}
                      </span>
                    </div>
                    {projectedBalance < 0 && (
                      <div className="flex items-center text-xs text-amber-700 bg-amber-50 rounded-md px-3 py-2 mt-2">
                        <svg
                          className="w-4 h-4 mr-1"
                          fill="currentColor"
                          viewBox="0 0 20 20"
                        >
                          <path
                            fillRule="evenodd"
                            d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                            clipRule="evenodd"
                          />
                        </svg>
                        Warning: This will result in a negative balance
                      </div>
                    )}
                  </div>
                )}

                {/* Notes */}
                <div>
                  <label
                    htmlFor="notes"
                    className="block text-sm font-medium text-gray-700 mb-2"
                  >
                    Notes{" "}
                    <span className="text-gray-400 font-normal">
                      (optional)
                    </span>
                  </label>
                  <textarea
                    id="notes"
                    name="notes"
                    rows={3}
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    className="block w-full rounded-lg border-2 border-gray-300 px-4 py-3 focus:border-indigo-500 focus:ring-indigo-500 transition-colors"
                    placeholder="Add a note about this transaction..."
                    disabled={createTransaction.isPending}
                    maxLength={500}
                  />
                  <p className="mt-1 text-xs text-gray-500 text-right">
                    {notes.length}/500 characters
                  </p>
                </div>
              </div>

              {/* Footer */}
              <div className="bg-gray-50 px-6 py-4 flex flex-row-reverse gap-3">
                <button
                  type="submit"
                  disabled={
                    createTransaction.isPending ||
                    !amount ||
                    parseFloat(amount) <= 0
                  }
                  className={`inline-flex justify-center items-center rounded-lg px-6 py-2.5 text-sm font-semibold text-white shadow-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed ${
                    transactionType === "Deposit"
                      ? "bg-green-600 hover:bg-green-700 hover:shadow-green-600/50"
                      : "bg-red-600 hover:bg-red-700 hover:shadow-red-600/50"
                  }`}
                >
                  {createTransaction.isPending ? (
                    <>
                      <svg
                        className="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
                        fill="none"
                        viewBox="0 0 24 24"
                      >
                        <circle
                          className="opacity-25"
                          cx="12"
                          cy="12"
                          r="10"
                          stroke="currentColor"
                          strokeWidth="4"
                        ></circle>
                        <path
                          className="opacity-75"
                          fill="currentColor"
                          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                        ></path>
                      </svg>
                      Processing...
                    </>
                  ) : (
                    <>
                      {transactionType === "Deposit"
                        ? "Add Funds"
                        : "Deduct Funds"}
                    </>
                  )}
                </button>
                <button
                  type="button"
                  onClick={handleClose}
                  disabled={createTransaction.isPending}
                  className="inline-flex justify-center rounded-lg border-2 border-gray-300 bg-white px-6 py-2.5 text-sm font-semibold text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Cancel
                </button>
              </div>
            </form>
          </DialogPanel>
        </div>
      </div>
    </Dialog>
  );
};
