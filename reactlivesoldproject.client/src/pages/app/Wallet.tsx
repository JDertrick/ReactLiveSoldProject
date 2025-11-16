import { useState } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import { useGetAllWallets, useGetReceipts } from "../../hooks/useWallet";
import { useGetCustomer } from "../../hooks/useCustomers";
import { Wallet } from "../../types/wallet.types";
import { CreateReceiptModal } from "../../components/customers/CreateReceiptModal";

const WalletPage = () => {
  const { data: wallets, isLoading } = useGetAllWallets();

  const [selectedWallet, setSelectedWallet] = useState<Wallet | null>(null);
  const [isCreateReceiptModalOpen, setIsCreateReceiptModalOpen] =
    useState(false);
  const [isReceiptDetailsModalOpen, setIsReceiptDetailsModalOpen] =
    useState(false);

  const { data: receipts } = useGetReceipts(selectedWallet?.customerId || "");
  const { data: selectedCustomer } = useGetCustomer(selectedWallet?.customerId || "");

  const handleOpenCreateReceiptModal = (wallet: Wallet) => {
    setSelectedWallet(wallet);
    setIsCreateReceiptModalOpen(true);
  };

  const handleViewReceipts = (wallet: Wallet) => {
    setSelectedWallet(wallet);
    setIsReceiptDetailsModalOpen(true); // This will open the ReceiptDetails modal, but it will show a list of receipts
  };

  const handleCloseModals = () => {
    setIsCreateReceiptModalOpen(false);
    setIsReceiptDetailsModalOpen(false);
    setSelectedWallet(null);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-semibold text-gray-900">
            Customer Wallets
          </h1>
          <p className="mt-2 text-sm text-gray-700">
            Manage customer wallet balances and view receipt history.
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-3">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg
                  className="h-6 w-6 text-gray-400"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                  />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Total Customers
                  </dt>
                  <dd className="text-2xl font-semibold text-gray-900">
                    {wallets?.length || 0}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg
                  className="h-6 w-6 text-gray-400"
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
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Total Balance
                  </dt>
                  <dd className="text-2xl font-semibold text-gray-900">
                    $
                    {wallets
                      ?.reduce((sum, w) => sum + w.balance, 0)
                      .toFixed(2) || "0.00"}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg
                  className="h-6 w-6 text-gray-400"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                  />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    Average Balance
                  </dt>
                  <dd className="text-2xl font-semibold text-gray-900">
                    $
                    {wallets && wallets.length > 0
                      ? (
                          wallets.reduce((sum, w) => sum + w.balance, 0) /
                          wallets.length
                        ).toFixed(2)
                      : "0.00"}
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Wallets Table */}
      <div className="mt-8 flex flex-col">
        <div className="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
          <div className="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
            <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
              <table className="min-w-full divide-y divide-gray-300">
                <thead className="bg-gray-50">
                  <tr>
                    <th
                      scope="col"
                      className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6"
                    >
                      Customer
                    </th>
                    <th
                      scope="col"
                      className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900"
                    >
                      Email
                    </th>
                    <th
                      scope="col"
                      className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900"
                    >
                      Balance
                    </th>
                    <th
                      scope="col"
                      className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900"
                    >
                      Last Updated
                    </th>
                    <th
                      scope="col"
                      className="relative py-3.5 pl-3 pr-4 sm:pr-6"
                    >
                      <span className="sr-only">Actions</span>
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 bg-white">
                  {wallets && wallets.length > 0 ? (
                    wallets.map((wallet) => (
                      <tr key={wallet.id}>
                        <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                          {wallet.customerName}
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          {wallet.customerEmail}
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm">
                          <span
                            className={`font-semibold ${
                              wallet.balance > 0
                                ? "text-green-600"
                                : "text-gray-500"
                            }`}
                          >
                            ${wallet.balance.toFixed(2)}
                          </span>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          {new Date(wallet.updatedAt).toLocaleDateString()}
                        </td>
                        <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                          <div className="flex gap-2 justify-end">
                            <button
                              onClick={() =>
                                handleOpenCreateReceiptModal(wallet)
                              }
                              className="text-indigo-600 hover:text-indigo-900"
                            >
                              Create Receipt
                            </button>
                            <button
                              onClick={() => handleViewReceipts(wallet)}
                              className="text-indigo-600 hover:text-indigo-900"
                            >
                              Receipts History
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td
                        colSpan={5}
                        className="px-3 py-4 text-sm text-gray-500 text-center"
                      >
                        No wallets found. Wallets are created automatically when
                        customers are added.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {/* Create Receipt Modal */}
      {selectedWallet && selectedCustomer && (
        <CreateReceiptModal
          isOpen={isCreateReceiptModalOpen}
          onClose={handleCloseModals}
          customer={selectedCustomer}
        />
      )}

      {/* Receipt Details Modal (or list of receipts) */}
      {selectedWallet && (
        <Dialog
          open={isReceiptDetailsModalOpen}
          onClose={handleCloseModals}
          className="relative z-10"
        >
          <DialogBackdrop
            transition
            className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
          />

          <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
            <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
              <DialogPanel
                transition
                className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-2xl data-closed:sm:translate-y-0 data-closed:sm:scale-95"
              >
                <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                  <DialogTitle
                    as="h3"
                    className="text-lg leading-6 font-medium text-gray-900 mb-4"
                  >
                    Receipt History - {selectedWallet.customerName}
                  </DialogTitle>
                  <div className="mt-4 max-h-96 overflow-y-auto">
                    {receipts && receipts.length > 0 ? (
                      <ul className="divide-y divide-gray-200">
                        {receipts.map((receipt) => (
                          <li key={receipt.id} className="py-3">
                            <div className="flex justify-between items-start">
                              <div className="flex-1">
                                <div className="flex items-center gap-2">
                                  <span
                                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                      receipt.type === "Deposit"
                                        ? "bg-green-100 text-green-800"
                                        : "bg-red-100 text-red-800"
                                    }`}
                                  >
                                    {receipt.type}
                                  </span>
                                  <span className="text-sm text-gray-500">
                                    {new Date(
                                      receipt.createdAt
                                    ).toLocaleString()}
                                  </span>
                                </div>
                                {receipt.notes && (
                                  <p className="mt-1 text-sm text-gray-600">
                                    {receipt.notes}
                                  </p>
                                )}
                                {receipt.createdByUserName && (
                                  <p className="mt-1 text-xs text-gray-500">
                                    By: {receipt.createdByUserName}
                                  </p>
                                )}
                              </div>
                              <span
                                className={`text-sm font-semibold ${
                                  receipt.type === "Deposit"
                                    ? "text-green-600"
                                    : "text-red-600"
                                }`}
                              >
                                {receipt.type === "Deposit" ? "+" : "-"}$
                                {receipt.totalAmount.toFixed(2)}
                              </span>
                            </div>
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="text-sm text-gray-500 text-center py-8">
                        No receipts yet
                      </p>
                    )}
                  </div>
                  <div className="mt-5">
                    <button
                      type="button"
                      onClick={handleCloseModals}
                      className="w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:text-sm"
                    >
                      Close
                    </button>
                  </div>
                </div>
              </DialogPanel>
            </div>
          </div>
        </Dialog>
      )}
    </div>
  );
};

export default WalletPage;
