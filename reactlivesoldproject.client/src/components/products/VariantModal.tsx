import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import { ProductVariantDto } from "../../types/product.types";
import VariantManager from "./VariantManager";

interface VariantModalProps {
  isOpen: boolean;
  productName?: string;
  variants: ProductVariantDto[];
  onClose: () => void;
  onSaveVariants: (variants: ProductVariantDto[]) => void;
}

const VariantModal = ({
  isOpen,
  productName = "Product",
  variants,
  onClose,
  onSaveVariants,
}: VariantModalProps) => {
  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-20">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
      />

      <div className="fixed inset-0 z-20 w-screen overflow-y-auto">
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
                Manage Variants - {productName}
              </DialogTitle>

              <VariantManager
                variants={variants}
                onVariantsChange={onSaveVariants}
              />
            </div>

            <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
              <button
                type="button"
                onClick={onClose}
                className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm"
              >
                Done
              </button>
            </div>
          </DialogPanel>
        </div>
      </div>
    </Dialog>
  );
};

export default VariantModal;
