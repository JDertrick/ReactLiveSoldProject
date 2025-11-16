import { useParams } from "react-router-dom";
import { useGetCustomer } from "../../hooks/useCustomers";
import { CustomerWalletTab } from "../../components/customers/CustomerWalletTab";
// import { Spinner } from '@/components/ui/spinner';

const CustomerWalletPage = () => {
  const { customerId } = useParams<{ customerId: string }>();

  if (!customerId) {
    return (
      <div className="text-center text-red-500">Customer ID is missing.</div>
    );
  }

  const { data: customer, isLoading, error } = useGetCustomer(customerId);

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
        Error loading customer data.
      </div>
    );
  }

  if (!customer) {
    return <div className="text-center text-gray-500">Customer not found.</div>;
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">
        Wallet for {customer.firstName} {customer.lastName}
      </h1>
      <CustomerWalletTab customer={customer} />
    </div>
  );
};

export default CustomerWalletPage;
