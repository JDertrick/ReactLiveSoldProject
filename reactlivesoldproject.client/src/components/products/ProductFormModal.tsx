import { useState, useEffect } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import {
  CreateProductDto,
  UpdateProductDto,
  Product,
} from "../../types/product.types";
import ProductForm from "./ProductForm";

interface ProductFormModalProps {
  isOpen: boolean;
  editingProduct: Product | null;
  isLoading: boolean;
  onClose: () => void;
  onSubmit: (
    data: CreateProductDto | UpdateProductDto,
    isEditing: boolean
  ) => Promise<void>;
}

const ProductFormModal = ({
  isOpen,
  editingProduct,
  isLoading,
  onClose,
  onSubmit,
}: ProductFormModalProps) => {
  const [formData, setFormData] = useState<CreateProductDto | UpdateProductDto>(
    {
      name: "",
      description: "",
      basePrice: 0,
      isPublished: true,
      productType: "Variable",
      categoryId: "",
    }
  );

  // Actualizar formData cuando editingProduct cambie
  useEffect(() => {
    if (editingProduct) {
      setFormData({
        name: editingProduct.name,
        description: editingProduct.description || "",
        basePrice: editingProduct.basePrice,
        isPublished: editingProduct.isPublished,
        categoryId: editingProduct.categoryId || "",
        productType: editingProduct.productType,
      });
    } else {
      setFormData({
        name: "",
        description: "",
        basePrice: 0,
        isPublished: true,
        productType: "Variable",
        categoryId: "",
      });
    }
  }, [editingProduct]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      // Guardar el producto
      await onSubmit(formData, !!editingProduct);
    } catch (error) {
      console.error("Error al guardar producto:", error);
      throw error;
    }
  };

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => {
    const { name, value, type } = e.target;
    setFormData({
      ...formData,
      [name]:
        type === "checkbox"
          ? (e.target as HTMLInputElement).checked
          : type === "number"
          ? parseFloat(value) || 0
          : value,
    });
  };


  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-10">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
      />

      <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
        <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
          <DialogPanel
            transition
            className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-xl data-closed:sm:translate-y-0 data-closed:sm:scale-95"
          >
            <form onSubmit={handleSubmit}>
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                <DialogTitle
                  as="h3"
                  className="text-lg leading-6 font-medium text-gray-900 mb-4"
                >
                  {editingProduct ? "Editar Producto" : "Crear Producto"}
                </DialogTitle>

                <div className="space-y-6">
                  {/* Product Form */}
                  <ProductForm
                    formData={formData}
                    onFormChange={handleChange}
                  />

                  {editingProduct && (
                    <div className="border-t pt-4">
                      <p className="text-sm text-gray-600">
                        Las variantes y tags se gestionan desde la lista de productos después de crear el producto.
                      </p>
                    </div>
                  )}
                </div>
              </div>

              <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                <button
                  type="submit"
                  disabled={isLoading}
                  className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
                >
                  {isLoading ? "Guardando..." : "Guardar Producto"}
                </button>
                <button
                  type="button"
                  onClick={onClose}
                  className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:w-auto sm:text-sm"
                >
                  Cancelar
                </button>
              </div>
            </form>
          </DialogPanel>
        </div>
      </div>
    </Dialog>
  );
};

export default ProductFormModal;
