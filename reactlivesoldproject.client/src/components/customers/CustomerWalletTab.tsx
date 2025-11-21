import { useState } from "react";
import { Customer } from "../../types/customer.types";
import { useGetReceipts } from "../../hooks/useWallet";
import { ReceiptList } from "./ReceiptList";
import { CreateReceiptModal } from "./CreateReceiptModal";
import { ReceiptDetails } from "./ReceiptDetails";
import { Receipt } from "../../types/wallet.types";
import { formatCurrency } from "@/utils/currencyHelper";
import { Card, CardContent } from "@/components/ui/card";
import { ArrowDownIcon, ArrowUpIcon, ReceiptIcon } from "lucide-react";

interface CustomerWalletTabProps {
  customer: Customer;
}

const StatCard = ({
  title,
  value,
  subtext,
  icon,
  colorClass,
  circleColorClass,
  changePercent,
  changeType,
}: {
  title: string;
  value: string | number;
  subtext?: string;
  icon: React.ReactNode;
  colorClass: string;
  circleColorClass: string;
  changePercent?: string;
  changeType?: "up" | "down" | "neutral";
}) => (
  <Card className="relative overflow-hidden border-0 shadow-md">
    <div
      className={`absolute -top-4 -right-4 w-24 h-24 rounded-full ${circleColorClass} opacity-20`}
    ></div>
    <CardContent className="pt-6">
      <div className="relative z-10">
        <div className="flex items-start justify-between mb-3">
          <div className="flex-1 pt-10">
            <p className="text-sm font-medium text-gray-500 mb-2">{title}</p>
            <p className={`text-3xl font-bold ${colorClass}`}>{value}</p>
            {subtext && <p className="text-xs text-gray-400 mt-1">{subtext}</p>}
          </div>
          <div className={`rounded-full p-3 ${colorClass} bg-opacity-10`}>
            {icon}
          </div>
        </div>
        {changePercent && (
          <div className="flex items-center gap-1 mt-2 pt-10">
            {changeType === "up" && (
              <span className="inline-flex items-center text-xs font-medium text-green-600 bg-green-100 rounded-full px-2 py-0.5">
                <ArrowUpIcon className="w-3 h-3 mr-1" />
                {changePercent}
              </span>
            )}
            {changeType === "down" && (
              <span className="inline-flex items-center text-xs font-medium text-red-600 bg-red-100 rounded-full px-2 py-0.5">
                <ArrowDownIcon className="w-3 h-3 mr-1" />
                {changePercent}
              </span>
            )}
            {changeType === "neutral" && (
              <span className="inline-flex items-center text-xs font-medium text-gray-600 bg-gray-100 rounded-full px-2 py-0.5">
                {changePercent}
              </span>
            )}
          </div>
        )}
      </div>
    </CardContent>
  </Card>
);

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

  // Get last receipt date
  const lastReceiptDate =
    receipts && receipts.length > 0
      ? new Date(receipts[0].createdAt).toLocaleDateString("es-ES", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        })
      : null;

  return (
    <div className="space-y-6 animate-fadeIn">
      {/* Header */}
      <div className="mb-6">
        <h2 className="text-3xl font-bold text-gray-900">Billetera Digital</h2>
        <p className="text-gray-500 mt-1">
          Gestionando fondos de:{" "}
          <span className="text-indigo-600 font-medium">
            {customer.firstName} {customer.lastName}
          </span>
        </p>
      </div>

      <div className="grid grid-flow-col gap-4">
        {/* Wallet Balance Card */}
        <div className="bg-gradient-to-br from-indigo-600 via-purple-600 to-pink-600 rounded-2xl p-8 text-white shadow-2xl relative overflow-hidden">
          {/* Decorative elements */}
          <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full -mr-32 -mt-32"></div>
          <div className="absolute bottom-0 left-0 w-48 h-48 bg-white/10 rounded-full -ml-24 -mb-24"></div>

          <div className="relative z-10">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <div className="inline-flex items-center gap-2 bg-white/20 backdrop-blur-sm rounded-full px-4 py-2 mb-4">
                  <div className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></div>
                  <span className="text-sm font-medium">Activa</span>
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M8.111 16.404a5.5 5.5 0 017.778 0M12 20h.01m-7.08-7.071c3.904-3.905 10.236-3.905 14.141 0M1.394 9.393c5.857-5.857 15.355-5.857 21.213 0"
                    />
                  </svg>
                </div>
                <p className="text-sm opacity-90 font-medium uppercase tracking-wider mb-2">
                  Saldo Disponible
                </p>
                <p className="text-5xl font-bold tracking-tight">
                  {formatCurrency(currentBalance ?? 0)}
                </p>
                <p className="text-sm opacity-80 mt-4 font-medium">TITULAR</p>
                <p className="text-lg font-semibold">
                  {customer.firstName} {customer.lastName}
                </p>
              </div>
              <div className="bg-white/20 backdrop-blur-sm rounded-xl p-4">
                <svg
                  className="w-10 h-10"
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
          </div>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <StatCard
            title="TOTAL DEPÓSITOS"
            value={formatCurrency(totalCredits)}
            subtext="En transacción"
            icon={<ArrowDownIcon className="w-6 h-6 text-green-600" />}
            colorClass="text-green-600"
            circleColorClass="bg-green-500"
            changePercent="+12%"
            changeType="up"
          />
          <StatCard
            title="TOTAL DÉBITOS"
            value={formatCurrency(totalDebits)}
            subtext="Sin retiros este mes"
            icon={<ArrowUpIcon className="w-6 h-6 text-red-600" />}
            colorClass="text-red-600"
            circleColorClass="bg-red-500"
            changePercent="0%"
            changeType="neutral"
          />
          <StatCard
            title="RECIBOS GENERADOS"
            value={receiptCount}
            subtext={
              lastReceiptDate ? `Último: ${lastReceiptDate}` : "Sin recibos"
            }
            icon={<ReceiptIcon className="w-6 h-6 text-indigo-600" />}
            colorClass="text-indigo-600"
            circleColorClass="bg-indigo-500"
          />
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-3">
        <button
          className="inline-flex items-center justify-center rounded-xl border-2 border-gray-300 bg-white px-6 py-3 text-sm font-semibold text-gray-700 hover:bg-gray-50 transition-colors"
          title="Estado de cuenta"
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
              d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
          Estado de Cuenta
        </button>
        <button
          onClick={() => setIsCreateReceiptModalOpen(true)}
          className="flex-1 inline-flex items-center justify-center rounded-xl bg-indigo-600 px-6 py-3 text-sm font-semibold text-white shadow-lg hover:bg-indigo-700 hover:shadow-xl transition-all hover:scale-105"
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
          Nueva Transacción
        </button>
      </div>

      {/* Receipt History */}
      <div>
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-xl font-semibold text-gray-900 flex items-center">
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
            {receiptCount > 0 && (
              <span className="ml-3 text-sm font-normal text-gray-500">
                {receiptCount} {receiptCount === 1 ? "recibo" : "recibos"}
              </span>
            )}
          </h3>
          <div className="flex items-center gap-2">
            <input
              type="text"
              placeholder="Buscar recibo..."
              className="px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors">
              <svg
                className="w-5 h-5 text-gray-500"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"
                />
              </svg>
            </button>
          </div>
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
