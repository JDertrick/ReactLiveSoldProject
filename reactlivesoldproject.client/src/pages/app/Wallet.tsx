import { useState } from 'react';
import { useGetAllWallets, useCreateWalletTransaction, useGetWalletTransactions } from '../../hooks/useWallet';
import { Wallet } from '../../types/wallet.types';

const WalletPage = () => {
  const { data: wallets, isLoading } = useGetAllWallets();
  const createTransaction = useCreateWalletTransaction();

  const [selectedWallet, setSelectedWallet] = useState<Wallet | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [transactionType, setTransactionType] = useState<'Credit' | 'Debit'>('Credit');
  const [amount, setAmount] = useState('');
  const [notes, setNotes] = useState('');
  const [viewTransactions, setViewTransactions] = useState(false);

  const { data: transactions } = useGetWalletTransactions(selectedWallet?.customerId || '');

  const handleOpenModal = (wallet: Wallet, type: 'Credit' | 'Debit') => {
    setSelectedWallet(wallet);
    setTransactionType(type);
    setAmount('');
    setNotes('');
    setIsModalOpen(true);
    setViewTransactions(false);
  };

  const handleViewTransactions = (wallet: Wallet) => {
    setSelectedWallet(wallet);
    setViewTransactions(true);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedWallet(null);
    setViewTransactions(false);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedWallet || !amount || parseFloat(amount) <= 0) {
      alert('Please enter a valid amount');
      return;
    }

    try {
      await createTransaction.mutateAsync({
        customerId: selectedWallet.customerId,
        type: transactionType,
        amount: parseFloat(amount),
        notes: notes || undefined,
      });

      handleCloseModal();
    } catch (error) {
      console.error('Error creating transaction:', error);
      alert('Failed to process transaction. Please try again.');
    }
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
          <h1 className="text-2xl font-semibold text-gray-900">Customer Wallets</h1>
          <p className="mt-2 text-sm text-gray-700">
            Manage customer wallet balances and view transaction history.
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-3">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">Total Customers</dt>
                  <dd className="text-2xl font-semibold text-gray-900">{wallets?.length || 0}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">Total Balance</dt>
                  <dd className="text-2xl font-semibold text-gray-900">
                    ${wallets?.reduce((sum, w) => sum + w.balance, 0).toFixed(2) || '0.00'}
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
                <svg className="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">Average Balance</dt>
                  <dd className="text-2xl font-semibold text-gray-900">
                    ${wallets && wallets.length > 0
                      ? (wallets.reduce((sum, w) => sum + w.balance, 0) / wallets.length).toFixed(2)
                      : '0.00'}
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
                    <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                      Customer
                    </th>
                    <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                      Email
                    </th>
                    <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                      Balance
                    </th>
                    <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                      Last Updated
                    </th>
                    <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
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
                          <span className={`font-semibold ${wallet.balance > 0 ? 'text-green-600' : 'text-gray-500'}`}>
                            ${wallet.balance.toFixed(2)}
                          </span>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                          {new Date(wallet.updatedAt).toLocaleDateString()}
                        </td>
                        <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                          <div className="flex gap-2 justify-end">
                            <button
                              onClick={() => handleOpenModal(wallet, 'Credit')}
                              className="text-green-600 hover:text-green-900"
                            >
                              Add Funds
                            </button>
                            <button
                              onClick={() => handleOpenModal(wallet, 'Debit')}
                              className="text-red-600 hover:text-red-900"
                            >
                              Deduct
                            </button>
                            <button
                              onClick={() => handleViewTransactions(wallet)}
                              className="text-indigo-600 hover:text-indigo-900"
                            >
                              History
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="px-3 py-4 text-sm text-gray-500 text-center">
                        No wallets found. Wallets are created automatically when customers are added.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {/* Modal */}
      {isModalOpen && selectedWallet && (
        <div className="fixed z-10 inset-0 overflow-y-auto">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={handleCloseModal}></div>

            <div className="inline-block align-bottom bg-white rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full sm:p-6">
              {viewTransactions ? (
                <div>
                  <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
                    Transaction History - {selectedWallet.customerName}
                  </h3>
                  <div className="mt-4 max-h-96 overflow-y-auto">
                    {transactions && transactions.length > 0 ? (
                      <ul className="divide-y divide-gray-200">
                        {transactions.map((transaction) => (
                          <li key={transaction.id} className="py-3">
                            <div className="flex justify-between items-start">
                              <div className="flex-1">
                                <div className="flex items-center gap-2">
                                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                    transaction.type === 'Credit' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                                  }`}>
                                    {transaction.type}
                                  </span>
                                  <span className="text-sm text-gray-500">
                                    {new Date(transaction.createdAt).toLocaleString()}
                                  </span>
                                </div>
                                {transaction.notes && (
                                  <p className="mt-1 text-sm text-gray-600">{transaction.notes}</p>
                                )}
                                {transaction.authorizedByUserName && (
                                  <p className="mt-1 text-xs text-gray-500">By: {transaction.authorizedByUserName}</p>
                                )}
                              </div>
                              <span className={`text-sm font-semibold ${
                                transaction.type === 'Credit' ? 'text-green-600' : 'text-red-600'
                              }`}>
                                {transaction.type === 'Credit' ? '+' : '-'}${transaction.amount.toFixed(2)}
                              </span>
                            </div>
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="text-sm text-gray-500 text-center py-8">No transactions yet</p>
                    )}
                  </div>
                  <div className="mt-5">
                    <button
                      type="button"
                      onClick={handleCloseModal}
                      className="w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:text-sm"
                    >
                      Close
                    </button>
                  </div>
                </div>
              ) : (
                <form onSubmit={handleSubmit}>
                  <div>
                    <h3 className="text-lg leading-6 font-medium text-gray-900">
                      {transactionType === 'Credit' ? 'Add Funds' : 'Deduct Funds'}
                    </h3>
                    <div className="mt-4">
                      <div className="bg-gray-50 rounded-lg p-4 mb-4">
                        <p className="text-sm font-medium text-gray-900">{selectedWallet.customerName}</p>
                        <p className="text-xs text-gray-500">{selectedWallet.customerEmail}</p>
                        <p className="mt-2 text-sm">
                          <span className="text-gray-600">Current Balance:</span>{' '}
                          <span className="font-semibold text-gray-900">${selectedWallet.balance.toFixed(2)}</span>
                        </p>
                      </div>

                      <div className="mb-4">
                        <label htmlFor="amount" className="block text-sm font-medium text-gray-700">
                          Amount
                        </label>
                        <div className="mt-1 relative rounded-md shadow-sm">
                          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <span className="text-gray-500 sm:text-sm">$</span>
                          </div>
                          <input
                            type="number"
                            name="amount"
                            id="amount"
                            step="0.01"
                            min="0.01"
                            required
                            value={amount}
                            onChange={(e) => setAmount(e.target.value)}
                            className="focus:ring-indigo-500 focus:border-indigo-500 block w-full pl-7 pr-12 sm:text-sm border-gray-300 rounded-md px-3 py-2 border"
                            placeholder="0.00"
                          />
                        </div>
                      </div>

                      <div className="mb-4">
                        <label htmlFor="notes" className="block text-sm font-medium text-gray-700">
                          Notes (optional)
                        </label>
                        <textarea
                          id="notes"
                          name="notes"
                          rows={3}
                          value={notes}
                          onChange={(e) => setNotes(e.target.value)}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                          placeholder="Reason for transaction..."
                        />
                      </div>

                      {amount && parseFloat(amount) > 0 && (
                        <div className="bg-indigo-50 rounded-lg p-4 mb-4">
                          <p className="text-sm">
                            <span className="text-gray-600">New Balance:</span>{' '}
                            <span className="font-semibold text-indigo-600">
                              ${(selectedWallet.balance + (transactionType === 'Credit' ? parseFloat(amount) : -parseFloat(amount))).toFixed(2)}
                            </span>
                          </p>
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="mt-5 sm:mt-6 sm:grid sm:grid-cols-2 sm:gap-3 sm:grid-flow-row-dense">
                    <button
                      type="submit"
                      disabled={createTransaction.isPending}
                      className={`w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 text-base font-medium text-white focus:outline-none focus:ring-2 focus:ring-offset-2 sm:col-start-2 sm:text-sm disabled:opacity-50 ${
                        transactionType === 'Credit'
                          ? 'bg-green-600 hover:bg-green-700 focus:ring-green-500'
                          : 'bg-red-600 hover:bg-red-700 focus:ring-red-500'
                      }`}
                    >
                      {createTransaction.isPending ? 'Processing...' : transactionType === 'Credit' ? 'Add Funds' : 'Deduct Funds'}
                    </button>
                    <button
                      type="button"
                      onClick={handleCloseModal}
                      className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:col-start-1 sm:text-sm"
                    >
                      Cancel
                    </button>
                  </div>
                </form>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default WalletPage;
