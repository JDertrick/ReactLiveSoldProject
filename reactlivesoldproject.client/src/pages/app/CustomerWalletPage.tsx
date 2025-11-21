import { useParams } from "react-router-dom";
import { useGetCustomer } from "../../hooks/useCustomers";
import { CustomerWalletTab } from "../../components/customers/CustomerWalletTab";
// import { Spinner } from '@/components/ui/spinner';

const CustomerWalletPage = () => {
  const { customerId } = useParams<{ customerId: string }>();

  const { data: customer, isLoading, error } = useGetCustomer(customerId || "");

  if (!customerId) {
    return (
      <div className="text-center text-red-500">Falta el ID del cliente.</div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        {/* <Spinner /> */}
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center text-red-500">
        Error al cargar los datos del cliente.
      </div>
    );
  }

  if (!customer) {
    return (
      <div className="text-center text-gray-500">Cliente no encontrado.</div>
    );
  }

  return (
    <div>
      <CustomerWalletTab customer={customer} />
    </div>
  );
};

export default CustomerWalletPage;
