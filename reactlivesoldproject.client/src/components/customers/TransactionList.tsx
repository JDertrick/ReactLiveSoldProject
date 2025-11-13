import { WalletTransaction } from "../../types/wallet.types";

interface TransactionListProps {
  transactions: WalletTransaction[];
  isLoading?: boolean;
}

export const TransactionList = ({ transactions, isLoading }: TransactionListProps) => {
  const getTransactionIcon = (type: string) => {
    const isCredit = type.toLowerCase() === "credit";
    return isCredit ? (
      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
      </svg>
    ) : (
      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
      </svg>
    );
  };

  const getTransactionColor = (type: string) => {
    const isCredit = type.toLowerCase() === "credit";
    return isCredit
      ? "bg-green-100 text-green-700 ring-green-600/20"
      : "bg-red-100 text-red-700 ring-red-600/20";
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div
            key={i}
            className="animate-pulse bg-gray-100 rounded-lg p-4 h-20"
          ></div>
        ))}
      </div>
    );
  }

  if (!transactions || transactions.length === 0) {
    return (
      <div className="text-center py-8">
        <svg
          className="mx-auto h-12 w-12 text-gray-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
          />
        </svg>
        <h3 className="mt-2 text-sm font-medium text-gray-900">No transactions yet</h3>
        <p className="mt-1 text-sm text-gray-500">
          Add funds or create a transaction to get started.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {transactions.map((transaction, index) => {
        const isCredit = transaction.type.toLowerCase() === "credit";
        return (
          <div
            key={transaction.id}
            className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-all duration-200 animate-slideInUp"
            style={{ animationDelay: `${index * 50}ms` }}
          >
            <div className="flex items-start justify-between">
              <div className="flex items-start space-x-3 flex-1">
                {/* Icon */}
                <div
                  className={`flex-shrink-0 rounded-full p-2 ring-2 ${getTransactionColor(
                    transaction.type
                  )}`}
                >
                  {getTransactionIcon(transaction.type)}
                </div>

                {/* Transaction Details */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-semibold text-gray-900 capitalize">
                      {transaction.type}
                    </p>
                    <p
                      className={`text-lg font-bold ${
                        isCredit ? "text-green-600" : "text-red-600"
                      }`}
                    >
                      {isCredit ? "+" : "-"}${Math.abs(transaction.amount).toFixed(2)}
                    </p>
                  </div>

                  {transaction.notes && (
                    <p className="mt-1 text-sm text-gray-600 line-clamp-2">
                      {transaction.notes}
                    </p>
                  )}

                  <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-gray-500">
                    <span className="flex items-center">
                      <svg className="mr-1 h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      {formatDate(transaction.createdAt)}
                    </span>

                    {transaction.authorizedByUserName && (
                      <span className="flex items-center">
                        <svg className="mr-1 h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                        </svg>
                        {transaction.authorizedByUserName}
                      </span>
                    )}

                    {transaction.relatedSalesOrderId && (
                      <span className="flex items-center">
                        <svg className="mr-1 h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                        </svg>
                        Order
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
};
